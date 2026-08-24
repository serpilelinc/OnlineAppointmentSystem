using AppointmentApi.DTOs;
using AppointmentApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace AppointmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(
            RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
            LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "E-posta veya şifre hatalı."
                });
            }

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("staff")]
        public async Task<ActionResult<AuthResponseDto>> CreateStaffUser(
            CreateStaffUserDto dto)
        {
            var result = await _authService.CreateStaffUserAsync(dto);

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("staff/{staffId:int}/has-account")]
        public async Task<IActionResult> StaffHasAccount(int staffId)
        {
            var hasAccount =
                await _authService.StaffHasUserAccountAsync(staffId);

            return Ok(new
            {
                hasAccount
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiModel)
        {
            if (tokenApiModel == null)
            {
                return BadRequest("Geçersiz istek.");
            }

            var result = await _authService.RefreshTokenAsync(tokenApiModel);

            if (result == null)
            {
                return BadRequest("Geçersiz veya süresi dolmuş Refresh Token.");
            }

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Geliştirme ortamında HTTP portu genelde 5014'tür.
            var resetUrlBase = "http://localhost:5014/Auth/ResetPassword"; 
            await _authService.ForgotPasswordAsync(dto, resetUrlBase);

            return Ok(new { message = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(dto);

            if (!result)
            {
                return BadRequest("Geçersiz veya süresi dolmuş token.");
            }

            return Ok(new { message = "Şifreniz başarıyla sıfırlandı." });
        }
    }
}