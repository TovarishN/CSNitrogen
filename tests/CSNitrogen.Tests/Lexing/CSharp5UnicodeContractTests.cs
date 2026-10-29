using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Lexing;
public class CSharp5UnicodeContractTests
{
 [Theory][InlineData("class 𐐀{}")][InlineData(@"class \U00010400{}")][InlineData("\u180eclass C{}")][InlineData("\u205fclass C{}")][InlineData("#define 𐐀\n#if 𐐀\nclass C{}\n#endif")]
 public void Fifth_uses_the_approved_frozen_scalar_repertoire_for_source_and_directives(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp5","unicode5.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.False(p.ProfileComplete);}
 [Theory][InlineData("class \u1c90{}")][InlineData("#define \u1c90\nclass C{}")]
 public void Host_current_letters_do_not_expand_the_fifth_frozen_contract(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp5","unicode5-invalid.cs",[]));Assert.False(p.Accepted);}
 [Fact]public void Fifth_metadata_records_the_approved_policy_while_source_admission_stays_pending()
 {var p=ProfileResolver.Resolve("CSharp5").Profile!;Assert.Contains("unicode-4.0-scalar",p.Policies);Assert.DoesNotContain("unicode-current-bmp",p.Policies);Assert.False(p.Complete);}
}
