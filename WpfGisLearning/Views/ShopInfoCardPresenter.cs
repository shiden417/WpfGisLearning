using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.Views;

public sealed class ShopInfoCardPresenter
{
    private const int InitialOffset = 18;
    private const int FadeDurationMilliseconds = 180;
    private const int SlideDurationMilliseconds = 220;

    private readonly FrameworkElement _resourceOwner;
    private readonly Border _card;
    private readonly TextBlock _name;
    private readonly TextBlock _type;
    private readonly TextBlock _address;
    private readonly TextBlock _price;
    private readonly TextBlock _rating;
    private readonly TextBlock _favorite;
    private readonly Border _businessHoursBadge;
    private readonly TextBlock _businessHoursStatus;

    public ShopInfoCardPresenter(
        FrameworkElement resourceOwner,
        Border card,
        TextBlock name,
        TextBlock type,
        TextBlock address,
        TextBlock price,
        TextBlock rating,
        TextBlock favorite,
        Border businessHoursBadge,
        TextBlock businessHoursStatus)
    {
        _resourceOwner = resourceOwner;
        _card = card;
        _name = name;
        _type = type;
        _address = address;
        _price = price;
        _rating = rating;
        _favorite = favorite;
        _businessHoursBadge = businessHoursBadge;
        _businessHoursStatus = businessHoursStatus;
    }

    public void Show(Shop shop)
    {
        _name.Text = shop.Name;
        _type.Text = shop.RamenType;
        _address.Text = string.IsNullOrWhiteSpace(shop.Address) ? "住所未登録" : shop.Address;
        _price.Text = $"¥{shop.Price:N0}";
        _rating.Text = $"★ {shop.Rating:F1}";
        _favorite.Text = shop.IsFavorite ? "♥ お気に入り" : string.Empty;

        if (!BusinessHoursStatusCalculator.HasOpeningHours(shop.OpeningHours))
        {
            SetBusinessHoursStatus("営業時間未登録", "MutedBrush", "PanelBrush", "BorderBrush");
        }
        else if (BusinessHoursStatusCalculator.IsOpen(shop.OpeningHours, shop.ClosedDay, DateTime.Now))
        {
            SetBusinessHoursStatus("● 営業中", "AccentDarkBrush", "AccentSoftBrush", "AccentBrush");
        }
        else
        {
            SetBusinessHoursStatus("● 営業時間外", "MutedBrush", "PanelBrush", "BorderBrush");
        }

        _card.Visibility = Visibility.Visible;
        Animate();
    }

    public void Hide()
    {
        _card.Visibility = Visibility.Collapsed;
    }

    private void SetBusinessHoursStatus(string text, string foregroundKey, string backgroundKey, string borderKey)
    {
        _businessHoursStatus.Text = text;
        _businessHoursStatus.Foreground = (Brush)_resourceOwner.FindResource(foregroundKey);
        _businessHoursBadge.Background = (Brush)_resourceOwner.FindResource(backgroundKey);
        _businessHoursBadge.BorderBrush = (Brush)_resourceOwner.FindResource(borderKey);
    }

    private void Animate()
    {
        var transform = (TranslateTransform)_card.RenderTransform;
        transform.X = InitialOffset;
        transform.Y = InitialOffset;
        _card.Opacity = 0;

        var storyboard = new Storyboard();
        AddAnimation(storyboard, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(FadeDurationMilliseconds)), _card, UIElement.OpacityProperty);
        AddAnimation(storyboard, new DoubleAnimation(InitialOffset, 0, TimeSpan.FromMilliseconds(SlideDurationMilliseconds)), transform, TranslateTransform.XProperty);
        AddAnimation(storyboard, new DoubleAnimation(InitialOffset, 0, TimeSpan.FromMilliseconds(SlideDurationMilliseconds)), transform, TranslateTransform.YProperty);
        storyboard.Begin();
    }

    private static void AddAnimation(Storyboard storyboard, AnimationTimeline animation, DependencyObject target, DependencyProperty property)
    {
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath(property));
        storyboard.Children.Add(animation);
    }
}
