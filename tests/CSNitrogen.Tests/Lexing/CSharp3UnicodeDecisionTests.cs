using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Lexing;
/// <summary>Observations of the current partial profile, retained for the historical-source contract decision.</summary>
public class CSharp3UnicodeDecisionTests
{
 [Theory]
 [InlineData("class \u0220{}",false,true,true)]
 [InlineData("class \U00010400{}",false,true,false)]
 [InlineData(@"class \U00010400{}",false,true,false)]
 [InlineData(@"class \u005f{}",false,false,true)]
 [InlineData("\u205fclass C{}",false,true,true)]
 public void Current_profile_observations_are_explicit_before_historical_admission(string source,bool first,bool second,bool third)
 {
  foreach(var (profile,accepted) in new[]{("CSharp1",first),("CSharp2",second),("CSharp3",third)})
  {using var parsed=CSharpParser.Parse(new(source,profile,"unicode-decision.cs",[]));Assert.Equal(accepted,parsed.Accepted);}
  Assert.Contains("unicode-current-bmp",ProfileResolver.Resolve("CSharp3").Profile!.Policies);
  Assert.False(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
}
