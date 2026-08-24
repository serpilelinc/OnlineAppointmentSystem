using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class CreateStaffViewModel
    {
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unvan zorunludur.")]
        public string Title { get; set; } = string.Empty;
    }
}