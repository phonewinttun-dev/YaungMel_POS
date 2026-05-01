using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.ProductsCatalog
{
    [ApiController]
    [Route("api/categories")]
    [Authorize(Roles = "Admin,Staff")]
    public class CategoriesController : ControllerBase
    {
        private readonly IProductCatalogService _service;

        public CategoriesController(IProductCatalogService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        // GET: api/categories/
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllCategoriesAsync();
            return Ok(result);
        }

        // GET: api/categories/paged?pageNo=1&pageSize=10
        [HttpGet("paged")]
        public async Task<IActionResult> GetCaetgoryByPage([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNo <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number and page size must be greater than zero.");
            }

            var result = await _service.GetCategoriesAsync(pageNo, pageSize);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetCategoryByIdAsync(id);

            if (!result.IsSuccess) return NotFound(result);

            return Ok(result);
        }

        // POST: api/categories/
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateCategoryAsync(request, GetCurrentUserId());

            if (!result.IsSuccess) return BadRequest(result);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },result);
        }

        // PATCH: api/categories/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateCategoryAsync(id, request, GetCurrentUserId());

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);
            
            return Ok(result);
        }

        // DELETE: api/categories/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {      
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.DeleteCategoryAsync(id, GetCurrentUserId());

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);
            
            return Ok(result);
        }

        // GET: api/categories/search?term=searchTerm
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(ApiResponse<object>.Fail("Search term cannot be empty."));

            var result = await _service.GetCategoriesByTermAsync(term);

            if (!result.IsSuccess)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }
    }
}
