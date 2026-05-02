namespace SLOY.UI.Common.Controls;

public class RadarView : GraphicsView
{
    private readonly List<RadarNode> _nodes = new();
    private float _rotationAngle;
    private bool _isScanning;

    public int NodeCount => _nodes.Count;

    public RadarView()
    {
        Drawable = new RadarDrawable(_nodes);
        StartScan();
    }

    public void AddNode(string id, float distance, float angle)
    {
        _nodes.Add(new RadarNode { Id = id, Distance = Math.Clamp(distance, 0, 1), Angle = angle });
        Invalidate();
    }

    public void RemoveNode(string id)
    {
        _nodes.RemoveAll(n => n.Id == id);
        Invalidate();
    }

    public void ClearNodes()
    {
        _nodes.Clear();
        Invalidate();
    }

    public async void StartScan()
    {
        _isScanning = true;
        while (_isScanning)
        {
            _rotationAngle = (_rotationAngle + 3f) % 360f;
            Invalidate();
            await Task.Delay(30);
        }
    }

    public void StopScan()
    {
        _isScanning = false;
    }

    private class RadarDrawable : IDrawable
    {
        private readonly List<RadarNode> _nodes;

        public RadarDrawable(List<RadarNode> nodes)
        {
            _nodes = nodes;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var centerX = dirtyRect.Width / 2;
            var centerY = dirtyRect.Height / 2;
            var radius = Math.Min(centerX, centerY) - 10;

            canvas.StrokeColor = Color.FromArgb("#00FFAA");
            canvas.StrokeSize = 1;

            for (int i = 1; i <= 3; i++)
            {
                canvas.DrawCircle(centerX, centerY, radius * i / 3);
            }

            canvas.DrawLine(0, centerY, dirtyRect.Width, centerY);
            canvas.DrawLine(centerX, 0, centerX, dirtyRect.Height);

            foreach (var node in _nodes)
            {
                var nx = centerX + MathF.Cos(node.Angle * MathF.PI / 180) * node.Distance * radius;
                var ny = centerY + MathF.Sin(node.Angle * MathF.PI / 180) * node.Distance * radius;

                canvas.FillColor = node.Distance < 0.4f ? Color.FromArgb("#00FFAA") : Color.FromArgb("#FFAA00");
                canvas.FillCircle(nx, ny, 6);

                canvas.FontSize = 9;
                canvas.FontColor = Color.FromArgb("#AAAAAA");
                canvas.DrawString(node.Id, nx + 10, ny - 5, HorizontalAlignment.Left);
            }
        }
    }

    private class RadarNode
    {
        public string Id { get; set; } = string.Empty;
        public float Distance { get; set; }
        public float Angle { get; set; }
    }
}