using AppointmentApi.DTOs;
using AppointmentApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AppointmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypesController : ControllerBase
    {
        private readonly ServiceTypeService _service;

        public ServiceTypesController(ServiceTypeService service)
        {
            _service = service;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ServiceTypeResponseDto>> Create(
                    CreateServiceTypeDto dto)
        {
            var result = await _service.CreateAsync(dto);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceTypeResponseDto>>> GetAll()
        {
            var serviceTypes = await _service.GetAllAsync();

            return Ok(serviceTypes);
        }
        [HttpGet("by-category/{categoryId:int}")]
        public async Task<ActionResult<List<ServiceTypeResponseDto>>> GetByCategory(
    int categoryId)
        {
            var services =
                await _service.GetByCategoryAsync(categoryId);

            return Ok(services);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceTypeResponseDto>> GetById(int id)
        {
            var serviceType = await _service.GetByIdAsync(id);

            return Ok(serviceType);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ServiceTypeResponseDto>> Update(
    int id,
    UpdateServiceTypeDto dto)
        {
            var result = await _service.UpdateAsync(
                id,
                dto
            );

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);

            return Ok(new
            {
                message = "Hizmet başarıyla silindi."
            });
        }

    }
}