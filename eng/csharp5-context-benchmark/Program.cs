using System.Diagnostics;
using System.Text.Json;
using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;

var data=new List<object>();
string fourth=string.Concat(Enumerable.Range(0,15).Select(i=>$"class C{i}<T>{{public T M(T value=default(T)){{return value;}} public void N(){{M(value:default(T));}}}}\n"));
var language=CSharp4Composition.CreateBuilder().Build();
MeasureRaw(fourth,language);
Measure("CSharp4/policy",fourth,"CSharp4");
MeasureIncremental("CSharp4/incremental-policy",fourth,"CSharp4");
if(!args.Contains("--baseline"))
{
 string fifth=string.Concat(Enumerable.Range(0,10).Select(i=>$"class C{i}<T>{{public T Id(T value=default(T)){{return value;}}public async Task M(){{await Id(value:default(T));}}object f=async x=>await Id(x);public void N(){{await(Id(default(T)));}}}}\n"));
 Measure("CSharp5/mixed-policy",fifth,"CSharp5");
 MeasureIncremental("CSharp5/incremental-policy",fifth,"CSharp5");
}
Console.WriteLine(JsonSerializer.Serialize(new{runtime=Environment.Version.ToString(),measurements=data}));

void MeasureRaw(string text,Language language)
{
 for(int i=0;i<1000;i++){using var p=language.Parse(text,CSharp4Composition.Start);CheckRaw(p);}
 var times=new double[7];var bytes=new long[7];ParseStats stats=default;
 for(int round=0;round<7;round++)
 {
  GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();long before=GC.GetAllocatedBytesForCurrentThread();var watch=Stopwatch.StartNew();
  for(int i=0;i<500;i++){using var p=language.Parse(text,CSharp4Composition.Start);CheckRaw(p);stats=p.Stats;}
  watch.Stop();times[round]=watch.Elapsed.TotalMilliseconds/500;bytes[round]=(GC.GetAllocatedBytesForCurrentThread()-before)/500;
 }
 data.Add(new{name="CSharp4/raw",characters=text.Length,iterations=500,milliseconds=times,allocatedBytesPerParse=bytes,nodes=stats.TreeNodes,memoHits=stats.MemoHits,memoMisses=stats.MemoMisses});
}
void Measure(string name,string text,string profile)
{
 var request=new ParseRequest(text,profile,"measurement.cs",[]);
 for(int i=0;i<1000;i++){using var p=CSharpParser.Parse(request);Check(p);}
 var times=new double[7];var bytes=new long[7];int nodes=0;
 for(int round=0;round<7;round++)
 {
  GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();long before=GC.GetAllocatedBytesForCurrentThread();var watch=Stopwatch.StartNew();
  for(int i=0;i<500;i++){using var p=CSharpParser.Parse(request);Check(p);nodes=p.Tree.NodeCount;}
  watch.Stop();times[round]=watch.Elapsed.TotalMilliseconds/500;bytes[round]=(GC.GetAllocatedBytesForCurrentThread()-before)/500;
 }
 data.Add(new{name,characters=text.Length,iterations=500,milliseconds=times,allocatedBytesPerParse=bytes,nodes});
}
void MeasureIncremental(string name,string text,string profile)
{
 int offset=text.IndexOf("C0",StringComparison.Ordinal);string changed=text[..offset]+"D"+text[(offset+1)..];
 var first=new ParseRequest(text,profile,"measurement.cs",[]);var nextRequest=new ParseRequest(changed,profile,"measurement.cs",[]);var changes=new TextChange[]{new(offset,1,1)};
 var times=new double[7];var bytes=new long[7];int nodes=0,reused=0;
 for(int round=0;round<7;round++)
 {
  var current=CSharpParser.ParseIncremental(first);Check(current);
  try
  {
   for(int i=0;i<1000;i++){var next=CSharpParser.Reparse(current,i%2==0?nextRequest:first,changes);Check(next);current.Dispose();current=next;}
   GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();long before=GC.GetAllocatedBytesForCurrentThread();var watch=Stopwatch.StartNew();
   for(int i=0;i<2000;i++){var next=CSharpParser.Reparse(current,i%2==0?nextRequest:first,changes);Check(next);nodes=next.Tree.NodeCount;reused=next.ReusedNodes;current.Dispose();current=next;}
   watch.Stop();times[round]=watch.Elapsed.TotalMilliseconds/2000;bytes[round]=(GC.GetAllocatedBytesForCurrentThread()-before)/2000;
  }
  finally{current.Dispose();}
 }
 data.Add(new{name,characters=text.Length,iterations=2000,milliseconds=times,allocatedBytesPerParse=bytes,nodes,reusedNodes=reused});
}
static void Check(ParseDocument document){if(!document.Accepted)throw new InvalidOperationException(string.Join(";",document.Issues));}
static void CheckRaw(ParseResult result){if(!result.Success||result.HasErrors)throw new InvalidOperationException("Raw measurement input must parse without errors.");}
