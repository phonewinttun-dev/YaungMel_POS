using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using YaungMel_POS.Domain.DTOs;
using System;
using YaungMel_POS.Shared;


namespace YaungMel_POS.Domain.Features.Sale
{
    [Route("api/sales")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _service;

        public SalesController(ISaleService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            }
            catch
            {
                return 0;
            }
        }

        // GET: api/sales/paged?pageNo=1&pageSize=10
        [HttpGet("paged")]
        public async Task<IActionResult> Get([FromQuery] PaginationRequest request)
        {
            var result = await _service.GetSalesAsync(request);
            if (!result.IsSuccess)  return BadRequest(result);
            return Ok(result);
        }
        
        // GET: api/sales/{voucherCode}
        [HttpGet("{voucherCode}")]
        public async Task<IActionResult> GetByVoucherCode(string voucherCode)
        {
            var result = await _service.GetSaleByVoucherCodeAsync(voucherCode);
            return Ok(result);
        }

        // POST: api/sales
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSaleDTO createRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _service.CreateSaleAsync(createRequest, GetCurrentUserId());

                if (!result.IsSuccess || result.Data == null)
                {
                    return BadRequest(result);
                }

                //if (result.Data == null)
                //{
                //    return BadRequest();
                //}

                return CreatedAtAction(
                    nameof(GetByVoucherCode),
                    new { voucherCode = result.Data.VoucherCode },
                    result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result<object>.SystemError($"Internal Server Error: {ex.Message}"));
            }
        }
    }
}
