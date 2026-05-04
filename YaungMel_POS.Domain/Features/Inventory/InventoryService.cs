using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using YaungMel_POS.database.Data;
using YaungMel_POS.database.Models;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly POSDbContext _db;

        public InventoryService(POSDbContext db)
        {
            _db = db;
        }

        private IQueryable<Tbl_Product> ActiveProduct => _db.Products.Where(p => !p.DeleteFlag);

        #region increase stock
        public async Task<Result<bool>> IncreaseStockAsync(int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                    return Result<bool>.SystemError("Quantity must be greater than zero.");

                var product = await ActiveProduct.FirstOrDefaultAsync(p => p.Id == productId);
                if (product is null) return Result<bool>.SystemError("Product not found");

                product.StockQuantity += quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Result<bool>.Success(true, "Stock increased successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }
        #endregion

        #region decrease stock
        public async Task<Result<bool>> DecreaseStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await ActiveProduct.FirstOrDefaultAsync(p => p.Id == productId);

                if (product is null) return Result<bool>.SystemError("Product not found");

                if (quantity <= 0) return Result<bool>.SystemError("Quantity must be greater than zero.");

                if (product.StockQuantity < quantity)
                    return Result<bool>.SystemError("Insufficient stock quantity available.");

                product.StockQuantity -= quantity;

                if (product.StockQuantity == 0) product.IsActive = false;

                product.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
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
        public async Task<Result<bool>> UpdatePriceAsync(int productId, decimal newPrice)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.DeleteFlag);
                if (product is null) return Result<bool>.SystemError("Product not found.");

                product.Price = newPrice;
                product.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
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
