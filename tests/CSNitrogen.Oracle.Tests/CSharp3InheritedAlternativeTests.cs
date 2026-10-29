using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3InheritedAlternativeTests
{
 private static JsonObject Read(string name)
 {using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;return JsonNode.Parse(s)!.AsObject();}
 private static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 [Fact]public void Every_explicit_inheritance_marker_has_exact_earlier_source_links()
 {Assert.Empty(Evaluate(Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json")));Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);}
 [Theory]
 [InlineData("B:8835:class-modifier/1","B:8694:class-modifier")]
 [InlineData("B:8836:struct-modifier/1","B:8775:struct-modifier")]
 [InlineData("B:8837:interface-modifier/1","B:8792:interface-modifier")]
 [InlineData("B:8838:delegate-modifier/1","B:8813:delegate-modifier")]
 [InlineData("B:8839:field-modifier/1","B:8716:field-modifier")]
 [InlineData("B:8840:method-modifier/1","B:8723:method-modifier")]
 [InlineData("B:8841:property-modifier/1","B:8734:property-modifier")]
 [InlineData("B:8842:event-modifier/1","B:8743:event-modifier")]
 [InlineData("B:8843:indexer-modifier/1","B:8749:indexer-modifier")]
 [InlineData("B:8844:operator-modifier/1","B:8753:operator-modifier")]
 [InlineData("B:8845:constructor-modifier/1","B:8763:constructor-modifier")]
 [InlineData("B:8848:embedded-statement/1","B:8625:embedded-statement")]
 [InlineData("B:8850:type/1","B:8510:type")]
 [InlineData("B:8853:primary-no-array-creation-expression/1","B:8539:primary-no-array-creation-expression")]
 [InlineData("B:8854:unary-expression/1","B:8573:unary-expression")]
 [InlineData("B:8860:embedded-statement/1","B:8625:embedded-statement,B:8848:embedded-statement")]
 [InlineData("B:8865:struct-member-declaration/1","B:8779:struct-member-declaration")]
 [InlineData("B:8872:local-variable-initializer/1","B:8635:local-variable-initializer")]
 public void Inheritance_origins_are_explicit_source_occurrences_not_name_only_aliases(string marker,string origins)
 {
  var index=Read("csharp3-standard-index.json");var rows=index["productions"]!.AsArray();var map=Read("csharp3-standard-crosswalk.json");
  var row=map["productions"]!.AsArray().Single(r=>Text(r!,"standardId")==marker[..marker.LastIndexOf('/')])!;
  var link=row["inheritedSourceAlternatives"]!.AsArray().Single(l=>Text(l!,"alternativeId")==marker)!;
  var expected=origins.Split(',').SelectMany(id=>rows.Single(r=>Text(r!,"id")==id)!["alternatives"]!.AsArray()).Where(a=>Text(a!,"kind")!="inherited-placeholder").Select(a=>Text(a!,"id")).Order();
  Assert.Equal(expected,link["sourceAlternativeIds"]!.AsArray().Select(n=>n!.GetValue<string>()).Order());
 }
 [Theory]
 [InlineData("missing-marker","marker set")][InlineData("invented-marker","marker set")]
 [InlineData("missing-parent","inherited source set")][InlineData("duplicate-parent","inherited source set")]
 [InlineData("future-parent","inherited source set")][InlineData("other-family-parent","inherited source set")]
 [InlineData("placeholder-parent","inherited source set")][InlineData("changed-status","link status")]
 [InlineData("wrong-count","evidence metadata")][InlineData("review-regression","review status")]
 public void Inheritance_mutations_fail_on_their_source_invariant(string change,string expectedError)
 {
  var index=Read("csharp3-standard-index.json");var map=Read("csharp3-standard-crosswalk.json");var rows=map["productions"]!.AsArray();
  var row=rows.Single(r=>Text(r!,"standardId")=="B:8835:class-modifier")!;var links=row["inheritedSourceAlternatives"]!.AsArray();var link=links[0]!;var parents=link["sourceAlternativeIds"]!.AsArray();
  switch(change)
  {
   case "missing-marker":links.RemoveAt(0);break;
   case "invented-marker":link["alternativeId"]="invented";break;
   case "missing-parent":parents.RemoveAt(0);break;
   case "duplicate-parent":parents.Add(parents[0]!.DeepClone());break;
   case "future-parent":parents[0]="B:8835:class-modifier/2";break;
   case "other-family-parent":parents[0]="B:8775:struct-modifier/1";break;
   case "placeholder-parent":parents[0]="B:8835:class-modifier/1";break;
   case "changed-status":link["status"]="verified";break;
   case "wrong-count":map["sourceInheritanceEvidence"]!["referenceCount"]=0;break;
   case "review-regression":row["status"]="candidate";break;
  }
  Assert.Contains(expectedError,Evaluate(index,map));Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 internal static IReadOnlyList<string> Evaluate(JsonObject index,JsonObject map)
 {
  var errors=new List<string>();
  try
  {
   var rows=index["productions"]!.AsArray();var markers=rows.SelectMany(r=>r!["alternatives"]!.AsArray()).Where(a=>Text(a!,"kind")=="inherited-placeholder").Select(a=>Text(a!,"id")).Order().ToArray();
   var evidence=map["sourceInheritanceEvidence"]!;
   if(Text(evidence,"status")!="source-mapped"||Text(evidence,"sourceSha256")!=Text(index,"sourceSha256")||evidence["markerCount"]!.GetValue<int>()!=18||evidence["referenceCount"]!.GetValue<int>()!=148)errors.Add("evidence metadata");
   var actual=new List<string>();int references=0;
   foreach(var row in map["productions"]!.AsArray())
   {
    if(Text(row!,"status")!="verified"||Text(row!,"reviewStatus")!="verified")errors.Add("review status");
    string id=Text(row!,"standardId");var heading=rows.Single(r=>Text(r!,"id")==id)!;
    foreach(var link in row!["inheritedSourceAlternatives"]?.AsArray()??new JsonArray())
    {
     string marker=Text(link!,"alternativeId");actual.Add(marker);
     if(Text(link!,"status")!="source-mapped")errors.Add("link status");
     var expected=rows.Where(r=>r!["line"]!.GetValue<int>()<heading["line"]!.GetValue<int>()&&Text(r,"name")==Text(heading,"name")).SelectMany(r=>r!["alternatives"]!.AsArray()).Where(a=>Text(a!,"kind")!="inherited-placeholder").Select(a=>Text(a!,"id")).Order().ToArray();
     var source=link!["sourceAlternativeIds"]!.AsArray().Select(n=>n!.GetValue<string>()).Order().ToArray();references+=source.Length;
     if(!marker.StartsWith(id+"/",StringComparison.Ordinal)||!markers.Contains(marker)||expected.Length==0||!expected.SequenceEqual(source)||source.Distinct().Count()!=source.Length)errors.Add("inherited source set");
    }
   }
   if(markers.Length!=18||!markers.SequenceEqual(actual.Order()))errors.Add("marker set");
   if(references!=148)errors.Add("reference count");
  }
  catch(Exception e){errors.Add(e.Message);}
  return errors;
 }
}
