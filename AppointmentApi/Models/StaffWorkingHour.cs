namespace AppointmentApi.Models
{
    public class StaffWorkingHour : ISoftDeletable
    {
        public int Id { get; set; }

        public int StaffId { get; set; }

        public Staff? Staff { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}