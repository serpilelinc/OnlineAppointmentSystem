using AppointmentApi.DTOs;
using AppointmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _service;

        public AppointmentsController(AppointmentService service)
        {
            _service = service;
        }

        // CUSTOMER - Yeni randevu oluştur
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult<AppointmentResponseDto>> Create(
            CreateAppointmentDto dto)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var result = await _service.CreateAsync(
                dto,
                userId
            );

            return Ok(result);
        }

        // CUSTOMER - Kendi randevularını getir
        [Authorize(Roles = "Customer")]
        [HttpGet("my")]
        public async Task<ActionResult<List<AppointmentResponseDto>>>
            GetMyAppointments()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var appointments =
                await _service.GetMyAppointmentsAsync(userId);

            return Ok(appointments);
        }

        // CUSTOMER - Kendi randevusunu iptal et
        [Authorize(Roles = "Customer")]
        [HttpPatch("my/{id:int}/cancel")]
        public async Task<ActionResult<AppointmentResponseDto>>
            CancelMyAppointment(int id)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var appointment =
                await _service.CancelMyAppointmentAsync(
                    id,
                    userId
                );

            return Ok(appointment);
        }

        // ADMIN - Tüm randevular
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AppointmentResponseDto>>>
            GetAll()
        {
            var appointments =
                await _service.GetAllAsync();

            return Ok(appointments);
        }

        // ADMIN - Randevu detayı
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentResponseDto>>
            GetById(int id)
        {
            var appointment =
                await _service.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        // ADMIN - Randevu düzenle
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AppointmentResponseDto>> Update(
            int id,
            UpdateAppointmentDto dto)
        {
            var appointment =
                await _service.UpdateAsync(id, dto);

            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        // ADMIN - Randevu sil
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted =
                await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // ADMIN + STAFF - Durum değiştir
        [Authorize(Roles = "Admin,Staff")]
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<AppointmentResponseDto>> UpdateStatus(
            int id,
            UpdateAppointmentStatusDto dto)
        {
            var role =
                User.FindFirstValue(ClaimTypes.Role);

            if (role == "Staff")
            {
                var userIdValue =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    );

                if (!int.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized();
                }

                var staffId =
                    await _service.GetStaffIdByUserIdAsync(
                        userId
                    );

                var appointment =
                    await _service.GetByIdAsync(id);

                if (appointment == null)
                {
                    return NotFound();
                }

                if (appointment.StaffId != staffId)
                {
                    return Forbid();
                }
            }

            var updatedAppointment =
                await _service.UpdateStatusAsync(
                    id,
                    dto
                );

            return Ok(updatedAppointment);
        }

        // STAFF - Kendi randevuları
        [Authorize(Roles = "Staff")]
        [HttpGet("staff/my")]
        public async Task<ActionResult<List<AppointmentResponseDto>>>
            GetMyStaffAppointments()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var appointments =
                await _service.GetMyStaffAppointmentsAsync(
                    userId
                );

            return Ok(appointments);
        }

        // ADMIN - Personele göre randevular
        [Authorize(Roles = "Admin")]
        [HttpGet("staff/{staffId:int}")]
        public async Task<ActionResult<List<AppointmentResponseDto>>>
            GetByStaff(int staffId)
        {
            var appointments =
                await _service.GetByStaffAsync(staffId);

            return Ok(appointments);
        }

        // ADMIN - Tarihe göre
        [Authorize(Roles = "Admin")]
        [HttpGet("date/{date}")]
        public async Task<ActionResult<List<AppointmentResponseDto>>>
            GetByDate(DateTime date)
        {
            var appointments =
                await _service.GetByDateAsync(date);

            return Ok(appointments);
        }

        // ADMIN - Duruma göre
        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<AppointmentResponseDto>>>
            GetByStatus(string status)
        {
            var appointments =
                await _service.GetByStatusAsync(status);

            return Ok(appointments);
        }

        // ADMIN - Filtreleme / pagination
        [Authorize(Roles = "Admin")]
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            [FromQuery] int? staffId,
            [FromQuery] DateTime? date,
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var appointments =
                await _service.FilterAsync(
                    staffId,
                    date,
                    status,
                    startDate,
                    endDate,
                    page,
                    pageSize,
                    search
                );

            return Ok(appointments);
        }

        // Randevu oluşturma ekranında gerekli
        [HttpGet("available-slots")]
        public async Task<ActionResult<List<AvailableSlotDto>>>
            GetAvailableSlots(
                int staffId,
                int serviceTypeId,
                DateTime date)
        {
            var slots =
                await _service.GetAvailableSlotsAsync(
                    staffId,
                    serviceTypeId,
                    date
                );

            return Ok(slots);
        }

        // ADMIN Dashboard
        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardDto>>
            GetDashboard()
        {
            var dashboard =
                await _service.GetDashboardAsync();

            return Ok(dashboard);
        }
    }
}