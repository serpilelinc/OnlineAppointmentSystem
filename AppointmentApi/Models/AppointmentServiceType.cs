using Microsoft.EntityFrameworkCore;

namespace AppointmentApi.Models
{
    public class AppointmentServiceType : ISoftDeletable
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}