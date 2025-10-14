namespace SRankAssistant;

internal class Globals
{
    internal static Dictionary<uint, string> AllItems = new();
    internal static Dictionary<uint, string> AllMonsters = new();
    internal static Dictionary<uint, string> AllFates = new();
    internal static Tracker tracker = Tracker.Load();
    internal static string CurrentWorld = string.Empty;
    internal static bool IsRunning = true;
}