using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YaungMel_POS.database.Data;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly POSDbContext _db;

    public DashboardService(POSDbContext db)
    {
        _db = db;
    }

    #region [1] Sales Overview
    public ApiResponse<SalesOverviewDTO> GetSalesOverview(DateTime startDate, DateTime endDate)
    {
        try
        {

            if (startDate > endDate)
                return ApiResponse<SalesOverviewDTO>.Fail("Start date must be before end date.");


            var sales = _db.Sales
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .ToList();

    
            var overview = new SalesOverviewDTO
            {
                TotalRevenue = sales.Sum(s => s.TotalPrice),
                TotalSales = sales.Count,
                StartDate = startDate,
                EndDate = endDate
            };

            return ApiResponse<SalesOverviewDTO>.Success(overview);
        }
        catch (Exception ex)
        {
            return ApiResponse<SalesOverviewDTO>.Fail($"Error: {ex.Message}");
        }
    }
    #endregion

    #region [2] Sales Per Period (day, week, month)
    public ApiResponse<SalesPerPeriodDTO> GetSalesPerPeriod(string period)
    {
        try
        {

            var validPeriods = new[] { "day", "week", "month" };
            var normalizedPeriod = period.ToLower().Trim();

            if (!validPeriods.Contains(normalizedPeriod))
                return ApiResponse<SalesPerPeriodDTO>.Fail("Invalid period. Use 'day', 'week', or 'month'.");

            var sales = _db.Sales.OrderBy(s => s.CreatedAt).ToList();

            List<SalesPeriodGroupDTO> groupedData;

            switch (normalizedPeriod)
            {
                case "day":
                    groupedData = sales
                        .GroupBy(s => s.CreatedAt.Date)
                        .Select(g => new SalesPeriodGroupDTO
                        {
                            Label = g.Key.ToString("yyyy-MM-dd"),
                            TotalRevenue = g.Sum(s => s.TotalPrice),
                            TotalSales = g.Count()
                        })
                        .OrderBy(g => g.Label)
                        .ToList();
                    break;

                case "week":
                    groupedData = sales
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
                    groupedData = sales
                        .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                        .Select(g => new SalesPeriodGroupDTO
                        {
                            Label = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                            TotalRevenue = g.Sum(s => s.TotalPrice),
                            TotalSales = g.Count()
                        })
                        .OrderBy(g => g.Label)
                        .ToList();
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

            return ApiResponse<SalesPerPeriodDTO>.Success(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<SalesPerPeriodDTO>.Fail($"Error: {ex.Message}");
        }
    }
    #endregion

    #region [3] Sales Report (1month, 3months, 6months, 9months, 1year)

    public ApiResponse<SalesReportDTO> GetSalesReport(string range)
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
                return ApiResponse<SalesReportDTO>.Fail("Invalid range. Use '1month', '3months', '6months', '9months', or '1year'.");

            var endDate = DateTime.Now;
            var startDate = endDate.AddMonths(-monthsBack);

   
            var sales = _db.Sales
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .ToList();

    
            var monthlySummary = sales
                .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                .Select(g => new SalesReportGroupDTO
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                    Revenue = g.Sum(s => s.TotalPrice),
                    Sales = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToList();

            var result = new SalesReportDTO
            {
                ReportRange = normalizedRange,
                TotalRevenue = sales.Sum(s => s.TotalPrice),
                TotalSales = sales.Count,
                MonthlySummary = monthlySummary
            };

            return ApiResponse<SalesReportDTO>.Success(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<SalesReportDTO>.Fail($"Error: {ex.Message}");
        }
    }
    #endregion

    #region [4] Top Products

    public ApiResponse<List<TopProductDTO>> GetTopProducts(int top = 10)
    {
        try
        {
            if (top <= 0)
                return ApiResponse<List<TopProductDTO>>.Fail("Top count must be greater than 0.");

            var topProducts = _db.SaleItems
                .Include(si => si.Product)
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
                .ToList();

            return ApiResponse<List<TopProductDTO>>.Success(topProducts);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TopProductDTO>>.Fail($"Error: {ex.Message}");
        }
    }
    #endregion
}
