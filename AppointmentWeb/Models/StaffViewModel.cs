namespace AppointmentWeb.Models
{
    public class StaffViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public bool HasUserAccount { get; set; }
    }
}