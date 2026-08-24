using System.Diagnostics;
using AppointmentWeb.Models;
using AppointmentWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentWeb.Controllers;

public class HomeController : Controller
{
    private readonly ApiClientService _apiClientService;

    public HomeController(ApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<IActionResult> Index()
    {
        var userRole =
            HttpContext.Session.GetString("UserRole");

        // Giriş yapılmamışsa ana karşılama sayfasını göster
        if (string.IsNullOrWhiteSpace(userRole))
        {
            return View(new DashboardViewModel());
        }

        // Customer kendi randevularına gider
        if (userRole == "Customer")
        {
            return RedirectToAction(
                "MyAppointments",
                "Appointment"
            );
        }

        // Staff kendi randevularına gider
        if (userRole == "Staff")
        {
            return RedirectToAction(
                "StaffAppointments",
                "Appointment"
            );
        }

        // Admin dashboard
        if (userRole == "Admin")
        {
            var client =
                _apiClientService.CreateClient();

            var dashboard =
                await client.GetFromJsonAsync<DashboardViewModel>(
                    "api/Appointments/dashboard"
                );

            return View(
                dashboard ?? new DashboardViewModel()
            );
        }

        // Bilinmeyen bir rol varsa çıkış yaptır
        HttpContext.Session.Clear();

        return RedirectToAction(
            "Login",
            "Auth"
        );
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            }
        );
    }
}