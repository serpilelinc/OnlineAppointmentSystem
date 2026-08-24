using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;
    }
}
