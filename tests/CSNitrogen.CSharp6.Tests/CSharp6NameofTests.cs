namespace CSNitrogen.CSharp6.Tests;
public class CSharp6NameofTests
{
 static string Field(string e)=>"class C{object o="+e+";}";
 [Theory]
 [InlineData("nameof(int)")]
 [InlineData("nameof(base)")]
 public void Only_the_named_entity_reading(string e)
 {
  string s=Field(e);
  Assert.Equal([Probe.At(s,e)],Probe.Spans("All",s,"NameofExpression"));
  Assert.Empty(Probe.Spans("All",s,"Ambiguous"));
  Assert.Empty(Probe.Spans("All",s,"InvocationExpression"));
 }
 [Theory]
 [InlineData("nameof(x)")]
 [InlineData("nameof(a.b)")]
 [InlineData("nameof(List<int>.Count)")]
 [InlineData("nameof(this.x)")]
 [InlineData("nameof(global::System.String)")]
 [InlineData("nameof(int.MaxValue)")]
 public void Both_readings_are_kept_in_one_ambiguous_node(string e)
 {
  foreach(var prefix in new[]{"","/*\U00010400*/"})
  {
   string s=prefix+Field(e);var at=Probe.At(s,e);
   Assert.Equal([at],Probe.Spans("All",s,"Ambiguous"));
   Assert.Equal([at],Probe.Spans("All",s,"NameofExpression"));
   Assert.Equal([at],Probe.Spans("All",s,"NameofInvocation"));
   Assert.Empty(Probe.Spans("All",s,"InvocationExpression"));
   Assert.Equal(["NameofExpression","NameofInvocation"],Probe.Children("All",s,"Ambiguous").Single().Order(StringComparer.Ordinal));
  }
 }
 [Theory]
 [InlineData("nameof(1+2)")]
 [InlineData("nameof(x,y)")]
 [InlineData("nameof()")]
 [InlineData("nameof(ref x)")]
 public void Only_the_invocation_reading(string e)
 {
  string s=Field(e);
  Assert.Empty(Probe.Spans("All",s,"NameofExpression"));
  Assert.Empty(Probe.Spans("All",s,"Ambiguous"));
  Assert.Equal([Probe.At(s,e)],Probe.Spans("All",s,"InvocationExpression"));
  Assert.Empty(Probe.Spans("All",s,"NameofInvocation"));
 }
 [Fact] public void Postfix_access_applies_to_the_ambiguous_node()
 {
  string s=Field("nameof(x).Length");
  Assert.Equal([Probe.At(s,"nameof(x).Length")],Probe.Spans("All",s,"MemberExpression"));
  Assert.Equal([Probe.At(s,"nameof(x)")],Probe.Spans("All",s,"Ambiguous"));
 }
 [Fact] public void Only_the_nameof_tie_is_sanctioned()
 {
  var l=CSNitrogen.CSharp6Probes.CSharp6AllProbeComposition.CreateBuilder().Build();
  using var p=l.Parse(Field("nameof(x)"),CSNitrogen.CSharp6Probes.CSharp6AllProbeComposition.Start);
  Assert.True(p.HasErrors);
  Assert.All(p.Diagnostics.ToArray(),d=>Assert.Equal(Nitrogen.DiagnosticCode.Ambiguous,d.Code));
  Assert.True(Probe.Complete(l,p));
 }
 [Fact] public void Ordinary_nameof_identifiers_stay_names()
 {
  const string s="class nameof{int nameof;object o=nameof;object p=x.nameof;}";
  Assert.Empty(Probe.Spans("All",s,"NameofExpression"));
 }
 [Fact] public void Verbatim_at_nameof_is_an_invocation()
 {
  string s=Field("@nameof(x)");
  Assert.Empty(Probe.Spans("All",s,"NameofExpression"));
 }
}
