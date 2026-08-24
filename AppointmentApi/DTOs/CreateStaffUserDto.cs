namespace AppointmentApi.DTOs
{
    public class CreateStaffUserDto
    {
        public int StaffId { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}