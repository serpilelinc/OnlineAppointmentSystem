using System.Net;

namespace AppointmentWeb.Middleware
{
    public class WebExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public WebExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    context.Response.Redirect("/Auth/Login");
                    return;
                }

                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    context.Response.Redirect("/Home/AccessDenied");
                    return;
                }

                context.Response.Redirect("/Home/Error");
            }
            catch (Exception)
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}