using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CoverageIdentityTests
{
    [Fact] public void Sorted_named_inputs_make_identity_independent_of_enumeration()
    {
        KeyValuePair<string,byte[]>[] inputs=[new("grammar/A.ngr",[1,2]),new("Preprocessing/A.cs",[3])];
        Assert.Equal(CoverageHash.Compute(inputs),CoverageHash.Compute(inputs.Reverse()));
        Assert.NotEqual(CoverageHash.Compute(inputs),CoverageHash.Compute([new("grammar/A.ngr",[1,4]),inputs[1]]));
        Assert.NotEqual(CoverageHash.Compute(inputs),CoverageHash.Compute([new("grammar/A.ngr",[1]),new("Preprocessing/A.cs",[2,3])]));
    }
    [Fact] public void Embedded_identity_covers_all_production_grammars_and_preprocessing_sources()
    {
        var names=typeof(FeatureCatalog).Assembly.GetManifestResourceNames();
        Assert.Contains(names,n=>n.Contains("Lexical.ngr"));Assert.Contains(names,n=>n.Contains("ConditionalPreprocessor.cs"));
        Assert.Contains("CSNitrogen.composition.json",names);
        Assert.Contains("CSNitrogen.reference-policy.json",names);
        Assert.Equal(64,FeatureCatalog.CoverageIdentity.Length);
    }
}
