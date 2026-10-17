namespace CSNitrogen.Profiles;
/// <summary>A reporting identity's required canonical bundles and compatibility policies.
/// This mapping does not make the reporting identity independently selectable.</summary>
public sealed record CoverageMapping(string InventoryId, IReadOnlyList<string> Capabilities, IReadOnlyList<string> Policies);
