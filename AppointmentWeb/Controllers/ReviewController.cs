using AppointmentWeb.Models;
using AppointmentWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentWeb.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApiClientService _apiClientService;

        public ReviewController(ApiClientService apiClientService)
        {
            _apiClientService = apiClientService;
        }

        [HttpGet]
        public IActionResult Create(int appointmentId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Customer")
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new CreateReviewViewModel
            {
                AppointmentId = appointmentId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Customer")
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _apiClientService.CreateClient();

            var response = await client.PostAsJsonAsync("api/Reviews", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Değerlendirmeniz başarıyla kaydedildi. Teşekkür ederiz!";
                return RedirectToAction("MyAppointments", "Appointment");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Değerlendirme kaydedilemedi: {errorMessage}");

            return View(model);
        }
    }
}
