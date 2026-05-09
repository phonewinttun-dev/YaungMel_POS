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
        public async Task<IActionResult> GetProductsPaged([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNo <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number and page size must be greater than zero.");
            }
            var result = await _service.GetProductsAsync(pageNo, pageSize);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetProductByIdAsync(id);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        // GET: api/products/availableProducts
        [HttpGet("availableProducts")]
        public async Task<IActionResult> GetAvailable()
        {
            var result = await _service.GetAvailableProductsAsync();
            return Ok(result);
        }

        // POST: api/products/
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateProductDTO createRequest)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = await _service.CreateProductAsync(createRequest, GetCurrentUserId());

        //    if (!result.IsSuccess)
        //        return BadRequest(result);

        //    return CreatedAtAction(
        //        nameof(GetById),
        //        new { id = result.Data!.Id },
        //        result);
        //}

        [Authorize(Roles = "Admin")]
        [HttpPost()]
        public async Task<IActionResult> Create([FromForm] CreateProductDTO createRequest, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Check if a file was actually provided
            if (photoFile == null || photoFile.Length == 0)
            {
                return BadRequest("Please provide a product photo.");
            }

            // 2. Open a read stream from the IFormFile
            using var stream = photoFile.OpenReadStream();

            // 3. Pass the stream and the filename to service
            var fileName = string.IsNullOrWhiteSpace(photoFile.FileName) ? "uploaded-photo" : photoFile.FileName;
            var result = await _service.CreateProductAsync(createRequest, stream, fileName, GetCurrentUserId());

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

            var result = await _service.BulkCreateProductsAsync(bulkRequest, GetCurrentUserId());

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

            var result = await _service.UpdateProductAsync(id, updateRequest, stream, fileName, GetCurrentUserId());

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

            var result = await _service.DeleteProductAsync(id, version, GetCurrentUserId());

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);

            return Ok(result);
        }

        // GET : api/products/search?term=searchTerm
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(term);

            var result = await _service.GetProductsByTermAsync(term);

            if (!result.IsSuccess)
                return StatusCode(500, result);

            return Ok(result);
        }


    }

}
