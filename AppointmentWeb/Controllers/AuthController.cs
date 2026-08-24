using AppointmentWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client =
                _httpClientFactory.CreateClient("AppointmentApi");

            var request = new
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password
            };

            var response = await client.PostAsJsonAsync(
                "api/Auth/register",
                request
            );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    $"Kayıt işlemi başarısız: {error}"
                );

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Kayıt başarılı. Şimdi giriş yapabilirsiniz.";

            return RedirectToAction(nameof(Login));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client =
                _httpClientFactory.CreateClient("AppointmentApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/login",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "E-posta veya şifre hatalı."
                );

                return View(model);
            }

            var auth =
                await response.Content
                    .ReadFromJsonAsync<AuthResponseViewModel>();

            if (auth == null || string.IsNullOrWhiteSpace(auth.Token))
            {
                ModelState.AddModelError(
                    "",
                    "Giriş işlemi sırasında bir hata oluştu."
                );

                return View(model);
            }

            HttpContext.Session.SetString(
                "JwtToken",
                auth.Token
            );

            HttpContext.Session.SetString(
                "UserName",
                auth.FullName
            );

            HttpContext.Session.SetString(
                "UserRole",
                auth.Role
            );

            HttpContext.Session.SetInt32(
                "UserId",
                auth.Id
            );

            if (!string.IsNullOrEmpty(auth.Phone))
                HttpContext.Session.SetString("UserPhone_Mock", auth.Phone);
                
            HttpContext.Session.SetString("EmailNotif_Mock", auth.EmailNotificationsEnabled ? "true" : "false");
            HttpContext.Session.SetString("SmsNotif_Mock", auth.SmsNotificationsEnabled ? "true" : "false");
            // API'den dönen email'i doğrudan da alalım
            HttpContext.Session.SetString("UserEmail_Mock", auth.Email);

            if (auth.Role == "Admin")
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            if (auth.Role == "Customer")
            {
                return RedirectToAction(
                    "Create",
                    "Appointment"
                );
            }

            return RedirectToAction(
                "Index",
                "Home"
            );
        }
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("AppointmentApi");
            var response = await client.PostAsJsonAsync("api/Auth/forgot-password", new { email = model.Email });

            TempData["SuccessMessage"] = "Eğer bu e-posta adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilmiştir.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("AppointmentApi");
            var response = await client.PostAsJsonAsync("api/Auth/reset-password", new 
            { 
                token = model.Token,
                email = model.Email,
                newPassword = model.NewPassword
            });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Lütfen yeni şifrenizle giriş yapın.";
                return RedirectToAction(nameof(Login));
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Şifre sıfırlama işlemi başarısız: {errorMessage}");

            return View(model);
        }
    }
}