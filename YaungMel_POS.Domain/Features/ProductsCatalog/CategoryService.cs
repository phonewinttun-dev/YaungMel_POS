using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Domain.Features.Audit;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public class CategoryService : ICategoryService
    {
        private readonly POSDbContext _db;
       
        public CategoryService(POSDbContext db)
        {
            _db = db;
        }

        #region get categories with pagination
        public async Task<PagedResult<CategoryListResponseModel>> GetAsync(int pageNo, int pageSize)
        {
            try
            {
                if (pageSize <= 0) return PagedResult<CategoryListResponseModel>.SystemError("Page size must be greater than 0.");
                var totalItems = await _db.Categories
                    .AsNoTracking()
                    .Where(c => !c.DeleteFlag)
                    .CountAsync();

                var pageCount = totalItems / pageSize;
                if (totalItems % pageSize > 0) pageCount++;

                var categories = await _db.Categories
                    .AsNoTracking()
                    .Where(c => !c.DeleteFlag)
                    .OrderByDescending(c => c.Id)
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync();

                var result = new CategoryListResponseModel
                {
                    Items = categories,
                    PageSetting = new PageSettingDTO(pageNo, pageSize, pageCount)
                };

                return PagedResult<CategoryListResponseModel>.Success(result);
            }
            catch (Exception ex)
            {
                return PagedResult<CategoryListResponseModel>.SystemError($"Error: {ex.Message}");
            }
        }
        #endregion

        #region get category by id
        public async Task<PagedResult<CategoryDTO>> GetByIdAsync(int id)
        {
            try
            {
                var category = await _db.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id && !c.DeleteFlag);

                if (category is null) return PagedResult<CategoryDTO>.NotFound("Category not found.");


                var data = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return PagedResult<CategoryDTO>.Success(data);
            }
            catch (Exception ex)
            {
                return PagedResult<CategoryDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region create category
        public async Task<PagedResult<CategoryDTO>> CreateAsync(CreateCategoryDTO request, int userId)
        {
            try
            {
                var duplicateCategory = await _db.Categories
                    .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && !c.DeleteFlag);

                if (duplicateCategory) return PagedResult<CategoryDTO>.SystemError("Category with same name exists.");

                var newCategory = new Tbl_Category
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Categories.Add(newCategory);
                await _db.SaveChangesAsync();

                var data = new CategoryDTO
                {
                    Id = newCategory.Id,
                    Name = newCategory.Name,
                    Description = newCategory.Description
                };

                return PagedResult<CategoryDTO>.Success(data, "Category created successfully.");
            }
            catch (Exception ex)
            {
                return PagedResult<CategoryDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region update category
        public async Task<PagedResult<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO request, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return PagedResult<CategoryDTO>.NotFound("Category not found.");

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var isDuplicate = await _db.Categories.AnyAsync(c =>
                        c.Id != id &&
                        !c.DeleteFlag &&
                        c.Name != null &&
                        c.Name.ToLower() == request.Name.Trim().ToLower());

                    if (isDuplicate)
                        return PagedResult<CategoryDTO>.SystemError("Another category with the same name already exists.");

                    category.Name = request.Name.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.Description))
                    category.Description = request.Description.Trim();

                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                var data = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return PagedResult<CategoryDTO>.Success(data, "Category updated successfully.");
            }
            catch (Exception ex)
            {
                return PagedResult<CategoryDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region delete category
        public async Task<PagedResult<bool>> DeleteAsync(int id, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return PagedResult<bool>.NotFound("Category not found!");

                var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id && !p.DeleteFlag);

                if (hasProducts) return PagedResult<bool>.SystemError("Cannot delete category with existing products.");

                category.DeleteFlag = true;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                return PagedResult<bool>.Success(true, "Category deleted successfully.");
            }
            catch (Exception ex)
            {
                return PagedResult<bool>.SystemError(ex.Message);
            }
        }
        #endregion

    }
}
