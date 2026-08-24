namespace AppointmentWeb.Models
{
    public class ApiErrorViewModel
    {
        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}