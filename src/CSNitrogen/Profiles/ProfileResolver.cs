namespace CSNitrogen.Profiles;
public static class ProfileResolver
{
    public static ProfileResolution Resolve(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Version? version = null;
        for(int major=1;major<=14;major++) if(name == $"CSharp{major}") version = new(major,0);
        for(int minor=1;minor<=3;minor++) if(name == $"CSharp7_{minor}") version = new(7,minor);
        if(version is null) return new(null,Array.AsReadOnly(new[]{new ProfileIssue("unknown-profile",$"Unknown profile '{name}'.")}));
        var custom = ResolveCustom(name,FeatureCatalog.Implemented.Select(f=>f.Id));
        var requested = FeatureCatalog.Inventory.Where(f => f.Classification == "syntax-or-policy" && f.Introduced <= version)
            .Select(f => f.Id).Order(StringComparer.Ordinal).ToArray();
        return custom with { Profile = custom.Profile! with {
            Version = version, RequestedFeatures = Array.AsReadOnly(requested),
            MissingFeatures = Array.AsReadOnly(requested.Except(custom.Profile!.Features,StringComparer.Ordinal).ToArray())
        } };
    }
    public static ProfileResolution ResolveCustom(string name, IEnumerable<string> featureIds,
        IReadOnlyList<FeatureDefinition>? definitions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name); ArgumentNullException.ThrowIfNull(featureIds);
        var features=featureIds.ToArray(); definitions ??= FeatureCatalog.Implemented;
        var issues=new List<ProfileIssue>();
        foreach(var duplicate in definitions.GroupBy(d=>d.Id,StringComparer.Ordinal).Where(g=>g.Count()>1))
            issues.Add(new("catalog-duplicate",$"Duplicate catalog feature '{duplicate.Key}'."));
        var catalog=definitions.GroupBy(d=>d.Id,StringComparer.Ordinal).ToDictionary(g=>g.Key,g=>g.First(),StringComparer.Ordinal);
        var visiting=new HashSet<string>(StringComparer.Ordinal); var visited=new HashSet<string>(StringComparer.Ordinal);
        void Visit(string id)
        {
            if(visited.Contains(id)) return;
            if(!visiting.Add(id)) { issues.Add(new("catalog-cycle",$"Dependency cycle at '{id}'.")); return; }
            foreach(var dependency in catalog[id].Requires.Order(StringComparer.Ordinal))
                if(catalog.ContainsKey(dependency)) Visit(dependency);
                else issues.Add(new("catalog-missing",$"Catalog feature '{id}' requires unknown '{dependency}'."));
            visiting.Remove(id); visited.Add(id);
        }
        foreach(var id in catalog.Keys.Order(StringComparer.Ordinal)) Visit(id);
        foreach(var group in features.GroupBy(id=>id,StringComparer.Ordinal).Where(g=>g.Count()>1))
            issues.Add(new("duplicate-feature",$"Feature '{group.Key}' was selected more than once."));
        var selected=features.ToHashSet(StringComparer.Ordinal);
        foreach(var id in selected.Order(StringComparer.Ordinal))
        {
            if(!catalog.TryGetValue(id,out var feature)) { issues.Add(new("unknown-feature",$"Unknown feature '{id}'.")); continue; }
            foreach(var requirement in feature.Requires.Where(r=>!selected.Contains(r)))
                issues.Add(new("missing-requirement",$"Feature '{id}' requires '{requirement}'."));
            foreach(var conflict in feature.Conflicts.Where(selected.Contains))
                issues.Add(new("conflict",$"Feature '{id}' conflicts with '{conflict}'."));
        }
        var orderedIssues=issues.Distinct().OrderBy(i=>i.Code,StringComparer.Ordinal).ThenBy(i=>i.Message,StringComparer.Ordinal).ToArray();
        if(orderedIssues.Length>0) return new(null,Array.AsReadOnly(orderedIssues));
        return new(new(name,null,Array.AsReadOnly(selected.Order(StringComparer.Ordinal).ToArray()),
            Array.AsReadOnly(selected.SelectMany(id=>catalog[id].Grammars).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()),
            Array.Empty<string>(),false),Array.Empty<ProfileIssue>());
    }
}
