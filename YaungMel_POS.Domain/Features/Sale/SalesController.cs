using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;


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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        // GET: api/sales/paged?pageNo=1&pageSize=10
        [HttpGet("paged")]
        public async Task<IActionResult> GetSalesPaged([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNo <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number and page size must be greater than zero.");
            }
            var result = await _service.GetSalesAsync(pageNo, pageSize);

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
            if (!ModelState.IsValid)
                return BadRequest(Result<object>.SystemError("Invalid sale data."));

            var result = await _service.CreateSaleAsync(createRequest, GetCurrentUserId());

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetByVoucherCode),
                new { voucherCode = result.Data!.VoucherCode },
                result);
        }
    }
}
