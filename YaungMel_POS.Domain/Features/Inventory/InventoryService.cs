using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Domain.Features.Audit;
using System.Text.Json;
using System.Text.Json.Serialization;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly POSDbContext _db;
        private readonly IAuditService _auditService;
        private readonly JsonSerializerOptions _jsonOptions;

        public InventoryService(POSDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        private IQueryable<Tbl_Product> ActiveProduct => _db.Products.Where(p => !p.DeleteFlag && p.IsActive);

        #region increase stock
        public async Task<Result<bool>> IncreaseStockAsync(int productId, int quantity, int userId)
        {
            try
            {
                if (quantity <= 0)
                    return Result<bool>.SystemError("Quantity must be greater than zero.");

                var product = await ActiveProduct.FirstOrDefaultAsync(p => p.Id == productId);
                if (product is null) return Result<bool>.SystemError("Product not found");

                var oldValues = JsonSerializer.Serialize(product, _jsonOptions);

                product.StockQuantity += quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                await _auditService.LogUpdateAsync(product, userId, oldValues, $"{product.Name}");

                return Result<bool>.Success(true, "Stock increased successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }
        #endregion

        #region decrease stock
        public async Task<Result<bool>> DecreaseStockAsync(int productId, int quantity, int userId)
        {
            try
            {
                var product = await ActiveProduct.FirstOrDefaultAsync(p => p.Id == productId);

                if (product is null) return Result<bool>.SystemError("Product not found");

                if (quantity <= 0) return Result<bool>.SystemError("Quantity must be greater than zero.");

                if (product.StockQuantity < quantity)
                    return Result<bool>.SystemError("Insufficient stock quantity available.");

                var oldValues = JsonSerializer.Serialize(product, _jsonOptions);

                product.StockQuantity -= quantity;

                if (product.StockQuantity == 0) product.IsActive = false;

                product.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                await _auditService.LogUpdateAsync(product, userId, oldValues, $"{product.Name}");

                return Result<bool>.Success(true, "Stock decreased successfully.");
            }
            catch (Exception)
            {
                return Result<bool>.SystemError("Unexpected error occured.");
            }
        }
        #endregion

        #region get low stock alert
        public async Task<Result<List<ProductDTO>>> GetLowStockAlertsAsync(int lowStock = 5)
        {
            try
            {
                var products = await _db.Products
                    .AsNoTracking()
                    .Where(p => !p.DeleteFlag && p.StockQuantity <= lowStock)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        DeleteFlag = p.DeleteFlag,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

                return Result<List<ProductDTO>>.Success(products, "Low stock products retrieved.");
            }
            catch (Exception ex)
            {
                return Result<List<ProductDTO>>.SystemError(ex.Message);
            }
        }
        #endregion

        #region update price
        public async Task<Result<bool>> UpdatePriceAsync(int productId, decimal newPrice, int userId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.DeleteFlag);
                if (product is null) return Result<bool>.SystemError("Product not found.");

                var oldValues = JsonSerializer.Serialize(product, _jsonOptions);

                product.Price = newPrice;
                product.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                await _auditService.LogUpdateAsync(product, userId, oldValues, "Product");

                return Result<bool>.Success(true, "Price updated successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }
        #endregion
    }
}
