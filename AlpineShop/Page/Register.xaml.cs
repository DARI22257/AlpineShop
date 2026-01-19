using AlpineShop.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlpineShop.Page;

public partial class Register : ContentPage
{
    public Register()
    {
        InitializeComponent();
    }
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        InfoLabel.Text = "";

        var login = (LoginEntry.Text ?? "").Trim();
        var p1 = PasswordEntry.Text ?? "";
        var p2 = Password2Entry.Text ?? "";

        if (login.Length < 3)
        {
            InfoLabel.Text = "Логин должен быть минимум 3 символа.";
            return;
        }

        if (p1.Length < 4)
        {
            InfoLabel.Text = "Пароль должен быть минимум 4 символа.";
            return;
        }

        if (p1 != p2)
        {
            InfoLabel.Text = "Пароли не совпадают.";
            return;
        }

        if (DB.Users.Any(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
        {
            InfoLabel.Text = "Такой логин уже занят.";
            return;
        }

        DB.Users.Add(new User
        {
            Login = login,
            Password = p1,
            IsAdmin = false
        });

        await DB.SaveUsersAsync();

        await DisplayAlert("Готово", "Аккаунт создан. Теперь войдите.", "OK");
        await Navigation.PopAsync();
    }
}