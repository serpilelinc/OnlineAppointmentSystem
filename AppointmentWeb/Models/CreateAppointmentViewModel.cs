using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class CreateAppointmentViewModel
    {
        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        public DateTime AppointmentDate { get; set; } =
    DateTime.Today.AddDays(1).AddHours(9);

        [Range(1, int.MaxValue, ErrorMessage = "Hizmet seçiniz.")]
        public int ServiceTypeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Personel seçiniz.")]
        public int StaffId { get; set; }
    }
}