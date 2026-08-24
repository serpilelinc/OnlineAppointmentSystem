using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class CreateStaffWorkingHourViewModel
    {
        public int Id { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Gün seçiniz.")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        public TimeSpan EndTime { get; set; }
    }
}