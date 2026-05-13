using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Summary;

public class SummaryService : ISummaryService
{
    private readonly POSDbContext _db;

    public SummaryService(POSDbContext db)
    {
        _db = db;
    }

    #region Create Summary
    public async Task<PagedResult<SummaryDTO>> CreateSummaryAsync()
    {
        try
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var now = DateTime.UtcNow;

            var salesSummary = await _db.Sales
                        .Where(s => s.CreatedAt >= today && s.CreatedAt <= now)
                        .GroupBy(s => 1)
                        .Select(g => new
                        {
                            TotalCount = g.Count(),
                            TotalAmount = g.Sum(s => s.TotalPrice)
                        })
                        .FirstOrDefaultAsync();

            var totalSale = salesSummary?.TotalCount ?? 0;
            var totalAmount = salesSummary?.TotalAmount ?? 0;

            var topProduct = await _db.SaleItems
                        .Where(si => si.Sale.CreatedAt >= today && si.Sale.CreatedAt <= now)
                        .GroupBy(si => new { si.ProductId, si.Product.Name })
                        .Select(g => new
                        {
                            ProductId = g.Key.ProductId,
                            ProductName = g.Key.Name,
                            TotalQuantity = g.Sum(x => x.Quantity)
                        })
                        .OrderByDescending(x => x.TotalQuantity)
                        .FirstOrDefaultAsync();

            // Check if summary for today already exists
            var existingSummary = await _db.Summaries
                                          .FirstOrDefaultAsync(s => s.Date == today);

            if (existingSummary != null)
            {
                existingSummary.TotalSale = totalSale;
                existingSummary.TotalAmount = totalAmount;
                existingSummary.TopSaleProductId = topProduct?.ProductId;

                _db.Summaries.Update(existingSummary);
            }
            else
            {
                var summary = new Tbl_Summary
                {
                    Date = today,
                    TotalSale = totalSale,
                    TotalAmount = totalAmount,
                    TopSaleProductId = topProduct?.ProductId
                };
                await _db.Summaries.AddAsync(summary);
            }

            await _db.SaveChangesAsync();

            var resModel = new SummaryDTO
            {
                Date = today,
                TotalSale = totalSale,
                TotalAmount = totalAmount,
                TotalAmountFormatted = totalAmount.ToString("N0"),
                TopSaleProductName = topProduct?.ProductName
            };

            return PagedResult<SummaryDTO>.Success(resModel);
        }
        catch (Exception ex)
        {
            return PagedResult<SummaryDTO>.SystemError(ex.Message);
        }
    }
    #endregion

    #region Get Summary By Pagination
    public async Task<PagedResult<SummaryListResponseModel>> GetSummaryByPagination(int pageNo = 1, int pageSize = 10)
    {
        try
        {
            if (pageSize <= 0) return PagedResult<SummaryListResponseModel>.SystemError("Page size must be greater than 0.");

            var totalItems = await _db.Summaries.CountAsync();
            int pageCount = totalItems / pageSize;
            if (totalItems % pageSize > 0) pageCount++;

            var summaries = await _db.Summaries
                .AsNoTracking()
                .OrderByDescending(s => s.Date)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SummaryDTO
                {
                    Date = s.Date,
                    TotalSale = s.TotalSale,
                    TotalAmount = s.TotalAmount,
                    TotalAmountFormatted = s.TotalAmount.ToString("N0"),
                    TopSaleProductName = s.TopSaleProduct != null ? s.TopSaleProduct.Name : null
                })
                .ToListAsync();

            var resModel = new SummaryListResponseModel
            {
                Items = summaries,
                PageSetting = new PageSettingDTO(pageNo, pageSize, pageCount)

            };
            return PagedResult<SummaryListResponseModel>.Success(resModel);
        }
        catch (Exception ex)
        {
            return PagedResult<SummaryListResponseModel>.SystemError(ex.Message);
        }
    }
    #endregion

    #region Get Summary By Date
    public async Task<PagedResult<SummaryDetailDto>> GetSummaryByDateAsync(DateTime date)
    {
        try
        {
            var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var nextDate = targetDate.AddDays(1);

            var summary = await _db.Summaries
                .Include(s => s.TopSaleProduct)
                .FirstOrDefaultAsync(s => s.Date == targetDate);

            if (summary is null) return PagedResult<SummaryDetailDto>.NotFound("Summary not found for the specified date.");

            var sales = await _db.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Where(s => s.CreatedAt >= targetDate && s.CreatedAt < nextDate)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var detail = new SummaryDetailDto
            {
                Summary = new SummaryDTO
                {
                    Date = summary.Date,
                    TotalSale = summary.TotalSale,
                    TotalAmount = summary.TotalAmount,
                    TotalAmountFormatted = summary.TotalAmount.ToString("N0"),
                    TopSaleProductName = summary.TopSaleProduct?.Name
                },
                Sales = sales.Select(s => new SaleDTO
                {
                    Id = s.Id,
                    VoucherCode = s.VoucherCode,
                    TotalPrice = s.TotalPrice,
                    SaleItems = s.SaleItems.Select(si => new SaleItemDTO
                    {
                        ProductName = si.Product?.Name ?? string.Empty,
                        Quantity = si.Quantity,
                        Price = si.Price,
                        PriceFormatted = si.Price.ToString("N0")
                    }).ToList()
                }).ToList()
            };

            return PagedResult<SummaryDetailDto>.Success(detail);
        }
        catch (Exception ex)
        {
            return PagedResult<SummaryDetailDto>.SystemError(ex.Message);
        }
    }
    #endregion

    #region Get Summary By Date Range
    public async Task<PagedResult<List<SummaryDTO>>> GetSummaryByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

            var summaries = await _db.Summaries
                .AsNoTracking()
                .Where(s => s.Date >= start && s.Date <= end)
                .OrderByDescending(s => s.Date)
                .Select(s => new SummaryDTO
                {
                    Date = s.Date,
                    TotalSale = s.TotalSale,
                    TotalAmount = s.TotalAmount,
                    TotalAmountFormatted = s.TotalAmount.ToString("N0"),
                    TopSaleProductName = s.TopSaleProduct != null ? s.TopSaleProduct.Name : null
                })
                .ToListAsync();

            return PagedResult<List<SummaryDTO>>.Success(summaries);
        }
        catch (Exception ex)
        {
            return PagedResult<List<SummaryDTO>>.SystemError(ex.Message);
        }
    }
    #endregion
}
