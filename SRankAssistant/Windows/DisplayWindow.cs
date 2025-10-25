using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using SRankAssistant.Trackers;
using System.Globalization;
using System.Numerics;

namespace SRankAssistant;

internal class DisplayWindow : Window, IDisposable
{
    public DisplayWindow() : base("S Rank Assistant", ImGuiWindowFlags.NoCollapse) => TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("Support Redheaded Witch on Ko-fi"), Icon = FontAwesomeIcon.Heart, IconOffset = new Vector2(1, 1), Click = _ => Util.OpenLink("https://ko-fi.com/theredheadedwitch") });

    public override void Draw()
    {
        uint zone = SERVICES.ClientState.TerritoryType;
        SRankCondition? condition = SRankData.GetCondition();
        string displayName = HuntLocations.GetSRankName();
        ImGui.SetWindowFontScale(2);
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(displayName).X) * 0.5f); ImGui.Text(displayName);
        ImGui.SetWindowFontScale(1);
        ImGui.Separator();
        if (condition != null)
        {
            ImGui.Text($"Type: {(condition.Type == SRankConditionType.Wee ? SRankConditionType.Minion.ToString() : condition.Type)}");
            switch (condition.Type)
            {
                case SRankConditionType.Killing:
                case SRankConditionType.Gathering:
                    foreach ((uint goal, uint target) in condition.Targets)
                    {
                        uint current = Globals.tracker.GetCount(target);
                        string targetName = condition.Type == SRankConditionType.Killing ? KillTracker.MonsterNames.TryGetValue(target, out string? mobName) ? ToTitleCase(mobName) : $"Unknown ({target})" : Globals.AllItems.TryGetValue(target, out string? itemName2) ? itemName2 : $"Unknown ({target})";
                        ImGui.Text($"{targetName}: {current} / {goal}");
                    }
                    if (ImGui.Button("Reset Zone Progress"))
                        Globals.tracker.ResetZone();
                    break;

                case SRankConditionType.Fate:
                    if (condition.Targets.Count == 0) break;
                    foreach ((uint goal, uint target) in condition.Targets)
                    {
                        uint current = Globals.tracker.GetCount(target);
                        string fateName = Globals.AllFates.TryGetValue(target, out string? fatename) ? fatename : $"Unknown FATE ({target})";
                        FateTracker.FateStatus? status = FateTracker.GetStatusForTarget((ushort)target);
                        ImGui.Text($"{fateName}: {current} / {goal}");
                    }
                    if (ImGui.Button("Reset FATE Progress"))
                        Globals.tracker.ResetFates();
                    break;

                case SRankConditionType.FateTimer:
                    TimeSpan? elapsed = FateFailureTracker.TimeSinceFailureOrEntry();
                    if (elapsed == null)
                        ImGui.Text("Timer starting...");
                    else
                    {
                        TimeSpan remaining = TimeSpan.FromHours(1) - elapsed.Value;
                        if (remaining.TotalMilliseconds <= 0)
                            ImGui.Text("Timer complete (1 hour passed)");
                        else
                        {
                            string text = remaining.Minutes > 0 ? $"{remaining.Minutes}:{remaining.Seconds:D2}.{remaining.Milliseconds / 10:D2} remaining" : $"{remaining.Seconds}.{remaining.Milliseconds / 10:D2} remaining";
                            ImGui.Text(text);
                        }
                    }
                    break;

                case SRankConditionType.Discard:
                    if (condition.Targets.Count == 0) break;
                    (uint goal, uint target) discardCondition = condition.Targets.First();
                    uint currentDiscard = Globals.tracker.GetDiscardCount();
                    string itemName = Globals.AllItems.TryGetValue(discardCondition.target, out string? name) ? name : $"Unknown Item ({discardCondition.target})";
                    ImGui.Text($"Discard {itemName}: {currentDiscard} / {discardCondition.goal}");
                    if (ImGui.Button("Reset Discard Progress"))
                        Globals.tracker.ResetDiscards();
                    break;

                case SRankConditionType.Wee:
                    if (condition.Targets.Count == 0) break;
                    foreach ((uint nameId, uint goal) in condition.Targets)
                    {
                        uint current = WeeTracker.GetCurrentCount(nameId); // Get real-time count
                        ImGui.Text($"Wee Ea minions spotted: {current} / {goal}");
                    }
                    break;

                case SRankConditionType.Minion:
                    if (condition.Targets.Count == 0) break;
                    foreach ((uint count, uint minionNameId) in condition.Targets)
                    {
                        bool hasCorrectMinion = MinionTracker.HasCorrectMinionSummoned(minionNameId);
                        uint lastSummoned = MinionTracker.GetLastSummonedMinionId();

                        string targetMinionName = MinionTracker.GetMinionName(minionNameId);

                        if (lastSummoned > 0)
                        {
                            string currentMinionName = MinionTracker.GetMinionName(lastSummoned);

                            if (hasCorrectMinion)
                                ImGui.TextColored(new Vector4(0, 1, 0, 1), $"Minion Out: {ToTitleCase(currentMinionName)}");
                            else
                                ImGui.Text($"Minion Out: {currentMinionName}");

                            if (!hasCorrectMinion)
                                ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), $"Need: {ToTitleCase(targetMinionName)}");
                        }
                        else
                        {
                            ImGui.Text($"Minion Out: ");
                            ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), $"Need: {ToTitleCase(targetMinionName)}");
                        }
                    }
                    break;
            }
            ImGui.Separator();
        }
        ImGui.TextWrapped($"{HuntLocations.GetCondition()}");
    }

    internal static string ToTitleCase(string name) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLowerInvariant());
    public void Dispose() { }
}