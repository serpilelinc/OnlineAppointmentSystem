using AppointmentApi.DTOs;
using AppointmentApi.Services;
using Microsoft.AspNetCore.Mvc;
using AppointmentApi.Models;
using Microsoft.AspNetCore.Authorization;
namespace AppointmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly StaffService _service;

        public StaffController(StaffService service)
        {
            _service = service;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<StaffResponseDto>> Create(
                    CreateStaffDto dto)
        {
            var staff = await _service.CreateAsync(dto);

            return Ok(staff);
        }

        [HttpGet]
        public async Task<ActionResult<List<StaffResponseDto>>> GetAll()
        {
            var staffs = await _service.GetAllAsync();

            return Ok(staffs);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StaffResponseDto>> GetById(int id)
        {
            var staff = await _service.GetByIdAsync(id);

            return Ok(staff);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("by-service/{serviceTypeId:int}")]
        public async Task<ActionResult<List<StaffResponseDto>>> GetByService(
    [FromRoute] int serviceTypeId)
        {
            var staffs =
                await _service.GetByServiceTypeAsync(serviceTypeId);

            return Ok(staffs);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-service")]
        public async Task<IActionResult> AssignService(
            AssignServiceToStaffDto dto)
        {
            await _service.AssignServiceAsync(dto);

            return Ok(new
            {
                message = "Hizmet personele başarıyla atandı."
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("working-hours")]
        public async Task<IActionResult> AddWorkingHour(
            CreateStaffWorkingHourDto dto)
        {
            await _service.AddWorkingHourAsync(dto);

            return Ok(new
            {
                message = "Çalışma saati başarıyla eklendi."
            });
        }

        [HttpGet("{staffId:int}/services")]
        public async Task<ActionResult<List<ServiceTypeResponseDto>>> GetServicesByStaff(
            [FromRoute] int staffId)
        {
            var services =
                await _service.GetServicesByStaffAsync(staffId);

            return Ok(services);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{staffId:int}/working-hours")]
        public async Task<ActionResult<List<StaffWorkingHour>>> GetWorkingHours(
            [FromRoute] int staffId)
        {
            var workingHours =
                await _service.GetWorkingHoursByStaffAsync(staffId);

            return Ok(workingHours);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("working-hours/{id:int}")]
        public async Task<IActionResult> DeleteWorkingHour(int id)
        {
            await _service.DeleteWorkingHourAsync(id);

            return Ok(new
            {
                message = "Çalışma saati başarıyla silindi."
            });
        }
        [HttpGet("available")]
        public async Task<ActionResult<List<StaffResponseDto>>> GetAvailableStaff(
    [FromQuery] int serviceTypeId,
    [FromQuery] DateTime date)
        {
            var staffs =
                await _service.GetAvailableStaffAsync(
                    serviceTypeId,
                    date
                );

            return Ok(staffs);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<StaffResponseDto>> Update(
            int id,
            UpdateStaffDto dto)
        {
            var staff =
                await _service.UpdateAsync(id, dto);

            return Ok(staff);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{staffId:int}/services/{serviceTypeId:int}")]
        public async Task<IActionResult> RemoveService(
            int staffId,
            int serviceTypeId)
        {
            await _service.RemoveServiceAsync(
                staffId,
                serviceTypeId
            );

            return Ok(new
            {
                message = "Hizmet ataması kaldırıldı."
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);

            return Ok(new
            {
                message = "Hizmet veren başarıyla silindi."
            });
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("my-working-hours")]
        public async Task<ActionResult<List<StaffWorkingHour>>> GetMyWorkingHours()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var staffId = await _service.GetStaffIdByUserIdAsync(userId);
            var workingHours = await _service.GetWorkingHoursByStaffAsync(staffId);

            return Ok(workingHours);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost("my-working-hours")]
        public async Task<IActionResult> UpdateMyWorkingHours(List<CreateStaffWorkingHourDto> dtos)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var staffId = await _service.GetStaffIdByUserIdAsync(userId);
            
            await _service.DeleteAllWorkingHoursByStaffAsync(staffId);
            
            foreach (var dto in dtos)
            {
                dto.StaffId = staffId;
                await _service.AddWorkingHourAsync(dto);
            }

            return Ok(new { message = "Çalışma saatleri başarıyla güncellendi." });
        }
    }
}