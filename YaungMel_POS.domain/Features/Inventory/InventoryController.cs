using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Inventory
{
    [Route("api/inventory")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        [HttpPatch("increase-stock")]
        public async Task<IActionResult> IncreaseStock([FromBody] StockAdjustmentDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Invalid request data."));

            var result = await _service.IncreaseStockAsync(request.ProductId, request.Quantity);

            return Ok(result);
        }

        [HttpPatch("reduce-stock")]
        public async Task<IActionResult> ReduceStock([FromBody] StockAdjustmentDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Invalid request data."));

            var result = await _service.DecreaseStockAsync(request.ProductId, request.Quantity);

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
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Invalid request data."));

            var result = await _service.UpdatePriceAsync(request.ProductId, request.NewPrice);

            return Ok(result);
        }
    }
}
