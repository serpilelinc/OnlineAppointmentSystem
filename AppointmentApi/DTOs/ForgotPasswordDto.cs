using System.ComponentModel.DataAnnotations;

namespace AppointmentApi.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;
    }
}
