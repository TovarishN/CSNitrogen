using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace CSNitrogen.Profiles;
/// <summary>Admission of one reviewed normalized C#4 syntax snapshot, independently of source acceptance.</summary>
internal static class CSharp4CoverageContract
{
 internal const string PrimarySha256="cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7";
 internal const string ApprovedLedgerSha256="9f60dd9655c33d46e23f1440eee231abcbafe1cda18be79c37d9754603d2ef9c";
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 static string Text(JsonElement e,string key)=>e.GetProperty(key).GetString()!;
 static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
 static bool Exact(IEnumerable<string> a,IEnumerable<string> b)
 {var x=a.Order(StringComparer.Ordinal).ToArray();var y=b.Order(StringComparer.Ordinal).ToArray();return x.Distinct().Count()==x.Length&&y.Distinct().Count()==y.Length&&x.SequenceEqual(y);}
 // Predecessor/self contract literals are excluded so the pin graph stays directed:4 ->3 ->2 refresh order.
 internal static bool Eligible(string name)=>name is "CSNitrogen.coverage.json" or "CSNitrogen.dependencies.json"
  ||name.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp4-artifact.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp4-review.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp4-evidence.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp3-evidence.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.source.",StringComparison.Ordinal)&&name is not ("CSNitrogen.source.Profiles/CSharp2CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp3CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp4CoverageContract.cs");
 internal static bool Reviewed(byte[] ledger,byte[] composition,IReadOnlyDictionary<string,byte[]> inputs,bool thirdReviewed)
 {
  try
  {
   if(!thirdReviewed||Hash(ledger)!=ApprovedLedgerSha256)return false;
   using var gateDoc=JsonDocument.Parse(ledger);using var configDoc=JsonDocument.Parse(composition);var gate=gateDoc.RootElement;
   if(Text(gate,"profile")!="CSharp4"||Text(gate,"status")!="verified"||Text(gate,"primarySha256")!=PrimarySha256)return false;
   var pins=gate.GetProperty("reviewInputs").EnumerateObject().ToArray();
   if(!Exact(inputs.Keys.Where(Eligible),pins.Select(p=>p.Name)))return false;
   foreach(var p in pins)if(!inputs.TryGetValue(p.Name,out var bytes)||Hash(bytes)!=p.Value.GetString())return false;
   JsonDocument Artifact(string file)=>JsonDocument.Parse(inputs["CSNitrogen.csharp4-artifact.csharp4-"+file+".json"]);
   using var indexDoc=Artifact("standard-index");using var mapDoc=Artifact("standard-crosswalk");using var reviewDoc=Artifact("reviewed-occurrences");using var referenceDoc=Artifact("reference-policy");using var deltaDoc=Artifact("source-delta-inventory");using var differencesDoc=Artifact("main-text-differences");using var dispositionsDoc=Artifact("main-text-dispositions");
   var index=indexDoc.RootElement;var map=mapDoc.RootElement;var review=reviewDoc.RootElement;var reference=referenceDoc.RootElement;
   if(Text(index,"sourceSha256")!=PrimarySha256||Text(map,"sourceSha256")!=PrimarySha256||Text(review,"sourceSha256")!=PrimarySha256||Text(reference.GetProperty("primary"),"sha256")!=PrimarySha256)return false;
   if(Text(reference,"status")!="verified"||Text(review,"status")!="verified"||Text(map,"status")!="owner-indexed-unreviewed"||Text(index,"status")!="indexed-unreviewed")return false;
   var unicode=reference.GetProperty("unicode");
   if(Text(unicode,"policy")!="unicode-4.0-scalar"||Text(unicode,"status")!="approved"||Text(unicode,"dataSha256")!="77818a7e04c563a6a628e4f10be60323bf13f08e9fb208bffce11057b008ad7e")return false;
   if(!Strings(reference.GetProperty("normalizedExclusions")).Contains("explicit-pointer-type-argument-legality"))return false;
   if(Text(review,"ownerCrosswalkSha256")!=Hash(inputs["CSNitrogen.csharp4-artifact.csharp4-standard-crosswalk.json"])||Text(review,"contextReportSha256")!=Hash(inputs["CSNitrogen.csharp4-review.csharp4-normative-context-review.md"])||Text(review,"mainTextDispositionsSha256")!=Hash(inputs["CSNitrogen.csharp4-artifact.csharp4-main-text-dispositions.json"]))return false;
   var configured=configDoc.RootElement.GetProperty("languages").EnumerateArray().Single(l=>Text(l,"name")=="CSharp4");var factory=gate.GetProperty("factory");
   if(Text(configured,"start")!="CSharp.CompilationUnits.CompilationUnit"||Text(configured,"namespace")!="CSNitrogen.Syntax"||Text(factory,"name")!="CSharp4"||Text(factory,"start")!="CompilationUnit"||Text(factory,"rootModule")!="CSharp.CompilationUnits"||!Exact(Strings(configured.GetProperty("grammars")),Strings(factory.GetProperty("grammars")))||Strings(factory.GetProperty("modules")).Length!=30)return false;
   var heads=index.GetProperty("productions").EnumerateArray().ToDictionary(h=>Text(h,"id"));var rows=map.GetProperty("productions").EnumerateArray().ToArray();var inspected=review.GetProperty("productions").EnumerateArray().ToDictionary(r=>Text(r,"standardId"));
   if(heads.Count!=458||!Exact(heads.Keys,rows.Select(r=>Text(r,"standardId")))||!Exact(heads.Keys,inspected.Keys))return false;
   var owners=Strings(gate.GetProperty("owners"));var groups=gate.GetProperty("policyGroups").EnumerateObject().ToDictionary(p=>p.Name,p=>Strings(p.Value));
   if(!Exact(owners,rows.SelectMany(r=>Strings(r.GetProperty("targets"))).Distinct()))return false;
   if(!Exact(groups.Keys,new[]{"attribute-argument-order","attribute-target-recognition","csharp-trivia","csharp1-preprocessing","generated-syntax-context-checks","csharp4.source-context-checks"})||!Exact(groups["csharp4.source-context-checks"],["Parsing/CSharp4ContextPolicy.cs"]))return false;
   foreach(var row in rows)
   {
    string id=Text(row,"standardId");var h=heads[id];var r=inspected[id];
    if(Text(row,"reviewStatus")!="owner-candidate"||Text(r,"status")!="verified"||Text(row,"sourceBodySha256")!=Text(h,"sourceBodySha256")||Text(r,"sourceBodySha256")!=Text(h,"sourceBodySha256"))return false;
    var dimensions=r.GetProperty("reviews").EnumerateObject().ToArray();
    if(!Exact(dimensions.Select(d=>d.Name),["normative-prose","positive-negative","ambiguity-context","spans-diagnostics"])||dimensions.Any(d=>d.Value.GetString()!="verified"))return false;
    var sr=r.GetProperty("sourceReview");int line=h.GetProperty("line").GetInt32();string domain=Text(h,"grammar")=="lexical"?"lexical":Text(h,"grammar")=="unsafe"?"unsafe":line<8942?"expression":line<8995?"statement":"declaration";
    if(Text(sr,"domain")!=domain||Text(sr,"occurrenceId")!=id||Text(sr,"sourceSha256")!=PrimarySha256||Text(sr,"status")!="source-inspected"||Text(sr,"report")!="docs/coverage/csharp4-normative-context-review.md")return false;
    var ts=Strings(row.GetProperty("targets"));var ps=Strings(row.GetProperty("policies"));
    if(ts.Length+ps.Length==0||!Exact(ts,ts)||!Exact(ps,ps)||ps.Any(p=>!groups.ContainsKey(p)))return false;
   }
   var correction=map.GetProperty("sourceCorrections").GetProperty("approved-generic-member-suffix");var corrected=rows.Where(r=>r.TryGetProperty("sourceCorrection",out _)).Select(r=>Text(r,"standardId")).ToArray();
   if(Text(correction,"status")!="approved-predecessor-continuity"||!Exact(corrected,Strings(correction.GetProperty("occurrences")))||corrected.Length!=3)return false;
   var all=heads.Values.SelectMany(h=>h.GetProperty("alternatives").EnumerateArray()).ToArray();if(all.Length!=1181)return false;
   var real=all.Where(a=>Text(a,"kind")!="inherited-placeholder").ToDictionary(a=>Text(a,"id"));int markers=0,refs=0;
   foreach(var h in heads.Values)foreach(var a in h.GetProperty("alternatives").EnumerateArray().Where(a=>Text(a,"kind")=="inherited-placeholder"))
   {
    var earlier=heads.Values.Where(p=>p.GetProperty("line").GetInt32()<h.GetProperty("line").GetInt32()&&Text(p,"name")==Text(h,"name")).SelectMany(p=>p.GetProperty("alternatives").EnumerateArray()).Where(p=>Text(p,"kind")!="inherited-placeholder").Select(p=>Text(p,"id")).ToArray();
    if(earlier.Length==0||!Exact(earlier,Strings(a.GetProperty("earlierSourceAlternativeIds"))))return false;markers++;refs+=earlier.Length;
   }
   if(real.Count!=1164||markers!=17||refs!=135)return false;
   using var matrixDoc=JsonDocument.Parse(inputs["CSNitrogen.csharp4-evidence.alternative-witness-matrix.json"]);using var specificDoc=JsonDocument.Parse(inputs["CSNitrogen.csharp4-evidence.source-specific-cases.json"]);
   var matrix=matrixDoc.RootElement;var specific=specificDoc.RootElement;
   if(Text(matrix,"profile")!="CSharp4"||Text(matrix,"sourceSha256")!=PrimarySha256||Text(matrix,"status")!="source-witnesses-unreviewed"||Text(specific,"sourceSha256")!=PrimarySha256||Text(specific,"profile")!="CSharp4")return false;
   if(Text(matrix,"indexSha256")!=Hash(inputs["CSNitrogen.csharp4-artifact.csharp4-standard-index.json"])||Text(matrix,"deltaSha256")!=Hash(inputs["CSNitrogen.csharp4-artifact.csharp4-source-delta-inventory.json"]))return false;
   if(Text(matrix.GetProperty("sourceSpecific"),"sha256")!=Hash(inputs["CSNitrogen.csharp4-evidence.source-specific-cases.json"]))return false;
   var cases=matrix.GetProperty("cases").EnumerateArray().ToArray();if(!Exact(real.Keys,cases.Select(c=>Text(c,"id"))))return false;
   var local=specific.GetProperty("cases").EnumerateArray().ToDictionary(c=>Text(c,"id"));if(local.Count!=22)return false;
   var old=new Dictionary<string,(string Domain,string Hash)>();
   foreach(string domain in new[]{"finite","lexical","expression","statement","unsafe","declaration"})
   {
    string file=domain=="finite"?"finite-alternative-cases.json":domain+"-production-cases.json";var bytes=inputs["CSNitrogen.csharp3-evidence."+file];var meta=matrix.GetProperty("predecessorCorpora").GetProperty(domain);
    if(Text(meta,"path")!="tests/Corpus/CSharp3/"+file||Text(meta,"sha256")!=Hash(bytes))return false;
    using var fixture=JsonDocument.Parse(bytes);if(Text(fixture.RootElement,"sourceSha256")!=CSharp3CoverageContract.PrimarySha256)return false;
    foreach(var entry in fixture.RootElement.GetProperty("cases").EnumerateArray())old.Add(Text(entry,"id"),(domain,Text(entry,"sourceAlternativeSha256")));
   }
   var candidates=deltaDoc.RootElement.GetProperty("productions").EnumerateArray().SelectMany(p=>p.GetProperty("alternativeCandidates").EnumerateArray()).ToDictionary(c=>Text(c,"alternativeId"),c=>Strings(c.GetProperty("predecessorAlternativeCandidates")));
   int inherited=0,novel=0;
   foreach(var c in cases)
   {
    string id=Text(c,"id"),hash=Text(real[id],"sourceAlternativeSha256"),occurrence=id[..id.LastIndexOf('/')];
    if(Text(c,"sourceOccurrence")!=occurrence||Text(c,"sourceAlternativeSha256")!=hash)return false;
    if(Text(c,"sourceKind")=="predecessor-corpus")
    {string witness=Text(c,"witnessId");if(!Exact(candidates[id],[witness])||!old.TryGetValue(witness,out var p)||p.Domain!=Text(c,"domain")||p.Hash!=hash)return false;inherited++;}
    else if(Text(c,"sourceKind")=="source-specific")
    {if(candidates[id].Length!=0||Text(c,"witnessId")!=id||Text(c,"domain")!="source-specific"||!local.TryGetValue(id,out var entry)||Text(entry,"sourceAlternativeSha256")!=hash)return false;novel++;}
    else return false;
   }
   if(inherited!=1142||novel!=22)return false;
   var diff=differencesDoc.RootElement;var disp=dispositionsDoc.RootElement;
   if(Text(diff,"sourceSha256")!=PrimarySha256||Text(diff,"thirdSourceSha256")!=CSharp3CoverageContract.PrimarySha256||Text(diff,"status")!="difference-index-unreviewed"||Text(disp,"status")!="source-inspected"||Text(disp,"sourceSha256")!=PrimarySha256||Text(disp,"differenceIndexSha256")!=Hash(inputs["CSNitrogen.csharp4-artifact.csharp4-main-text-differences.json"])||Text(disp,"reportSha256")!=Hash(inputs["CSNitrogen.csharp4-review.csharp4-normative-context-review.md"]))return false;
   var regions=diff.GetProperty("regions").EnumerateArray().ToDictionary(r=>Text(r,"id"));var dispositions=disp.GetProperty("regions").EnumerateArray().ToArray();
   if(regions.Count!=268||!Exact(regions.Keys,dispositions.Select(d=>Text(d,"id"))))return false;
   foreach(var d in dispositions){var r=regions[Text(d,"id")];if(Text(d,"thirdNormalizedSha256")!=Text(r.GetProperty("third"),"normalizedSha256")||Text(d,"fourthNormalizedSha256")!=Text(r.GetProperty("fourth"),"normalizedSha256")||!disp.GetProperty("dispositions").TryGetProperty(Text(d,"disposition"),out _))return false;}
   return true;
  }
  catch(Exception e) when(e is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException or FormatException or OverflowException){return false;}
 }
}
