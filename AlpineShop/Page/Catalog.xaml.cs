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
        BindingContext = this;
        _login = login;

        HelloLabel.Text = _isAdmin
            ? $"Вы вошли как админ: {_login}"
            : $"Вы вошли как пользователь: {_login}";

        AddButton.IsVisible = _isAdmin;

        ProductsView.ItemsSource = DB.Products;
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Возврат на экран входа
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

    private async void OnProductSelected(object sender, SelectionChangedEventArgs e)
    {
        var product = e.CurrentSelection?.FirstOrDefault() as Product;
        if (product == null) return;

        // чтобы можно было нажать на тот же товар ещё раз
        ProductsView.SelectedItem = null;

        if (!_isAdmin)
            return;

        var action = await DisplayActionSheet(
            "Действия с товаром",
            "Отмена",
            null,
            "Редактировать",
            "Удалить");

        if (action == "Редактировать")
        {
            await Navigation.PushAsync(new Edit(product));
        }
        else if (action == "Удалить")
        {
            var ok = await DisplayAlert("Удаление", $"Удалить товар «{product.Name}»?", "Удалить", "Отмена");
            if (!ok) return;

            // удалить картинку (опционально)
            try
            {
                if (!string.IsNullOrWhiteSpace(product.ImageFile) && File.Exists(product.ImageFile))
                    File.Delete(product.ImageFile);
            }
            catch { /* можно не падать из-за файла */ }

            DB.Products.Remove(product);
            await DB.SaveProductsAsync();
        }
    }
    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (!_isAdmin) return;

        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            var page = new Edit(product);
            page.Disappearing += (s, e) => RefreshProducts();
            await Navigation.PushAsync(page);
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (!_isAdmin) return;

        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            var ok = await DisplayAlert("Удаление", $"Удалить товар «{product.Name}»?", "Удалить", "Отмена");
            if (!ok) return;

            // опционально: удалить файл картинки
            try
            {
                if (!string.IsNullOrWhiteSpace(product.ImageFile) && File.Exists(product.ImageFile))
                    File.Delete(product.ImageFile);
            }
            catch { }

            DB.Products.Remove(product);
            await DB.SaveProductsAsync();
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