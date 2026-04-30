using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POSSampleOWN.domain.Features.ProductsCatalog;
using POSSampleOWN.database.Models;
using POSSampleOWN.Responses;
using System.Threading.Tasks;
using System.Security.Claims;
using POSSampleOWN.domain.DTOs;


namespace POSSampleOWN.Controllers
{
    [Route("api/products")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductCatalogService _service;

        public ProductsController(IProductCatalogService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        // GET: api/products/
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllProductsAsync();
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO createRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateProductAsync(createRequest, GetCurrentUserId());

            if (!result.IsSuccess)
                return BadRequest(result);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                result);
        }

        // this endpoint is just for testing
        // POST: api/products/bulk
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO updateRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateProductAsync(id, updateRequest, GetCurrentUserId());

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);
            
            return Ok(result);
        }

        // DELETE: api/products/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid product ID."));

            var result = await _service.DeleteProductAsync(id, GetCurrentUserId());

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