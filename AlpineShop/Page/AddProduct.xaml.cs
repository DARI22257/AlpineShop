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
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите фото товара",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null) return;

            var ext = Path.GetExtension(result.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var destPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using var src = await result.OpenReadAsync();
            using var dst = File.Create(destPath);
            await src.CopyToAsync(dst);

            _imagePath = destPath;

            // Превью
            PreviewImage.Source = ImageSource.FromFile(destPath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
        }
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