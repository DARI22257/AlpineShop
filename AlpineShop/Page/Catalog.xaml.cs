using AlpineShop.Models;

namespace AlpineShop.Page;

public partial class Catalog : ContentPage
{
    private readonly bool _isAdmin;
    private readonly string _login;

    public bool IsAdmin => _isAdmin;
    public bool IsNotAdmin => !_isAdmin;

    public Catalog(bool isAdmin, string login)
    {
        InitializeComponent();

        _isAdmin = isAdmin;
        _login = login;

        BindingContext = this;

        HelloLabel.Text = _isAdmin
            ? $"Вы вошли как админ: {_login}"
            : $"Вы вошли как пользователь: {_login}";

        ProductsView.ItemsSource = DB.Products;
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage =
            new NavigationPage(new Login());
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (!_isAdmin)
        {
            await DisplayAlert("Доступ запрещён", "Добавлять товары может только админ.", "OK");
            return;
        }

        await Navigation.PushAsync(new AddProduct());
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (!_isAdmin) return;

        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            var page = new Edit(product);
            page.Disappearing += (s, e2) => RefreshProducts();
            await Navigation.PushAsync(page);
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (!_isAdmin) return;

        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            var ok = await DisplayAlert(
                "Удаление",
                $"Удалить товар «{product.Name}»?",
                "Удалить",
                "Отмена");

            if (!ok) return;

            var deleted = await DB.DeleteProductAsync(product);

            if (!deleted)
            {
                await DisplayAlert(
                    "Удаление запрещено",
                    "Нельзя удалить товар, потому что он находится в корзине с корзиной.",
                    "OK");
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(product.ImageFile) && File.Exists(product.ImageFile))
                    File.Delete(product.ImageFile);
            }
            catch { }

            RefreshProducts();
        }
    }

    private async void OnCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(_login));
    }

    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            await DB.AddToCartAsync(_login, product);
            await DisplayAlert("Корзина", $"Добавлено: {product.Name}", "OK");
        }
    }

    public void RefreshProducts()
    {
        ProductsView.ItemsSource = null;
        ProductsView.ItemsSource = DB.Products;
    }
}