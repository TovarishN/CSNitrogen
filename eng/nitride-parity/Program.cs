// Compares Nitride's parse of a CSNitrogen profile with Nitrogen's generated parser.
// Both parse the raw composed grammar: no preprocessing, Unicode or source policies.
// Usage: dotnet run --project eng/nitride-parity -- <nitride> <profile> <source>...
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Nitrogen;

if (args.Length < 3) { Console.Error.WriteLine("usage: <nitride> <profile> <source>..."); return 2; }
string nitride = args[0], profile = args[1];
string[] sources = args[2..];
string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
string project = Path.Combine(root, "src/CSNitrogen.Syntax/Languages", profile, profile + ".nproj");
string prepared = Path.GetTempFileName();
try
{
    Run(nitride, ["prepare", project, "--output", prepared]);
    var keep = Kept(JsonNode.Parse(File.ReadAllText(prepared))!);
    var observed = Run(nitride, ["parse", project, .. sources]).Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(line => JsonNode.Parse(line)!).ToDictionary(o => (string)o["path"]!);
    var composition = typeof(CSNitrogen.Syntax.CoreModule).Assembly.GetType($"CSNitrogen.Syntax.{profile}Composition")
        ?? throw new ArgumentException($"unknown profile {profile}");
    var language = ((LanguageBuilder)composition.GetMethod("CreateBuilder")!.Invoke(null, null)!).Build();
    var start = (Rule)composition.GetProperty("Start")!.GetValue(null)!;
    var modules = typeof(CSNitrogen.Syntax.CoreModule).Assembly.GetTypes()
        .Where(t => typeof(SyntaxModule).IsAssignableFrom(t) && !t.IsAbstract)
        .Select(t => t.GetProperty("Instance")?.GetValue(null) ?? t.GetField("Instance")?.GetValue(null))
        .OfType<SyntaxModule>().ToDictionary(m => m.Id, m => m.Name);
    int same = 0;
    var differences = new List<object>();
    foreach (var source in sources)
    {
        string expected = Observe(File.ReadAllText(source), language, start, modules, keep);
        string actual = Normalize(observed[source]);
        if (expected == actual) same++;
        else differences.Add(new { source, nitrogen = Summary(expected), nitride = Summary(actual) });
    }
    Console.WriteLine(JsonSerializer.Serialize(new { profile, files = sources.Length, same, differences }));
    return differences.Count == 0 ? 0 : 1;
}
finally { File.Delete(prepared); }

static string Run(string program, string[] arguments)
{
    var info = new ProcessStartInfo(program) { RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var argument in arguments) info.ArgumentList.Add(argument);
    using var process = Process.Start(info)!;
    var output = process.StandardOutput.ReadToEndAsync();
    string error = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (error.Length > 0 && process.ExitCode != 0 && output.Result.Length == 0) throw new Exception(error);
    return output.Result;
}

// Nitride's observer omits alias rules (references or choices of them); so does this one.
static HashSet<string> Kept(JsonNode data)
{
    static bool Alias(JsonNode e) => (string)e["kind"]! switch
    {
        "ref" => true,
        "label" => Alias(e["inner"]!),
        "scope" => Alias(e["inner"]!),
        "choice" => e["items"]!.AsArray().Count > 0 && e["items"]!.AsArray().All(i => Alias(i!)),
        _ => false,
    };
    return data["rules"]!.AsArray().Where(r => !Alias(r!["expr"]!)).Select(r => (string)r!["name"]!).ToHashSet();
}

static string Observe(string source, Language language, Rule start, Dictionary<int, string> modules, HashSet<string> keep)
{
    using var parsed = language.Parse(source, start);
    int[] Span(TextSpan span) => [Encoding.UTF8.GetByteCount(source.AsSpan(0, span.Start)), Encoding.UTF8.GetByteCount(source.AsSpan(0, span.End))];
    if (!parsed.Success) return JsonSerializer.Serialize(new { ok = false, span = Span(parsed.Diagnostics[0].Span) });
    var tree = parsed.Tree;
    var nodes = new List<object>();
    var stack = new Stack<(int Node, int? Parent)>();
    stack.Push((tree.Root, null));
    while (stack.TryPop(out var next))
    {
        int? parent = next.Parent;
        int kind = tree.Kind(next.Node);
        if (modules.TryGetValue(kind >> 16, out var module) && keep.Contains(module + "." + language.GetKindName(kind)))
        {
            parent = nodes.Count;
            nodes.Add(new { rule = module + "." + language.GetKindName(kind), span = Span(tree.Span(next.Node)), parent = next.Parent });
        }
        for (int i = tree.ChildCount(next.Node) - 1; i >= 0; i--)
            if (tree.Child(next.Node, i) is var child && child >= 0) stack.Push((child, parent));
    }
    return JsonSerializer.Serialize(new { ok = true, nodes });
}

static string Normalize(JsonNode observation) => (bool)observation["ok"]!
    ? JsonSerializer.Serialize(new
    {
        ok = true,
        nodes = observation["nodes"]!.AsArray().Select(n => new
        {
            rule = (string)n!["rule"]!,
            span = n["span"]!.AsArray().Select(v => (int)v!).ToArray(),
            parent = (int?)n["parent"],
        }),
    })
    : JsonSerializer.Serialize(new { ok = false, span = observation["span"]!.AsArray().Select(v => (int)v!).ToArray() });

static string Summary(string observation) => observation.Length <= 240 ? observation : observation[..240] + "…";
