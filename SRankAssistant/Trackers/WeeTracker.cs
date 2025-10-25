using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace SRankAssistant.Trackers
{
    internal class WeeTracker
    {
        private static readonly Dictionary<ulong, TrackedNpcData> TrackedMinions = new();

        internal static void Initialize() => SERVICES.Framework.Update += OnUpdate;
        internal static void Dispose() { SERVICES.Framework.Update -= OnUpdate; TrackedMinions.Clear(); }

        internal static uint GetCurrentCount(uint nameId) => (uint)TrackedMinions.Values.Count(npc => npc.NameId == nameId);

        private static void OnUpdate(IFramework framework)
        {
            SRankCondition? condition = SRankData.GetCondition();
            if (condition == null || condition.Type != SRankConditionType.Wee) return;
            HashSet<uint> targetNameIds = condition.Targets.Select(t => t.Item1).ToHashSet();
            HashSet<ulong> currentPlayerIds = new();
            foreach (var obj in SERVICES.Objects)
            {
                if (obj.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) continue;
                if (obj is not IPlayerCharacter player) continue;
                Lumina.Excel.RowRef<Companion>? currentMinion = player.CurrentMinion;
                if (currentMinion == null || !currentMinion.HasValue) continue;
                Lumina.Excel.RowRef<Companion> minionRef = currentMinion.Value;
                if (!minionRef.IsValid || minionRef.RowId == 0) continue;
                uint minionNameId = minionRef.RowId;
                if (!targetNameIds.Contains(minionNameId)) continue;
                ulong playerEntityId = player.EntityId;
                currentPlayerIds.Add(playerEntityId);
                string playerName = player.Name.TextValue;
                if (!TrackedMinions.ContainsKey(playerEntityId))
                {
                    TrackedMinions[playerEntityId] = new TrackedNpcData
                    {
                        EntityId = (uint)playerEntityId,
                        NameId = minionNameId,
                        Name = playerName,
                        IsDeadAndCounted = true
                    };
                    LOG.Debug($"WEE MINION SPOTTED - Player: {playerName}, MinionId: {minionNameId}, Current Count: {GetCurrentCount(minionNameId)}");
                }
            }
            List<ulong> missing = TrackedMinions.Keys.Except(currentPlayerIds).ToList();
            foreach (ulong id in missing)
            {
                LOG.Debug($"Player {TrackedMinions[id].Name} removed (unsummoned or left area)");
                TrackedMinions.Remove(id);
            }
        }
    }
}