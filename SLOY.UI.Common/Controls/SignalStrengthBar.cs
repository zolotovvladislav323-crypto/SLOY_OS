namespace SLOY.UI.Common.Controls;

public class SignalStrengthBar : GraphicsView
{
    private double _strength;

    public static readonly BindableProperty StrengthProperty = BindableProperty.Create(
        nameof(Strength), typeof(double), typeof(SignalStrengthBar), 0.5,
        propertyChanged: OnStrengthChanged);

    public double Strength
    {
        get => (double)GetValue(StrengthProperty);
        set => SetValue(StrengthProperty, value);
    }

    public SignalStrengthBar()
    {
        Drawable = new SignalDrawable();
        HeightRequest = 20;
        WidthRequest = 60;
    }

    private static void OnStrengthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SignalStrengthBar bar)
        {
            bar.Invalidate();
        }
    }

    private class SignalDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var strength = 0.5;
            var barWidth = 8f;
            var spacing = 4f;
            var totalBars = 5;

            for (int i = 0; i < totalBars; i++)
            {
                var barHeight = (dirtyRect.Height - 4) * (i + 1) / totalBars;
                var x = i * (barWidth + spacing);
                var y = dirtyRect.Height - barHeight - 2;
                var opacity = i < strength * totalBars ? 1f : 0.2f;

                canvas.FillColor = strength > 0.3 ? Color.FromArgb("#00FFAA").WithAlpha(opacity) : Color.FromArgb("#FF4444").WithAlpha(opacity);
                canvas.FillRoundedRectangle(x, y, barWidth, barHeight, 2);
            }
        }
    }
}