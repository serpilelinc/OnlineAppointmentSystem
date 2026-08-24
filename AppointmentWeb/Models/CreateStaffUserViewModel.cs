using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class CreateStaffUserViewModel
    {
        [Required]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;
    }
}