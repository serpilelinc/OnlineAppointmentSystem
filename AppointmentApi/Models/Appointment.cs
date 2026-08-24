namespace AppointmentApi.Models
{
    public class Appointment : ISoftDeletable
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? ServiceTypeId { get; set; }

        public AppointmentServiceType? ServiceType { get; set; }

        public int? StaffId { get; set; }

        public Staff? Staff { get; set; }

        public int? UserId { get; set; }

        public User? User { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}