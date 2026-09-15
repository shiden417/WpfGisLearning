using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Wpf;
using System.Windows.Controls;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views
{
    public partial class DetailView : UserControl
    {
        private readonly MapControl _mapControl;

        public DetailView(DetailViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            _mapControl = DetailMap;

            InitializeMap(viewModel);
        }

        private void InitializeMap(DetailViewModel viewModel)
        {
            _mapControl.Map?.Layers.Clear();

            _mapControl.Map?.Layers.Add(
                OpenStreetMap.CreateTileLayer());

            if (viewModel.Shop is null)
            {
                return;
            }

            var shop = viewModel.Shop;

            if (double.IsNaN(shop.Latitude) ||
                double.IsNaN(shop.Longitude) ||
                double.IsInfinity(shop.Latitude) ||
                double.IsInfinity(shop.Longitude))
            {
                return;
            }

            if (shop.Latitude < -90 ||
                shop.Latitude > 90 ||
                shop.Longitude < -180 ||
                shop.Longitude > 180)
            {
                return;
            }

            if (shop.Latitude == 0 &&
                shop.Longitude == 0)
            {
                return;
            }

            var point =
                SphericalMercator
                    .FromLonLat(
                        shop.Longitude,
                        shop.Latitude)
                    .ToMPoint();

            var feature =
                new PointFeature(point);

            feature.Styles.Add(
                new SymbolStyle
                {
                    SymbolScale = 0.8,
                    Fill = new Brush(
                        new Mapsui.Styles.Color(
                            180,
                            84,
                            43)),
                    Outline = new Pen(
                        new Mapsui.Styles.Color(
                            255,
                            255,
                            255),
                        2)
                });

            var layer =
                new MemoryLayer
                {
                    Name = "Shop",
                    Features = new[] { feature }
                };

            _mapControl.Map?.Layers.Add(layer);

            _mapControl.Map?.Navigator.CenterOnAndZoomTo(
                point,
                500);
        }

        private void DeleteButton_Click(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            var dlg = new ConfirmDeleteDialog();

            var owner =
                System.Windows.Window.GetWindow(this);

            if (owner != null)
            {
                dlg.Owner = owner;
            }

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                System.Windows.MessageBox.Show(
                    "削除を承認しました（実際の削除は行いません）。",
                    "削除",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "削除をキャンセルしました。",
                    "削除",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
    }
}