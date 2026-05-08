using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YaungMel_POS.Shared.Responses;
using System;
using System.Threading.Tasks;

namespace YaungMel_POS.Domain.Features.Dashboard;

[Route("api/dashboard")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetSalesOverview([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.GetSalesOverviewAsync(startDate, endDate);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("sales-per-period")]
    public async Task<IActionResult> GetSalesPerPeriod([FromQuery] string period = "day")
    {
        var result = await _service.GetSalesPerPeriodAsync(period);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("report")]
    public async Task<IActionResult> GetSalesReport([FromQuery] string range = "1month")
    {
        var result = await _service.GetSalesReportAsync(range);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int top = 10)
    {
        var result = await _service.GetTopProductsAsync(top);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
