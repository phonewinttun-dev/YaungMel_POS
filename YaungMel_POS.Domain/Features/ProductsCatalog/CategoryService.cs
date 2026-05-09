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
using YaungMel_POS.Shared.Responses;

namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public class CategoryService : ICategoryService
    {
        private readonly POSDbContext _db;
        private readonly IAuditService _auditService;
        private readonly JsonSerializerOptions _jsonOptions = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles };

        public CategoryService(POSDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;
        }

        #region get categories pagination
        public async Task<Result<CategoryListResponseModel>> GetCategoriesAsync(int pageNo, int pageSize)
        {
            try
            {
                if (pageSize <= 0) return Result<CategoryListResponseModel>.SystemError("Page size must be greater than 0.");
                var totalItems = await _db.Categories
                    .AsNoTracking()
                    .CountAsync();

                var pageCount = totalItems / pageSize;
                if (totalItems % pageSize > 0) pageCount++;

                var categories = await _db.Categories
                    .AsNoTracking()
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

                return Result<CategoryListResponseModel>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<CategoryListResponseModel>.SystemError($"Error: {ex.Message}");
            }
        }
        #endregion

        #region get category by id
        public async Task<Result<CategoryDTO>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _db.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return Result<CategoryDTO>.NotFound("Category not found.");


                var data = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return Result<CategoryDTO>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<CategoryDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region create category
        public async Task<Result<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO request, int userId)
        {
            try
            {
                var duplicateCategory = await _db.Categories
                    .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && !c.DeleteFlag);

                if (duplicateCategory) return Result<CategoryDTO>.SystemError("Category with same name exists.");

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

                return Result<CategoryDTO>.Success(data, "Category created successfully.");
            }
            catch (Exception ex)
            {
                return Result<CategoryDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region update category
        public async Task<Result<CategoryDTO>> UpdateCategoryAsync(int id, UpdateCategoryDTO request, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return Result<CategoryDTO>.NotFound("Category not found.");

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var isDuplicate = await _db.Categories.AnyAsync(c =>
                        c.Id != id &&
                        !c.DeleteFlag &&
                        c.Name != null &&
                        c.Name.ToLower() == request.Name.Trim().ToLower());

                    if (isDuplicate)
                        return Result<CategoryDTO>.SystemError("Another category with the same name already exists.");

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

                return Result<CategoryDTO>.Success(data, "Category updated successfully.");
            }
            catch (Exception ex)
            {
                return Result<CategoryDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region delete category
        public async Task<Result<bool>> DeleteCategoryAsync(int id, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return Result<bool>.NotFound("Category not found!");

                var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id && !p.DeleteFlag);

                if (hasProducts) return Result<bool>.SystemError("Cannot delete category with existing products.");

                category.DeleteFlag = true;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                return Result<bool>.Success(true, "Category deleted successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }
        #endregion

        #region get categories by term
        public async Task<Result<List<CategoryDTO>>> GetCategoriesByTermAsync(string term)
        {
            try
            {
                var categories = await _db.Categories
                    .AsNoTracking()
                    .Where(c => c.Name.Contains(term) || c.Description != null && c.Description.Contains(term))
                    .Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync();

                return Result<List<CategoryDTO>>.Success(categories);
            }
            catch (Exception ex)
            {
                return Result<List<CategoryDTO>>.SystemError(ex.Message);
            }
        }
        #endregion

    }
}
