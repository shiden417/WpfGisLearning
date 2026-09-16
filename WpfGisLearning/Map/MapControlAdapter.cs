using Mapsui;
using Mapsui.Layers;
using Mapsui.UI.Wpf;

namespace WpfGisLearning.Map;

public sealed class MapControlAdapter : IMapControlAdapter
{
    private readonly MapControl _mapControl;

    public MapControlAdapter(MapControl mapControl)
    {
        _mapControl = mapControl;
    }

    public Mapsui.Map? Map
    {
        get => _mapControl.Map;
        set
        {
            if (value is not null)
                _mapControl.Map = value;
        }
    }

    public double ActualWidth => _mapControl.ActualWidth;
    public double ActualHeight => _mapControl.ActualHeight;

    public void Refresh() => _mapControl.Refresh();

    public MapInfo? GetMapInfo(Mapsui.Manipulations.ScreenPosition position, IEnumerable<ILayer> layers) =>
        _mapControl.GetMapInfo(position, layers);
}
