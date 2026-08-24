namespace AppointmentApi.DTOs
{
    public class ServiceTypeResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}