using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;

namespace YaungMel_POS.Domain.Features.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly POSDbContext _db;

    public DashboardService(POSDbContext db)
    {
        _db = db;
    }

    #region [1] Sales Overview
    public async Task<Result<SalesOverviewDTO>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return Result<SalesOverviewDTO>.SystemError("Start date must be before end date.");

            var overview = await _db.Sales
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .GroupBy(s => 1)
                .Select(g => new SalesOverviewDTO
                {
                    TotalRevenue = g.Sum(s => s.TotalPrice),
                    TotalSales = g.Count(),
                    StartDate = startDate,
                    EndDate = endDate
                })
                .FirstOrDefaultAsync() ?? new SalesOverviewDTO
                {
                    TotalRevenue = 0,
                    TotalSales = 0,
                    StartDate = startDate,
                    EndDate = endDate
                };

            return Result<SalesOverviewDTO>.Success(overview);
        }
        catch (Exception ex)
        {
            return Result<SalesOverviewDTO>.SystemError($"Error: {ex.Message}");
        }
    }
    #endregion

    #region [2] Sales Per Period (day, week, month)
    public async Task<Result<SalesPerPeriodDTO>> GetSalesPerPeriodAsync(string period)
    {
        try
        {
            var validPeriods = new[] { "day", "week", "month" };
            var normalizedPeriod = period.ToLower().Trim();

            if (!validPeriods.Contains(normalizedPeriod))
                return Result<SalesPerPeriodDTO>.SystemError("Invalid period. Use 'day', 'week', or 'month'.");

            List<SalesPeriodGroupDTO> groupedData;

            switch (normalizedPeriod)
            {
                case "day":
                    groupedData = await _db.Sales
                        .GroupBy(s => s.CreatedAt.Date)
                        .Select(g => new SalesPeriodGroupDTO
                        {
                            Label = g.Key.ToString("yyyy-MM-dd"),
                            TotalRevenue = g.Sum(s => s.TotalPrice),
                            TotalSales = g.Count()
                        })
                        .OrderBy(g => g.Label)
                        .ToListAsync();
                    break;

                case "week":
                    // Grouping by week in SQL is complex due to different calendar rules.
                    // For now, we filter and group in-memory but we could optimize later if needed.
                    // To avoid OOM, we only fetch the necessary data.
                    var salesData = await _db.Sales
                        .Select(s => new { s.CreatedAt, s.TotalPrice })
                        .ToListAsync();

                    groupedData = salesData
                        .GroupBy(s => new
                        {
                            s.CreatedAt.Year,
                            Week = CultureInfo.CurrentCulture.Calendar
                                .GetWeekOfYear(s.CreatedAt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                        })
                        .Select(g => new SalesPeriodGroupDTO
                        {
                            Label = $"{g.Key.Year} - Week {g.Key.Week}",
                            TotalRevenue = g.Sum(s => s.TotalPrice),
                            TotalSales = g.Count()
                        })
                        .OrderBy(g => g.Label)
                        .ToList();
                    break;

                case "month":
                    groupedData = await _db.Sales
                        .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                        .Select(g => new SalesPeriodGroupDTO
                        {
                            // We construct the label in-memory after fetching grouped data to use MonthName
                            Label = g.Key.Year + "-" + g.Key.Month.ToString("D2"), 
                            TotalRevenue = g.Sum(s => s.TotalPrice),
                            TotalSales = g.Count()
                        })
                        .OrderBy(g => g.Label)
                        .ToListAsync();

                    // Refine labels to use month names
                    foreach (var item in groupedData)
                    {
                        var parts = item.Label.Split('-');
                        var year = int.Parse(parts[0]);
                        var month = int.Parse(parts[1]);
                        item.Label = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
                    }
                    break;

                default:
                    groupedData = new List<SalesPeriodGroupDTO>();
                    break;
            }

            var result = new SalesPerPeriodDTO
            {
                Period = normalizedPeriod,
                Data = groupedData
            };

            return Result<SalesPerPeriodDTO>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<SalesPerPeriodDTO>.SystemError($"Error: {ex.Message}");
        }
    }
    #endregion

    #region [3] Sales Report (1month, 3months, 6months, 9months, 1year)
    public async Task<Result<SalesReportDTO>> GetSalesReportAsync(string range)
    {
        try
        {
            var validRanges = new Dictionary<string, int>
            {
                { "1month", 1 },
                { "3months", 3 },
                { "6months", 6 },
                { "9months", 9 },
                { "1year", 12 }
            };

            var normalizedRange = range.ToLower().Trim();

            if (!validRanges.TryGetValue(normalizedRange, out int monthsBack))
                return Result<SalesReportDTO>.SystemError("Invalid range. Use '1month', '3months', '6months', '9months', or '1year'.");

            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-monthsBack);

            var salesSummary = await _db.Sales
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .GroupBy(s => 1)
                .Select(g => new
                {
                    TotalRevenue = g.Sum(s => s.TotalPrice),
                    TotalSales = g.Count()
                })
                .FirstOrDefaultAsync();

            var monthlySummary = await _db.Sales
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                .Select(g => new 
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(s => s.TotalPrice),
                    Sales = g.Count()
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var result = new SalesReportDTO
            {
                ReportRange = normalizedRange,
                TotalRevenue = salesSummary?.TotalRevenue ?? 0,
                TotalSales = salesSummary?.TotalSales ?? 0,
                MonthlySummary = monthlySummary.Select(m => new SalesReportGroupDTO
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month)} {m.Year}",
                    Revenue = m.Revenue,
                    Sales = m.Sales
                }).ToList()
            };

            return Result<SalesReportDTO>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<SalesReportDTO>.SystemError($"Error: {ex.Message}");
        }
    }
    #endregion

    #region [4] Top Products
    public async Task<Result<List<TopProductDTO>>> GetTopProductsAsync(int top = 10)
    {
        try
        {
            if (top <= 0)
                return Result<List<TopProductDTO>>.SystemError("Top count must be greater than 0.");

            var topProducts = await _db.SaleItems
                .GroupBy(si => new { si.ProductId, si.Product.Name })
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantitySold = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.Quantity * si.Price)
                })
                .OrderByDescending(p => p.TotalQuantitySold)
                .Take(top)
                .ToListAsync();

            return Result<List<TopProductDTO>>.Success(topProducts);
        }
        catch (Exception ex)
        {
            return Result<List<TopProductDTO>>.SystemError($"Error: {ex.Message}");
        }
    }
    #endregion
}
