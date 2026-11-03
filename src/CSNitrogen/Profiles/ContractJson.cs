using System.Security.Cryptography;
using System.Text.Json;
namespace CSNitrogen.Profiles;
/// <summary>Shared checks for the reviewed-snapshot admission contracts.</summary>
internal static class ContractJson
{
 internal static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 internal static string Text(JsonElement e,string key)=>e.GetProperty(key).GetString()!;
 internal static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
 internal static bool Exact(IEnumerable<string> a,IEnumerable<string> b)
 {var x=a.Order(StringComparer.Ordinal).ToArray();var y=b.Order(StringComparer.Ordinal).ToArray();return x.Distinct().Count()==x.Length&&y.Distinct().Count()==y.Length&&x.SequenceEqual(y);}
 /// <summary>Each inheritance marker names exactly the earlier non-placeholder alternatives of its family; returns the real alternatives by id.</summary>
 internal static Dictionary<string,JsonElement>? Alternatives(IReadOnlyCollection<JsonElement> heads,int alternatives,int real,int markers,int references)
 {
  var all=heads.SelectMany(h=>h.GetProperty("alternatives").EnumerateArray()).ToArray();if(all.Length!=alternatives)return null;
  var result=all.Where(a=>Text(a,"kind")!="inherited-placeholder").ToDictionary(a=>Text(a,"id"));int seen=0,refs=0;
  foreach(var h in heads)foreach(var a in h.GetProperty("alternatives").EnumerateArray().Where(a=>Text(a,"kind")=="inherited-placeholder"))
  {
   var earlier=heads.Where(p=>p.GetProperty("line").GetInt32()<h.GetProperty("line").GetInt32()&&Text(p,"name")==Text(h,"name")).SelectMany(p=>p.GetProperty("alternatives").EnumerateArray()).Where(p=>Text(p,"kind")!="inherited-placeholder").Select(p=>Text(p,"id")).ToArray();
   if(earlier.Length==0||!Exact(earlier,Strings(a.GetProperty("earlierSourceAlternativeIds"))))return null;seen++;refs+=earlier.Length;
  }
  return result.Count==real&&seen==markers&&refs==references?result:null;
 }
 /// <summary>Every difference region has one defined disposition with matching predecessor/successor normalized hashes.</summary>
 internal static bool Dispositions(JsonElement differences,JsonElement dispositions,string before,string after,int count)
 {
  var regions=differences.GetProperty("regions").EnumerateArray().ToDictionary(r=>Text(r,"id"));var rows=dispositions.GetProperty("regions").EnumerateArray().ToArray();
  if(regions.Count!=count||!Exact(regions.Keys,rows.Select(d=>Text(d,"id"))))return false;
  foreach(var d in rows)
  {
   var r=regions[Text(d,"id")];
   if(Text(d,before+"NormalizedSha256")!=Text(r.GetProperty(before),"normalizedSha256")||Text(d,after+"NormalizedSha256")!=Text(r.GetProperty(after),"normalizedSha256")||!dispositions.GetProperty("dispositions").TryGetProperty(Text(d,"disposition"),out _))return false;
  }
  return true;
 }
}
