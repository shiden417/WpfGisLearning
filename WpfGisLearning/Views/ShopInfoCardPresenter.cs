using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.Views;

/// <summary>
/// MainWindow上の店舗情報カードへShopの内容を反映し、表示アニメーションを担当するPresenterです。
/// ViewModelからWPFコントロールを直接操作するのを避けるためのUI補助クラスです。
/// </summary>
public sealed class ShopInfoCardPresenter
{
    /// <summary>カード表示開始時に設定する縦横方向の移動量です。</summary>
    private const int InitialOffset = 18;

    /// <summary>フェードインに使用するアニメーション時間（ミリ秒）です。</summary>
    private const int FadeDurationMilliseconds = 180;

    /// <summary>スライドインに使用するアニメーション時間（ミリ秒）です。</summary>
    private const int SlideDurationMilliseconds = 220;

    /// <summary>StaticResourceを検索するための基準要素です。</summary>
    private readonly FrameworkElement _resourceOwner;

    /// <summary>店舗情報カード本体です。</summary>
    private readonly Border _card;

    /// <summary>店舗名表示用TextBlockです。</summary>
    private readonly TextBlock _name;

    /// <summary>ラーメン種別表示用TextBlockです。</summary>
    private readonly TextBlock _type;

    /// <summary>住所表示用TextBlockです。</summary>
    private readonly TextBlock _address;

    /// <summary>価格表示用TextBlockです。</summary>
    private readonly TextBlock _price;

    /// <summary>評価表示用TextBlockです。</summary>
    private readonly TextBlock _rating;

    /// <summary>お気に入り表示用TextBlockです。</summary>
    private readonly TextBlock _favorite;

    /// <summary>営業時間バッジのBorderです。</summary>
    private readonly Border _businessHoursBadge;

    /// <summary>営業時間状態の文字表示用TextBlockです。</summary>
    private readonly TextBlock _businessHoursStatus;

    /// <summary>表示対象のWPFコントロールを受け取ってPresenterを構成します。</summary>
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

    /// <summary>
    /// 店舗情報をカードへ反映し、営業時間状態を判定した後に表示アニメーションを開始します。
    /// </summary>
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

    /// <summary>店舗情報カードを非表示にします。</summary>
    public void Hide()
    {
        _card.Visibility = Visibility.Collapsed;
    }

    /// <summary>営業時間の文字色・背景色・枠色をResourceDictionaryから取得して設定します。</summary>
    private void SetBusinessHoursStatus(string text, string foregroundKey, string backgroundKey, string borderKey)
    {
        _businessHoursStatus.Text = text;
        _businessHoursStatus.Foreground = (Brush)_resourceOwner.FindResource(foregroundKey);
        _businessHoursBadge.Background = (Brush)_resourceOwner.FindResource(backgroundKey);
        _businessHoursBadge.BorderBrush = (Brush)_resourceOwner.FindResource(borderKey);
    }

    /// <summary>カードを右下方向からフェード＋スライドさせて表示します。</summary>
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

    /// <summary>StoryboardへAnimationTimelineを追加し、対象DependencyObjectとDependencyPropertyを関連付けます。</summary>
    private static void AddAnimation(Storyboard storyboard, AnimationTimeline animation, DependencyObject target, DependencyProperty property)
    {
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath(property));
        storyboard.Children.Add(animation);
    }
}
