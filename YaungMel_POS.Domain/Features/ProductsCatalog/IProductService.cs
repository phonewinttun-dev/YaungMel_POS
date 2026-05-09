using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;

namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public interface IProductService
    {
        Task<Result<ProductListResponseDTO>> GetProductsAsync(int pageNo, int pageSize);

        Task<Result<ProductDTO>> GetProductByIdAsync(int id);
        Task<Result<List<ProductDTO>>> GetAvailableProductsAsync();
        //Task<Result<ProductDTO>> CreateProductAsync(CreateProductDTO request, int userId);
        Task<Result<ProductDTO>> CreateProductAsync(CreateProductDTO request, Stream photoStream, string fileName, int userId);
        Task<Result<List<ProductDTO>>> BulkCreateProductsAsync(List<CreateProductDTO> request, int userId);
        Task<Result<ProductDTO>> UpdateProductAsync(int id, UpdateProductDTO request, Stream photoStream, string fileName, int userId);
        Task<Result<bool>> DeleteProductAsync(int id, uint version, int userId);
        Task<Result<List<ProductDTO>>> GetProductsByTermAsync(string term);
    }
}
