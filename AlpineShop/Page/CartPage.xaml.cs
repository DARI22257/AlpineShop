
using AlpineShop.Models;

namespace AlpineShop.Page;

public partial class CartPage : ContentPage
{
    private readonly string _login;

    public CartPage(string login)
    {
        InitializeComponent();
        _login = login;

        Refresh();
    }

    private void Refresh()
    {
        var cart = DB.GetCart(_login);
        CartView.ItemsSource = null;
        CartView.ItemsSource = cart;

        var total = cart.Sum(x => x.Price * x.Quantity);
        TotalLabel.Text = $"Итого: {total} ₽";
    }

    private async void OnPlusClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Cart item)
        {
            await DB.ChangeQuantityAsync(_login, item, +1);
            Refresh();
        }
    }

    private async void OnMinusClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Cart item)
        {
            await DB.ChangeQuantityAsync(_login, item, -1);
            Refresh();
        }
    }

    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Cart item)
        {
            await DB.RemoveFromCartAsync(_login, item);
            Refresh();
        }
    }

    private async void OnCheckoutClicked(object sender, EventArgs e)
    {
        var cart = DB.GetCart(_login);
        if (cart.Count == 0)
        {
            await DisplayAlert("Корзина", "Корзина пустая.", "OK");
            return;
        }

        await DB.ClearCartAsync(_login);
        Refresh();
        await DisplayAlert("Заказ", "Заказ оформлен (для учебного проекта корзина очищена).", "OK");
    }
}