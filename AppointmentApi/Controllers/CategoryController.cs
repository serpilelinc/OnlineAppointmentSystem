using AppointmentApi.DTOs;
using AppointmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;

        public CategoryController(CategoryService service)
        {
            _service = service;
        }

        // Kategorileri herkes görüntüleyebilir
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetAll()
        {
            var categories = await _service.GetAllAsync();

            return Ok(categories);
        }

        // Sadece Admin kategori oluşturabilir
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> Create(
            CreateCategoryDto dto)
        {
            var category = await _service.CreateAsync(dto);

            return Ok(category);
        }

        // Sadece Admin kategori silebilir
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);

            return Ok(new
            {
                message = "Kategori başarıyla silindi."
            });
        }
    }
}