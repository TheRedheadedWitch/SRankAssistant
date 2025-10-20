using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;

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
    internal static readonly Dictionary<uint, string> MonsterNames = SERVICES.Data.GetExcelSheet<BNpcName>().ToDictionary(e => e.RowId, e => e.Singular.ToString());

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
            if (monster.BaseId == 0 || obj.EntityId == 0) continue;
            currentObjects.Add(obj.EntityId);
            string monsterName = MonsterNames.TryGetValue(monster.NameId, out string? name) ? name : monster.Name.TextValue;
            if (!TrackedNpcs.ContainsKey(obj.EntityId))
                TrackedNpcs[obj.EntityId] = new TrackedNpcData { DataId = monster.BaseId, Name = monsterName, IsDeadAndCounted = false };
            else if (monster.CurrentHp == 0 && !TrackedNpcs[obj.EntityId].IsDeadAndCounted)
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
        List<uint> deadObjects = TrackedNpcs.Keys.Except(currentObjects).ToList();
        foreach (uint deadId in deadObjects)
            TrackedNpcs.Remove(deadId);
    }
}