namespace SLOY.App.Unified.Views.Desktop;

public partial class NetworkMapView : ContentView
{
    public NetworkMapView()
    {
        InitializeComponent();

        TopologyGraph.AddNode("node1", "Ты", 0, 0, 0, Color.FromArgb("#00FFAA"));
        TopologyGraph.AddNode("node2", "Пир 1", 1, 0.5f, 0, Color.FromArgb("#00AAFF"));
        TopologyGraph.AddNode("node3", "Пир 2", -1, -0.5f, 0.5f, Color.FromArgb("#FFAA00"));
        TopologyGraph.AddEdge("node1", "node2");
        TopologyGraph.AddEdge("node1", "node3");
    }
}