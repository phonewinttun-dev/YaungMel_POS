using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Inventory
{
    public interface IInventoryService
    {
        Task<PagedResult<bool>> IncreaseStockAsync(int productId, int quantity, int userId);
        Task<PagedResult<bool>> DecreaseStockAsync(int productId, int quantity, int userId);
        Task<PagedResult<List<ProductDTO>>> GetLowStockAlertsAsync(int lowStock);
        Task<PagedResult<bool>> UpdatePriceAsync(int productId, decimal newPrice, int userId);
    }
}
