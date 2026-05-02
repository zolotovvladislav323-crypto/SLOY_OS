using System.Text.RegularExpressions;

namespace SLOY.App.Unified.Behaviors;

public class RegexValidationBehavior : Behavior<Entry>
{
    public string Pattern { get; set; } = ".*";
    public string InvalidMessage { get; set; } = "Недопустимое значение";

    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        entry.TextChanged += OnTextChanged;
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        base.OnDetachingFrom(entry);
        entry.TextChanged -= OnTextChanged;
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue;
        if (string.IsNullOrEmpty(text)) return;

        if (!Regex.IsMatch(text, Pattern))
        {
            ((Entry)sender!).Text = e.OldTextValue;
        }
    }
}