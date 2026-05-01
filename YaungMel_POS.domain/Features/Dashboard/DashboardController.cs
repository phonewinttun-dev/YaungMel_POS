using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YaungMel_POS.shared.Responses;
using System;

namespace YaungMel_POS.domain.Features.Dashboard;

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
    public IActionResult GetSalesOverview([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = _service.GetSalesOverview(startDate, endDate);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("sales-per-period")]
    public IActionResult GetSalesPerPeriod([FromQuery] string period = "day")
    {
        var result = _service.GetSalesPerPeriod(period);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("report")]
    public IActionResult GetSalesReport([FromQuery] string range = "1month")
    {
        var result = _service.GetSalesReport(range);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("top-products")]
    public IActionResult GetTopProducts([FromQuery] int top = 10)
    {
        var result = _service.GetTopProducts(top);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
