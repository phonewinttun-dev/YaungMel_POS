using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Inventory
{
    public interface IInventoryService
    {
        Task<Result<bool>> IncreaseStockAsync(int productId, int quantity);
        Task<Result<bool>> DecreaseStockAsync(int productId, int quantity);
        Task<Result<List<ProductDTO>>> GetLowStockAlertsAsync(int lowStock);
        Task<Result<bool>> UpdatePriceAsync(int productId, decimal newPrice);
    }
}
