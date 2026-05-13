using System;
using System.Collections.Generic;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Dashboard;

public interface IDashboardService
{
    Task<PagedResult<SalesOverviewDTO>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate);
    Task<PagedResult<SalesPerPeriodDTO>> GetSalesPerPeriodAsync(string period);
    Task<PagedResult<SalesReportDTO>> GetSalesReportAsync(string range);
    Task<PagedResult<List<TopProductDTO>>> GetTopProductsAsync(int top = 10);
}
