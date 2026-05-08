using System;
using System.Collections.Generic;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;

namespace YaungMel_POS.Domain.Features.Dashboard;

public interface IDashboardService
{
    Task<Result<SalesOverviewDTO>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate);
    Task<Result<SalesPerPeriodDTO>> GetSalesPerPeriodAsync(string period);
    Task<Result<SalesReportDTO>> GetSalesReportAsync(string range);
    Task<Result<List<TopProductDTO>>> GetTopProductsAsync(int top = 10);
}
