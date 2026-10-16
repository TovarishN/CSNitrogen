using System.Text.Json;
namespace CSNitrogen.Tests.Profiles;
public class CoverageLedgerTests
{
    [Fact] public void Inventory_is_explicit_about_status_and_unreviewed_boundaries()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"features.json")));
        var features=doc.RootElement.GetProperty("features").EnumerateArray().ToArray();
        var ids=features.Select(f=>f.GetProperty("id").GetString()).ToArray(); Assert.Equal(ids.Length,ids.Distinct().Count());
        foreach(var feature in features)
        {
            Assert.Contains(feature.GetProperty("status").GetString(),new[]{"unimplemented","implemented","verified","blocked"});
            Assert.True(Uri.TryCreate(feature.GetProperty("reference").GetString(),UriKind.Absolute,out _));
            Assert.True(Version.TryParse(feature.GetProperty("introduced").GetString(),out _));
            if(feature.GetProperty("status").GetString()=="verified")
            { Assert.True(feature.GetProperty("reviewed").GetBoolean()); Assert.NotEmpty(feature.GetProperty("fixtures").EnumerateArray()); }
        }
        foreach(int major in Enumerable.Range(1,14)) Assert.Contains(features,f=>f.GetProperty("introduced").GetString()!.StartsWith(major+"."));
    }
    [Fact] public void Ledger_requirement_references_exist_and_are_acyclic()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"features.json")));
        var graph=doc.RootElement.GetProperty("features").EnumerateArray().ToDictionary(f=>f.GetProperty("id").GetString()!,f=>f.GetProperty("requires").EnumerateArray().Select(x=>x.GetString()!).ToArray());
        var active=new HashSet<string>();var visited=new HashSet<string>();
        void Visit(string id)
        {
            if(visited.Contains(id))return;Assert.True(active.Add(id),"Dependency cycle: "+id);
            foreach(var required in graph[id]){Assert.True(graph.ContainsKey(required),"Missing dependency: "+required);Visit(required);}
            active.Remove(id);visited.Add(id);
        }
        foreach(var id in graph.Keys)Visit(id);
    }
}
