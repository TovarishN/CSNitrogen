using Nitrogen;
using CSNitrogen.Syntax;
using CSNitrogen.Preprocessing;
namespace CSNitrogen.Parsing;
/// <summary>Owns a pooled Nitrogen tree. All borrowed views expire on disposal.</summary>
public sealed class ParseDocument : IDisposable
{
    private readonly ParseResult _parsed;
    internal ParseDocument(ParseRequest request, ParseResult parsed, IEnumerable<ParseIssue> issues, PreprocessResult preprocessing,bool profileComplete)
    {
        OriginalText = request.Source; Profile = request.Profile; ProfileComplete=profileComplete; DocumentId = request.DocumentId;
        _parsed = parsed; Preprocessing=preprocessing; Issues = Array.AsReadOnly(issues.ToArray());
        Accepted = parsed.Success && !Issues.Any(i => i.Severity == DiagnosticSeverity.Error);
    }
    internal ParseResult Parsed => _parsed;
    /// <summary>True for a document from <see cref="CSharpParser.ParseIncremental"/> or <see cref="CSharpParser.Reparse"/>.</summary>
    public bool IsIncremental => _parsed.IsIncremental;
    /// <summary>Tree nodes a reparse copied from the previous document's tree.</summary>
    public int ReusedNodes => _parsed.Stats.ReusedNodes;
    public string OriginalText { get; }
    public PreprocessResult Preprocessing { get; }
    public string MaskedText => Preprocessing.MaskedText;
    public IReadOnlyList<DirectiveRecord> Directives => Preprocessing.Directives;
    public IReadOnlyList<TextSpan> InactiveSpans => Preprocessing.InactiveSpans;
    public IReadOnlyList<LineMapping> LineMappings => Preprocessing.LineMappings;
    public IReadOnlyList<ExactLineMapping> ExactLineMappings => Preprocessing.ExactLineMappings;
    public ExactMappedLocation MapLocationExact(int offset) => Preprocessing.MapLocationExact(offset);
    public bool TryMapLocation(int offset,out MappedLocation? location) => Preprocessing.TryMapLocation(offset,out location);
    public MappedLocation MapLocation(int offset) => Preprocessing.MapLocation(offset);
    public string Profile { get; }
    public string DocumentId { get; }
    public bool Accepted { get; }
    public bool ProfileComplete { get; }
    public string CoverageIdentity => CSNitrogen.Profiles.FeatureCatalog.CoverageIdentity;
    public SyntaxTree Tree => _parsed.Tree;
    public CompilationUnitNode Root => SyntaxView.Cast<CompilationUnitNode>(Tree, Tree.Root);
    public IReadOnlyList<ClassDeclarationNode> Classes
    {
        get
        {
            var result=new List<ClassDeclarationNode>();
            for(int n=0;n<Tree.NodeCount;n++)
            {
                if(Tree.Language!.GetKindName(Tree.Kind(n))!="ClassDeclaration")continue;
                bool top=true;
                for(int parent=Tree.Parent(n);parent>=0;parent=Tree.Parent(parent))
                    if(Tree.Language.GetKindName(Tree.Kind(parent)) is "NamespaceDeclaration" or "ClassDeclaration" or "StructDeclaration" or "InterfaceDeclaration"){top=false;break;}
                if(top)result.Add(SyntaxView.Cast<ClassDeclarationNode>(Tree,n));
            }
            return Array.AsReadOnly(result.ToArray());
        }
    }
    public IReadOnlyList<ParseIssue> Issues { get; }
    public void Dispose() => _parsed.Dispose();
}
