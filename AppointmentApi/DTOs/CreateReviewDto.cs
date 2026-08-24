using System.ComponentModel.DataAnnotations;

namespace AppointmentApi.DTOs
{
    public class CreateReviewDto
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
