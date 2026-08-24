using AppointmentWeb.Models;
using AppointmentWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApiClientService _apiClientService;

        public CategoryController(
            ApiClientService apiClientService)
        {
            _apiClientService = apiClientService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client =
                _apiClientService.CreateClient();

            var categories =
                await client.GetFromJsonAsync<
                    List<CategoryViewModel>>(
                    "api/Category"
                );

            return View(
                categories ?? new List<CategoryViewModel>()
            );
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userRole =
                HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrWhiteSpace(userRole))
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            if (userRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home"
                );
            }

            return View(
                new CreateCategoryViewModel()
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    CreateCategoryViewModel model)
        {
            var userRole =
                HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrWhiteSpace(userRole))
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            if (userRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home"
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client =
                _apiClientService.CreateClient();

            var response =
                await client.PostAsJsonAsync(
                    "api/Category",
                    model
                );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    $"Kategori eklenemedi. HTTP " +
                    $"{(int)response.StatusCode} " +
                    $"{response.StatusCode}. {error}"
                );

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Kategori başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client =
                _apiClientService.CreateClient();

            var response =
                await client.DeleteAsync(
                    $"api/Category/{id}"
                );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Kategori silinemedi. HTTP " +
                    $"{(int)response.StatusCode} " +
                    $"{response.StatusCode}. {error}";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Kategori başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}