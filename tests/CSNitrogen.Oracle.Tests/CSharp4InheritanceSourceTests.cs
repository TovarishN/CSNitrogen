using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4InheritanceSourceTests
{
 static JsonObject Read()=>JsonNode.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"CSharp4/csharp4-standard-index.json")))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 public static IEnumerable<object[]> Markers()
 {foreach(var row in Read()["productions"]!.AsArray())foreach(var a in row!["alternatives"]!.AsArray())if(Text(a!,"kind")=="inherited-placeholder")yield return [Text(a!,"id")];}
 [Theory][MemberData(nameof(Markers))]
 public void Each_explicit_marker_names_all_earlier_alternatives_of_its_source_family(string id)
 {
  var index=Read();var row=index["productions"]!.AsArray().Single(r=>r!["alternatives"]!.AsArray().Any(a=>Text(a!,"id")==id))!;
  var marker=row["alternatives"]!.AsArray().Single(a=>Text(a!,"id")==id)!;
  Assert.Equal(Expected(index,row).Order(),marker["earlierSourceAlternativeIds"]!.AsArray().Select(n=>n!.GetValue<string>()).Order());Assert.True(ProfileResolver.Resolve("CSharp4").Profile!.Complete);
 }
 [Fact]public void Source_inheritance_has_seventeen_markers_and_one_hundred_thirty_five_exact_references()
 {Assert.Empty(Evaluate(Read()));}
 [Theory]
 [InlineData("missing-marker","InheritanceMetadata")][InlineData("missing-parent","ParentSet")]
 [InlineData("duplicate-parent","ParentSet")][InlineData("future-parent","ParentSet")]
 [InlineData("other-family-parent","ParentSet")][InlineData("placeholder-parent","ParentSet")]
 public void Inheritance_corruption_fails_on_the_source_invariant(string change,string expected)
 {
  var index=Read();var row=index["productions"]!.AsArray().First(r=>r!["alternatives"]!.AsArray().Any(a=>Text(a!,"kind")=="inherited-placeholder"))!;
  var marker=row["alternatives"]!.AsArray().First(a=>Text(a!,"kind")=="inherited-placeholder")!;var parents=marker["earlierSourceAlternativeIds"]!.AsArray();
  switch(change)
  {
   case "missing-marker":row["alternatives"]!.AsArray().Remove(marker);break;
   case "missing-parent":parents.RemoveAt(0);break;
   case "duplicate-parent":parents.Add(parents[0]!.DeepClone());break;
   case "future-parent":parents[0]="B:9194:stackalloc-initializer/1";break;
   case "other-family-parent":parents[0]="B:8712:input/1";break;
   case "placeholder-parent":parents[0]=Text(marker,"id");break;
  }
  Assert.Contains(Evaluate(index),e=>e.StartsWith(expected,StringComparison.Ordinal));Assert.True(ProfileResolver.Resolve("CSharp4").Profile!.Complete);
 }
 static IEnumerable<string> Expected(JsonObject index,JsonNode row)=>index["productions"]!.AsArray().Where(r=>r!["line"]!.GetValue<int>()<row["line"]!.GetValue<int>()&&Text(r,"name")==Text(row,"name")).SelectMany(r=>r!["alternatives"]!.AsArray()).Where(a=>Text(a!,"kind")!="inherited-placeholder").Select(a=>Text(a!,"id"));
 static IReadOnlyList<string> Evaluate(JsonObject index)
 {
  var errors=new List<string>();int markers=0,references=0;
  foreach(var row in index["productions"]!.AsArray())foreach(var marker in row!["alternatives"]!.AsArray())
  {
   if(Text(marker!,"kind")!="inherited-placeholder")continue;markers++;
   var observed=marker!["earlierSourceAlternativeIds"]!.AsArray().Select(n=>n!.GetValue<string>()).ToArray();references+=observed.Length;
   var expected=Expected(index,row).Order().ToArray();
   if(expected.Length==0||observed.Distinct().Count()!=observed.Length||!expected.SequenceEqual(observed.Order()))errors.Add("ParentSet: "+Text(marker,"id"));
  }
  if(markers!=17||references!=135)errors.Add("InheritanceMetadata: expected17markers135references");return errors;
 }
}
