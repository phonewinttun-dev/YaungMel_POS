using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Inventory
{
    public interface IInventoryService
    {
        Task<Result<bool>> IncreaseStockAsync(int productId, int quantity, int userId);
        Task<Result<bool>> DecreaseStockAsync(int productId, int quantity, int userId);
        Task<Result<List<ProductDTO>>> GetLowStockAlertsAsync(int lowStock);
        Task<Result<bool>> UpdatePriceAsync(int productId, decimal newPrice, int userId);
    }
}
