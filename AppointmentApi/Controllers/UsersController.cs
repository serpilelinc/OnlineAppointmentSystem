using AppointmentApi.Data;
using AppointmentApi.DTOs;
using AppointmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public UsersController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();

            // Yeni bir JWT üretip gönderelim ki Frontend güncel claim'leri alsın
            // Pratiklik açısından AuthService'deki Login yapısına benzer bir dto dönebiliriz.
            // Ama basitçe 200 OK dönebiliriz, Web projesi token'ı yenilemek için Refresh çağırabilir veya Session'da kendi güncelleyebilir.
            return Ok(new { message = "Profil başarıyla güncellendi." });
        }

        [HttpPut("notifications")]
        public async Task<IActionResult> UpdateNotifications([FromBody] UpdateNotificationsDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;
            user.SmsNotificationsEnabled = dto.SmsNotificationsEnabled;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim tercihleri başarıyla güncellendi." });
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var success = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Mevcut şifrenizi yanlış girdiniz." });
            }

            return Ok(new { message = "Şifreniz güvenli bir şekilde değiştirildi." });
        }
    }
}
