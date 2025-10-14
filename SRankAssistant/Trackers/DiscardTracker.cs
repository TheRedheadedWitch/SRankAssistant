using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;


namespace SRankAssistant;

public static class DiscardTracker
{
    public static void Initialize() => SERVICES.Chat.ChatMessage += OnChatMessage;
    public static void Dispose() => SERVICES.Chat.ChatMessage -= OnChatMessage;

    private static void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (type == XivChatType.SystemMessage)
            if (message.TextValue.StartsWith("You discard"))
                LOG.Info($"[LOG] Item Discarded: {message.TextValue}");
    }
}