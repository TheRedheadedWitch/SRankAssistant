using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;


namespace SRankAssistant;

internal struct TrackedNpcData
{
    public uint DataId;
    public string Name;
    public bool IsDeadAndCounted;
}

internal static class KillTracker
{
    private static readonly Dictionary<uint, TrackedNpcData> TrackedNpcs = new();
    internal static readonly Dictionary<uint, string> MonsterNames = new()
    {
        { 131u, "Earth Sprite" },
        { 6675u, "Yumemi" },
        { 6674u, "Naked Yumemi" },
        { 13529u, "Asvattha" },
        { 13526u, "Pisaca" },
        { 13524u, "Vajralangula" },
        { 10276u, "Cracked Ronkan Doll" },
        { 10277u, "Cracked Ronkan Thorn" },
        { 10280u, "Cracked Ronkan Vessel" },
        { 13367u, "Thinkers" },
        { 13365u, "Wanderers" },
        { 13366u, "Weepers" },
        { 4141u, "Allagan Chimera" },
        { 4014u, "Lesser Hydra" },
        { 4080u, "Meracydian Vouivre" },
        { 7435u, "Leshy" },
        { 6654u, "Diakka" },
    };

    internal static void Initialize() => SERVICES.Framework.Update += OnUpdate;
    internal static void Dispose() { SERVICES.Framework.Update -= OnUpdate; TrackedNpcs.Clear(); }

    private static void OnUpdate(IFramework framework)
    {
        SRankCondition? condition = SRankData.GetCondition();
        if (condition == null || condition.Type != SRankConditionType.Killing) return;
        HashSet<uint> currentObjects = new();
        foreach (IGameObject obj in SERVICES.Objects)
        {
            if (obj.ObjectKind != ObjectKind.BattleNpc && obj.ObjectKind != ObjectKind.EventNpc) continue;
            if (obj is not IBattleChara monster) continue;
            if (monster.BaseId != 0 && obj.EntityId != 0)
            {
                currentObjects.Add(obj.EntityId);
                if (!TrackedNpcs.ContainsKey(obj.EntityId))
                    TrackedNpcs[obj.EntityId] = new TrackedNpcData { DataId = monster.BaseId, Name = monster.Name.TextValue, IsDeadAndCounted = false };
                else
                    if (monster.CurrentHp == 0 && !TrackedNpcs[obj.EntityId].IsDeadAndCounted)
                    {
                        TrackedNpcData npcData = TrackedNpcs[obj.EntityId];
                        LOG.Debug($"KILLED MONSTER - ID: {npcData.DataId}, Name: {npcData.Name}, Zone: {SERVICES.ClientState.TerritoryType}");
                        foreach ((uint goal, uint targetId) in condition.Targets)
                            if (npcData.DataId == targetId)
                            {
                                Globals.tracker.Increment(targetId);
                                break;
                            }
                        TrackedNpcs[obj.EntityId] = new TrackedNpcData { DataId = npcData.DataId, Name = npcData.Name, IsDeadAndCounted = true };
                    }
            }
        }
        List<uint> deadObjects = TrackedNpcs.Keys.Except(currentObjects).ToList();
        foreach (uint deadId in deadObjects)
            TrackedNpcs.Remove(deadId);
    }
}