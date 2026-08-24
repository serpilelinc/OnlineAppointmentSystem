using AppointmentWeb.Services;
var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Appointment API bağlantısı
builder.Services.AddHttpClient("AppointmentApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5065/");
});

// Session
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApiClientService>();

var app = builder.Build();
app.UseMiddleware<AppointmentWeb.Middleware.WebExceptionMiddleware>();

// Always show developer exceptions for debugging
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.UseRouting();

// Session middleware
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();