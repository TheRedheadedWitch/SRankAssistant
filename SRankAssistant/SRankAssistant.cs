using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SRankAssistant;

public sealed class SRankAssistant : IDalamudPlugin
{
    public readonly WindowSystem WindowSystem = new("S Rank Assistant");
    internal static DisplayWindow DisplayWindow { get; set; } = new();

    public SRankAssistant(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<SERVICES>();
        ExcelSheet<Item> itemSheet = SERVICES.Data.GetExcelSheet<Item>();
        if (itemSheet != null)
            foreach (Item item in itemSheet)
                if (!Globals.AllItems.ContainsKey(item.RowId))
                    Globals.AllItems[item.RowId] = item.Name.ToString();
        ExcelSheet<Fate> fateSheet = SERVICES.Data.GetExcelSheet<Fate>();
        if (fateSheet != null)
            foreach (Fate fate in fateSheet)
                if (!Globals.AllFates.ContainsKey(fate.RowId))
                    Globals.AllFates[fate.RowId] = fate.Name.ToString();
        WindowSystem.AddWindow(DisplayWindow);
        SERVICES.Interface.UiBuilder.Draw += SRankAssistDrawUI;
        SERVICES.Interface.UiBuilder.OpenMainUi += SRankAssistUI;
        SERVICES.ClientState.TerritoryChanged += MoveLocations;
        DisplayWindow.IsOpen = HuntLocations.IsHuntLocation();
        FateTracker.Initialize();
        KillTracker.Initialize();
        GatheringTracker.Initialize();
        DiscardTracker.Initialize();
        Globals.tracker = Tracker.Load();
        SERVICES.CommandManager.AddHandler("/sranktracker", new CommandInfo(OnSRankTracker) { HelpMessage = "/sranktracker on|off|true|false|yes|no {Enables or Disables the plugin}" });
    }

    public void Dispose()
    {
        SERVICES.CommandManager.RemoveHandler("/sranktracker");
        SERVICES.ClientState.TerritoryChanged -= MoveLocations;
        SERVICES.Interface.UiBuilder.OpenMainUi -= SRankAssistUI;
        SERVICES.Interface.UiBuilder.Draw -= SRankAssistDrawUI;
        DisplayWindow.Dispose();
        FateTracker.Dispose();
        KillTracker.Dispose();
        GatheringTracker.Dispose();
        DiscardTracker.Dispose();
        WindowSystem.RemoveAllWindows();
    }

    private void OnSRankTracker(string command, string args)
    {
        string arg = args.ToLowerInvariant();
        if (arg is "true" or "on" or "yes")
        {
            Globals.IsRunning = true;
            if (HuntLocations.IsHuntLocation())
                DisplayWindow.IsOpen = true;
        }
        else if (arg is "false" or "off" or "no")
        {
            Globals.IsRunning = false;
            DisplayWindow.IsOpen = false;
        }
    }
    private void SRankAssistDrawUI() => WindowSystem.Draw();
    private void SRankAssistUI() => DisplayWindow.Toggle();
    private void MoveLocations(ushort newZone)
    {
        if (!Globals.IsRunning)
        {
            DisplayWindow.IsOpen = false;
            return;
        }
        bool IsHuntZone = HuntLocations.IsHuntLocation();
        string newWorldName = IsHuntZone ? Globals.CurrentWorld : string.Empty;
        DisplayWindow.IsOpen = IsHuntZone;
        if (IsHuntZone && string.IsNullOrEmpty(SERVICES.ClientState.LocalPlayer?.CurrentWorld.Value.Name.ToString()) && Globals.CurrentWorld.Equals(newWorldName, StringComparison.OrdinalIgnoreCase))
            return;
        if (IsHuntZone && !string.IsNullOrEmpty(newWorldName))
        {
            if (!newWorldName.Equals(Globals.CurrentWorld, StringComparison.OrdinalIgnoreCase))
                LOG.Debug("[Faloop Log] World change detected OR entering hunt zone. Connecting to " + newWorldName + ".");
            else
                LOG.Debug("[Faloop Log] Entered a hunt zone. Attempting connection to " + newWorldName + ".");
            Globals.CurrentWorld = newWorldName;
        }
        else if (!IsHuntZone)
            Globals.CurrentWorld = string.Empty;
    }
}