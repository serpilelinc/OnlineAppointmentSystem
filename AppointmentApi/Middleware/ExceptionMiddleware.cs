using System.Net;
using System.Text.Json;
using AppointmentApi.Exceptions;

namespace AppointmentApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(
    HttpContext context,
    Exception exception)
        {
            context.Response.ContentType = "application/json";

            if (exception is NotFoundException)
            {
                context.Response.StatusCode =
                    (int)HttpStatusCode.NotFound;
            }
            else if (exception is ConflictException)
            {
                context.Response.StatusCode =
                    (int)HttpStatusCode.Conflict;
            }
            else
            {
                context.Response.StatusCode =
                    (int)HttpStatusCode.InternalServerError;
            }

            var response = new
            {
                statusCode = context.Response.StatusCode,

                message = exception is NotFoundException ||
                          exception is ConflictException
                    ? exception.Message
                    : "Beklenmeyen bir hata oluştu."
            };

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}