namespace SRankAssistant;

internal enum SRankConditionType
{
    Killing,
    Gathering,
    Fate,
    FateTimer,
    Discard,
}

internal sealed class SRankCondition
{
    internal SRankConditionType Type { get; }
    internal List<(uint Count, uint TargetId)> Targets { get; }

    internal SRankCondition(SRankConditionType type, IEnumerable<(uint, uint)> targets)
    {
        Type = type;
        Targets = new List<(uint, uint)>(targets);
    }
}

internal static class SRankData
{
    internal static readonly Dictionary<uint, SRankCondition> Zones = new()
    {
        { 147u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 131u) }) },
        { 613u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 6675u), (100u, 6674u) }) },
        { 957u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 13529u), (100u, 13526u), (100u, 13524u) }) },
        { 817u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 10276u), (100u, 10277u), (100u, 10280u) }) },
        { 959u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 13367u), (100u, 13365u), (100u, 13366u) }) },
        { 402u, new SRankCondition(SRankConditionType.Killing, new[] { (50u, 4141u), (50u, 4014u), (50u, 4080u) }) },
        { 612u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 7435u), (100u, 6654u) }) },
        { 814u, new SRankCondition(SRankConditionType.Gathering, new[] { (50u, 27759u) }) },
        { 400u, new SRankCondition(SRankConditionType.Gathering, new[] { (50u, 12536u), (50u, 12634u) }) },
        { 146u, new SRankCondition(SRankConditionType.FateTimer, new[] { (3600u, 0u) }) },
        { 398u, new SRankCondition(SRankConditionType.Fate, new[] { (5u, 831u) }) },
        { 621u, new SRankCondition(SRankConditionType.Discard, new[] { (50u, 0u) }) },
        { 1190u, new SRankCondition(SRankConditionType.Fate, new[] { (3u, 1862u) }) },
    };

    internal static SRankCondition? GetCondition() => Zones.TryGetValue(SERVICES.ClientState.TerritoryType, out SRankCondition? condition) ? condition : null;

    internal static bool IsZone() => Zones.ContainsKey(SERVICES.ClientState.TerritoryType);
}

/*
*     internal static readonly Dictionary<uint, SRankCondition> Zones = new()
{
    { 147, new SRankCondition(SRankConditionType.Killing, new[] { (100u, "Earth Sprite") }) },
    { 613, new SRankCondition(SRankConditionType.Killing, new[] { (100u, "Yumemi"), (100u, "Naked Yumemi") }) },
    { 814, new SRankCondition(SRankConditionType.Gathering, new[] { (50u, "Dwarven Cotton Boll") }) },
    { 400, new SRankCondition(SRankConditionType.Gathering, new[] { (50u, "Aurum Regis ore"), (50u, "Seventh Heaven") }) },
    { 402, new SRankCondition(SRankConditionType.Killing, new[] { (50u, "Allagan Chimera"), (50u, "Lesser Hydra"), (50u, "Meracydian Vouivre") }) },
    { 817, new SRankCondition(SRankConditionType.Killing, new[] { (100u, "Cracked Ronkan Doll"), (100u, "Cracked Ronkan Thorn"), (100u, "Cracked Ronkan Vessel")}) },
    { 612, new SRankCondition(SRankConditionType.Killing, new[] { (100u, "Leshy"), (100u, "Diakka") }) },
    { 957, new SRankCondition(SRankConditionType.Killing, new[] { (100u, "Asvattha"), (100u, "Pisaca"), (100u, "Vajralangula") }) },
    { 959, new SRankCondition(SRankConditionType.Killing, new[] { (100u, "Thinkers"), (100u, "Wanderers"), (100u, "Weepers") }) },
    { 146, new SRankCondition(SRankConditionType.FateTimer, new[] { (3600u, string.Empty) }) },
    { 398, new SRankCondition(SRankConditionType.Fate, new[] { (5u, "Cerf's Up") }) }, // Fate ID 831
};
*/