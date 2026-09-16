using Microsoft.Extensions.DependencyInjection;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Tests;

[TestClass]
public class NavigationServiceTests
{
    [TestMethod]
    public void NavigateToDetail_CreatesDetailViewAndShowsDialog()
    {
        var factory = new FakeNavigationViewFactory();
        var host = new FakeNavigationHost();
        var service = CreateService(host, factory);

        service.NavigateToDetail(42);

        Assert.AreEqual(42, factory.DetailId);
        Assert.AreEqual("detail", host.DialogContent);
        Assert.AreEqual("店舗詳細 - Ramenia", host.DialogTitle);
        Assert.AreEqual(1000, host.DialogWidth);
        Assert.AreEqual(900, host.DialogHeight);
    }

    [TestMethod]
    public void NavigateToDetail_RefreshesShopDataAfterDialogCloses()
    {
        var factory = new FakeNavigationViewFactory();
        var host = new FakeNavigationHost { InvokeClosedCallback = true };
        var service = CreateService(host, factory);

        service.NavigateToDetail(1);

        Assert.AreEqual(1, host.RefreshShopDataCallCount);
    }

    [TestMethod]
    public void NavigateToShopEdit_PassesIdAndUsesEditTitle()
    {
        var factory = new FakeNavigationViewFactory();
        var host = new FakeNavigationHost();
        var service = CreateService(host, factory);

        service.NavigateToShopEdit(7);

        Assert.AreEqual(7, factory.ShopEditId);
        Assert.AreEqual("edit", host.DialogContent);
        Assert.AreEqual("店舗を編集 - Ramenia", host.DialogTitle);
        Assert.AreEqual(1250, host.DialogWidth);
        Assert.AreEqual(850, host.DialogHeight);
    }

    [TestMethod]
    public void NavigateToShopEdit_UsesRegistrationTitle_WhenIdIsNull()
    {
        var factory = new FakeNavigationViewFactory();
        var host = new FakeNavigationHost();
        var service = CreateService(host, factory);

        service.NavigateToShopEdit();

        Assert.IsNull(factory.ShopEditId);
        Assert.AreEqual("店舗を登録 - Ramenia", host.DialogTitle);
    }

    [TestMethod]
    public void NavigateToShopPageFrame_NavigatesToCreatedContent()
    {
        var factory = new FakeNavigationViewFactory();
        var host = new FakeNavigationHost();
        var service = CreateService(host, factory);

        service.NavigateToShopPageFrame();

        Assert.AreEqual("frame", host.MainContent);
    }

    [TestMethod]
    public void NavigateToShopList_NavigatesToCreatedContent()
    {
        var factory = new FakeNavigationViewFactory();
        var host = new FakeNavigationHost();
        var service = CreateService(host, factory);

        service.NavigateToShopList();

        Assert.AreEqual("list", host.MainContent);
    }

    private static NavigationService CreateService(FakeNavigationHost host, FakeNavigationViewFactory factory)
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        return new NavigationService(provider, host, factory);
    }

    private sealed class FakeNavigationHost : INavigationHost
    {
        public object? MainContent { get; private set; }
        public object? DialogContent { get; private set; }
        public string? DialogTitle { get; private set; }
        public double DialogWidth { get; private set; }
        public double DialogHeight { get; private set; }
        public bool InvokeClosedCallback { get; init; }
        public int RefreshShopDataCallCount { get; private set; }

        public void NavigateMainContent(object content) => MainContent = content;

        public void ShowDialog(
            object content,
            string title,
            double width,
            double height,
            double minWidth,
            double minHeight,
            Action? closed = null)
        {
            DialogContent = content;
            DialogTitle = title;
            DialogWidth = width;
            DialogHeight = height;
            if (InvokeClosedCallback)
                closed?.Invoke();
        }

        public void Show(object content, string title)
        {
        }

        public void RefreshShopData() => RefreshShopDataCallCount++;
    }

    private sealed class FakeNavigationViewFactory : INavigationViewFactory
    {
        public int? DetailId { get; private set; }
        public int? ShopEditId { get; private set; }

        public object CreateDetailView(int id)
        {
            DetailId = id;
            return "detail";
        }

        public object CreateShopEditView(int? id)
        {
            ShopEditId = id;
            return "edit";
        }

        public object CreateShopPageFrame() => "frame";

        public object CreateShopListView() => "list";
    }
}
