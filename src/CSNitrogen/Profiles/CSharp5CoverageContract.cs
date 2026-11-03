using System.Security.Cryptography;
using System.Text.Json;
namespace CSNitrogen.Profiles;
/// <summary>Admission of one reviewed normalized C#5 syntax snapshot, independently of source acceptance.</summary>
internal static class CSharp5CoverageContract
{
 internal const string PrimarySha256="6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519";
 internal const string ApprovedLedgerSha256="1f44f85689d40b295877cd95aa9fe27c499ecfab2aa63a731ae3fd88c8a09883";
 static readonly string[] Additions=["unary-expression","await-expression","lambda-expression","anonymous-method-expression","statement-expression","method-modifier"];
 static readonly string[] Dimensions=["normative-prose","positive-negative","ambiguity-context","spans-diagnostics"];
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 static string Text(JsonElement e,string key)=>e.GetProperty(key).GetString()!;
 static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
 static bool Exact(IEnumerable<string> a,IEnumerable<string> b)
 {var x=a.Order(StringComparer.Ordinal).ToArray();var y=b.Order(StringComparer.Ordinal).ToArray();return x.Distinct().Count()==x.Length&&y.Distinct().Count()==y.Length&&x.SequenceEqual(y);}
 // Predecessor/self contract literals are excluded so the pin graph stays directed:5 ->4 ->3 ->2 refresh order.
 internal static bool Eligible(string name)=>name is "CSNitrogen.coverage.json" or "CSNitrogen.dependencies.json"
  ||name.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp5-artifact.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp5-review.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp5-evidence.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp4-artifact.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp4-evidence.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp3-evidence.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.source.",StringComparison.Ordinal)&&name is not ("CSNitrogen.source.Profiles/CSharp2CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp3CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp4CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp5CoverageContract.cs");
 internal static bool Reviewed(byte[] ledger,byte[] composition,IReadOnlyDictionary<string,byte[]> inputs,bool fourthReviewed)
 {
  try
  {
   if(!fourthReviewed||Hash(ledger)!=ApprovedLedgerSha256)return false;
   using var gateDoc=JsonDocument.Parse(ledger);using var configDoc=JsonDocument.Parse(composition);var gate=gateDoc.RootElement;
   if(Text(gate,"profile")!="CSharp5"||Text(gate,"status")!="verified"||Text(gate,"primarySha256")!=PrimarySha256)return false;
   var pins=gate.GetProperty("reviewInputs").EnumerateObject().ToArray();
   if(!Exact(inputs.Keys.Where(Eligible),pins.Select(p=>p.Name)))return false;
   foreach(var p in pins)if(!inputs.TryGetValue(p.Name,out var bytes)||Hash(bytes)!=p.Value.GetString())return false;
   byte[] Input(string name)=>inputs["CSNitrogen."+name];
   JsonDocument Artifact(string file)=>JsonDocument.Parse(Input("csharp5-artifact.csharp5-"+file+".json"));
   using var indexDoc=Artifact("standard-index");using var mapDoc=Artifact("standard-crosswalk");using var reviewDoc=Artifact("reviewed-occurrences");using var referenceDoc=Artifact("reference-policy");using var deltaDoc=Artifact("source-delta-inventory");using var differencesDoc=Artifact("main-text-differences");using var dispositionsDoc=Artifact("main-text-dispositions");
   using var oldMapDoc=JsonDocument.Parse(Input("csharp4-artifact.csharp4-standard-crosswalk.json"));
   var index=indexDoc.RootElement;var map=mapDoc.RootElement;var review=reviewDoc.RootElement;var reference=referenceDoc.RootElement;
   if(Text(index,"sourceSha256")!=PrimarySha256||Text(map,"sourceSha256")!=PrimarySha256||Text(review,"sourceSha256")!=PrimarySha256||Text(reference.GetProperty("primary"),"sha256")!=PrimarySha256)return false;
   if(Text(reference,"status")!="verified"||Text(review,"status")!="verified"||Text(map,"status")!="owner-indexed-unreviewed"||Text(index,"status")!="indexed-unreviewed")return false;
   var unicode=reference.GetProperty("unicode");
   if(Text(unicode,"policy")!="unicode-4.0-scalar"||Text(unicode,"status")!="approved"||Text(unicode,"dataSha256")!="77818a7e04c563a6a628e4f10be60323bf13f08e9fb208bffce11057b008ad7e")return false;
   var exclusions=Strings(reference.GetProperty("normalizedExclusions"));
   if(!exclusions.Contains("general-modifier-compatibility-and-production-finite-sets")||!exclusions.Contains("async-awaitability-task-types-and-return-types"))return false;
   string report=Hash(Input("csharp5-review.csharp5-normative-context-review.md"));
   if(Text(review,"ownerCrosswalkSha256")!=Hash(Input("csharp5-artifact.csharp5-standard-crosswalk.json"))||Text(review,"contextReportSha256")!=report||Text(review,"mainTextDispositionsSha256")!=Hash(Input("csharp5-artifact.csharp5-main-text-dispositions.json"))
    ||Text(review,"alternativeMatrixSha256")!=Hash(Input("csharp5-evidence.alternative-witness-matrix.json"))||Text(review,"predecessorOverlaySha256")!=Hash(Input("csharp4-artifact.csharp4-reviewed-occurrences.json")))return false;
   if(Text(map,"predecessorCrosswalkSha256")!=Hash(Input("csharp4-artifact.csharp4-standard-crosswalk.json"))||Text(map,"indexSha256")!=Hash(Input("csharp5-artifact.csharp5-standard-index.json"))||Text(map,"deltaSha256")!=Hash(Input("csharp5-artifact.csharp5-source-delta-inventory.json")))return false;
   var configured=configDoc.RootElement.GetProperty("languages").EnumerateArray().Single(l=>Text(l,"name")=="CSharp5");var factory=gate.GetProperty("factory");
   if(Text(configured,"start")!="CSharp.CompilationUnits.CompilationUnit"||Text(configured,"namespace")!="CSNitrogen.Syntax"||Text(factory,"name")!="CSharp5"||Text(factory,"start")!="CompilationUnit"||Text(factory,"rootModule")!="CSharp.CompilationUnits"
    ||!Exact(Strings(configured.GetProperty("grammars")),Strings(factory.GetProperty("grammars")))||!Exact(Strings(map.GetProperty("factory").GetProperty("grammars")),Strings(factory.GetProperty("grammars")))||Strings(factory.GetProperty("modules")).Length!=32)return false;
   var heads=index.GetProperty("productions").EnumerateArray().ToDictionary(h=>Text(h,"id"));var rows=map.GetProperty("productions").EnumerateArray().ToArray();var inspected=review.GetProperty("productions").EnumerateArray().ToDictionary(r=>Text(r,"standardId"));
   var oldRows=oldMapDoc.RootElement.GetProperty("productions").EnumerateArray().ToDictionary(r=>Text(r,"standardId"));
   var links=deltaDoc.RootElement.GetProperty("productions").EnumerateArray().ToDictionary(r=>Text(r,"standardId"));
   if(heads.Count!=458||!Exact(heads.Keys,rows.Select(r=>Text(r,"standardId")))||!Exact(heads.Keys,inspected.Keys))return false;
   var additions=map.GetProperty("mainTextAdditions").EnumerateArray().ToArray();var reviewedAdditions=review.GetProperty("mainTextAdditions").EnumerateArray().ToDictionary(a=>Text(a,"id"));
   if(!Exact(Additions,additions.Select(a=>Text(a,"name")))||!Exact(additions.Select(a=>Text(a,"id")),reviewedAdditions.Keys))return false;
   var owners=Strings(gate.GetProperty("owners"));var groups=gate.GetProperty("policyGroups").EnumerateObject().ToDictionary(p=>p.Name,p=>Strings(p.Value));
   if(!Exact(owners,rows.SelectMany(r=>Strings(r.GetProperty("targets"))).Concat(additions.SelectMany(a=>Strings(a.GetProperty("targets")))).Distinct()))return false;
   if(!Exact(groups.Keys,new[]{"attribute-argument-order","attribute-target-recognition","csharp-trivia","csharp1-preprocessing","generated-syntax-context-checks","csharp4.source-context-checks","csharp5.source-context-checks"})||!Exact(groups["csharp5.source-context-checks"],["Parsing/CSharp5ContextPolicy.cs"]))return false;
   bool Verified(JsonElement r,string id,string basis,string domain)
   {
    var dimensions=r.GetProperty("reviews").EnumerateObject().ToArray();var sr=r.GetProperty("sourceReview");
    return Text(r,"status")=="verified"&&Text(r,"basis")==basis&&Exact(dimensions.Select(d=>d.Name),Dimensions)&&dimensions.All(d=>d.Value.GetString()=="verified")
     &&Text(sr,"domain")==domain&&Text(sr,"occurrenceId")==id&&Text(sr,"sourceSha256")==PrimarySha256&&Text(sr,"status")=="source-inspected"&&Text(sr,"report")=="docs/coverage/csharp5-normative-context-review.md";
   }
   using var oldReviewDoc=JsonDocument.Parse(Input("csharp4-artifact.csharp4-reviewed-occurrences.json"));
   var oldReviews=oldReviewDoc.RootElement.GetProperty("productions").EnumerateArray().ToDictionary(r=>Text(r,"standardId"));
   foreach(var row in rows)
   {
    string id=Text(row,"standardId");var h=heads[id];var r=inspected[id];var link=links[id];string predecessor=Text(link,"predecessorId");
    if(Text(row,"reviewStatus")!="owner-candidate"||Text(row,"sourceBodySha256")!=Text(h,"sourceBodySha256")||Text(r,"sourceBodySha256")!=Text(h,"sourceBodySha256")||Text(row,"predecessorId")!=predecessor||Text(r,"predecessorId")!=predecessor)return false;
    if(!oldRows.TryGetValue(predecessor,out var prior)||!oldReviews.TryGetValue(predecessor,out var priorReview)||Text(priorReview,"status")!="verified")return false;
    // Appendix owners, policies and mappings are the admitted fourth ones exactly.
    if(!Strings(row.GetProperty("targets")).SequenceEqual(Strings(prior.GetProperty("targets")))||!Strings(row.GetProperty("policies")).SequenceEqual(Strings(prior.GetProperty("policies")))||Text(row,"mapping")!=Text(prior,"mapping"))return false;
    string basis=Text(link,"bodyRelation")=="body-identical"?"predecessor-review-carried":"version5-source-review";
    if(!Verified(r,id,basis,Text(priorReview.GetProperty("sourceReview"),"domain")))return false;
    var ts=Strings(row.GetProperty("targets"));var ps=Strings(row.GetProperty("policies"));
    if(ts.Length+ps.Length==0||ps.Any(p=>!groups.ContainsKey(p)))return false;
   }
   foreach(var a in additions)
   {
    string id=Text(a,"id"),name=Text(a,"name");var r=reviewedAdditions[id];
    if(id!=$"M:{a.GetProperty("mainTextLine").GetInt32()}:{name}"||Text(a,"reviewStatus")!="owner-candidate"||Text(r,"sourceLineSha256")!=Text(a,"sourceLineSha256"))return false;
    if(!Exact(Strings(a.GetProperty("policies")),["csharp5.source-context-checks"])||Strings(a.GetProperty("targets")).Length==0)return false;
    var appendix=a.GetProperty("appendixOccurrence");
    if(name=="await-expression"?appendix.ValueKind!=JsonValueKind.Null:!heads.TryGetValue(appendix.GetString()!,out var omitted)||Text(omitted,"name")!=name)return false;
    string domain=name=="statement-expression"?"statement":name=="method-modifier"?"declaration":"expression";
    if(!Verified(r,id,"version5-main-text-review",domain))return false;
   }
   var all=heads.Values.SelectMany(h=>h.GetProperty("alternatives").EnumerateArray()).ToArray();if(all.Length!=1181)return false;
   var real=all.Where(a=>Text(a,"kind")!="inherited-placeholder").ToDictionary(a=>Text(a,"id"));int markers=0,refs=0;
   foreach(var h in heads.Values)foreach(var a in h.GetProperty("alternatives").EnumerateArray().Where(a=>Text(a,"kind")=="inherited-placeholder"))
   {
    var earlier=heads.Values.Where(p=>p.GetProperty("line").GetInt32()<h.GetProperty("line").GetInt32()&&Text(p,"name")==Text(h,"name")).SelectMany(p=>p.GetProperty("alternatives").EnumerateArray()).Where(p=>Text(p,"kind")!="inherited-placeholder").Select(p=>Text(p,"id")).ToArray();
    if(earlier.Length==0||!Exact(earlier,Strings(a.GetProperty("earlierSourceAlternativeIds"))))return false;markers++;refs+=earlier.Length;
   }
   if(real.Count!=1164||markers!=17||refs!=135)return false;
   using var matrixDoc=JsonDocument.Parse(Input("csharp5-evidence.alternative-witness-matrix.json"));using var specificDoc=JsonDocument.Parse(Input("csharp5-evidence.source-specific-cases.json"));using var fourthMatrixDoc=JsonDocument.Parse(Input("csharp4-evidence.alternative-witness-matrix.json"));
   var matrix=matrixDoc.RootElement;var specific=specificDoc.RootElement;
   if(Text(matrix,"profile")!="CSharp5"||Text(matrix,"sourceSha256")!=PrimarySha256||Text(matrix,"status")!="source-witnesses-unreviewed"||Text(specific,"sourceSha256")!=PrimarySha256||Text(specific,"profile")!="CSharp5")return false;
   if(Text(matrix,"indexSha256")!=Hash(Input("csharp5-artifact.csharp5-standard-index.json"))||Text(matrix,"deltaSha256")!=Hash(Input("csharp5-artifact.csharp5-source-delta-inventory.json")))return false;
   if(Text(matrix.GetProperty("sourceSpecific"),"sha256")!=Hash(Input("csharp5-evidence.source-specific-cases.json"))||Text(matrix.GetProperty("predecessorMatrix"),"sha256")!=Hash(Input("csharp4-evidence.alternative-witness-matrix.json")))return false;
   var fourthCases=fourthMatrixDoc.RootElement.GetProperty("cases").EnumerateArray().ToDictionary(c=>Text(c,"id"));
   var cases=matrix.GetProperty("cases").EnumerateArray().ToArray();if(!Exact(real.Keys,cases.Select(c=>Text(c,"id"))))return false;
   var local=specific.GetProperty("cases").EnumerateArray().ToDictionary(c=>Text(c,"id"));if(local.Count!=3)return false;
   var candidates=deltaDoc.RootElement.GetProperty("productions").EnumerateArray().SelectMany(p=>p.GetProperty("alternativeCandidates").EnumerateArray()).ToDictionary(c=>Text(c,"alternativeId"),c=>Strings(c.GetProperty("predecessorAlternativeCandidates")));
   int inherited=0,novel=0;
   foreach(var c in cases)
   {
    string id=Text(c,"id"),hash=Text(real[id],"sourceAlternativeSha256"),occurrence=id[..id.LastIndexOf('/')];
    if(Text(c,"sourceOccurrence")!=occurrence||Text(c,"sourceAlternativeSha256")!=hash)return false;
    if(Text(c,"sourceKind")=="predecessor-matrix")
    {
     string predecessor=Text(c,"predecessorAlternative");
     if(!Exact(candidates[id],[predecessor])||!fourthCases.TryGetValue(predecessor,out var p)||Text(p,"sourceAlternativeSha256")!=hash||Text(p,"witnessId")!=Text(c,"witnessId"))return false;
     if(Text(c,"domain")!=(Text(p,"domain")=="source-specific"?"fourth-source-specific":Text(p,"domain")))return false;inherited++;
    }
    else if(Text(c,"sourceKind")=="source-specific")
    {if(candidates[id].Length!=0||Text(c,"witnessId")!=id||Text(c,"domain")!="source-specific"||!local.TryGetValue(id,out var entry)||Text(entry,"sourceAlternativeSha256")!=hash)return false;novel++;}
    else return false;
   }
   if(inherited!=1161||novel!=3)return false;
   var diff=differencesDoc.RootElement;var disp=dispositionsDoc.RootElement;
   if(Text(diff,"sourceSha256")!=PrimarySha256||Text(diff,"fourthSourceSha256")!=CSharp4CoverageContract.PrimarySha256||Text(diff,"status")!="difference-index-unreviewed"||Text(disp,"status")!="source-inspected"||Text(disp,"sourceSha256")!=PrimarySha256||Text(disp,"differenceIndexSha256")!=Hash(Input("csharp5-artifact.csharp5-main-text-differences.json"))||Text(disp,"reportSha256")!=report)return false;
   var regions=diff.GetProperty("regions").EnumerateArray().ToDictionary(r=>Text(r,"id"));var dispositions=disp.GetProperty("regions").EnumerateArray().ToArray();
   if(regions.Count!=707||!Exact(regions.Keys,dispositions.Select(d=>Text(d,"id"))))return false;
   foreach(var d in dispositions){var r=regions[Text(d,"id")];if(Text(d,"fourthNormalizedSha256")!=Text(r.GetProperty("fourth"),"normalizedSha256")||Text(d,"fifthNormalizedSha256")!=Text(r.GetProperty("fifth"),"normalizedSha256")||!disp.GetProperty("dispositions").TryGetProperty(Text(d,"disposition"),out _))return false;}
   return true;
  }
  catch(Exception e) when(e is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException or FormatException or OverflowException){return false;}
 }
}
