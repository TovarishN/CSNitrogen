using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3AlternativeMatrixTests
{
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static JsonObject Read(string name){using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;return JsonNode.Parse(s)!.AsObject();}
 [Fact]public void Entire_source_alternative_matrix_has_one_correct_evidence_class_per_identity()
 {Assert.Empty(Evaluate(Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json")));Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);}
 static List<string> Evaluate(JsonObject index,JsonObject map)
 {
  var errors=new List<string>();var identities=new List<string>();
  try
  {
   var indexed=index["productions"]!.AsArray().ToDictionary(r=>Text(r!,"id"),r=>r!);
   foreach(var row0 in map["productions"]!.AsArray())
   {
    var row=row0!;string occurrence=Text(row,"standardId");var source=indexed[occurrence];
    var alternatives=source["alternatives"]!.AsArray().ToDictionary(a=>Text(a!,"id"),a=>a!);
    foreach(string group in new[]{"finiteAlternativeWitnesses","lexicalProductionAlternativeWitnesses","productionAlternativeWitnesses","inheritedSourceAlternatives"})
    foreach(var link0 in row[group]?.AsArray()??new JsonArray())
    {
     var link=link0!;string id=Text(link,"alternativeId");identities.Add(id);
     string kind=group=="finiteAlternativeWitnesses"?"one-of-token":group=="inheritedSourceAlternatives"?"inherited-placeholder":"production";
     if(!alternatives.TryGetValue(id,out var alt)||Text(alt,"kind")!=kind)errors.Add("source evidence class");
     if(group=="lexicalProductionAlternativeWitnesses"&&Text(source,"grammar")!="lexical")errors.Add("lexical domain");
     if(group=="productionAlternativeWitnesses")
     {
      int line=source["line"]!.GetValue<int>();string expected=Text(source,"grammar")=="unsafe"?"unsafe":line<=8622?"expression":line<=8675?"statement":"declaration";
      if(Text(source,"grammar")=="lexical"||Text(link,"domain")!=expected)errors.Add("production domain");
     }
     if(group!="inheritedSourceAlternatives"&&Text(link,"caseId")!=id)errors.Add("case identity");
    }
   }
   var expectedAll=indexed.Values.SelectMany(r=>r["alternatives"]!.AsArray()).Select(a=>Text(a!,"id")).Order().ToArray();
   if(expectedAll.Length!=1175||!expectedAll.SequenceEqual(identities.Order()))errors.Add("complete alternative matrix");
  }
  catch(Exception e){errors.Add(e.Message);}
  return errors;
 }
 [Theory]
 [InlineData("missing","complete alternative matrix")][InlineData("duplicate","complete alternative matrix")][InlineData("marker","complete alternative matrix")]
 [InlineData("domain","production domain")][InlineData("kind","source evidence class")][InlineData("case","case identity")]
 public void Whole_matrix_mutations_reject_missing_duplicate_and_misclassified_evidence(string mutation,string error)
 {
  var index=Read("csharp3-standard-index.json");var map=Read("csharp3-standard-crosswalk.json");var rows=map["productions"]!.AsArray();
  var first=rows.First(r=>r!["productionAlternativeWitnesses"] is JsonArray a&&a.Count>0)!["productionAlternativeWitnesses"]!.AsArray();
  switch(mutation)
  {
   case "missing":first.RemoveAt(0);break;case "duplicate":first.Add(first[0]!.DeepClone());break;
   case "marker":rows.First(r=>r!["inheritedSourceAlternatives"] is JsonArray a&&a.Count>0)!["inheritedSourceAlternatives"]!.AsArray().RemoveAt(0);break;
   case "domain":first[0]!["domain"]="invented";break;
   case "kind":var row=rows.First(r=>r!["finiteAlternativeWitnesses"] is JsonArray a&&a.Count>0)!;row["productionAlternativeWitnesses"]=new JsonArray(new JsonObject{["alternativeId"]=Text(row["finiteAlternativeWitnesses"]![0]!,"alternativeId"),["caseId"]="invented",["domain"]="expression"});break;
   case "case":first[0]!["caseId"]="invented";break;
  }
  Assert.Contains(error,Evaluate(index,map));
 }
}
