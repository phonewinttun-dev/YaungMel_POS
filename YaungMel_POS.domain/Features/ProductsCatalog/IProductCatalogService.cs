using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.ProductsCatalog
{
    public interface IProductCatalogService
    {
        Task<ApiResponse<List<ProductDTO>>> GetAllProductsAsync();
        Task<ApiResponse<ProductListResponseDTO>> GetProductsAsync(int pageNo, int pageSize);

        Task<ApiResponse<ProductDTO>> GetProductByIdAsync(int id);
        Task<ApiResponse<List<ProductDTO>>> GetAvailableProductsAsync();
        Task<ApiResponse<ProductDTO>> CreateProductAsync(CreateProductDTO request, int userId);

        Task<ApiResponse<List<ProductDTO>>> BulkCreateProductsAsync(List<CreateProductDTO> request, int userId);
        Task<ApiResponse<ProductDTO>> UpdateProductAsync(int id, UpdateProductDTO request, int userId);
        Task<ApiResponse<bool>> DeleteProductAsync(int id, int userId);
        Task<ApiResponse<List<ProductDTO>>> GetProductsByTermAsync(string term);
        Task<ApiResponse<List<CategoryDTO>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryDTO>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<CategoryListResponseModel>> GetCategoriesAsync(int pageNo, int pageSize);

        Task<ApiResponse<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO request, int userId);
        Task<ApiResponse<CategoryDTO>> UpdateCategoryAsync(int id, UpdateCategoryDTO request, int userId);
        Task<ApiResponse<List<CategoryDTO>>> GetCategoriesByTermAsync(string term);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int id, int userId);
    }
}
