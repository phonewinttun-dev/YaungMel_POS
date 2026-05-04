using System;
using System.Collections.Generic;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Dashboard;

public interface IDashboardService
{
    Result<SalesOverviewDTO> GetSalesOverview(DateTime startDate, DateTime endDate);
    Result<SalesPerPeriodDTO> GetSalesPerPeriod(string period);
    Result<SalesReportDTO> GetSalesReport(string range);
    Result<List<TopProductDTO>> GetTopProducts(int top = 10);
}
