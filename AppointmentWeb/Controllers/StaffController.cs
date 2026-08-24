using AppointmentWeb.Models;
using AppointmentWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentWeb.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApiClientService _apiClientService;

        public StaffController(ApiClientService apiClientService)
        {
            _apiClientService = apiClientService;
        }

        public async Task<IActionResult> Index()
        {
            var client = _apiClientService.CreateClient();

            var staffs =
                await client.GetFromJsonAsync<List<StaffViewModel>>(
                    "api/Staff"
                ) ?? new List<StaffViewModel>();

            foreach (var staff in staffs)
            {
                var accountStatus =
                    await client.GetFromJsonAsync<StaffAccountStatusViewModel>(
                        $"api/Auth/staff/{staff.Id}/has-account"
                    );

                staff.HasUserAccount =
                    accountStatus?.HasAccount ?? false;
            }

            return View(staffs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _apiClientService.CreateClient();

            var response = await client.PostAsJsonAsync(
                "api/Staff",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    $"Personel eklenemedi: {error}"
                );

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Personel başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var client = _apiClientService.CreateClient();

            var staff =
                await client.GetFromJsonAsync<StaffViewModel>(
                    $"api/Staff/{id}"
                );

            if (staff == null)
            {
                return NotFound();
            }

            var serviceTypes =
                await client.GetFromJsonAsync<List<ServiceTypeViewModel>>(
                    "api/ServiceTypes"
                );

            ViewBag.ServiceTypes =
                serviceTypes ?? new List<ServiceTypeViewModel>();

            var assignedServices =
                await client.GetFromJsonAsync<List<ServiceTypeViewModel>>(
                    $"api/Staff/{id}/services"
                );

            ViewBag.AssignedServices =
                assignedServices ?? new List<ServiceTypeViewModel>();

            var workingHours =
                await client.GetFromJsonAsync<List<CreateStaffWorkingHourViewModel>>(
                    $"api/Staff/{id}/working-hours"
                );

            ViewBag.WorkingHours =
                workingHours ?? new List<CreateStaffWorkingHourViewModel>();

            return View(staff);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _apiClientService.CreateClient();

            var staff =
                await client.GetFromJsonAsync<StaffViewModel>(
                    $"api/Staff/{id}"
                );

            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            StaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _apiClientService.CreateClient();

            var response =
                await client.PutAsJsonAsync(
                    $"api/Staff/{id}",
                    model
                );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    $"Hizmet veren güncellenemedi: {error}"
                );

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Hizmet veren bilgileri başarıyla güncellendi.";

            return RedirectToAction(
                nameof(Manage),
                new { id }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveService(
            int staffId,
            int serviceTypeId)
        {
            var client = _apiClientService.CreateClient();

            var response =
                await client.DeleteAsync(
                    $"api/Staff/{staffId}/services/{serviceTypeId}"
                );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Hizmet kaldırılamadı: {error}";

                return RedirectToAction(
                    nameof(Manage),
                    new { id = staffId }
                );
            }

            TempData["SuccessMessage"] =
                "Hizmet ataması kaldırıldı.";

            return RedirectToAction(
                nameof(Manage),
                new { id = staffId }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignService(
            AssignServiceToStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    nameof(Manage),
                    new { id = model.StaffId }
                );
            }

            var client = _apiClientService.CreateClient();

            var response = await client.PostAsJsonAsync(
                "api/Staff/assign-service",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Hizmet atanamadı: {error}";

                return RedirectToAction(
                    nameof(Manage),
                    new { id = model.StaffId }
                );
            }

            TempData["SuccessMessage"] =
                "Hizmet personele başarıyla atandı.";

            return RedirectToAction(
                nameof(Manage),
                new { id = model.StaffId }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkingHour(
            CreateStaffWorkingHourViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Çalışma saati bilgileri geçersiz.";

                return RedirectToAction(
                    nameof(Manage),
                    new { id = model.StaffId }
                );
            }

            if (model.EndTime <= model.StartTime)
            {
                TempData["ErrorMessage"] =
                    "Bitiş saati başlangıç saatinden sonra olmalıdır.";

                return RedirectToAction(
                    nameof(Manage),
                    new { id = model.StaffId }
                );
            }

            var client = _apiClientService.CreateClient();

            var response = await client.PostAsJsonAsync(
                "api/Staff/working-hours",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Çalışma saati eklenemedi: {error}";

                return RedirectToAction(
                    nameof(Manage),
                    new { id = model.StaffId }
                );
            }

            TempData["SuccessMessage"] =
                "Çalışma saati başarıyla eklendi.";

            return RedirectToAction(
                nameof(Manage),
                new { id = model.StaffId }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkingHour(
            int id,
            int staffId)
        {
            var client = _apiClientService.CreateClient();

            var response = await client.DeleteAsync(
                $"api/Staff/working-hours/{id}"
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Çalışma saati silinemedi.";

                return RedirectToAction(
                    nameof(Manage),
                    new { id = staffId }
                );
            }

            TempData["SuccessMessage"] =
                "Çalışma saati başarıyla silindi.";

            return RedirectToAction(
                nameof(Manage),
                new { id = staffId }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserAccount(
            CreateStaffUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Personel hesabı bilgileri geçersiz.";

                return RedirectToAction(nameof(Index));
            }

            var client = _apiClientService.CreateClient();

            var response = await client.PostAsJsonAsync(
                "api/Auth/staff",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Personel hesabı oluşturulamadı: {error}";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Personel kullanıcı hesabı başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _apiClientService.CreateClient();

            var response = await client.DeleteAsync(
                $"api/Staff/{id}"
            );

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiErrorViewModel>();

                TempData["ErrorMessage"] =
                    error?.Message ??
                    "Hizmet veren silinemedi.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Hizmet veren başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Oturum açmış olan personelin çalışma saatlerini görüntüleyen GET metodudur.
        /// </summary>
        /// <returns>Çalışma saatleri formunu içeren View döner.</returns>
        [HttpGet]
        public async Task<IActionResult> MyWorkingHours()
        {
            // Sadece 'Staff' rolüne sahip kullanıcıların bu sayfaya erişimine izin verilir.
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Staff")
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _apiClientService.CreateClient();

            // API üzerinden personelin mevcut çalışma saatleri listesi çekilir.
            var workingHours = await client.GetFromJsonAsync<List<CreateStaffWorkingHourViewModel>>("api/Staff/my-working-hours");

            ViewBag.WorkingHours = workingHours ?? new List<CreateStaffWorkingHourViewModel>();

            return View();
        }

        /// <summary>
        /// Personelin kendi çalışma saatlerini güncellediği POST metodudur.
        /// </summary>
        /// <param name="dtos">Formdan dönen güncel çalışma saati listesi.</param>
        /// <returns>İşlem sonucuna göre yönlendirme sağlar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyWorkingHours(List<CreateStaffWorkingHourViewModel> dtos)
        {
            // Rol kontrolü: İşlem güvenliği için yetki doğrulaması yapılır.
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Staff")
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _apiClientService.CreateClient();

            // Mantıksal doğrulama: Bitiş saati, başlangıç saatinden büyük olan geçerli kayıtlar filtrelenir.
            var validDtos = dtos.Where(d => d.StartTime < d.EndTime).ToList();

            // API uç noktasına geçerli çalışma saatleri yollanır.
            var response = await client.PostAsJsonAsync("api/Staff/my-working-hours", validDtos);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Çalışma saatleriniz başarıyla güncellendi.";
                return RedirectToAction(nameof(MyWorkingHours));
            }

            // API'den başarısız yanıt dönmesi durumunda hata mesajı okunarak kullanıcıya yansıtılır.
            var errorMessage = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Saatler güncellenemedi: {errorMessage}";

            return RedirectToAction(nameof(MyWorkingHours));
        }
    }
}