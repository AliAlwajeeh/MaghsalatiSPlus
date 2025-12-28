

using MaghsalatiSPlus.Data;
using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace MaghsalatiSPlus.Controllers
{
    public class CategoryDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;


        public string? ShopOwnerId { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
   
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        private string? GetOwnerIdFromClaims() =>
            User?.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories([FromQuery] string? shopOwnerId = null)
        {
            var ownerId = GetOwnerIdFromClaims() ?? shopOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                return BadRequest(new { Message = "ShopOwnerId is required (pass as query when auth is disabled)." });

            var list = await _context.Categories
                                     .Where(c => c.ShopOwnerId == ownerId)
                                     .AsNoTracking()
                                     .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id, [FromQuery] string? shopOwnerId = null)
        {
            var ownerId = GetOwnerIdFromClaims() ?? shopOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                return BadRequest(new { Message = "ShopOwnerId is required (pass as query when auth is disabled)." });

            var category = await _context.Categories
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(c => c.Id == id && c.ShopOwnerId == ownerId);

            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory([FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ownerId = GetOwnerIdFromClaims() ?? categoryDto.ShopOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                return BadRequest(new { Message = "ShopOwnerId is required (include it in body when auth is disabled)." });

            var newCategory = new Category
            {
                Name = categoryDto.Name,
                ShopOwnerId = ownerId
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id, shopOwnerId = ownerId }, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryDto categoryDto, [FromQuery] string? shopOwnerId = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ownerId = GetOwnerIdFromClaims() ?? shopOwnerId ?? categoryDto.ShopOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                return BadRequest(new { Message = "ShopOwnerId is required (query/body when auth is disabled)." });

            var categoryToUpdate = await _context.Categories
                                                 .FirstOrDefaultAsync(c => c.Id == id && c.ShopOwnerId == ownerId);

            if (categoryToUpdate == null) return NotFound();

            categoryToUpdate.Name = categoryDto.Name;
            _context.Entry(categoryToUpdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id, [FromQuery] string? shopOwnerId = null)
        {
            var ownerId = GetOwnerIdFromClaims() ?? shopOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                return BadRequest(new { Message = "ShopOwnerId is required (pass as query when auth is disabled)." });

            var category = await _context.Categories
                                         .FirstOrDefaultAsync(c => c.Id == id && c.ShopOwnerId == ownerId);

            if (category == null) return NotFound();

            var inUse = await _context.OrderItems.AnyAsync(oi => oi.CategoryId == id);
            if (inUse)
                return BadRequest(new { Message = "This category cannot be deleted because it is used in existing orders." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
