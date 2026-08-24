using System.ComponentModel.DataAnnotations;

namespace AppointmentWeb.Models
{
    public class AssignServiceToStaffViewModel
    {
        [Required]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Hizmet seçiniz.")]
        public int ServiceTypeId { get; set; }
    }
}