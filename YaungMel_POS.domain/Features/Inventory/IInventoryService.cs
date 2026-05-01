using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Inventory
{
    public interface IInventoryService
    {
        Task<ApiResponse<bool>> IncreaseStockAsync(int productId, int quantity);
        Task<ApiResponse<bool>> DecreaseStockAsync(int productId, int quantity);
        Task<ApiResponse<List<ProductDTO>>> GetLowStockAlertsAsync(int lowStock);
        Task<ApiResponse<bool>> UpdatePriceAsync(int productId, decimal newPrice);
    }
}
