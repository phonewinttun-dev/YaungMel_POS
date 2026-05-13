using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public interface ICategoryService
    {
        Task<Result<CategoryDTO>> GetByIdAsync(int id);
        Task<PagedResult<CategoryDTO>> GetAsync(PaginationRequest request);
        Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO request, int userId);
        Task<Result<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO request, int userId);
        Task<Result<bool>> DeleteAsync(int id, int userId);
    }
}
