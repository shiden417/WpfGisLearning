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

    public static ImageStyle CreateDetailShopMarker()
    {
        const string pinSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"36\" height=\"44\" viewBox=\"0 0 36 44\"><path d=\"M18 2C9.716 2 3 8.716 3 17c0 10 15 25 15 25s15-15 15-25C33 8.716 26.284 2 18 2z\" fill=\"#C74B4B\" stroke=\"#C74B4B\" stroke-width=\"1\"/></svg>";

        return new ImageStyle
        {
            Image = new Image { Source = $"svg-content://{pinSvg}" },
            RelativeOffset = new RelativeOffset(0, 0.5),
            SymbolScale = NormalShopScale
        };
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
