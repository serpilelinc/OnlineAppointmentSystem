using System.ComponentModel.DataAnnotations;

namespace AppointmentApi.DTOs
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}
