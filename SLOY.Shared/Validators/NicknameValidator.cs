using System.Text.RegularExpressions;

namespace SLOY.Shared.Validators;

public static class NicknameValidator
{
    private static readonly Regex ValidPattern = new(@"^[\w\-\.]{2,30}$", RegexOptions.Compiled);

    public static bool IsValid(string? nickname)
    {
        return !string.IsNullOrWhiteSpace(nickname) && ValidPattern.IsMatch(nickname.Trim());
    }

    public static string? GetError(string? nickname)
    {
        var text = nickname?.Trim();
        if (string.IsNullOrEmpty(text))
            return "Nickname обязателен.";

        if (text.Length < 2)
            return "Минимум 2 символа.";

        if (text.Length > 30)
            return "Максимум 30 символов.";

        if (!ValidPattern.IsMatch(text))
            return "Только буквы, цифры, _, -, .";

        return null;
    }

    public static string Sanitize(string nickname)
    {
        return Regex.Replace(nickname.Trim(), @"[^\w\-\.]", "");
    }
}