using AppointmentApi.Data;
using AppointmentApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApi.Services
{
    public class BackgroundJobService
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailService _emailService;

        public BackgroundJobService(AppDbContext context, IUnitOfWork unitOfWork, EmailService emailService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task ProcessDailyAppointmentsAsync()
        {
            // 1. Mark past appointments as Completed if they are still Approved
            var pastAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate < DateTime.Now && a.Status == "Approved")
                .ToListAsync();

            if (pastAppointments.Any())
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    foreach (var app in pastAppointments)
                    {
                        app.Status = "Completed";
                    }
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }

            // 2. Send reminder emails for tomorrow's appointments
            var tomorrowStart = DateTime.Today.AddDays(1);
            var tomorrowEnd = tomorrowStart.AddDays(1).AddTicks(-1);

            var tomorrowAppointments = await _context.Appointments
                .Include(a => a.ServiceType)
                .Where(a => a.AppointmentDate >= tomorrowStart && 
                            a.AppointmentDate <= tomorrowEnd && 
                            a.Status == "Approved")
                .ToListAsync();

            foreach (var app in tomorrowAppointments)
            {
                if (!string.IsNullOrEmpty(app.CustomerEmail))
                {
                    string serviceName = app.ServiceType?.Name ?? "Hizmet";
                    string time = app.AppointmentDate.ToString("HH:mm");
                    string content = $"<p>Merhaba {app.CustomerName},</p>" +
                                     $"<p>Yarın saat <strong>{time}</strong> itibarıyla <strong>{serviceName}</strong> randevunuz bulunmaktadır. Lütfen zamanında gelmeyi unutmayın.</p>";

                    await _emailService.SendEmailAsync(app.CustomerEmail, "Randevu Hatırlatması", content);
                }
            }
        }
    }
}
