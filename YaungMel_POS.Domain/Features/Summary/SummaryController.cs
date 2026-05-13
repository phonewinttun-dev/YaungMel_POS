using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Summary
{

    [ApiController]
    [Route("api/summaries")]
    [Authorize(Roles = "Admin, Staff")]
    public class SummaryController : ControllerBase
    {
        private readonly ISummaryService _service;

        public SummaryController(ISummaryService service)
        {
            _service = service;
        }

        // POST: api/summaries/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateSummary()
        {
            var result = await _service.CreateSummaryAsync();

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // GET: api/summaries
        // GET: api/summaries/paged?pageNo=1&pageSize=10
        [HttpGet]
        [HttpGet("paged")]
        public async Task<IActionResult> GetSummaryByPagination(
            [FromQuery] int pageNo = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNo <= 0 || pageSize <= 0)
            {
                return BadRequest(PagedResult<object>.SystemError("Page number and page size must be greater than zero."));
            }

            var result = await _service.GetSummaryByPagination(pageNo, pageSize);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // GET: api/summaries/by-date?date=2026-05-07
        [HttpGet("by-date")]
        public async Task<IActionResult> GetSummaryByDate([FromQuery] DateTime date)
        {
            if (date == default)
            {
                return BadRequest(PagedResult<object>.SystemError("Date is required."));
            }

            var result = await _service.GetSummaryByDateAsync(date);

            if (!result.IsSuccess)
            {
                return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/summaries/by-date-range?startDate=2026-05-01&endDate=2026-05-07
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetSummaryByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default)
            {
                return BadRequest(PagedResult<object>.SystemError("Start date and end date are required."));
            }

            var result = await _service.GetSummaryByDateRangeAsync(startDate, endDate);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}