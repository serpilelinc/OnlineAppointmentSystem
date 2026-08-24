namespace AppointmentApi.DTOs
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public int? ServiceTypeId { get; set; }

        public string ServiceTypeName { get; set; } = string.Empty;

        public int? StaffId { get; set; }

        public string StaffName { get; set; } = string.Empty;

        public bool IsReviewed { get; set; }
    }
}