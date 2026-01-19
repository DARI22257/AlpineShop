using AlpineShop.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlpineShop.Page;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
	}
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Register());
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        InfoLabel.Text = "";

        var login = (LoginEntry.Text ?? "").Trim();
        var pass = PasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
        {
            InfoLabel.Text = "Введите логин и пароль.";
            return;
        }

        var user = DB.Users.FirstOrDefault(u =>
            u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
            u.Password == pass);

        if (user == null)
        {
            InfoLabel.Text = "Неверный логин или пароль.";
            return;
        }

        // Переход в каталог, передаём роль
        await Navigation.PushAsync(new Catalog(user.IsAdmin, user.Login));

        // чтобы нельзя было кнопкой "назад" вернуться на вход (опционально):
        Navigation.RemovePage(this);
    }
}