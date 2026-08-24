namespace AppointmentApi.DTOs
{
    public class DashboardDto
    {
        public int TodayAppointments { get; set; }

        public int PendingAppointments { get; set; }

        public int ConfirmedAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int TotalAppointments { get; set; }

        public List<AppointmentResponseDto> TodayAppointmentList { get; set; } = new();
    }
}