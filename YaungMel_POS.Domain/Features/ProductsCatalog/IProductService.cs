using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public interface IProductService
    {
        Task<PagedResult<ProductDTO>> GetAsync(PaginationRequest request);
        Task<Result<ProductDTO>> GetByIdAsync(int id);
        Task<Result<ProductDTO>> CreateAsync(CreateProductDTO request, Stream? photoStream, string fileName, int userId);
        Task<Result<ProductDTO>> UpdateAsync(int id, UpdateProductDTO request, Stream? photoStream, string fileName, int userId);
        Task<Result<bool>> DeleteAsync(int id, uint version, int userId);
    }
}
