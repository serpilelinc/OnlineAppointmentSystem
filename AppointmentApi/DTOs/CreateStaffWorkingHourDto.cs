namespace AppointmentApi.DTOs
{
    public class CreateStaffWorkingHourDto
    {
        public int StaffId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}