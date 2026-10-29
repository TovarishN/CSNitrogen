using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace CSNitrogen.Profiles;
/// <summary>Admission of the reviewed, normalized Version3 syntax snapshot; no semantic compilation.</summary>
internal static class CSharp3CoverageContract
{
 internal const string PrimarySha256="ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1";
 internal const string ApprovedLedgerSha256="1fccca1503110eb03259f4e7da81347e4a9191609afa16fd5843fa20d6b328e3";
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 static string Text(JsonElement e,string key)=>e.GetProperty(key).GetString()!;
 static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
 static bool Exact(IEnumerable<string> a,IEnumerable<string> b)
 {var x=a.Order(StringComparer.Ordinal).ToArray();var y=b.Order(StringComparer.Ordinal).ToArray();return x.Distinct().Count()==x.Length&&y.Distinct().Count()==y.Length&&x.SequenceEqual(y);}
 // The predecessor pins this contract; its self-updating literal is excluded here to keep the hash graph directed.
 internal static bool Eligible(string name)=>name is "CSNitrogen.coverage.json" or "CSNitrogen.dependencies.json"
  ||name.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp3-review.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp3-evidence.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.source.",StringComparison.Ordinal)&&name is not ("CSNitrogen.source.Profiles/CSharp2CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp3CoverageContract.cs");
 internal static bool Reviewed(byte[] ledger,byte[] index,byte[] crosswalk,byte[] reference,byte[] composition,IReadOnlyDictionary<string,byte[]> inputs,bool secondReviewed)
 {
  try
  {
   if(!secondReviewed||Hash(ledger)!=ApprovedLedgerSha256)return false;
   using var g=JsonDocument.Parse(ledger);using var i=JsonDocument.Parse(index);using var m=JsonDocument.Parse(crosswalk);using var r=JsonDocument.Parse(reference);using var c=JsonDocument.Parse(composition);
   var gate=g.RootElement;var source=i.RootElement;var map=m.RootElement;var authority=r.RootElement;
   if(Text(gate,"profile")!="CSharp3"||Text(gate,"status")!="verified"||Text(map,"profile")!="CSharp3"||Text(map,"status")!="verified"||Text(authority,"status")!="verified")return false;
   if(Text(source,"sourceSha256")!=PrimarySha256||Text(map,"sourceSha256")!=PrimarySha256||Text(authority.GetProperty("primary"),"sha256")!=PrimarySha256)return false;
   if(Hash(index)!=Text(gate,"indexSha256")||Hash(crosswalk)!=Text(gate,"crosswalkSha256")||Hash(reference)!=Text(gate,"referenceSha256"))return false;
   if(Text(authority.GetProperty("unicode"),"policy")!="unicode-4.0-scalar"||Text(authority.GetProperty("unicode"),"status")!="approved"||Text(authority.GetProperty("unicode"),"dataSha256")!="77818a7e04c563a6a628e4f10be60323bf13f08e9fb208bffce11057b008ad7e")return false;
   string[] corrections=["B:8542:member-access","B:8548:base-access","B:8856:pointer-member-access"];
   if(Text(authority.GetProperty("sourceCorrections"),"status")!="approved"||!Exact(corrections,Strings(authority.GetProperty("sourceCorrections").GetProperty("occurrences"))))return false;
   var reviewed=gate.GetProperty("reviewInputs").EnumerateObject().ToArray();
   if(!Exact(inputs.Keys.Where(Eligible),reviewed.Select(x=>x.Name)))return false;
   foreach(var input in reviewed)if(!inputs.TryGetValue(input.Name,out var bytes)||Hash(bytes)!=input.Value.GetString())return false;
   var configuration=c.RootElement.GetProperty("languages").EnumerateArray().Single(x=>Text(x,"name")=="CSharp3");var factory=gate.GetProperty("factory");
   if(Text(configuration,"start")!="CSharp.CompilationUnits.CompilationUnit"||Text(configuration,"namespace")!="CSNitrogen.Syntax"||Text(factory,"name")!="CSharp3"||Text(factory,"start")!="CompilationUnit"||Text(factory,"rootModule")!="CSharp.CompilationUnits")return false;
   if(!Exact(Strings(configuration.GetProperty("grammars")),Strings(factory.GetProperty("grammars")))||Strings(factory.GetProperty("modules")).Length!=26)return false;
   var heads=source.GetProperty("productions").EnumerateArray().ToArray();var rows=map.GetProperty("productions").EnumerateArray().ToArray();
   if(heads.Length!=455||!Exact(heads.Select(x=>Text(x,"id")),rows.Select(x=>Text(x,"standardId"))))return false;
   var all=heads.SelectMany(h=>h.GetProperty("alternatives").EnumerateArray()).ToArray();
   string identity=string.Concat(all.Select(a=>Text(a,"id")+"|"+Text(a,"kind")+"|"+Text(a,"sourceAlternativeSha256")+"\n"));
   if(all.Length!=1175||Hash(Encoding.UTF8.GetBytes(identity))!="7d4761a0a218673210b624e57baf88f738b1319f53bd67cf2eeddc421ae3c6f2")return false;
   var owners=Strings(gate.GetProperty("owners")).ToHashSet(StringComparer.Ordinal);var policies=gate.GetProperty("policyGroups").EnumerateObject().Select(p=>p.Name).ToHashSet(StringComparer.Ordinal);
   var links=new Dictionary<string,List<string>>(StringComparer.Ordinal);foreach(string domain in new[]{"finite","lexical","expression","statement","unsafe","declaration"})links[domain]=[];
   int inheritedCount=0,inheritedReferences=0;
   foreach(var row in rows)
   {
    string id=Text(row,"standardId");var head=heads.Single(h=>Text(h,"id")==id);var alternatives=head.GetProperty("alternatives").EnumerateArray().ToDictionary(a=>Text(a,"id"));
    if(Text(row,"status")!="verified"||Text(row,"reviewStatus")!="verified"||Text(row,"sourceBodySha256")!=Text(head,"sourceBodySha256"))return false;
    var dimensions=row.GetProperty("reviews").EnumerateObject().ToArray();
    if(!Exact(["normative-prose","positive-negative","ambiguity-context","spans-diagnostics"],dimensions.Select(x=>x.Name))||dimensions.Any(x=>x.Value.GetString()!="verified"))return false;
    var review=row.GetProperty("sourceReview");if(Text(review,"occurrenceId")!=id||Text(review,"sourceSha256")!=PrimarySha256||Text(review,"status")!="source-inspected")return false;
    var targets=Strings(row.GetProperty("targets"));var ps=Strings(row.GetProperty("policies"));
    if(targets.Length+ps.Length==0||!Exact(targets,targets)||!Exact(ps,ps)||targets.Any(t=>!owners.Contains(t))||ps.Any(p=>!policies.Contains(p)))return false;
    var represented=new List<string>();
    foreach(string group in new[]{"finiteAlternativeWitnesses","lexicalProductionAlternativeWitnesses","productionAlternativeWitnesses"})
    if(row.TryGetProperty(group,out var groupLinks))foreach(var link in groupLinks.EnumerateArray())
    {
     string aid=Text(link,"alternativeId"),kind=group=="finiteAlternativeWitnesses"?"one-of-token":"production";
     if(!alternatives.TryGetValue(aid,out var a)||Text(a,"kind")!=kind||Text(link,"caseId")!=aid)return false;
     string domain=group=="finiteAlternativeWitnesses"?"finite":group=="lexicalProductionAlternativeWitnesses"?"lexical":Text(link,"domain");
     if(!links.ContainsKey(domain))return false;
     if(group!="finiteAlternativeWitnesses")
     {
      int line=head.GetProperty("line").GetInt32();string expected=Text(head,"grammar")=="lexical"?"lexical":Text(head,"grammar")=="unsafe"?"unsafe":line<=8622?"expression":line<=8675?"statement":"declaration";
      if(domain!=expected)return false;
     }
     links[domain].Add(aid);represented.Add(aid);
    }
    if(row.TryGetProperty("inheritedSourceAlternatives",out var inherited))foreach(var link in inherited.EnumerateArray())
    {
     string aid=Text(link,"alternativeId");if(!alternatives.TryGetValue(aid,out var a)||Text(a,"kind")!="inherited-placeholder"||Text(link,"status")!="source-mapped")return false;
     var earlier=heads.Where(h=>h.GetProperty("line").GetInt32()<head.GetProperty("line").GetInt32()&&Text(h,"name")==Text(head,"name")).SelectMany(h=>h.GetProperty("alternatives").EnumerateArray()).Where(a=>Text(a,"kind")!="inherited-placeholder").Select(a=>Text(a,"id")).ToArray();
     if(earlier.Length==0||!Exact(earlier,Strings(link.GetProperty("sourceAlternativeIds"))))return false;
     inheritedCount++;inheritedReferences+=earlier.Length;represented.Add(aid);
    }
    if(!Exact(alternatives.Keys,represented))return false;
   }
   if(inheritedCount!=18||inheritedReferences!=148)return false;
   foreach(var (domain,count) in new[]{("finite",209),("lexical",184),("expression",241),("statement",100),("unsafe",58),("declaration",365)})
   {
    var evidence=domain=="finite"?map.GetProperty("finiteAlternativeEvidence"):domain=="lexical"?map.GetProperty("lexicalProductionAlternativeEvidence"):map.GetProperty("productionAlternativeEvidence").GetProperty(domain);
    string file=domain=="finite"?"finite-alternative-cases.json":domain=="lexical"?"lexical-production-cases.json":domain+"-production-cases.json";
    if(!inputs.TryGetValue("CSNitrogen.csharp3-evidence."+file,out var bytes)||Text(evidence,"path")!="tests/Corpus/CSharp3/"+file||Text(evidence,"sha256")!=Hash(bytes)||evidence.GetProperty("count").GetInt32()!=count||links[domain].Count!=count)return false;
    using var fixture=JsonDocument.Parse(bytes);var cases=fixture.RootElement.GetProperty("cases").EnumerateArray().ToArray();
    if(Text(fixture.RootElement,"sourceSha256")!=PrimarySha256||Text(fixture.RootElement,"profile")!="CSharp3"||!Exact(links[domain],cases.Select(x=>Text(x,"id"))))return false;
    foreach(var entry in cases)if(!all.Any(a=>Text(a,"id")==Text(entry,"id")&&Text(a,"sourceAlternativeSha256")==Text(entry,"sourceAlternativeSha256")))return false;
   }
   return true;
  }
  catch(Exception e) when(e is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException or FormatException or OverflowException){return false;}
 }
}
