using System.ComponentModel.DataAnnotations;

namespace AppointmentApi.DTOs
{
    public class CreateAppointmentDto
    {
        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Müşteri adı 2 ile 100 karakter arasında olmalıdır."
        )]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir hizmet seçiniz.")]
        public int ServiceTypeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir personel seçiniz.")]
        public int StaffId { get; set; }
    }
}