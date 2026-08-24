namespace AppointmentApi.DTOs
{
    public class UpdateNotificationsDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
    }
}
