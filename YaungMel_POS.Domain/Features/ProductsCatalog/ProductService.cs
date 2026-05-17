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
    public class ProductService: IProductService
    {
        private readonly POSDbContext _db;
        private readonly IPhotoService _photoService;

        public ProductService(POSDbContext db, IPhotoService photoService)
        {
            _db = db;
            _photoService = photoService;
        }

        private IQueryable<Tbl_Product> ActiveProductQuery => _db.Products
            .AsNoTracking()
            .Where(p => !p.DeleteFlag && p.IsActive);

        #region get product with pagination
        public async Task<PagedResult<ProductDTO>> GetAsync(PaginationRequest request)
        {
            if (request == null) 
                return PagedResult<ProductDTO>.ValidationError("Page size must be greater than zero");

            try
            {   
                var totalProducts = await ActiveProductQuery.CountAsync();

                if (totalProducts == 0)
                    return PagedResult<ProductDTO>.NotFound("No Product Found!");

                var products = await ActiveProductQuery
                    .AsNoTracking()
                    .OrderByDescending(p => p.Id)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        PriceFormatted = p.Price.ToString("N0"),
                        ImageId = p.ImageId,
                        ImageUrl = p.ImageUrl,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        DeleteFlag = p.DeleteFlag,
                        IsActive = p.IsActive,
                        Version = p.xmin,
                    })
                    .ToListAsync();

                var pagination = new Pagination(request.PageNumber, request.PageSize, totalProducts);
                return PagedResult<ProductDTO>.Success(products, pagination, "Products retrieved successfully!");
            }
            catch (Exception ex)
            {
                return PagedResult<ProductDTO>.SystemError("Errors occured while retrieving data!");
            }
        }
        #endregion

        #region get active products by id
        public async Task<Result<ProductDTO>> GetByIdAsync(int id)
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
                    ImageUrl = product.ImageUrl,
                    ImageId = product.ImageId,
                    Price = product.Price,
                    PriceFormatted = product.Price.ToString("N0"),
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    IsActive = product.IsActive,
                    DeleteFlag = product.DeleteFlag,
                    Version = product.xmin
                };

                return Result<ProductDTO>.Success(data, $"{data.Name} retrieved successfully!");
            }
            catch (Exception ex)
            {
                return Result<ProductDTO>.SystemError("Unexpected error occured.");
            }
        }
        #endregion

        #region create product with photo upload
        public async Task<Result<ProductDTO>> CreateAsync(CreateProductDTO request, Stream? photoStream, string fileName, int userId)
        {
            string photoPublicId = null;
            try
            {
                // Validations
                if (string.IsNullOrWhiteSpace(request.Name))
                    return Result<ProductDTO>.ValidationError("Product name is required!");

                if (request.Price <= 0)
                    return Result<ProductDTO>.ValidationError("Price must be greater than zero!");

                if (request.StockQuantity < 0)
                    return Result<ProductDTO>.ValidationError("Stock quantity cannot be negative!");

                var duplicateProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == request.Name.Trim().ToLower() && !p.DeleteFlag);

                if (duplicateProduct)
                    return Result<ProductDTO>.ValidationError("Product with the same name already exists.");

                var categoryExists = await _db.Categories
                    .AnyAsync(c => c.Id == request.CategoryId && !c.DeleteFlag);

                if (!categoryExists)
                    return Result<ProductDTO>.ValidationError("Category not found");

                // photo upload
                string photoUrl = null;
                if (photoStream != null)
                {
                    using (photoStream)
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
                }

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

        #region update product
        public async Task<Result<ProductDTO>> UpdateAsync(int id, UpdateProductDTO request, Stream? photoStream, string fileName, int userId)
        {
            string? newImageId = null;
            string? oldImageId = null;
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product is null || product.DeleteFlag == true)
                    return Result<ProductDTO>.NotFound("Product not found");

                // Set the RowVersion from the client request so EF Core can detect concurrent modifications
                _db.Entry(product).Property(p => p.xmin).OriginalValue = request.Version;

                // Partial Update: Only update if the value is provided
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

                if (request.Price.HasValue)
                    product.Price = request.Price.Value;

                if (request.StockQuantity.HasValue)
                    product.StockQuantity = request.StockQuantity.Value;

                if (request.CategoryId.HasValue)
                {
                    var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value && !c.DeleteFlag);
                    if (!categoryExists)
                        return Result<ProductDTO>.ValidationError("Selected category does not exist.");
                    
                    product.CategoryId = request.CategoryId.Value;
                }

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

                    // Track IDs for cleanup/rollback
                    oldImageId = product.ImageId;
                    newImageId = uploadResult.PublicId;

                    product.ImageUrl = uploadResult.SecureUrl.ToString();
                    product.ImageId = newImageId;
                }

                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = userId;

                await _db.SaveChangesAsync();

                // Success: Clean up the old photo if it was replaced
                if (!string.IsNullOrEmpty(newImageId) && !string.IsNullOrEmpty(oldImageId))
                {
                    await _photoService.DeletePhotoAsync(oldImageId);
                }

                var data = new ProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    Version = product.xmin,
                    ImageUrl = product.ImageUrl,
                    ImageId = product.ImageId
                };

                return Result<ProductDTO>.Success(data, "Product updated successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                // Rollback: Delete the newly uploaded photo since the DB update failed
                if (!string.IsNullOrEmpty(newImageId))
                {
                    await _photoService.DeletePhotoAsync(newImageId);
                }
                return Result<ProductDTO>.SystemError("The product was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                // Rollback: Delete the newly uploaded photo since the DB update failed
                if (!string.IsNullOrEmpty(newImageId))
                {
                    await _photoService.DeletePhotoAsync(newImageId);
                }
                return Result<ProductDTO>.SystemError(ex.Message);
            }
        }
        #endregion

        #region delete product
        public async Task<Result<bool>> DeleteAsync(int id, uint version, int userId)
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

                return Result<bool>.DeleteSuccess("Product deleted successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<bool>.SystemError("The product is being modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }
        #endregion

    }
}
