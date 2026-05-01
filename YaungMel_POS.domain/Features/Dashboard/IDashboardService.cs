using System;
using System.Collections.Generic;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Dashboard;

public interface IDashboardService
{
    ApiResponse<SalesOverviewDTO> GetSalesOverview(DateTime startDate, DateTime endDate);
    ApiResponse<SalesPerPeriodDTO> GetSalesPerPeriod(string period);
    ApiResponse<SalesReportDTO> GetSalesReport(string range);
    ApiResponse<List<TopProductDTO>> GetTopProducts(int top = 10);
}
