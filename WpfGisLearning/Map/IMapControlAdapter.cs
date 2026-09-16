using Mapsui;
using Mapsui.Layers;
using Mapsui.UI.Wpf;

namespace WpfGisLearning.Map;

public interface IMapControlAdapter
{
    Mapsui.Map? Map { get; set; }
    double ActualWidth { get; }
    double ActualHeight { get; }
    void Refresh();
    MapInfo? GetMapInfo(Mapsui.Manipulations.ScreenPosition position, IEnumerable<ILayer> layers);
}
