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
        public async Task<PagedResult<CategoryDTO>> GetAsync(PaginationRequest request)
        {
            if (request is null) return PagedResult<CategoryDTO>.ValidationError("Request cannot be null!");

            try
            {
                var totalCategories = await _db.Categories
                                            .AsNoTracking()
                                            .Where(c => !c.DeleteFlag)
                                            .CountAsync();

                var categories = await _db.Categories
                    .AsNoTracking()
                    .Where(c => !c.DeleteFlag)
                    .OrderByDescending(c => c.Id)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync();

                var pagination = new Pagination(request.PageSize, request.PageNumber, totalCategories);
                return PagedResult<CategoryDTO>.Success(categories, pagination, "Categories retrieved successfully!");
            }
            catch (Exception ex)
            {
                return PagedResult<CategoryDTO>.SystemError($"Error: {ex.Message}");
            }
        }
        #endregion

        #region get category by id
        public async Task<Result<CategoryDTO>> GetByIdAsync(int id)
        {
            try
            {
                var category = await _db.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id && !c.DeleteFlag);

                if (category is null) return Result<CategoryDTO>.NotFound("Category not found.");


                var data = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return Result<CategoryDTO>.Success(data, $"{data.Name} retreived successfully!");
            }
            catch (Exception ex)
            {
                return Result<CategoryDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region create category
        public async Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO request, int userId)
        {
            try
            {
                var duplicateCategory = await _db.Categories
                    .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && !c.DeleteFlag);

                if (duplicateCategory) return Result<CategoryDTO>.ValidationError("Category with same name exists.");

                if (string.IsNullOrWhiteSpace(request.Name))
                    return Result<CategoryDTO>.ValidationError("Category name is required.");

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
        public async Task<Result<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO request, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return Result<CategoryDTO>.NotFound("Category not found.");

                if (!string.IsNullOrWhiteSpace(request.Name))
                    category.Name = request.Name.Trim();

                if (!string.IsNullOrWhiteSpace(request.Description))
                    category.Description = request.Description.Trim();

                var isDuplicate = await _db.Categories.AnyAsync(c =>
                        c.Id != id &&
                        !c.DeleteFlag &&
                        c.Name != null &&
                        c.Name.ToLower() == request.Name!.Trim().ToLower());

                if (isDuplicate)
                    return Result<CategoryDTO>.ValidationError("Another category with the same name already exists.");

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
        public async Task<Result<bool>> DeleteAsync(int id, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return Result<bool>.NotFound("Category not found!");
                    
                var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id && !p.DeleteFlag);

                if (hasProducts) return Result<bool>.ValidationError("Cannot delete category with existing products.");

                category.DeleteFlag = true;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                return Result<bool>.DeleteSuccess("Category deleted successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }
        #endregion

    }
}
