using System;

namespace AppointmentApi.Models
{
    public class Review : ISoftDeletable
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        public int StaffId { get; set; }
        public Staff? Staff { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;
    }
}
