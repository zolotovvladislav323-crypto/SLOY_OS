using SLOY.Shared.Models;

namespace SLOY.Core.Factory;

public static class MessageFactory
{
    public static Message Create(string senderNickname, string senderNodeId, string content) => new()
    {
        SenderNickname = senderNickname,
        SenderNodeId = senderNodeId,
        Content = content
    };

    public static Message CreateSystem(string content) => new()
    {
        SenderNickname = "SLOY",
        SenderNodeId = "SYSTEM",
        Content = content
    };
}