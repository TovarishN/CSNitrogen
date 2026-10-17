using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;

public class StatementConformanceTests
{
    [Theory]
    [InlineData("switch(x){default:break;default:break;}", "CSN2006", "default")]
    [InlineData("switch(x){default:default:break;}", "CSN2006", "default")]
    [InlineData("switch(x){case 1:default:break;default:return;}", "CSN2006", "default")]
    public void Default_label_cardinality_is_local_to_one_switch(string statement, string code, string marker)
    {
        const string prefix = "class C{void M(){";
        using var parsed = CSharpParser.Parse(new(prefix + statement + "}}", "CSharp1", "statement.cs", []));
        Assert.False(parsed.Accepted);
        var issue = Assert.Single(parsed.Issues, issue => issue.Code == code);
        Assert.Equal(IssueCategory.Syntax, issue.Category);
        Assert.Equal(new TextSpan(prefix.Length + statement.LastIndexOf(marker, StringComparison.Ordinal), marker.Length), issue.Span);
    }

    [Theory]
    [InlineData("using(System.IDisposable x=F(),y=G()){T z;}")]
    [InlineData("using(F()){T x;}")]
    [InlineData("using(F())using(G()){T x;}")]
    [InlineData("switch(x){default:switch(y){default:break;}break;}")]
    [InlineData("switch(x){default:break;}switch(y){default:break;}")]
    [InlineData("switch(x){case 1:case 2:default:break;}")]
    [InlineData("for(T x,y;;){break;}")]
    [InlineData("{T x;const int y=1;}")]
    public void Statement_context_state_does_not_leak(string statement)
    {
        using var parsed = CSharpParser.Parse(new("class C{void M(){" + statement + "}}", "CSharp1", "control.cs", []));
        Assert.True(parsed.Accepted, string.Join(";", parsed.Issues));
    }

    [Theory]
    [InlineData("if(x) T y;")][InlineData("while(x) const int y=1;")]
    [InlineData("for(;;) label:F();")][InlineData("foreach(T x in xs) T y;")]
    [InlineData("do T x;while(a);")][InlineData("using(F()) T x;")]
    [InlineData("lock(x) label:F();")][InlineData("switch(x){case 1:}")]
    [InlineData("switch(x){default:}")][InlineData("try{}")]
    [InlineData("try{}finally{}catch(E e){}")][InlineData("try{}catch{}catch(E e){}")]
    [InlineData("if(x)")][InlineData("if(x)F();else")][InlineData("do F();while(x)")]
    [InlineData("for(a+b;;);")][InlineData("for(;;a+b);")][InlineData("(F());")]
    [InlineData("x;")][InlineData("1;")][InlineData("new int[1];")]
    public void Malformed_or_ineligible_statement_contexts_are_rejected(string statement)
    {
        using var parsed = CSharpParser.Parse(new("class C{void M(){" + statement + "}}", "CSharp1", "bad-statement.cs", []));
        Assert.False(parsed.Accepted);
    }
    [Theory]
    [InlineData("CSharp1")][InlineData("CSharp14")]
    public void Syntax_only_statement_constraints_apply_to_selected_baselines(string profile)
    {
        foreach (string statement in new[] { "switch(x){default:default:break;}" })
        {
            using var parsed = CSharpParser.Parse(new("class C{void M(){" + statement + "}}", profile, "baseline-flow.cs", []));
            Assert.False(parsed.Accepted);
            Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2006");
        }
    }

    [Fact]
    public void Multiple_defaults_accumulate_without_diagnosing_resource_initializers()
    {
        const string source = "class C{void M(){using(System.IDisposable x,y){}switch(x){default:default:default:break;}}}";
        using var parsed = CSharpParser.Parse(new(source, "CSharp1", "multiple-flow-errors.cs", []));
        Assert.False(parsed.Accepted);
        Assert.DoesNotContain(parsed.Issues, issue => issue.Code == "CSN2007");
        Assert.Equal(2, parsed.Issues.Count(issue => issue.Code == "CSN2006"));
    }
    [Theory]
    [InlineData("using(System.IDisposable x){}")]
    [InlineData("using(System.IDisposable x=F(),y){}")]
    [InlineData("using(System.IDisposable x,y=F()){}")]
    public void Missing_using_initializers_remain_an_explicit_semantic_exclusion(string statement)
    {
        using var parsed = CSharpParser.Parse(new("class C{void M(){" + statement + "}}", "CSharp1", "semantic-resource.cs", []));
        Assert.True(parsed.Accepted, string.Join(";", parsed.Issues));
    }
}
