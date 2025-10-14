using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;


namespace SRankAssistant;

public static class DiscardTracker
{
    public static void Initialize() => SERVICES.Chat.ChatMessage += OnChatMessage;
    public static void Dispose() => SERVICES.Chat.ChatMessage -= OnChatMessage;

    private static void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        SRankCondition? condition = SRankData.GetCondition();
        if (type != XivChatType.SystemMessage || condition == null || condition.Type != SRankConditionType.Discard) return;
        string mess = message.ToString().ToLower();
        if (mess.StartsWith("you discard") || mess.StartsWith("you throw away"))
            Globals.tracker.IncrementDiscard();
    }
}