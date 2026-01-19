using AlpineShop.Models;
using System.Globalization;

namespace AlpineShop.Page;

public partial class Edit : ContentPage
{
    private readonly Product _product;
    private string _newImagePath = "";

    public Edit(Product product)
    {
        InitializeComponent();
        _product = product;

        NameEntry.Text = _product.Name;
        CategoryEntry.Text = _product.Category;
        PriceEntry.Text = _product.Price.ToString(CultureInfo.InvariantCulture);

        if (!string.IsNullOrWhiteSpace(_product.ImageFile) && File.Exists(_product.ImageFile))
            PreviewImage.Source = ImageSource.FromFile(_product.ImageFile);
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

            _newImagePath = destPath;
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
        var priceText = (PriceEntry.Text ?? "").Trim().Replace(',', '.');

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(cat) ||
            string.IsNullOrWhiteSpace(priceText))
        {
            InfoLabel.Text = "Заполните все поля.";
            return;
        }

        if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            InfoLabel.Text = "Некорректная цена.";
            return;
        }

        // если выбрали новую картинку — удалим старую (опционально)
        if (!string.IsNullOrWhiteSpace(_newImagePath))
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_product.ImageFile) && File.Exists(_product.ImageFile))
                    File.Delete(_product.ImageFile);
            }
            catch { }

            _product.ImageFile = _newImagePath;
        }

        _product.Name = name;
        _product.Category = cat;
        _product.Price = price;

        await DB.SaveProductsAsync();
        await DisplayAlert("Готово", "Изменения сохранены.", "OK");
        await Navigation.PopAsync();
    }
}