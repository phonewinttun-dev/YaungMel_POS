using Microsoft.EntityFrameworkCore;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Search
{
    public class SearchService : ISearchService
    {
        private readonly POSDbContext _db;

        public SearchService(POSDbContext db)
        {
            _db = db;
        }

        private IQueryable<Tbl_Product> ActiveProductQuery => _db.Products
            .AsNoTracking()
            .Where(p => !p.DeleteFlag);

        private IQueryable<Tbl_Category> ActiveCategoryQuery => _db.Categories
            .AsNoTracking()
            .Where(c => !c.DeleteFlag);

        public async Task<Result<List<CategoryDTO>>> SearchCategoryAsync(SearchCategoryRequestDTO request)
        {
            if (request == null)
                return Result<List<CategoryDTO>>.ValidationError("Search request cannot be null.");

            try
            {
                var query = ActiveCategoryQuery;

                if (!string.IsNullOrWhiteSpace(request.Name))
                    query = query.Where(c => c.Name.ToLower().Contains(request.Name.ToLower()));

                query = request.IsDescending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name);

                var categories = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                    })
                    .ToListAsync();

                return Result<List<CategoryDTO>>.Success(categories);
            }
            catch (Exception ex)
            {
                return Result<List<CategoryDTO>>.SystemError(ex.Message);
            }
        }

        public async Task<PagedResult<ProductDTO>> SearchProductsAsync(SearchProductRequestDTO request)
        {
            if (request == null)
                return PagedResult<ProductDTO>.ValidationError("Search request cannot be null.");

            try
            {
                var query = ActiveProductQuery;

                // Filtering
                if (request.CategoryId.HasValue)
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);

                if (request.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= request.MinPrice.Value);

                if (request.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);

                if (request.StartDate.HasValue)
                    query = query.Where(p => p.CreatedAt >= request.StartDate.Value);

                if (request.EndDate.HasValue)
                    query = query.Where(p => p.CreatedAt <= request.EndDate.Value);

                if (request.MinStockQuantity.HasValue)
                    query = query.Where(p => p.StockQuantity >= request.MinStockQuantity.Value);

                if (request.MaxStockQuantity.HasValue)
                    query = query.Where(p => p.StockQuantity <= request.MaxStockQuantity.Value);

                if (!string.IsNullOrWhiteSpace(request.Name))
                    query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));

                // Sorting
                query = request.SortBy switch
                {
                    SearchProductRequestDTO.SortOptions.name
                        => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),

                    SearchProductRequestDTO.SortOptions.price
                        => request.IsDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),

                    SearchProductRequestDTO.SortOptions.createdDate
                        => request.IsDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),

                    _ => query.OrderBy(p => p.Name)   // Default
                };

                var totalCount = await query.CountAsync();

                var products = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        PriceFormatted = p.Price.ToString("N0") + " MMK",
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        DeleteFlag = p.DeleteFlag,
                        ImageUrl = p.ImageUrl,
                        IsActive = p.IsActive,
                    })
                    .ToListAsync();

                var pagination = new Pagination(request.PageNumber, request.PageSize, totalCount);

                return PagedResult<ProductDTO>.Success(products, pagination);
            }
            catch (Exception ex)
            {
                return PagedResult<ProductDTO>.SystemError(ex.Message);
            }
        }
    }
}