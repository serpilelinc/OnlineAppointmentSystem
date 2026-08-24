using AppointmentWeb.Models;
using AppointmentWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentWeb.Controllers
{
    public class ServiceTypeController : Controller
    {
        private readonly ApiClientService _apiClientService;

        public ServiceTypeController(
            ApiClientService apiClientService)
        {
            _apiClientService = apiClientService;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var client = _apiClientService.CreateClient();

            var services =
                await client.GetFromJsonAsync<List<ServiceTypeViewModel>>(
                    "api/ServiceTypes"
                ) ?? new List<ServiceTypeViewModel>();

            var categories =
                await client.GetFromJsonAsync<List<CategoryViewModel>>(
                    "api/Category"
                ) ?? new List<CategoryViewModel>();

            if (categoryId.HasValue)
            {
                services = services
                    .Where(x => x.CategoryId == categoryId.Value)
                    .ToList();
            }

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            return View(services);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = _apiClientService.CreateClient();

            var categories =
                await client.GetFromJsonAsync<List<CategoryViewModel>>(
                    "api/Category"
                );

            ViewBag.Categories =
                categories ?? new List<CategoryViewModel>();

            return View(
                new ServiceTypeViewModel()
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceTypeViewModel model)
        {
            var client = _apiClientService.CreateClient();

            if (!ModelState.IsValid)
            {
                await LoadCategories(client);

                return View(model);
            }

            var response =
                await client.PostAsJsonAsync(
                    "api/ServiceTypes",
                    model
                );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    $"Hizmet eklenemedi: {error}"
                );

                await LoadCategories(client);

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Hizmet başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _apiClientService.CreateClient();

            var service =
                await client.GetFromJsonAsync<ServiceTypeViewModel>(
                    $"api/ServiceTypes/{id}"
                );

            if (service == null)
            {
                return NotFound();
            }

            await LoadCategories(client);

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ServiceTypeViewModel model)
        {
            var client = _apiClientService.CreateClient();

            if (!ModelState.IsValid)
            {
                await LoadCategories(client);

                return View(model);
            }

            var response =
                await client.PutAsJsonAsync(
                    $"api/ServiceTypes/{id}",
                    model
                );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    $"Hizmet güncellenemedi: {error}"
                );

                await LoadCategories(client);

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Hizmet başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _apiClientService.CreateClient();

            var response =
                await client.DeleteAsync(
                    $"api/ServiceTypes/{id}"
                );

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Hizmet silinemedi.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Hizmet başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategories(
            HttpClient client)
        {
            var categories =
                await client.GetFromJsonAsync<List<CategoryViewModel>>(
                    "api/Category"
                );

            ViewBag.Categories =
                categories ?? new List<CategoryViewModel>();
        }
    }
}