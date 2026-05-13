using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Inventory
{
    [Route("api/inventory")]
    [ApiController]
    [Authorize(Roles = "Admin, Staff")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpPatch("increase-stock")]
        public async Task<IActionResult> IncreaseStock([FromBody] StockAdjustmentDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(PagedResult<object>.SystemError("Invalid request data."));

            var result = await _service.IncreaseStockAsync(request.ProductId, request.Quantity, GetCurrentUserId());

            return Ok(result);
        }

        [HttpPatch("reduce-stock")]
        public async Task<IActionResult> ReduceStock([FromBody] StockAdjustmentDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(PagedResult<object>.SystemError("Invalid request data."));

            var result = await _service.DecreaseStockAsync(request.ProductId, request.Quantity, GetCurrentUserId());

            return Ok(result);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> LowStock([FromQuery] int lowStock = 5)
        {
            var result = await _service.GetLowStockAlertsAsync(lowStock);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePrice([FromBody] PriceUpdateDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(PagedResult<object>.SystemError("Invalid request data."));

            var result = await _service.UpdatePriceAsync(request.ProductId, request.NewPrice, GetCurrentUserId());

            return Ok(result);
        }
    }
}
