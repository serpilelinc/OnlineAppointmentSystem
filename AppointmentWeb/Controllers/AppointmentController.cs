using AppointmentWeb.Models;
using AppointmentWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentWeb.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApiClientService _apiClientService;

        public AppointmentController(
            ApiClientService apiClientService)
        {
            _apiClientService = apiClientService;
        }

        // ADMIN - Tüm randevular
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            DateTime? startDate,
            DateTime? endDate,
            int page = 1)
        {
            var client = _apiClientService.CreateClient();

            const int pageSize = 5;

            var url =
                $"api/Appointments/filter?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                url += $"&status={Uri.EscapeDataString(status)}";
            }

            if (startDate.HasValue)
            {
                url += $"&startDate={startDate.Value:yyyy-MM-dd}";
            }

            if (endDate.HasValue)
            {
                url += $"&endDate={endDate.Value:yyyy-MM-dd}";
            }

            var result =
                await client.GetFromJsonAsync<
                    PagedResultViewModel<AppointmentViewModel>>(
                    url
                );

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.CurrentPage = result?.Page ?? 1;
            ViewBag.TotalPages = result?.TotalPages ?? 1;
            ViewBag.TotalCount = result?.TotalCount ?? 0;

            return View(
                result?.Items ?? new List<AppointmentViewModel>()
            );
        }

        // CUSTOMER - Kendi randevuları
        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            var client = _apiClientService.CreateClient();

            var appointments =
                await client.GetFromJsonAsync<List<AppointmentViewModel>>(
                    "api/Appointments/my"
                );

            return View(
                appointments ?? new List<AppointmentViewModel>()
            );
        }
        [HttpGet]
        public async Task<IActionResult> StaffAppointments()
        {
            var userRole =
                HttpContext.Session.GetString("UserRole");

            if (userRole != "Staff")
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            var client =
                _apiClientService.CreateClient();

            var appointments =
                await client.GetFromJsonAsync<List<AppointmentViewModel>>(
                    "api/Appointments/staff/my"
                );

            return View(
                appointments ?? new List<AppointmentViewModel>()
            );
        }

        // Yeni randevu ekranı
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = _apiClientService.CreateClient();

            // Kategorileri getir
            var categories =
                await client.GetFromJsonAsync<List<CategoryViewModel>>(
                    "api/Category"
                );

            ViewBag.Categories =
                categories ?? new List<CategoryViewModel>();

            return View(
                new CreateAppointmentViewModel()
            );
        }

        // Yeni randevu kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateAppointmentViewModel model)
        {
            var client = _apiClientService.CreateClient();

            if (!ModelState.IsValid)
            {
                await LoadCreateDropdowns(client);

                return View(model);
            }

            var response = await client.PostAsJsonAsync(
                "api/Appointments",
                model
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "Randevunuz başarıyla oluşturuldu.";

                return RedirectToAction(
                    nameof(MyAppointments)
                );
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                $"Randevu oluşturulamadı: {errorMessage}"
            );

            await LoadCreateDropdowns(client);

            return View(model);
        }

        private async Task LoadCreateDropdowns(
            HttpClient client)
        {
            var serviceTypes =
                await client.GetFromJsonAsync<List<ServiceTypeViewModel>>(
                    "api/ServiceTypes"
                );

            var staff =
                await client.GetFromJsonAsync<List<StaffViewModel>>(
                    "api/Staff"
                );

            ViewBag.ServiceTypes =
                serviceTypes ?? new List<ServiceTypeViewModel>();

            ViewBag.Staff =
                staff ?? new List<StaffViewModel>();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _apiClientService.CreateClient();

            var appointment =
                await client.GetFromJsonAsync<AppointmentViewModel>(
                    $"api/Appointments/{id}"
                );

            if (appointment == null)
            {
                return NotFound();
            }

            var serviceTypes =
                await client.GetFromJsonAsync<List<ServiceTypeViewModel>>(
                    "api/ServiceTypes"
                );

            var staff =
                await client.GetFromJsonAsync<List<StaffViewModel>>(
                    "api/Staff"
                );

            ViewBag.ServiceTypes =
                serviceTypes ?? new List<ServiceTypeViewModel>();

            ViewBag.Staff =
                staff ?? new List<StaffViewModel>();

            var model = new CreateAppointmentViewModel
            {
                CustomerName = appointment.CustomerName,
                CustomerEmail = appointment.CustomerEmail,
                AppointmentDate = appointment.AppointmentDate,
                ServiceTypeId = appointment.ServiceTypeId,
                StaffId = appointment.StaffId ?? 0
            };

            ViewBag.AppointmentId = id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CreateAppointmentViewModel model)
        {
            var client = _apiClientService.CreateClient();

            if (!ModelState.IsValid)
            {
                await LoadCreateDropdowns(client);

                ViewBag.AppointmentId = id;

                return View(model);
            }

            var response = await client.PutAsJsonAsync(
                $"api/Appointments/{id}",
                model
            );

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                $"Randevu güncellenemedi: {errorMessage}"
            );

            await LoadCreateDropdowns(client);

            ViewBag.AppointmentId = id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _apiClientService.CreateClient();

            var response = await client.DeleteAsync(
                $"api/Appointments/{id}"
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Randevu silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(
            int staffId,
            int serviceTypeId,
            DateTime date)
        {
            var client = _apiClientService.CreateClient();

            var url =
                $"api/Appointments/available-slots" +
                $"?staffId={staffId}" +
                $"&serviceTypeId={serviceTypeId}" +
                $"&date={date:yyyy-MM-dd}";

            var slots =
                await client.GetFromJsonAsync<List<AvailableSlotViewModel>>(
                    url
                );

            return Json(
                slots ?? new List<AvailableSlotViewModel>()
            );
        }
        [HttpGet]
        public async Task<IActionResult> GetServicesByCategory(
    int categoryId)
        {
            var client =
                _apiClientService.CreateClient();

            var services =
                await client.GetFromJsonAsync<List<ServiceTypeViewModel>>(
                    $"api/ServiceTypes/by-category/{categoryId}"
                );

            return Json(
                services ?? new List<ServiceTypeViewModel>()
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableStaff(
    int serviceTypeId,
    DateTime date)
        {
            var client = _apiClientService.CreateClient();

            var staffs =
                await client.GetFromJsonAsync<List<StaffViewModel>>(
                    $"api/Staff/available" +
                    $"?serviceTypeId={serviceTypeId}" +
                    $"&date={date:yyyy-MM-dd}"
                );

            return Json(
                staffs ?? new List<StaffViewModel>()
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
     int id,
     string status)
        {
            var client = _apiClientService.CreateClient();

            var response = await client.PatchAsJsonAsync(
                $"api/Appointments/{id}/status",
                new
                {
                    status
                }
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Randevu durumu güncellenemedi.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Randevu durumu başarıyla güncellendi.";
            }

            var userRole =
                HttpContext.Session.GetString("UserRole");

            // Personel ise kendi randevularına dön
            if (userRole == "Staff")
            {
                return RedirectToAction(
                    nameof(StaffAppointments)
                );
            }

            // Admin ise tüm randevulara dön
            return RedirectToAction(
                nameof(Index)
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelMyAppointment(int id)
        {
            var userRole =
                HttpContext.Session.GetString("UserRole");

            if (userRole != "Customer")
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            var client = _apiClientService.CreateClient();

            var response = await client.PatchAsync(
                $"api/Appointments/my/{id}/cancel",
                null
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "Randevunuz başarıyla iptal edildi.";
            }
            else
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Randevu iptal edilemedi: {error}";
            }

            return RedirectToAction(
                nameof(MyAppointments)
            );
        }
    }
}