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
        Task<PagedResult<CategoryDTO>> GetByIdAsync(int id);
        Task<PagedResult<CategoryListResponseModel>> GetAsync(int pageNo, int pageSize);

        Task<PagedResult<CategoryDTO>> CreateAsync(CreateCategoryDTO request, int userId);
        Task<PagedResult<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO request, int userId);
        Task<PagedResult<bool>> DeleteAsync(int id, int userId);
    }
}
