using AlpineShop.Models;
using System.Globalization;

namespace AlpineShop.Page;

public partial class AddProduct : ContentPage
{
    private string _imagePath = "";

    public AddProduct()
    {
        InitializeComponent();
    }
    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        _imagePath = await PickImageAsync(PreviewImage);
    }

    private async Task<string> PickImageAsync(Image preview)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            FileTypes = FilePickerFileType.Images
        });

        if (result == null)
            return "";

        var path = Path.Combine(FileSystem.AppDataDirectory, result.FileName);

        using var src = await result.OpenReadAsync();
        using var dst = File.Create(path);
        await src.CopyToAsync(dst);

        preview.Source = ImageSource.FromFile(path);

        return path;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        InfoLabel.Text = "";

        var name = (NameEntry.Text ?? "").Trim();
        var cat = (CategoryEntry.Text ?? "").Trim();
        var priceText = (PriceEntry.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(cat) ||
            string.IsNullOrWhiteSpace(priceText))
        {
            InfoLabel.Text = "Заполните все поля.";
            return;
        }

        priceText = priceText.Replace(',', '.');

        if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            InfoLabel.Text = "Некорректная цена.";
            return;
        }

        DB.Products.Add(new Product
        {
            Name = name,
            Category = cat,
            Price = price,
            ImageFile = _imagePath // полный путь
        });

        await DB.SaveProductsAsync();

        await DisplayAlert("Готово", "Товар добавлен в каталог.", "OK");
        await Navigation.PopAsync();
    }
}