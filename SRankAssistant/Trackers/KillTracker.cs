using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace SRankAssistant;

internal struct TrackedNpcData
{
    public uint NameId;
    public string Name;
    public bool IsDeadAndCounted;
}

internal static class KillTracker
{
    private static readonly Dictionary<uint, TrackedNpcData> TrackedNpcs = new();
    internal static readonly Dictionary<uint, string> MonsterNames = SERVICES.Data.GetExcelSheet<BNpcName>().ToDictionary(e => e.RowId, e => e.Singular.ToString());

    internal static void Initialize() => SERVICES.Framework.Update += OnUpdate;
    internal static void Dispose() { SERVICES.Framework.Update -= OnUpdate; TrackedNpcs.Clear(); }

    private static void OnUpdate(IFramework framework)
    {
        SRankCondition? condition = SRankData.GetCondition();
        if (condition == null || condition.Type != SRankConditionType.Killing) return;
        HashSet<uint> currentNameIds = new();
        foreach (IBattleNpc monster in SERVICES.Objects.OfType<IBattleNpc>())
        {
            if (monster.NameId == 0) continue;
            currentNameIds.Add(monster.NameId);
            string name = MonsterNames.TryGetValue(monster.NameId, out string? n) ? n : monster.Name.TextValue;
            if (!TrackedNpcs.ContainsKey(monster.NameId))
                TrackedNpcs[monster.NameId] = new TrackedNpcData { NameId = monster.NameId, Name = name, IsDeadAndCounted = false };
            else if (monster.CurrentHp == 0 && !TrackedNpcs[monster.NameId].IsDeadAndCounted)
            {
                TrackedNpcData npcData = TrackedNpcs[monster.NameId];
                LOG.Debug($"KILLED MONSTER - NameId: {npcData.NameId}, Name: {npcData.Name}, Zone: {SERVICES.ClientState.TerritoryType}");
                foreach ((uint goal, uint targetId) in condition.Targets)
                    if (npcData.NameId == targetId)
                    {
                        Globals.tracker.Increment(targetId);
                        break;
                    }
                TrackedNpcs[monster.NameId] = new TrackedNpcData { NameId = npcData.NameId, Name = npcData.Name, IsDeadAndCounted = true };
            }
        }
        List<uint> missing = TrackedNpcs.Keys.Except(currentNameIds).ToList();
        foreach (uint id in missing) TrackedNpcs.Remove(id);
    }
}
