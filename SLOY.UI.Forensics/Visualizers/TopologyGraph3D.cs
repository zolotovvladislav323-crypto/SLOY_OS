namespace SLOY.UI.Forensics.Visualizers;

public class TopologyGraph3D : GraphicsView
{
    private readonly List<TopologyNode> _nodes = new();
    private readonly List<(string from, string to)> _edges = new();
    private float _rotationX;
    private float _rotationY;

    public TopologyGraph3D()
    {
        Drawable = new TopologyDrawable(this);
        this.BackgroundColor = Color.FromArgb("#0D0D0D");
    }

    public void AddNode(string id, string label, float x, float y, float z, Color? color = null)
    {
        _nodes.Add(new TopologyNode
        {
            Id = id,
            Label = label,
            X = x, Y = y, Z = z,
            Color = color ?? Color.FromArgb("#00FFAA")
        });
        Invalidate();
    }

    public void AddEdge(string fromId, string toId)
    {
        _edges.Add((fromId, toId));
        Invalidate();
    }

    public void Clear()
    {
        _nodes.Clear();
        _edges.Clear();
        Invalidate();
    }

    public void Rotate(float deltaX, float deltaY)
    {
        _rotationX += deltaX;
        _rotationY += deltaY;
        Invalidate();
    }

    private class TopologyDrawable : IDrawable
    {
        private readonly TopologyGraph3D _owner;

        public TopologyDrawable(TopologyGraph3D owner)
        {
            _owner = owner;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var centerX = dirtyRect.Width / 2;
            var centerY = dirtyRect.Height / 2;
            var scale = Math.Min(centerX, centerY) * 0.4f;

            var projected = new Dictionary<string, (float x, float y)>();

            foreach (var node in _owner._nodes)
            {
                var (px, py) = Project(node.X, node.Y, node.Z, centerX, centerY, scale);
                projected[node.Id] = (px, py);

                canvas.FillColor = node.Color;
                canvas.FillCircle(px, py, 8);

                canvas.FontSize = 9;
                canvas.FontColor = Color.FromArgb("#AAAAAA");
                canvas.DrawString(node.Label, px + 12, py - 4, HorizontalAlignment.Left);
            }

            canvas.StrokeColor = Color.FromArgb("#333333");
            canvas.StrokeSize = 1;

            foreach (var (from, to) in _owner._edges)
            {
                if (projected.TryGetValue(from, out var fromP) && projected.TryGetValue(to, out var toP))
                {
                    canvas.DrawLine(fromP.x, fromP.y, toP.x, toP.y);
                }
            }
        }

        private (float x, float y) Project(float x, float y, float z, float centerX, float centerY, float scale)
        {
            var rx = _owner._rotationX * Math.PI / 180;
            var ry = _owner._rotationY * Math.PI / 180;

            var cosRx = MathF.Cos((float)rx);
            var sinRx = MathF.Sin((float)rx);
            var cosRy = MathF.Cos((float)ry);
            var sinRy = MathF.Sin((float)ry);

            var y1 = y * cosRx - z * sinRx;
            var z1 = y * sinRx + z * cosRx;
            var x1 = x * cosRy + z1 * sinRy;
            var z2 = -x * sinRy + z1 * cosRy;

            var perspective = 1 / (1 + z2 * 0.001f);

            return (
                centerX + x1 * scale * perspective,
                centerY - y1 * scale * perspective
            );
        }
    }

    private class TopologyNode
    {
        public string Id { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public float X { get; init; }
        public float Y { get; init; }
        public float Z { get; init; }
        public Color Color { get; init; } = Color.FromArgb("#00FFAA");
    }
}