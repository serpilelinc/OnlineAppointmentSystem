using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppointmentWeb.Services;

namespace AppointmentWeb.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApiClientService _apiClientService;

        public SettingsController(ApiClientService apiClientService)
        {
            _apiClientService = apiClientService;
        }

        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var mockEmail = HttpContext.Session.GetString("UserEmail_Mock");
            if (!string.IsNullOrEmpty(mockEmail))
            {
                ViewBag.UserEmail = mockEmail;
            }
            else
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    ViewBag.UserEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value;
                }
                catch
                {
                    ViewBag.UserEmail = "";
                }
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Mevcut Kullanıcı";
            ViewBag.UserPhone = HttpContext.Session.GetString("UserPhone_Mock");
            
            // Bildirim tercihlerini session'dan al (Varsayılan olarak E-posta: true, SMS: false)
            ViewBag.EmailNotif = HttpContext.Session.GetString("EmailNotif_Mock") ?? "true";
            ViewBag.SmsNotif = HttpContext.Session.GetString("SmsNotif_Mock") ?? "false";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string phone)
        {
            var client = _apiClientService.CreateClient();
            var response = await client.PutAsJsonAsync("api/Users/profile", new { FullName = fullName, Email = email, Phone = phone });

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                    HttpContext.Session.SetString("UserName", fullName);
                    
                if (!string.IsNullOrWhiteSpace(email))
                    HttpContext.Session.SetString("UserEmail_Mock", email);
                    
                if (!string.IsNullOrWhiteSpace(phone))
                    HttpContext.Session.SetString("UserPhone_Mock", phone);

                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Güncelleme başarısız oldu.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Yeni şifreler eşleşmiyor.";
                return RedirectToAction("Index");
            }

            var client = _apiClientService.CreateClient();
            var response = await client.PutAsJsonAsync("api/Users/password", new 
            { 
                CurrentPassword = currentPassword, 
                NewPassword = newPassword 
            });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Şifreniz güvenli bir şekilde değiştirildi.";
            }
            else
            {
                // API'den dönen hata mesajını (Mevcut şifre yanlış vb.) yakalayalım.
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<dynamic>();
                    TempData["ErrorMessage"] = errorResponse?.message ?? "Şifre değiştirme başarısız oldu.";
                }
                catch
                {
                    TempData["ErrorMessage"] = "Eski şifrenizi yanlış girdiniz veya bir hata oluştu.";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNotifications(bool emailNotif, bool smsNotif)
        {
            var client = _apiClientService.CreateClient();
            var response = await client.PutAsJsonAsync("api/Users/notifications", new { EmailNotificationsEnabled = emailNotif, SmsNotificationsEnabled = smsNotif });

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.SetString("EmailNotif_Mock", emailNotif ? "true" : "false");
                HttpContext.Session.SetString("SmsNotif_Mock", smsNotif ? "true" : "false");
                TempData["SuccessMessage"] = "Bildirim tercihleriniz kaydedildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bildirim tercihleri güncellenemedi.";
            }
            
            return RedirectToAction("Index");
        }
    }
}
