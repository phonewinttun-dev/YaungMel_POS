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
    [Authorize(Roles = "Admin, Staff")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET : api/reports
        [HttpGet]
        public async Task<IActionResult> GenerateDetailedDailyReport([FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                return BadRequest(new { isSuccess = false, message = "Invalid date format. Please use YYYY-MM-DD." });
            }

            try
            {
                var result = await _reportService.GenerateDetailedDailyPdfAsync(parsedDate);
                if (result == null || result.Length == 0)
                {
                    return NotFound(new { isSuccess = false, message = "Report data not found for the specified date." });
                }

                return File(result, "application/pdf", $"Daily_Report_{parsedDate:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET : api/reports/range
        [HttpGet("range")]
        public async Task<IActionResult> GenerateDetailedRangeReport([FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (!DateTime.TryParse(startDate, out var parsedStart) || !DateTime.TryParse(endDate, out var parsedEnd))
            {
                return BadRequest(new { isSuccess = false, message = "Invalid date range format. Please use YYYY-MM-DD." });
            }

            try
            {
                var result = await _reportService.GenerateDetailedRangePdfAsync(parsedStart, parsedEnd);
                if (result == null || result.Length == 0)
                {
                    return NotFound(new { isSuccess = false, message = "Report data not found for the specified range." });
                }

                return File(result, "application/pdf", $"Range_Report_{parsedStart:yyyy-MM-dd}_to_{parsedEnd:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
