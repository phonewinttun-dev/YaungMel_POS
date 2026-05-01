using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.database.Data;
using YaungMel_POS.database.Models;
using YaungMel_POS.domain.DTOs;
using static YaungMel_POS.domain.DTOs.SearchProductRequestDTO;

namespace YaungMel_POS.domain.Features.Search
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

        public Task<List<CategoryDTO>> SearchCategoryAsync(SearchCategoryRequestDTO searchRequest)
        {
            var query = ActiveCategoryQuery.AsQueryable();

            if(searchRequest.Name != null)
                query = query.Where(c => c.Name.ToLower().Contains(searchRequest.Name.ToLower()));

            if(searchRequest.IsDescending)
                query = query.OrderByDescending(c => c.Name);
            else
                query = query.OrderBy(c => c.Name);

            var categories = query
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                })
                .ToListAsync();

           return categories;
        }

        public async Task<List<ProductDTO>> SearchProductsAsync(SearchProductRequestDTO searchRequest)
        {
            var query = ActiveProductQuery.AsQueryable();

            if(searchRequest.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == searchRequest.CategoryId.Value);

            if(searchRequest.MinPrice.HasValue)
                query = query.Where(p => p.Price >= searchRequest.MinPrice.Value);

            if(searchRequest.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= searchRequest.MaxPrice.Value);

            if(searchRequest.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= searchRequest.StartDate.Value);

            if(searchRequest.EndDate.HasValue)
                query = query.Where(p => p.CreatedAt <= searchRequest.EndDate.Value);

            if(searchRequest.MinStockQuantity.HasValue)
                query = query.Where(p => p.StockQuantity >= searchRequest.MinStockQuantity.Value);

            if(searchRequest.MaxStockQuantity.HasValue)
                query = query.Where(p => p.StockQuantity <= searchRequest.MaxStockQuantity.Value);

            if (!string.IsNullOrEmpty(searchRequest.Name))
                query = query.Where(p => p.Name.ToLower().Contains(searchRequest.Name.ToLower()));

            query = searchRequest.SortBy switch
            {
                SortOptions.name => searchRequest.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                SortOptions.price => searchRequest.IsDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                SortOptions.createdDate => searchRequest.IsDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query
            };

            var products = await query
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    DeleteFlag = p.DeleteFlag
                }).ToListAsync();

            return products;
        }

        
    }
}
