using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;

namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public interface ICategoryService
    {
        Task<Result<CategoryDTO>> GetCategoryByIdAsync(int id);
        Task<Result<CategoryListResponseModel>> GetCategoriesAsync(int pageNo, int pageSize);

        Task<Result<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO request, int userId);
        Task<Result<CategoryDTO>> UpdateCategoryAsync(int id, UpdateCategoryDTO request, int userId);
        Task<Result<List<CategoryDTO>>> GetCategoriesByTermAsync(string term);
        Task<Result<bool>> DeleteCategoryAsync(int id, int userId);
    }
}
