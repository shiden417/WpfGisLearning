using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.ViewModels;

public partial class DetailViewModel : ObservableObject
{
    private readonly IShopService _shopService;
    private readonly IPhotoService _photoService;

    public int Id { get; }
    public string ServiceInstanceId { get; }
    public Shop? Shop { get; private set; }
    public ObservableCollection<string> PhotoPaths { get; } = new();

    [ObservableProperty]
    private bool isFavorite;

    public bool HasOpeningHours => BusinessHoursStatusCalculator.HasOpeningHours(Shop?.OpeningHours);

    public bool IsCurrentlyOpen => HasOpeningHours && Shop is not null &&
        BusinessHoursStatusCalculator.IsOpen(Shop.OpeningHours, Shop.ClosedDay, DateTime.Now);

    public string BusinessHoursStatus => IsCurrentlyOpen ? "営業中" : "営業時間外";

    public DetailViewModel(int id, IShopService shopService, IPhotoService photoService)
    {
        Id = id;
        _shopService = shopService;
        _photoService = photoService;
        ServiceInstanceId = shopService.GetHashCode().ToString();
        Reload();
    }

    public void Reload()
    {
        Shop = _shopService.GetShops().FirstOrDefault(shop => shop.Id == Id);
        IsFavorite = Shop?.IsFavorite ?? false;
        OnPropertyChanged(nameof(Shop));
        OnPropertyChanged(nameof(HasOpeningHours));
        OnPropertyChanged(nameof(IsCurrentlyOpen));
        OnPropertyChanged(nameof(BusinessHoursStatus));
        LoadPhotoPaths();
    }

    private void LoadPhotoPaths()
    {
        PhotoPaths.Clear();
        if (Shop is null) return;
        foreach (var photo in Shop.Photos.OrderByDescending(x => x.IsMain).ThenBy(x => x.SortOrder))
        {
            var path = _photoService.GetPhotoPath(Shop, photo);
            if (File.Exists(path)) PhotoPaths.Add(path);
        }
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Shop is null) return;
        _shopService.ToggleFavorite(Shop.Id);
        IsFavorite = Shop.IsFavorite;
    }
}
