using AlpineShop.Page;

namespace AlpineShop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new ContentPage(); // заглушка
            _ = StartAsync();

        }
        private async Task StartAsync()
        {
            await AlpineShop.Models.DB.InitializeAsync();
            MainPage = new NavigationPage(new AlpineShop.Page.Login());
        }


    }
}