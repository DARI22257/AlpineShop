using AlpineShop.Models;
using System.Linq;

namespace AlpineShop.Page;

public partial class Login : ContentPage
{
    private static bool _inited = false;

    public Login()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_inited) return;
        _inited = true;

        await DB.InitializeAsync();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Register());
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        InfoLabel.Text = "";

        var login = (LoginEntry.Text ?? "").Trim();
        var pass = (PasswordEntry.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
        {
            InfoLabel.Text = "Введите логин и пароль.";
            return;
        }

        await DB.InitializeAsync();

        var user = DB.Users.FirstOrDefault(u =>
            u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
            u.Password == pass);

        if (user == null)
        {
            InfoLabel.Text = "Неверный логин или пароль.";
            return;
        }

        await Navigation.PushAsync(new Catalog(user.IsAdmin, user.Login));
        Navigation.RemovePage(this);
    }
}