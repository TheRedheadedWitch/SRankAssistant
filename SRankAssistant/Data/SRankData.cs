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
        { 147u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 113u) }) },
        { 402u, new SRankCondition(SRankConditionType.Killing, new[] { (50u, 3540u), (50u, 3556u), (50u, 3580u) }) },
        { 612u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 5671u), (100u, 5685u) }) },
        { 613u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 5750u), (100u, 5751u) }) },
        { 817u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 8598u), (100u, 8599u), (100u, 8789u) }) },
        { 957u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 10701u), (100u, 10698u), (100u, 10697u) }) },
        { 959u, new SRankCondition(SRankConditionType.Killing, new[] { (100u, 10461u), (100u, 10462u), (100u, 10463u) }) },
        { 400u, new SRankCondition(SRankConditionType.Gathering, new[] { (50u, 12536u), (50u, 12634u) }) },
        { 814u, new SRankCondition(SRankConditionType.Gathering, new[] { (50u, 27759u) }) },
        { 146u, new SRankCondition(SRankConditionType.FateTimer, new[] { (3600u, 0u) }) },
        { 398u, new SRankCondition(SRankConditionType.Fate, new[] { (5u, 831u) }) },
        { 1190u, new SRankCondition(SRankConditionType.Fate, new[] { (3u, 1862u) }) },
        { 621u, new SRankCondition(SRankConditionType.Discard, new[] { (50u, 0u) }) },
    };

    internal static SRankCondition? GetCondition() => Zones.TryGetValue(SERVICES.ClientState.TerritoryType, out SRankCondition? condition) ? condition : null;

    internal static bool IsZone() => Zones.ContainsKey(SERVICES.ClientState.TerritoryType);
}