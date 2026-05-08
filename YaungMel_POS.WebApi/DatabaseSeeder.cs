using Microsoft.EntityFrameworkCore;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;

namespace YaungMel_POS.WebApi;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<POSDbContext>();

        var random = new Random();

        // ───────────────── USERS ─────────────────
        //if (!await db.Users.AnyAsync())
        //{
            var users = new List<Tbl_User>
            {
                new()
                {
                    Name = "Admin User",
                    MobileNum = "09100000001",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = Tbl_User.UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    DeleteFlag = false
                },
                new()
                {
                    Name = "Staff User",
                    MobileNum = "09100000002",
                    Password = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                    Role = Tbl_User.UserRole.Staff,
                    CreatedAt = DateTime.UtcNow,
                    DeleteFlag = false
                },
                new()
                {
                    Name = "Customer User",
                    MobileNum = "09100000003",
                    Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                    Role = Tbl_User.UserRole.Customer,
                    CreatedAt = DateTime.UtcNow,
                    DeleteFlag = false
                }
            };

            await db.Users.AddRangeAsync(users);
            await db.SaveChangesAsync();
        //}

        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Role == Tbl_User.UserRole.Admin);
        var staffUser = await db.Users.FirstOrDefaultAsync(u => u.Role == Tbl_User.UserRole.Staff);

        // ───────────────── CATEGORIES ─────────────────
        if (!await db.Categories.AnyAsync())
        {
            var categoryNames = new[]
            {
                "Whiskey",
                "Vodka",
                "Rum",
                "Gin",
                "Tequila",
                "Brandy",
                "Wine",
                "Red Wine",
                "White Wine",
                "Sparkling Wine",
                "Champagne",
                "Beer",
                "Craft Beer",
                "Lager",
                "Stout",
                "Cider",
                "Cocktail",
                "Liqueur",
                "Sake",
                "Soju"
            };

            var categories = categoryNames.Select(name => new Tbl_Category
            {
                Name = name,
                Description = $"{name} alcoholic beverages",
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                DeleteFlag = false
            }).ToList();

            await db.Categories.AddRangeAsync(categories);
            await db.SaveChangesAsync();
        }

        var categoriesList = await db.Categories.ToListAsync();

        // ───────────────── PRODUCTS ─────────────────
        if (!await db.Products.AnyAsync())
        {
            var alcoholProducts = new List<string>
            {
                "Jack Daniel's",
                "Johnnie Walker Black",
                "Johnnie Walker Red",
                "Chivas Regal",
                "Ballantine's",
                "Jameson",
                "Jim Beam",
                "Maker's Mark",
                "Macallan 12",
                "Glenfiddich 15",
                "Absolut Vodka",
                "Smirnoff Vodka",
                "Grey Goose",
                "Belvedere",
                "Bacardi White Rum",
                "Captain Morgan",
                "Havana Club",
                "Bombay Sapphire",
                "Tanqueray",
                "Hendrick's Gin",
                "Patron Silver",
                "Don Julio",
                "Jose Cuervo",
                "Hennessy VSOP",
                "Martell VSOP",
                "Rémy Martin",
                "Corona Extra",
                "Heineken",
                "Tiger Beer",
                "Carlsberg",
                "Guinness Stout",
                "Hoegaarden",
                "Budweiser",
                "Asahi Beer",
                "Sapporo",
                "Strongbow",
                "Somersby",
                "Mojito",
                "Margarita",
                "Negroni",
                "Old Fashioned",
                "Martini",
                "Baileys",
                "Jägermeister",
                "Kahlúa",
                "Malibu",
                "Suntory Whisky",
                "Yamazaki 12",
                "Choya Umeshu",
                "Jinro Soju"
            };

            var products = new List<Tbl_Product>();

            for (int i = 1; i <= 100; i++)
            {
                var randomCategory = categoriesList[random.Next(categoriesList.Count)];
                var randomProduct = alcoholProducts[random.Next(alcoholProducts.Count)];

                products.Add(new Tbl_Product
                {
                    Name = $"{randomProduct} {i}",
                    Description = $"Premium {randomCategory.Name} alcoholic beverage",
                    Price = random.Next(5000, 150000),
                    StockQuantity = random.Next(20, 300),
                    IsActive = true,
                    CategoryId = randomCategory.Id,
                    CreatedBy = adminUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    DeleteFlag = false
                });
            }

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();
        }

        // ───────────────── SALES + SALE ITEMS ─────────────────
        if (!await db.Sales.AnyAsync())
        {
            var products = await db.Products.ToListAsync();

            var sales = new List<Tbl_Sale>();

            for (int day = 0; day < 30; day++)
            {
                var saleCountPerDay = random.Next(5, 15);

                for (int s = 0; s < saleCountPerDay; s++)
                {
                    var createdDate = DateTime.UtcNow.Date.AddDays(-day);

                    var sale = new Tbl_Sale
                    {
                        VoucherCode = $"VC-{createdDate:yyyyMMdd}-{s + 1:D3}",
                        //CreatedBy = staffUser.Id,
                        CreatedAt = createdDate,
                        TotalPrice = 0
                    };

                    var itemCount = random.Next(2, 6);

                    sale.SaleItems = new List<Tbl_SaleItem>();

                    for (int i = 0; i < itemCount; i++)
                    {
                        var product = products[random.Next(products.Count)];
                        var quantity = random.Next(1, 5);

                        sale.SaleItems.Add(new Tbl_SaleItem
                        {
                            ProductId = product.Id,
                            Quantity = quantity,
                            Price = product.Price
                        });
                    }

                    sale.TotalPrice = sale.SaleItems.Sum(x => x.Price * x.Quantity);

                    sales.Add(sale);
                }
            }

            await db.Sales.AddRangeAsync(sales);
            await db.SaveChangesAsync();
        }

        // ───────────────── SUMMARY ─────────────────
        if (!await db.Summaries.AnyAsync())
        {
            var sales = await db.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            var summaries = new List<Tbl_Summary>();

            for (int day = 0; day < 30; day++)
            {
                var targetDate = DateTime.UtcNow.Date.AddDays(-day);

                var dailySales = sales
                    .Where(s => s.CreatedAt.Date == targetDate.Date)
                    .ToList();

                var totalSale = dailySales.Count;

                var totalAmount = dailySales.Sum(s => s.TotalPrice);

                var topProduct = dailySales
                    .SelectMany(s => s.SaleItems)
                    .GroupBy(si => si.ProductId)
                    .OrderByDescending(g => g.Sum(x => x.Quantity))
                    .FirstOrDefault();

                summaries.Add(new Tbl_Summary
                {
                    Date = targetDate,
                    TotalSale = totalSale,
                    TotalAmount = totalAmount,
                    TopSaleProductId = topProduct?.Key
                });
            }

            await db.Summaries.AddRangeAsync(summaries);
            await db.SaveChangesAsync();
        }
    }
}
