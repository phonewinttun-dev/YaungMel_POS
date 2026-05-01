using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using POSSampleOWN.domain.Features.ProductsCatalog;
using POSSampleOWN.domain.Features.Search;
using POSSampleOWN.domain.Features.Inventory;
using POSSampleOWN.domain.Features.Sale;
using POSSampleOWN.domain.Features.Dashboard;
using POSSampleOWN.domain.Features.Auth;
using POSSampleOWN.domain.Features.Point;

namespace POSSampleOWN.domain.Features
{
    public static class FeaturesManager
    {
        public static void AddDomain(this WebApplicationBuilder builder)
        {
            
            builder.Services.AddDbContext<POSSampleOWN.database.Data.POSDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("POSConnectionString")));
            var loyaltySettings = builder.Configuration.GetSection("LoyaltyApiSettings");

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
            builder.Services.AddScoped<IProductCatalogService, ProductCatalogService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<ISaleService, SaleService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
        }
    }
}
