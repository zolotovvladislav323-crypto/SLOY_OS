using System.Text.RegularExpressions;

namespace SLOY.Shared.Models;

public class Identity
{
    /// <summary>
    /// Обязательное поле. Только буквы, цифры, подчёркивание, дефис, точка. 2–30 символов.
    /// </summary>
    public string Nickname { get; }

    /// <summary>
    /// Необязательное поле.
    /// </summary>
    public string? FIO { get; set; }

    /// <summary>
    /// Публичный ключ для E2E.
    /// </summary>
    public byte[]? PublicKey { get; set; }

    /// <summary>
    /// Уникальный идентификатор узла.
    /// </summary>
    public string NodeId { get; } = Guid.NewGuid().ToString("N")[..12];

    /// <summary>
    /// Контрольная сумма для быстрой валидации.
    /// </summary>
    public string Checksum => Convert.ToBase64String(
        System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{Nickname}|{NodeId}")
        )
    )[..8];

    public Identity(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("Nickname обязателен.", nameof(nickname));

        if (!Regex.IsMatch(nickname, @"^[\w\-\.]{2,30}$"))
            throw new ArgumentException(
                "Nickname: 2–30 символов, только буквы, цифры, _, -, .",
                nameof(nickname));

        Nickname = nickname.Trim();
    }

    public override string ToString() => $"{Nickname}#{NodeId[..6]}";
}