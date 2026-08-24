using System.Net.Http.Headers;

namespace AppointmentWeb.Services
{
    public class ApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClientService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpClient CreateClient()
        {
            var client =
                _httpClientFactory.CreateClient("AppointmentApi");

            var token =
                _httpContextAccessor.HttpContext?
                    .Session.GetString("JwtToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );
            }

            return client;
        }
    }
}