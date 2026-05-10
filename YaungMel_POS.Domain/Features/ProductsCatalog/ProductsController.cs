using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;


namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    [Route("api/products")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        // GET: api/products/paged?pageNo=1&pageSize=10
        [HttpGet("paged")]
        public async Task<IActionResult> Get([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNo <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number and page size must be greater than zero.");
            }
            var result = await _service.GetAsync(pageNo, pageSize);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        // POST: api/products/
        [Authorize(Roles = "Admin")]
        [HttpPost()]
        public async Task<IActionResult> Create([FromForm] CreateProductDTO createRequest, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Check if a file was provided (optional)
            Stream? stream = null;
            string fileName = string.Empty;
            if (photoFile != null && photoFile.Length > 0)
            {
                stream = photoFile.OpenReadStream();
                fileName = string.IsNullOrWhiteSpace(photoFile.FileName) ? "uploaded-photo" : photoFile.FileName;
            }

            var result = await _service.CreateAsync(createRequest, stream, fileName, GetCurrentUserId());

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                result);
        }

        // this endpoint is just for testing
        // POST: api/products/bulk
        [Authorize(Roles = "Admin")]
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CreateProductDTO> bulkRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.BulkCreateAsync(bulkRequest, GetCurrentUserId());

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // PATCH: api/products/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDTO updateRequest, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var stream = photoFile?.Length > 0 ? photoFile.OpenReadStream() : null;
            var fileName = string.Empty;
            if (photoFile != null && photoFile.Length > 0)
            {
                fileName = string.IsNullOrWhiteSpace(photoFile.FileName) ? "uploaded-photo" : photoFile.FileName;
            }

            var result = await _service.UpdateAsync(id, updateRequest, stream, fileName, GetCurrentUserId());

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);

            return Ok(result);
        }

        // DELETE: api/products/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] uint version)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<object>.SystemError("Invalid product ID."));

            var result = await _service.DeleteAsync(id, version, GetCurrentUserId());

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);

            return Ok(result);
        }

    }

}
