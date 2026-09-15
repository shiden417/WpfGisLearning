using Mapsui.Styles;

namespace WpfGisLearning.Services;

public static class MapMarkerStyleFactory
{
    private const string NormalShopColor = "#C74B4B";
    private const string SelectedShopColor = "#C56B4D";
    private const string CurrentLocationColor = "#4A90E2";
    private const double NormalShopScale = 1.15;
    private const double SelectedShopScale = 1.4;
    private const double CurrentLocationScale = 0.65;

    public static ImageStyle CreateShopMarker(bool selected)
    {
        return ImageStyles.CreatePinStyle(
            Color.FromString(selected ? SelectedShopColor : NormalShopColor),
            Color.White,
            selected ? SelectedShopScale : NormalShopScale);
    }

    public static SymbolStyle CreateCurrentLocationMarker()
    {
        return new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = CurrentLocationScale,
            Fill = new Brush(Color.FromString(CurrentLocationColor)),
            Outline = new Pen
            {
                Color = Color.White,
                Width = 3
            }
        };
    }
}
