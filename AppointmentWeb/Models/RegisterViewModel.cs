using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrar zorunludur.")]
        [Compare(
            nameof(Password),
            ErrorMessage = "Şifreler eşleşmiyor."
        )]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}