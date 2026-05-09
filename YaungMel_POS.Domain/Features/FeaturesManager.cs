using CloudinaryDotNet;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Domain.Features.Audit;
using YaungMel_POS.Domain.Features.Auth;
using YaungMel_POS.Domain.Features.Dashboard;
using YaungMel_POS.Domain.Features.Inventory;
using YaungMel_POS.Domain.Features.Point;
using YaungMel_POS.Domain.Features.ProductsCatalog;
using YaungMel_POS.Domain.Features.Report;
using YaungMel_POS.Domain.Features.Sale;
using YaungMel_POS.Domain.Features.Search;
using YaungMel_POS.Domain.Features.Summary;

namespace YaungMel_POS.Domain.Features
{
    public static class FeaturesManager
    {
        public static void AddDomain(this WebApplicationBuilder builder)
        {

            builder.Services.AddDbContext<POSDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("POSConnectionString")));
            var loyaltySettings = builder.Configuration.GetSection("LoyaltyApiSettings");

            // DinkToPDF 
            builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            //cloudinary config
            var cloudName = builder.Configuration["Cloudinary:CloudName"]?.Trim();
            var apiKey = builder.Configuration["Cloudinary:ApiKey"]?.Trim();
            var apiSecret = builder.Configuration["Cloudinary:ApiSecret"]?.Trim();
            var acc = new Account(cloudName, apiKey, apiSecret);
            builder.Services.AddSingleton(new Cloudinary(acc));
            builder.Services.AddScoped<IPhotoService, PhotoService>();

            //point system
            var pointEnabled = builder.Configuration.GetValue<bool>("Features:PointSystemEnabled");

            if (pointEnabled)
            {
                builder.Services.AddHttpClient<IPointService, PointService>(client =>
                {
                    var baseUrl = builder.Configuration["LoyaltyApiSettings:BaseUrl"];
                    client.BaseAddress = new Uri(baseUrl);
                    var systemId = builder.Configuration["LoyaltyApiSettings:SystemId"];
                    client.DefaultRequestHeaders.Add("X-System-Id", systemId);
                }).AddStandardResilienceHandler();
            }
            else
            {
                builder.Services.AddScoped<IPointService, DisabledPointService>();
            }
            // Register Features
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<ISaleService, SaleService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ISummaryService, SummaryService>();  
            builder.Services.AddScoped<IReportService, ReportService>();
        }
    }
}
