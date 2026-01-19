using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlpineShop.Models
{
    internal class DB
    {
        private const string UsersFile = "users.json";
        private const string ProductsFile = "products.json";
        private const string CartsFile = "carts.json";

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public static List<User> Users { get; private set; } = new();
        public static ObservableCollection<Product> Products { get; private set; } = new();
        public static Dictionary<string, List<Cart>> Carts { get; private set; } = new();


        private static string GetPath(string fileName)
            => Path.Combine(FileSystem.AppDataDirectory, fileName);


        public static async Task InitializeAsync()
        {
            var users = await LoadAsync<List<User>>(UsersFile);
            if (users == null || users.Count == 0)
            {
                Users = new List<User>
            {
                new User { Login = "admin", Password = "admin", IsAdmin = true }
            };
                await SaveUsersAsync();
            }
            else
            {
                Users = users;
            }

            var products = await LoadAsync<List<Product>>(ProductsFile);
            if (products == null || products.Count == 0)
            {
                Products = new ObservableCollection<Product>(new List<Product>
            {
                new Product { Name = "Каска Petzl", Category = "Безопасность", Price = 4500 },
                new Product { Name = "Верёвка 60м", Category = "Верёвки", Price = 12500 },
                new Product { Name = "Карабин HMS", Category = "Железо", Price = 900 },
            });

                await SaveProductsAsync();
            }
            else
            {
                Products = new ObservableCollection<Product>(products);
            }
            var carts = await LoadAsync<Dictionary<string, List<Cart>>>(CartsFile);
            Carts = carts ?? new Dictionary<string, List<Cart>>();
        }


        public static Task SaveUsersAsync()
            => SaveAsync(UsersFile, Users);

        public static Task SaveProductsAsync()
            => SaveAsync(ProductsFile, Products.ToList());


        private static async Task SaveAsync<T>(string fileName, T data)
        {
            var path = GetPath(fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var json = JsonSerializer.Serialize(data, Options);
            await File.WriteAllTextAsync(path, json);
        }

        private static async Task<T?> LoadAsync<T>(string fileName)
        {
            var path = GetPath(fileName);
            if (!File.Exists(path)) return default;

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    
    public static Task SaveCartsAsync() => SaveAsync(CartsFile, Carts);

        public static List<Cart> GetCart(string login)
        {
            if (!Carts.TryGetValue(login, out var cart))
            {
                cart = new List<Cart>();
                Carts[login] = cart;
            }
            return cart;
        }

        public static async Task AddToCartAsync(string login, Product product)
        {
            var cart = GetCart(login);

            var existing = cart.FirstOrDefault(x => x.ProductName == product.Name && x.Price == product.Price);
            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(new Cart
                {
                    ProductName = product.Name,
                    Category = product.Category,
                    Price = product.Price,
                    ImageFile = product.ImageFile,
                    Quantity = 1
                });

            await SaveCartsAsync();
        }

        public static async Task RemoveFromCartAsync(string login, Cart item)
        {
            var cart = GetCart(login);
            cart.Remove(item);
            await SaveCartsAsync();
        }

        public static async Task ChangeQuantityAsync(string login, Cart item, int delta)
        {
            var cart = GetCart(login);

            var found = cart.FirstOrDefault(x =>
                x.ProductName == item.ProductName &&
                x.Price == item.Price &&
                x.ImageFile == item.ImageFile);

            if (found == null) return;

            found.Quantity += delta;

            if (found.Quantity <= 0)
                cart.Remove(found);

            await SaveCartsAsync();
        }

        public static async Task ClearCartAsync(string login)
        {
            GetCart(login).Clear();
            await SaveCartsAsync();
        }
    }
}
