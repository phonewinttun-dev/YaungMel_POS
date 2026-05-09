using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaungMel_POS.Domain.Features.Report
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET : api/reports
        [HttpGet]
        public async Task<IActionResult> GenerateDetailedDailyReport([FromQuery] DateTime date)
        {
            var result = await _reportService.GenerateDetailedDailyPdfAsync(date);
            if (result == null || result.Length == 0)
            {
                return NotFound();
            }

            return File(result, "application/pdf", $"Daily_Report_{date:yyyy-MM-dd}.pdf");
        }

        // GET : api/reports/range
        [HttpGet("range")]
        public async Task<IActionResult> GenerateDetailedRangeReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _reportService.GenerateDetailedRangePdfAsync(startDate, endDate);
            if (result == null || result.Length == 0)
            {
                return NotFound();
            }

            return File(result, "application/pdf", $"Range_Report_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.pdf");
        }
    }
}
