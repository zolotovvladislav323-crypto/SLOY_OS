namespace SLOY.UI.Forensics.Visualizers;

public class SpectrumGraph : GraphicsView
{
    private double[] _spectrumData = Array.Empty<double>();
    private double _maxValue = 1;
    private readonly Color _lineColor = Color.FromArgb("#00FFAA");
    private readonly Color _fillColor = Color.FromArgb("#00FFAA").WithAlpha(0.2f);
    private readonly Color _gridColor = Color.FromArgb("#333333");

    public SpectrumGraph()
    {
        Drawable = new SpectrumDrawable(this);
    }

    public void UpdateSpectrum(double[] data)
    {
        _spectrumData = data;
        _maxValue = data.Length > 0 ? data.Max() : 1;
        Invalidate();
    }

    private class SpectrumDrawable : IDrawable
    {
        private readonly SpectrumGraph _owner;

        public SpectrumDrawable(SpectrumGraph owner)
        {
            _owner = owner;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var width = dirtyRect.Width;
            var height = dirtyRect.Height;
            var data = _owner._spectrumData;

            // Сетка
            canvas.StrokeColor = _owner._gridColor;
            canvas.StrokeSize = 0.5f;
            for (int i = 0; i < 10; i++)
            {
                var y = height * i / 10;
                canvas.DrawLine(0, y, width, y);
            }

            if (data.Length == 0) return;

            // Заливка
            var path = new PathF();
            path.MoveTo(0, height);

            var maxVal = _owner._maxValue;
            for (int i = 0; i < data.Length; i++)
            {
                var x = width * i / data.Length;
                var y = height - (float)(data[i] / maxVal * height * 0.9);
                path.LineTo(x, y);
            }

            path.LineTo(width, height);
            path.Close();

            canvas.FillColor = _owner._fillColor;
            canvas.FillPath(path);

            // Линия спектра
            canvas.StrokeColor = _owner._lineColor;
            canvas.StrokeSize = 1.5f;
            canvas.DrawPath(path);
        }
    }
}