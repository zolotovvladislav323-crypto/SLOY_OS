using SLOY.Shared.Enums;

namespace SLOY.Shared.Models;

public class Packet
{
    /// <summary>Уникальный идентификатор пакета</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N")[..16];

    /// <summary>Отправитель</summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>Получатель (может быть broadcast)</summary>
    public string ReceiverId { get; set; } = string.Empty;

    /// <summary>Тип протокола</summary>
    public ProtocolType Protocol { get; set; } = ProtocolType.TextMessage;

    /// <summary>Приоритет</summary>
    public PacketPriority Priority { get; set; } = PacketPriority.Normal;

    /// <summary>Полезная нагрузка (зашифрованные данные)</summary>
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    /// <summary>Порядковый номер для сборки фрагментов</summary>
    public int SequenceNumber { get; set; }

    /// <summary>Метка времени создания (Unix миллисекунды)</summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>Размер в байтах</summary>
    public int Size => Payload.Length;

    /// <summary>Быстрый хеш для проверки целостности</summary>
    public string Hash => Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(Payload)
    )[..8];

    public override string ToString() => $"[{Id}] {SenderId}->{ReceiverId} | {Protocol} | {Priority}";
}