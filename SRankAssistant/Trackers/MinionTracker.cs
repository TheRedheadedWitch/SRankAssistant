using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SRankAssistant.Trackers
{
    internal class MinionTracker
    {
        private static uint _lastSummonedMinionId = 0;
        private static readonly Dictionary<uint, string> MinionNames = new();

        internal static void Initialize()
        {
            ExcelSheet<Companion> companionSheet = SERVICES.Data.GetExcelSheet<Companion>();
            if (companionSheet != null)
                foreach (Companion companion in companionSheet)
                    if (companion.RowId > 0 && !string.IsNullOrEmpty(companion.Singular.ToString()))
                        MinionNames[companion.RowId] = companion.Singular.ToString();
            SERVICES.Framework.Update += OnUpdate;
        }

        internal static void Dispose()
        {
            SERVICES.Framework.Update -= OnUpdate;
            _lastSummonedMinionId = 0;
            MinionNames.Clear();
        }

        internal static uint GetLastSummonedMinionId() => _lastSummonedMinionId;
        internal static bool HasCorrectMinionSummoned(uint targetMinionId) => _lastSummonedMinionId == targetMinionId;
        internal static string GetMinionName(uint minionId) => MinionNames.TryGetValue(minionId, out string? name) ? name : $"Unknown ({minionId})";

        private static void OnUpdate(IFramework framework)
        {
            SRankCondition? condition = SRankData.GetCondition();
            if (condition == null || condition.Type != SRankConditionType.Minion) return;
            Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter? localPlayer = SERVICES.ClientState.LocalPlayer;
            if (localPlayer == null) return;
            RowRef<Companion> currentMinion = localPlayer.CurrentMinion!.Value;
            if (currentMinion.RowId != 0)
            {
                uint minionId = currentMinion.RowId;
                if (_lastSummonedMinionId != minionId)
                {
                    _lastSummonedMinionId = minionId;
                    LOG.Debug($"Player summoned minion - NameId: {minionId}, Name: {GetMinionName(minionId)}");
                }
            }
        }
    }
}