using System.Security.Cryptography;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3ExpressionReviewTests
{
 private static JsonObject Read(string suffix)
 {using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+suffix)!;return JsonNode.Parse(s)!.AsObject();}
 [Fact]public void Expression_disposition_covers_exact_source_occurrences_and_existing_selected_factory_witnesses()
 {Assert.Empty(Evaluate(Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json")));Assert.False(ProfileResolver.Resolve("CSharp3").Profile!.Complete);}
 [Theory]
 [InlineData("missing-source-review")][InlineData("wrong-review-hash")][InlineData("wrong-source-occurrence")]
 [InlineData("invented-witness")][InlineData("missing-witness-group")][InlineData("duplicate-witness")]
 public void Review_and_witness_drift_is_detected(string change)
 {
  var map=Read("csharp3-standard-crosswalk.json");var row=map["productions"]!.AsArray().Single(r=>Text(r!,"standardId")=="B:8537:argument")!;
  switch(change)
  {
   case "missing-source-review":row.AsObject().Remove("sourceReview");break;
   case "wrong-review-hash":map["sourceReviews"]!.AsArray().Single(r=>Text(r!,"domain")=="expression-type")!["sha256"]=new string('0',64);break;
   case "wrong-source-occurrence":row["sourceReview"]!["occurrenceId"]="invented";break;
   case "invented-witness":row["witnesses"]!["positive-negative"]![0]!["method"]="Invented";break;
   case "missing-witness-group":row["witnesses"]!.AsObject().Remove("spans-diagnostics");break;
   case "duplicate-witness":row["witnesses"]!["positive-negative"]!.AsArray().Add(row["witnesses"]!["positive-negative"]![0]!.DeepClone());break;
  }
  Assert.NotEmpty(Evaluate(Read("csharp3-standard-index.json"),map));
 }
 private static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 internal static IReadOnlyList<string> Evaluate(JsonObject index,JsonObject map)
 {
  var errors=new List<string>();
  try
  {
   const string path="docs/coverage/csharp3-expression-source-review.md";
   var report=map["sourceReviews"]!.AsArray().Single(r=>Text(r!,"domain")=="expression-type")!;
   if(Text(report,"path")!=path||Text(report,"status")!="source-inspected")errors.Add("report metadata");
   using var stream=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen.csharp3-review.csharp3-expression-source-review.md")!;
   if(Convert.ToHexStringLower(SHA256.HashData(stream))!=Text(report,"sha256"))errors.Add("report hash");
   var expected=index["productions"]!.AsArray().Where(r=>r!["line"]!.GetValue<int>()>=8506&&r["line"]!.GetValue<int>()<=8622).Select(r=>Text(r!,"id")).Order().ToArray();
   var rows=map["productions"]!.AsArray().Where(r=>r!["sourceReview"] is {} review&&Text(review,"domain")=="expression-type").ToArray();
   if(expected.Length!=114||!expected.SequenceEqual(rows.Select(r=>Text(r!,"standardId")).Order()))errors.Add("source set");
   var methods=new Dictionary<string,HashSet<string>>();
   foreach(var row in rows)
   {
    string id=Text(row!,"standardId");var review=row!["sourceReview"]!;
    if(Text(review,"report")!=path||Text(review,"occurrenceId")!=id||Text(review,"sourceSha256")!=Text(index,"sourceSha256")||Text(review,"status")!="source-inspected")errors.Add("source review "+id);
    if(Text(row,"status")!="candidate"||Text(row,"reviewStatus")!="pending")errors.Add("premature admission "+id);
    var groups=row["witnesses"]!.AsObject();
    if(!groups.Select(g=>g.Key).Order().SequenceEqual(new[]{"ambiguity-context","positive-negative","spans-diagnostics"}))errors.Add("groups "+id);
    foreach(var group in groups)
    {
     var witnesses=group.Value!.AsArray();var seen=new HashSet<string>();if(witnesses.Count==0)errors.Add("empty witness "+id);
     foreach(var witness in witnesses)
     {
      string file=Text(witness!,"path"),method=Text(witness!,"method");
      if(!seen.Add(file+"::"+method)||!file.StartsWith("tests/",StringComparison.Ordinal)||file.Contains("..",StringComparison.Ordinal)||file.Contains('\\')||Path.IsPathRooted(file))errors.Add("witness path "+id);
      if(!methods.TryGetValue(file,out var names))
      {
       var tree=CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"CoverageInputs",file)));
       names=tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m=>m.AttributeLists.SelectMany(a=>a.Attributes).Any(a=>a.Name.ToString() is "Fact" or "Theory")).Select(m=>m.Identifier.ValueText).ToHashSet();methods.Add(file,names);
      }
      if(!names.Contains(method))errors.Add("missing test "+id);
     }
    }
   }
  }
  catch(Exception e){errors.Add(e.Message);}
  return errors;
 }
}
