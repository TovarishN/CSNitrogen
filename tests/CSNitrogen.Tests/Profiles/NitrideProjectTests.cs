using System.Text.Json;
namespace CSNitrogen.Tests.Profiles;
/// <summary>The Nitride projects (eng/generate-nitride-projects.py) mirror nitrogen.json exactly.</summary>
public class NitrideProjectTests
{
    static readonly string Root=Path.Combine(AppContext.BaseDirectory,"Nitride");
    [Fact] public void Every_language_has_a_project_with_its_ordered_grammars_and_start()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(Root,"nitrogen.json")));
        var languages=doc.RootElement.GetProperty("languages").EnumerateArray().ToArray();
        Assert.Equal(languages.Select(l=>l.GetProperty("name").GetString()).Order(),
            Directory.GetDirectories(Path.Combine(Root,"Languages")).Select(Path.GetFileName).Order());
        foreach(var language in languages)
        {
            string name=language.GetProperty("name").GetString()!;
            var lines=File.ReadAllLines(Path.Combine(Root,"Languages",name,name+".nproj")).Select(l=>l.Trim()).ToArray();
            Assert.Contains($"language {name}",lines);
            Assert.Contains($"extensions .{name.ToLowerInvariant()};",lines);
            Assert.Contains($"start {language.GetProperty("start").GetString()};",lines);
            Assert.Contains($"namespace {language.GetProperty("namespace").GetString()};",lines);
            Assert.Equal(language.GetProperty("grammars").EnumerateArray().Select(g=>$"grammar ../../{g.GetString()};"),
                lines.Where(l=>l.StartsWith("grammar ",StringComparison.Ordinal)));
        }
        var includes=File.ReadAllLines(Path.Combine(Root,"nitride.nproj")).Select(l=>l.Trim()).Where(l=>l.StartsWith("include ",StringComparison.Ordinal));
        Assert.Equal(languages.Select(l=>$"include src/CSNitrogen.Syntax/Languages/{l.GetProperty("name").GetString()};"),includes);
    }
}
