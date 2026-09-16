using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WpfGisLearning.Models;
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

    public DetailViewModel(int id, IShopService shopService, IPhotoService photoService)
    {
        Id = id;
        _shopService = shopService;
        _photoService = photoService;
        ServiceInstanceId = shopService.GetHashCode().ToString();
        Shop = shopService.GetShops().FirstOrDefault(shop => shop.Id == id);
        IsFavorite = Shop?.IsFavorite ?? false;
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
