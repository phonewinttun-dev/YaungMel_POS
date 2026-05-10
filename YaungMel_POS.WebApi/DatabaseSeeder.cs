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


        //Clear all tables and restart identities
        // await db.Database.ExecuteSqlRawAsync(@"
        //    TRUNCATE TABLE ""Tbl_Summary"", ""Tbl_SaleItem"", ""Tbl_Sale"", ""Tbl_Product"", ""Tbl_Category"", ""Tbl_User"", ""Tbl_User_Token"", ""Tbl_AuditLog"" RESTART IDENTITY CASCADE;
        // ");

        var random = new Random();

        // ───────────────── USERS ─────────────────
        if (!await db.Users.AnyAsync())
        {
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
        }

        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Role == Tbl_User.UserRole.Admin) 
            ?? throw new Exception("Admin user not found for seeding");
        var staffUser = await db.Users.FirstOrDefaultAsync(u => u.Role == Tbl_User.UserRole.Staff)
            ?? throw new Exception("Staff user not found for seeding");

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
                "Baileys",
                "Jägermeister",
                "Malibu",
                "Suntory Whisky",
                "Yamazaki 12",
                "Choya Umeshu",
                "Jinro Soju"
            };

            var sampleImages = new[]
            {
                new { Name = "Jack Daniel's", Id = "yaungmel_pos_product_photos/Jack_Daniel_s_dxfhzq", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238437/Jack_Daniel_s_dxfhzq.jpg" },
                new { Name = "Johnnie Walker Black", Id = "yaungmel_pos_product_photos/Johnnie_Walker_Black_bln1wi", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238443/Johnnie_Walker_Black_bln1wi.jpg" },
                new { Name = "Johnnie Walker Red", Id = "yaungmel_pos_product_photos/Johnnie_Wakker_Red_vxfngr", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238442/Johnnie_Wakker_Red_vxfngr.jpg" },
                new { Name = "Chivas Regal", Id = "yaungmel_pos_product_photos/chivas_regal_lef8tx", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238434/chivas_regal_lef8tx.jpg" },
                new { Name = "Ballantine's", Id = "yaungmel_pos_product_photos/Ballantine_s_hagn2h", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238434/Ballantine_s_hagn2h.jpg" },
                new { Name = "Jameson", Id = "yaungmel_pos_product_photos/Jameson_obyxde", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238438/Jameson_obyxde.jpg" },
                new { Name = "Jim Beam", Id = "yaungmel_pos_product_photos/Jim_Beam_q9xvts", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238440/Jim_Beam_q9xvts.jpg" },
                new { Name = "Maker's Mark", Id = "yaungmel_pos_product_photos/Maker_s_Mark_hbpbn9", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238447/Maker_s_Mark_hbpbn9.jpg" },
                new { Name = "Macallan 12", Id = "yaungmel_pos_product_photos/Macallan_12_yd1vab", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238445/Macallan_12_yd1vab.jpg" },
                new { Name = "Glenfiddich 15", Id = "yaungmel_pos_product_photos/glenfiddich_ho7dir", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778238435/glenfiddich_ho7dir.jpg" },
                new { Name = "Absolut Vodka", Id = "yaungmel_pos_product_photos/AbsolutVodka_qomjdz", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239262/AbsolutVodka_qomjdz.webp" },
                new { Name = "Smirnoff Vodka", Id = "yaungmel_pos_product_photos/smirnoff_oh9oay", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239286/smirnoff_oh9oay.jpg" },
                new { Name = "Grey Goose", Id = "yaungmel_pos_product_photos/GreyGoose_xrpogo", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239276/GreyGoose_xrpogo.webp" },
                new { Name = "Belvedere", Id = "yaungmel_pos_product_photos/Belvedere_iqevut", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239267/Belvedere_iqevut.jpg" },
                new { Name = "Bacardi White Rum", Id = "yaungmel_pos_product_photos/Bacardi_qztlo0", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239264/Bacardi_qztlo0.jpg" },
                new { Name = "Captain Morgan", Id = "yaungmel_pos_product_photos/captainMorgan_ywnwu9", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239273/captainMorgan_ywnwu9.webp" },
                new { Name = "Havana Club", Id = "yaungmel_pos_product_photos/HavanaClub_fntdrb", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239280/HavanaClub_fntdrb.webp" },
                new { Name = "Bombay Sapphire", Id = "yaungmel_pos_product_photos/BombaySapphire_ghww69", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239270/BombaySapphire_ghww69.png" },
                new { Name = "Tanqueray", Id = "yaungmel_pos_product_photos/Tanqueray_v4tjzq", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239289/Tanqueray_v4tjzq.png" },
                new { Name = "Hendrick's Gin", Id = "yaungmel_pos_product_photos/HendrickGin_zgq8qh", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778239283/HendrickGin_zgq8qh.png" },
                new { Name = "Patron Silver", Id = "yaungmel_pos_product_photos/patron_silver_rqdjdy", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241495/patron_silver_rqdjdy.jpg" },
                new { Name = "Don Julio", Id = "yaungmel_pos_product_photos/donjulio_wwqoc5", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241493/donjulio_wwqoc5.jpg" },
                new { Name = "Jose Cuervo", Id = "yaungmel_pos_product_photos/josecuervo_krdfmh", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241495/josecuervo_krdfmh.png" },
                new { Name = "Hennessy VSOP", Id = "yaungmel_pos_product_photos/hennessy_bx5ba0", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241493/hennessy_bx5ba0.jpg" },
                new { Name = "Martell VSOP", Id = "yaungmel_pos_product_photos/matell_lsso4d", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241495/matell_lsso4d.jpg" },
                new { Name = "Rémy Martin", Id = "yaungmel_pos_product_photos/remymartin_kywlq3", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241495/remymartin_kywlq3.jpg" },
                new { Name = "Corona Extra", Id = "yaungmel_pos_product_photos/corona_dnuwzb", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241493/corona_dnuwzb.jpg" },
                new { Name = "Heineken", Id = "yaungmel_pos_product_photos/heineken_m0l1az", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241495/heineken_m0l1az.png" },
                new { Name = "Tiger Beer", Id = "yaungmel_pos_product_photos/tiger_oxjtlf", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241496/tiger_oxjtlf.jpg" },
                new { Name = "Carlsberg", Id = "yaungmel_pos_product_photos/carlsberg_epy51u", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241493/carlsberg_epy51u.jpg" },
                new { Name = "Guinness Stout", Id = "yaungmel_pos_product_photos/Guinness_jz5713", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241494/Guinness_jz5713.jpg" },
                new { Name = "Hoegaarden", Id = "yaungmel_pos_product_photos/Hoegaarden_g9jokm", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241494/Hoegaarden_g9jokm.jpg" },
                new { Name = "Budweiser", Id = "yaungmel_pos_product_photos/budweiser_g0bap0", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241493/budweiser_g0bap0.jpg" },
                new { Name = "Asahi Beer", Id = "yaungmel_pos_product_photos/asahi_oo0crb", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241493/asahi_oo0crb.jpg" },
                new { Name = "Sapporo", Id = "yaungmel_pos_product_photos/sapporo_qauvkm", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778241495/sapporo_qauvkm.jpg" },
                new { Name = "Strongbow", Id = "yaungmel_pos_product_photos/strongbow_thslxo", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236960/strongbow_thslxo.webp" },
                new { Name = "Somersby", Id = "yaungmel_pos_product_photos/somersby_kaxvjr", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236960/somersby_kaxvjr.webp" },
                new { Name = "Baileys", Id = "yaungmel_pos_product_photos/baileys_znjpp9", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236959/baileys_znjpp9.webp" },
                new { Name = "Jägermeister", Id = "yaungmel_pos_product_photos/pozmunyihgig27migtad", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778076312/yaungmel_pos_product_photos/pozmunyihgig27migtad.webp" },
                new { Name = "Malibu", Id = "yaungmel_pos_product_photos/malibu_originalnew750_egbuid", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236960/malibu_originalnew750_egbuid.webp" },
                new { Name = "Suntory Whisky", Id = "yaungmel_pos_product_photos/suntory_cjztne", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236960/suntory_cjztne.webp" },
                new { Name = "Yamazaki 12", Id = "yaungmel_pos_product_photos/yamazaki-12-years-old-single-malt-whisky-2_l3d0pw", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236960/yamazaki-12-years-old-single-malt-whisky-2_l3d0pw.webp" },
                new { Name = "Choya Umeshu", Id = "yaungmel_pos_product_photos/Choya-Umeshu__62287_diy7qh", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236959/Choya-Umeshu__62287_diy7qh.webp" },
                new { Name = "Jinro Soju", Id = "yaungmel_pos_product_photos/JinroGrapefruitSoju375_900x_v2jv2g", Url = "https://res.cloudinary.com/dsqzhkhkk/image/upload/v1778236959/JinroGrapefruitSoju375_900x_v2jv2g.webp" }
            };

            var products = new List<Tbl_Product>();
            int productCount = 0;

            foreach (var productName in alcoholProducts)
            {
                var randomCategory = categoriesList[random.Next(categoriesList.Count)];
                var productImg = sampleImages.FirstOrDefault(x => x.Name == productName);

                products.Add(new Tbl_Product
                {
                    Name = productName,
                    Description = $"Premium {randomCategory.Name} alcoholic beverage - {productName}",
                    ImageUrl = productImg?.Url,
                    ImageId = productImg?.Id,
                    Price = random.Next(50, 200) * 1000, // Price ends with 000
                    StockQuantity = random.Next(20, 300),
                    IsActive = true,
                    CategoryId = randomCategory.Id,
                    CreatedBy = adminUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    DeleteFlag = false
                });
                productCount++;
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
                var createdDate = DateTime.UtcNow.Date.AddDays(-day);
                var saleCountPerDay = random.Next(5, 15);

                for (int s = 0; s < saleCountPerDay; s++)
                {
                    var saleTime = createdDate.AddHours(random.Next(8, 20)).AddMinutes(s * 5).AddSeconds(random.Next(60));
                    var sale = new Tbl_Sale
                    {
                        VoucherCode = $"YM-{saleTime:yyyyMMddHHmmss}",
                        CreatedBy = staffUser.Id,
                        CreatedAt = saleTime,
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
