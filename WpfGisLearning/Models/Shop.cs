using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfGisLearning.Models;

public partial class Shop : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string RamenType { get; set; } = "醤油";
    public string Tags { get; set; } = string.Empty;
    public string RecommendedMenu { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
    public string ClosedDay { get; set; } = string.Empty;
    public double Rating { get; set; }

    [ObservableProperty]
    private bool isFavorite;
}
