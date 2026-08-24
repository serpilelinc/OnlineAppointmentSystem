namespace AppointmentWeb.Models
{
    public class DashboardViewModel
    {
        public int TodayAppointments { get; set; }

        public int PendingAppointments { get; set; }

        public int ConfirmedAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int TotalAppointments { get; set; }
        public List<AppointmentViewModel> TodayAppointmentList { get; set; } = new();
    }
}