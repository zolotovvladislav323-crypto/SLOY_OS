using SLOY.Shared.Enums;
using SLOY.Shared.Models;

namespace SLOY.Shared.Validators;

public static class PacketValidator
{
    public static bool IsValid(Packet packet)
    {
        return !string.IsNullOrWhiteSpace(packet.SenderId) &&
               !string.IsNullOrWhiteSpace(packet.ReceiverId) &&
               packet.Size <= 65535 &&
               Enum.IsDefined(packet.Protocol) &&
               Enum.IsDefined(packet.Priority);
    }
}