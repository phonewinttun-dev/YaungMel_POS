using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;

namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public class ProductCatalogService: IProductCatalogService
    {
        private readonly POSDbContext _db;
        private readonly IPhotoService _photoService;

        public ProductCatalogService(POSDbContext db, IPhotoService photoService)
        {
            _db = db;
            _photoService = photoService;
        }

        private IQueryable<Tbl_Product> ActiveProductQuery => _db.Products
            .AsNoTracking()
            .Where(p => !p.DeleteFlag && p.IsActive);

        #region get product with pagination
        public async Task<Result<ProductListResponseDTO>> GetProductsAsync(int pageNo, int pageSize)
        {
            try
            {
                if (pageSize <= 0) return Result<ProductListResponseDTO>.SystemError("Page size must be greater than 0.");
                var totalItems = await ActiveProductQuery
                    .AsNoTracking()
                    .CountAsync();

                 var pageCount = totalItems / pageSize;
                 if (totalItems % pageSize > 0) pageCount++;

                var products = await ActiveProductQuery
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
                        ImageId = p.ImageId,
                        ImageUrl = p.ImageUrl,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        DeleteFlag = p.DeleteFlag,
                        IsActive = p.IsActive,
                        Version = p.xmin,
                    })
                    .ToListAsync();

                var result = new ProductListResponseDTO
                {
                    Items = products,
                    PageSetting = new PageSettingDTO(pageNo, pageSize, pageCount)
                };

                return Result<ProductListResponseDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<ProductListResponseDTO>.SystemError($"Error: {ex.Message}");
            }
        }
        #endregion

        #region get active products by id
        public async Task<Result<ProductDTO>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await ActiveProductQuery
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product is null) return Result<ProductDTO>.NotFound("Product not found");

                var data = new ProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    ImageUrl= product.ImageUrl,
                    ImageId= product.ImageId,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    IsActive = product.IsActive,
                    DeleteFlag = product.DeleteFlag,
                    Version = product.xmin
                };

                return Result<ProductDTO>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<ProductDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region get available products
        public async Task<Result<List<ProductDTO>>> GetAvailableProductsAsync()
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
                        ImageUrl = p.ImageUrl,
                        ImageId= p.ImageId,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        IsActive = p.IsActive,
                        DeleteFlag = p.DeleteFlag,
                        Version = p.xmin
                    })
                    .ToListAsync();

                return Result<List<ProductDTO>>.Success(products);
            }
            catch (Exception ex)
            {
                return Result<List<ProductDTO>>.SystemError(ex.Message);
            }
        }
        #endregion

        #region create product
        public async Task<Result<ProductDTO>> CreateProductAsync(CreateProductDTO request, int userId)
        {
            try
            {
                var duplicateProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == request.Name.Trim().ToLower() && !p.DeleteFlag);

                if (duplicateProduct) return Result<ProductDTO>.SystemError("Product with the same name already exists.");

                var categoryExists = await _db.Categories
                    .AnyAsync(c => c.Id == request.CategoryId && !c.DeleteFlag);

                if (!categoryExists) return Result<ProductDTO>.SystemError("Category not found");

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
                    IsActive = newProduct.IsActive,
                    Version = newProduct.xmin
                };

                return Result<ProductDTO>.Success(data, "Product created successfully.");
            }
            catch (Exception ex)
            {
                return Result<ProductDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region create product with photo upload
        public async Task<Result<ProductDTO>> CreateProductWithPhotoAsync(CreateProductDTO request, Stream photoStream, string fileName, int userId)
        {
            
                var duplicateProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == request.Name.Trim().ToLower() && !p.DeleteFlag);

                if (duplicateProduct) return Result<ProductDTO>.SystemError("Product with the same name already exists.");

                var categoryExists = await _db.Categories
                    .AnyAsync(c => c.Id == request.CategoryId && !c.DeleteFlag);

                if (!categoryExists) return Result<ProductDTO>.SystemError("Category not found");

                string photoUrl = null;
                string photoPublicId = null;

                if (photoStream != null)
                {
                    var uploadFileName = string.IsNullOrWhiteSpace(fileName) ? request.Name : fileName;
                    var uploadResult = await _photoService.UploadPhotoAsync(photoStream, uploadFileName);

                    if (uploadResult == null || uploadResult.Error != null || uploadResult.SecureUrl == null)
                    {
                        var uploadError = uploadResult?.Error?.Message;
                        var message = string.IsNullOrWhiteSpace(uploadError)
                            ? "Photo upload failed."
                            : $"Photo upload failed: {uploadError}";
                        return Result<ProductDTO>.SystemError(message);
                    }

                    photoUrl = uploadResult.SecureUrl.ToString();
                    photoPublicId = uploadResult.PublicId;
                }
            try
            {
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
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = photoUrl,
                    ImageId = photoPublicId
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
                    IsActive = newProduct.IsActive,
                    Version = newProduct.xmin,
                    ImageUrl = newProduct.ImageUrl,
                    ImageId = newProduct.ImageId
                };

                return Result<ProductDTO>.Success(data, "Product created successfully.");
            }
            catch (Exception ex)
            {
                // if db save fail, rollback cloud upload
                if (!string.IsNullOrEmpty(photoPublicId))
                {
                    await _photoService.DeletePhotoAsync(photoPublicId);
                }

                return Result<ProductDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region bulk insert product
        public async Task<Result<List<ProductDTO>>> BulkCreateProductsAsync(List<CreateProductDTO> request, int userId)
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
                    IsActive = p.IsActive,
                    Version = p.xmin
                }).ToList();

                return Result<List<ProductDTO>>.Success(data, $"{data.Count} products created successfully.");
            }
            catch (Exception ex)
            {
                return Result<List<ProductDTO>>.SystemError(ex.Message);
            }
        }
        #endregion

        #region update product
        public async Task<Result<ProductDTO>> UpdateProductAsync(int id, UpdateProductDTO request, Stream photoStream, string fileName, int userId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product is null || product.DeleteFlag == true)
                    return Result<ProductDTO>.NotFound("Product not found");

                // Set the RowVersion from the client request so EF Core can detect concurrent modifications
                _db.Entry(product).Property(p => p.xmin).OriginalValue = request.Version;

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var isDuplicate = await _db.Products.AnyAsync(p =>
                        p.Id != id &&
                        !p.DeleteFlag &&
                        p.Name != null &&
                        p.Name.ToLower() == request.Name.Trim().ToLower());

                    if (isDuplicate)
                        return Result<ProductDTO>.SystemError("Another product with the same name already exists.");

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

                string oldImageId = null;
                if (photoStream != null)
                {
                    var uploadResult = await _photoService.UploadPhotoAsync(photoStream, fileName);
                    if (uploadResult == null || uploadResult.Error != null || uploadResult.SecureUrl == null)
                    {
                        var uploadError = uploadResult?.Error?.Message;
                        var message = string.IsNullOrWhiteSpace(uploadError)
                            ? "Photo upload failed."
                            : $"Photo upload failed: {uploadError}";
                        return Result<ProductDTO>.SystemError(message);
                    }

                    oldImageId = product.ImageId;

                    product.ImageUrl = uploadResult.SecureUrl.ToString();
                    product.ImageId = uploadResult.PublicId;
                }

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
                    CategoryId = product.CategoryId,
                    Version = product.xmin
                };

                return Result<ProductDTO>.Success(data, "Product updated successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<ProductDTO>.SystemError("The product was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                return Result<ProductDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region delete product
        public async Task<Result<bool>> DeleteProductAsync(int id, uint version, int userId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product is null || product.DeleteFlag)
                    return Result<bool>.NotFound("Product not found");

                _db.Entry(product).Property(p => p.xmin).OriginalValue = version;

                product.DeleteFlag = true;
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                return Result<bool>.Success(true, "Product deleted successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<bool>.SystemError("The product was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }
        #endregion

        #region Search Products By Term
        public async Task<Result<List<ProductDTO>>> GetProductsByTermAsync(string term)
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

                return Result<List<ProductDTO>>.Success(products);
            }
            catch (Exception ex)
            {
                return Result<List<ProductDTO>>.SystemError(ex.Message);
            }
        }
        #endregion

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
