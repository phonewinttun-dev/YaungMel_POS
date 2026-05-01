using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.database.Data;
using YaungMel_POS.database.Models;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.ProductsCatalog
{
    public class ProductCatalogService: IProductCatalogService
    {
        private readonly POSDbContext _db;

        public ProductCatalogService(POSDbContext db)
        {
            _db = db;
        }

        private IQueryable<Tbl_Product> ActiveProductQuery => _db.Products
            .AsNoTracking()
            .Where(p => !p.DeleteFlag && p.IsActive);

        #region get all products
        public async Task<ApiResponse<List<ProductDTO>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _db.Products
                    .AsNoTracking()
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        DeleteFlag = p.DeleteFlag,
                        IsActive = p.IsActive,
                    })
                    .ToListAsync();

                return ApiResponse<List<ProductDTO>>.Success(products);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDTO>>.Fail(ex.Message);
            }
        }

        #endregion

        #region get Product Pagination
        public async Task<ApiResponse<ProductListResponseDTO>> GetProductsAsync(int pageNo, int pageSize)
        {
            try
            {
                var totalItems = await _db.Products
                    .AsNoTracking()
                    .CountAsync();

                 var pageCount = totalItems / pageSize;
                 if (totalItems % pageSize > 0) pageCount++;

                var products = await _db.Products
                    .AsNoTracking()
                    .OrderByDescending(p => p.Id) 
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        DeleteFlag = p.DeleteFlag,
                        IsActive = p.IsActive,
                    })
                    .ToListAsync();

                var result = new ProductListResponseDTO
                {
                    Items = products,
                    PageSetting = new PageSettingDTO(pageNo, pageSize, pageCount)
                };

                return ApiResponse<ProductListResponseDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductListResponseDTO>.Fail($"Error: {ex.Message}");
            }
        }
        #endregion

        #region get active products by id
        public async Task<ApiResponse<ProductDTO>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await ActiveProductQuery
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product is null) return ApiResponse<ProductDTO>.Fail("Product not found");

                var data = new ProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    IsActive = product.IsActive,
                    DeleteFlag= product.DeleteFlag
                };

                return ApiResponse<ProductDTO>.Success(data);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDTO>.Fail(ex.Message);
            }
        }
        #endregion

        #region get available products
        public async Task<ApiResponse<List<ProductDTO>>> GetAvailableProductsAsync()
        {
            try
            {
                var products = await ActiveProductQuery
                    .Where(p => p.StockQuantity > 0)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        IsActive = p.IsActive,
                        DeleteFlag= p.DeleteFlag
                    })
                    .ToListAsync();

                return ApiResponse<List<ProductDTO>>.Success(products);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDTO>>.Fail(ex.Message);
            }
        }
        #endregion

        #region create product
        public async Task<ApiResponse<ProductDTO>> CreateProductAsync(CreateProductDTO request, int userId)
        {
            try
            {
                var duplicateProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == request.Name.Trim().ToLower() && !p.DeleteFlag);

                if (duplicateProduct) return ApiResponse<ProductDTO>.Fail("Product with the same name already exists.");

                var categoryExists = await _db.Categories
                    .AnyAsync(c => c.Id == request.CategoryId && !c.DeleteFlag);

                if (!categoryExists) return ApiResponse<ProductDTO>.Fail("Category not found");

                var newProduct = new Tbl_Product
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    Price = request.Price,
                    StockQuantity = request.StockQuantity,
                    CategoryId = request.CategoryId,
                    IsActive = true,
                    DeleteFlag = false,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Products.Add(newProduct);

                await _db.SaveChangesAsync();

                var data = new ProductDTO
                {
                    Id = newProduct.Id,
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    StockQuantity = newProduct.StockQuantity,
                    CategoryId = newProduct.CategoryId,
                    DeleteFlag = newProduct.DeleteFlag,
                    IsActive = newProduct.IsActive
                };

                return ApiResponse<ProductDTO>.Success(data, "Product created successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDTO>.Fail(ex.Message);
            }
        }
        #endregion

        #region bulk insert product
        public async Task<ApiResponse<List<ProductDTO>>> BulkCreateProductsAsync(List<CreateProductDTO> request, int userId)
        {
            try
            {
                var products = request.Select(p => new Tbl_Product
                {
                    Name = p.Name.Trim(),
                    Description = p.Description?.Trim(),
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    IsActive = true,
                    DeleteFlag = false,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _db.Products.AddRange(products);
                await _db.SaveChangesAsync();

                var data = products.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    DeleteFlag = p.DeleteFlag,
                    IsActive = p.IsActive
                }).ToList();

                return ApiResponse<List<ProductDTO>>.Success(data, $"{data.Count} products created successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDTO>>.Fail(ex.Message);
            }
        }
        #endregion

        #region update product
        public async Task<ApiResponse<ProductDTO>> UpdateProductAsync(int id, UpdateProductDTO request, int userId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product is null || product.DeleteFlag == true)
                    return ApiResponse<ProductDTO>.Fail("Product not found");

                if (!string.IsNullOrWhiteSpace(request.Name))
                { 
                    var isDuplicate = await _db.Products.AnyAsync(p =>
                        p.Id != id &&
                        !p.DeleteFlag &&
                        p.Name != null &&
                        p.Name.ToLower() == request.Name.Trim().ToLower());

                    if (isDuplicate)
                        return ApiResponse<ProductDTO>.Fail("Another product with the same name already exists.");

                    product.Name = request.Name.Trim();
                }

                if (request.Description != null)
                    product.Description = request.Description.Trim();

                if (request.Price != null)
                    product.Price = request.Price.Value;

                if (request.StockQuantity != null)
                    product.StockQuantity = request.StockQuantity.Value;

                if (request.CategoryId != null)
                    product.CategoryId = request.CategoryId.Value;

                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                var data = new ProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId
                };

                return ApiResponse<ProductDTO>.Success(data, "Product updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDTO>.Fail(ex.Message);
            }
        }
        #endregion

        #region delete product
        public async Task<ApiResponse<bool>> DeleteProductAsync(int id, int userId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product is null) return ApiResponse<bool>.Fail("Product not found");

                product.DeleteFlag = true;
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail(ex.Message);
            }
        }
        #endregion

        #region Search Products By Term
        public async Task<ApiResponse<List<ProductDTO>>> GetProductsByTermAsync(string term)
        {
            try
            {
                var products = await ActiveProductQuery
                    .Where(p => p.Name.Contains(term) || p.Description != null && p.Description.Contains(term))
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId
                    })
                    .ToListAsync();

                return ApiResponse<List<ProductDTO>>.Success(products);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDTO>>.Fail(ex.Message);
            }
        }
        #endregion

        #region get all categories
        public async Task<ApiResponse<List<CategoryDTO>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _db.Categories
                    .AsNoTracking()
                    .Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync();

                return ApiResponse<List<CategoryDTO>>.Success(categories);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CategoryDTO>>.Fail(ex.Message);
            }
        }
        #endregion

        #region get categories pagination 
        public async Task<ApiResponse<CategoryListResponseModel>> GetCategoriesAsync(int pageNo, int pageSize)
        {
            try
            {
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

                return ApiResponse<CategoryListResponseModel>.Success(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryListResponseModel>.Fail($"Error: {ex.Message}");
            }
        }
        #endregion

        #region get category by id
        public async Task<ApiResponse<CategoryDTO>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _db.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return ApiResponse<CategoryDTO>.Fail("Category not found.");


                var data = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return ApiResponse<CategoryDTO>.Success(data);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDTO>.Fail(ex.Message);
            }
        }
        #endregion

        #region create category
        public async Task<ApiResponse<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO request, int userId)
        {
            try
            {
                var duplicateCategory = await _db.Categories
                    .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && !c.DeleteFlag);

                if (duplicateCategory) return ApiResponse<CategoryDTO>.Fail("Category with same name exists.");
    
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

                return ApiResponse<CategoryDTO>.Success(data, "Category created successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDTO>.Fail(ex.Message);
            }
        }
        #endregion

        #region update category
        public async Task<ApiResponse<CategoryDTO>> UpdateCategoryAsync(int id, UpdateCategoryDTO request, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return ApiResponse<CategoryDTO>.Fail("Category not found.");

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var isDuplicate = await _db.Categories.AnyAsync(c =>
                        c.Id != id &&
                        !c.DeleteFlag &&
                        c.Name != null &&
                        c.Name.ToLower() == request.Name.Trim().ToLower());

                    if (isDuplicate)
                        return ApiResponse<CategoryDTO>.Fail("Another category with the same name already exists.");

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

                return ApiResponse<CategoryDTO>.Success(data, "Category updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDTO>.Fail(ex.Message);
            }
        }
        #endregion

        #region delete category
        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id, int userId)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category is null) return ApiResponse<bool>.Fail("Category not found!");

                var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id && !p.DeleteFlag);

                if (hasProducts) return ApiResponse<bool>.Fail("Cannot delete category with existing products.");

                category.DeleteFlag = true;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Category deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail(ex.Message);
            }
        }
        #endregion

        #region get categories by term
        public async Task<ApiResponse<List<CategoryDTO>>> GetCategoriesByTermAsync(string term)
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

                return ApiResponse<List<CategoryDTO>>.Success(categories);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CategoryDTO>>.Fail(ex.Message);
            }
        }
        #endregion
    }
}
