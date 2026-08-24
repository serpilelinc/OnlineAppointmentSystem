using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class CreateReviewViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Lütfen bir puan seçiniz.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
