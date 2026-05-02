namespace SLOY.Shared.Constants;

public static class Defaults
{
    public static readonly string[] Greetings = new[]
    {
        "Приветствую, Странник.",
        "Добро пожаловать в защищённую сеть.",
        "Канал установлен.",
        "Никто не узнает, что ты здесь.",
        "Тишина — лучший протокол.",
        "Сеть уже рядом.",
        "Всё под контролем.",
        "Начинаем безопасную сессию.",
        "Шёпот громче крика.",
        "Твой узел активирован."
    };

    public static string GetRandomGreeting()
    {
        var random = new Random();
        return Greetings[random.Next(Greetings.Length)];
    }
}