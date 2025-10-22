using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace SRankAssistant;

internal struct TrackedNpcData
{
    public uint EntityId;
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

        // Get the target NameIds we care about
        HashSet<uint> targetNameIds = condition.Targets.Select(t => t.Item2).ToHashSet();

        HashSet<uint> currentEntityIds = new();

        foreach (IBattleNpc monster in SERVICES.Objects.OfType<IBattleNpc>())
        {
            if (monster.NameId == 0) continue;

            // Only track monsters that match our target NameIds
            if (!targetNameIds.Contains(monster.NameId)) continue;

            uint entityId = monster.EntityId;
            currentEntityIds.Add(entityId);
            string name = MonsterNames.TryGetValue(monster.NameId, out string? n) ? n : monster.Name.TextValue;

            if (!TrackedNpcs.ContainsKey(entityId))
            {
                TrackedNpcs[entityId] = new TrackedNpcData { EntityId = entityId, NameId = monster.NameId, Name = name, IsDeadAndCounted = false };
            }
            else if (monster.CurrentHp == 0 && !TrackedNpcs[entityId].IsDeadAndCounted)
            {
                TrackedNpcData npcData = TrackedNpcs[entityId];
                LOG.Debug($"KILLED MONSTER - EntityId: {npcData.EntityId}, NameId: {npcData.NameId}, Name: {npcData.Name}, Zone: {SERVICES.ClientState.TerritoryType}");

                Globals.tracker.Increment(npcData.NameId);

                TrackedNpcs[entityId] = new TrackedNpcData { EntityId = npcData.EntityId, NameId = npcData.NameId, Name = npcData.Name, IsDeadAndCounted = true };
            }
        }

        List<uint> missing = TrackedNpcs.Keys.Except(currentEntityIds).ToList();
        foreach (uint id in missing) TrackedNpcs.Remove(id);
    }
}
