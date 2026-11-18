using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static CSNitrogen.Profiles.ContractJson;
namespace CSNitrogen.Profiles;
/// <summary>Admission of one reviewed normalized C#5 syntax snapshot, independently of source acceptance.</summary>
internal static class CSharp5CoverageContract
{
 internal const string PrimarySha256="6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519";
 internal const string ApprovedLedgerSha256="43d96ac77496424f0ea005d4a26721b107a5aec7425f02b3d01aacf31fee2494";
 static readonly string[] Additions=["unary-expression","await-expression","lambda-expression","anonymous-method-expression","statement-expression","method-modifier"];
 /// <summary>Reviewed source paths (as tools/approve-csharp5-review.py records them) and their embedded resources.</summary>
 static readonly IReadOnlyDictionary<string,string> ApprovedInputs=new Dictionary<string,string>(StringComparer.Ordinal)
 {
  ["docs/coverage/csharp5-reference-policy.json"]="csharp5-artifact.csharp5-reference-policy.json",
  ["docs/coverage/csharp5-standard-index.json"]="csharp5-artifact.csharp5-standard-index.json",
  ["docs/coverage/csharp5-source-delta-inventory.json"]="csharp5-artifact.csharp5-source-delta-inventory.json",
  ["docs/coverage/csharp5-standard-crosswalk.json"]="csharp5-artifact.csharp5-standard-crosswalk.json",
  ["docs/coverage/csharp5-main-text-differences.json"]="csharp5-artifact.csharp5-main-text-differences.json",
  ["docs/coverage/csharp5-main-text-dispositions.json"]="csharp5-artifact.csharp5-main-text-dispositions.json",
  ["docs/coverage/csharp5-normative-context-review.md"]="csharp5-review.csharp5-normative-context-review.md",
  ["docs/coverage/csharp4-reviewed-occurrences.json"]="csharp4-artifact.csharp4-reviewed-occurrences.json",
  ["tests/Corpus/CSharp5/alternative-witness-matrix.json"]="csharp5-evidence.alternative-witness-matrix.json",
  ["tests/Corpus/CSharp5/source-specific-cases.json"]="csharp5-evidence.source-specific-cases.json",
 };
 static readonly string[] Dimensions=["normative-prose","positive-negative","ambiguity-context","spans-diagnostics"];
 // Predecessor/self contract literals are excluded so the pin graph stays directed:5 ->4 ->3 ->2 refresh order.
 internal static bool Eligible(string name)=>name is "CSNitrogen.coverage.json" or "CSNitrogen.dependencies.json"
  ||name.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp5-artifact.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp5-review.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp5-evidence.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp4-artifact.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.csharp4-evidence.",StringComparison.Ordinal)||name.StartsWith("CSNitrogen.csharp3-evidence.",StringComparison.Ordinal)
  ||name.StartsWith("CSNitrogen.source.",StringComparison.Ordinal)&&name is not ("CSNitrogen.source.Profiles/CSharp2CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp3CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp4CoverageContract.cs" or "CSNitrogen.source.Profiles/CSharp5CoverageContract.cs");
 internal static bool Reviewed(byte[] ledger,byte[] composition,IReadOnlyDictionary<string,byte[]> inputs,bool fourthReviewed)
  =>Reviewed(ledger,composition,inputs,fourthReviewed,ApprovedLedgerSha256);
 /// <summary>The guard against an explicit ledger digest; tests re-pin mutated inputs to reach the structural checks.</summary>
 internal static bool Reviewed(byte[] ledger,byte[] composition,IReadOnlyDictionary<string,byte[]> inputs,bool fourthReviewed,string approvedLedgerSha256)
 {
  try
  {
   if(!fourthReviewed||Hash(ledger)!=approvedLedgerSha256)return false;
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
   // A named reviewer must have approved exactly the current reviewed inputs; the overlay binds that record.
   var approvalBytes=Input("csharp5-artifact.csharp5-review-approval.json");using var approvalDoc=JsonDocument.Parse(approvalBytes);var approval=approvalDoc.RootElement;
   if(Text(approval,"profile")!="CSharp5"||Text(approval,"status")!="approved"||string.IsNullOrWhiteSpace(Text(approval,"reviewer"))||Text(review,"approvalSha256")!=Hash(approvalBytes))return false;
   var approved=approval.GetProperty("inputs").EnumerateObject().ToDictionary(p=>p.Name,p=>p.Value.GetString()!);
   if(!Exact(approved.Keys,ApprovedInputs.Keys)||ApprovedInputs.Any(p=>approved[p.Key]!=Hash(Input(p.Value))))return false;
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
   // Every owner must be declared by the selected factory's own grammar sources.
   var declared=DeclaredOwners(Strings(factory.GetProperty("grammars")).Select(g=>Encoding.UTF8.GetString(Input("grammar."+g["Grammars/".Length..]))));
   if(owners.Any(o=>!declared.Contains(o)))return false;
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
    if(ts.Length+ps.Length==0||ts.Distinct().Count()!=ts.Length||ps.Distinct().Count()!=ps.Length||ps.Any(p=>!groups.ContainsKey(p)))return false;
    string correction=row.TryGetProperty("sourceCorrection",out var c)?c.GetString()!:"",priorCorrection=prior.TryGetProperty("sourceCorrection",out var pc)?pc.GetString()!:"";
    if(correction!=priorCorrection)return false;
   }
   // The approved predecessor corrections carry forward to exactly the corresponding fifth occurrences.
   var corrections=map.GetProperty("sourceCorrections");var oldCorrections=oldMapDoc.RootElement.GetProperty("sourceCorrections");
   if(!Exact(corrections.EnumerateObject().Select(p=>p.Name),oldCorrections.EnumerateObject().Select(p=>p.Name)))return false;
   foreach(var p in corrections.EnumerateObject())
   {
    var old=oldCorrections.GetProperty(p.Name);
    if(Text(p.Value,"status")!=Text(old,"status")||Text(p.Value,"decision")!=Text(old,"decision")||Text(p.Value,"status")!="approved-predecessor-continuity")return false;
    var corrected=rows.Where(r=>r.TryGetProperty("sourceCorrection",out var c)&&c.GetString()==p.Name).Select(r=>Text(r,"standardId")).ToArray();
    if(corrected.Length!=Strings(old.GetProperty("occurrences")).Length||!Exact(corrected,Strings(p.Value.GetProperty("occurrences"))))return false;
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
   var real=Alternatives(heads.Values,1181,1164,17,135);if(real is null)return false;
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
   if(!Dispositions(diff,disp,"fourth","fifth",707))return false;
   return true;
  }
  catch(Exception e) when(e is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException or FormatException or OverflowException){return false;}
 }

 static readonly Regex Module=new(@"\Gsyntax\s+module\s+([\w.]+)\s*\{",RegexOptions.CultureInvariant);
 static readonly Regex RuleHead=new(@"\G(?:token|syntax)\s+(\w+)\s*=",RegexOptions.CultureInvariant);
 static readonly Regex Block=new(@"\G(extensible|extend)\s+syntax\s+([\w.]+)\s*\{",RegexOptions.CultureInvariant);
 static readonly Regex Alternative=new(@"\G\|\s*(?:transparent\s+)?(\w+)\s*=",RegexOptions.CultureInvariant);
 /// <summary>Owner identities declared by grammar sources: module rules, extensible alternatives and extensions.</summary>
 internal static HashSet<string> DeclaredOwners(IEnumerable<string> grammars)
 {
  var owners=new HashSet<string>(StringComparer.Ordinal);
  foreach(string source in grammars)
  {
   // Blank comments and string/character literals so braces inside them do not count.
   var text=source.ToCharArray();
   for(int i=0;i<text.Length;i++)
   {
    if(text[i]=='/'&&i+1<text.Length&&text[i+1]=='/'){while(i<text.Length&&text[i]!='\n')text[i++]=' ';continue;}
    if(text[i] is '"' or '\'')
    {char quote=text[i];text[i++]=' ';while(i<text.Length&&text[i]!=quote){if(text[i]=='\\'&&i+1<text.Length)text[i++]=' ';text[i++]=' ';}if(i<text.Length)text[i]=' ';}
   }
   string plain=new(text);string module="";int depth=0;string? block=null,target=null;
   for(int i=0;i<plain.Length;i++)
   {
    bool start=i==0||!(char.IsLetterOrDigit(plain[i-1])||plain[i-1]=='_');
    if(depth==0&&start&&Module.Match(plain,i) is {Success:true} m){module=m.Groups[1].Value;i+=m.Length-1;depth=1;continue;}
    if(depth==1&&start&&Block.Match(plain,i) is {Success:true} b)
    {block=b.Groups[1].Value;target=b.Groups[2].Value;if(block=="extensible")owners.Add(module+"/rule:"+target);i+=b.Length-1;depth=2;continue;}
    if(depth==1&&start&&RuleHead.Match(plain,i) is {Success:true} r){owners.Add(module+"/rule:"+r.Groups[1].Value);i+=r.Length-1;continue;}
    if(depth==2&&block is not null&&Alternative.Match(plain,i) is {Success:true} a)
    {owners.Add(module+(block=="extensible"?"/alternative:":"/extension:")+target+"."+a.Groups[1].Value);i+=a.Length-1;continue;}
    if(plain[i]=='{')depth++;
    else if(plain[i]=='}'){depth--;if(depth==1)block=null;if(depth==0)module="";}
   }
  }
  return owners;
 }
}
