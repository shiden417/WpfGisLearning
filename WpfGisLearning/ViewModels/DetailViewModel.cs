using CommunityToolkit.Mvvm.ComponentModel;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public class DetailViewModel : ObservableObject
{
    public int Id { get; }

    public Guid ServiceInstanceId { get; }

    public Shop? Shop { get; }

    public DetailViewModel(
        int id,
        LifetimeTestService lifetimeTestService,
        IShopService shopService)
    {
        Id = id;

        // DI 管理のサービスはコンストラクタ注入される
        ServiceInstanceId = lifetimeTestService.Id;

        // 店舗IDから店舗情報を取得する
        Shop = shopService
            .GetShops()
            .FirstOrDefault(shop => shop.Id == id);
    }
}