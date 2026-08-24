namespace AppointmentApi.Models
{
    public class Staff : ISoftDeletable
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public User? User { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}