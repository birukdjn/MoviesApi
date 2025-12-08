using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Data;


namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // Add Category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AddCategory([FromBody] CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Category name cannot be empty." });

            if (_context.Categories.Any(c => c.Name.Equals(dto.Name, StringComparison.CurrentCultureIgnoreCase)))
                return Conflict(new { message = "Category already exists." });

            var category = new Category { Name = dto.Name };
            _context.Categories.Add(category);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        //  Get all categories
        [HttpGet]
        [Authorize(Roles = "Admin,User")] 
        public IActionResult GetCategories()
        {
            var categories = _context.Categories.ToList();
            return Ok(categories);
        }

        // Get category by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public IActionResult GetCategoryById(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        //  Update category
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            category.Name = dto.Name;
            _context.SaveChanges();

            return Ok(category);
        }

        // Delete category
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return NoContent();
        }
    }


    public class CategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
