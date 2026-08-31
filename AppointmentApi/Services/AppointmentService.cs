using AutoMapper;
using AppointmentApi.Data;
using AppointmentApi.DTOs;
using AppointmentApi.Models;
using Microsoft.EntityFrameworkCore;
using AppointmentApi.Exceptions;
using AppointmentApi.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using AppointmentApi.Hubs;


namespace AppointmentApi.Services
{
    public class AppointmentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public AppointmentService(
            AppDbContext context, 
            IMapper mapper, 
            EmailService emailService, 
            IUnitOfWork unitOfWork,
            IHubContext<AppointmentHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Müşterinin yeni bir randevu oluşturması işlemini (İş kuralları doğrulaması ile birlikte) gerçekleştirir.
        /// </summary>
        /// <param name="dto">Müşteri tarafından seçilen hizmet, personel, tarih ve iletişim bilgileri.</param>
        /// <param name="userId">Randevuyu oluşturan müşterinin (User tablosundaki) sistem ID'si.</param>
        /// <returns>Oluşturulan randevunun detaylarını döner.</returns>
        public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto, int userId)
        {
            // --- 1. VERİ BÜTÜNLÜĞÜ KONTROLLERİ ---
            // Müşterinin seçtiği hizmet veritabanında gerçekten var mı?
            var serviceType = await _context.ServiceTypes.FindAsync(dto.ServiceTypeId);
            if (serviceType == null)
            {
                throw new NotFoundException("Seçilen hizmet bulunamadı.");
            }

            // Seçilen personel veritabanında gerçekten var mı?
            var staff = await _context.Staffs.FindAsync(dto.StaffId);
            if (staff == null)
            {
                throw new NotFoundException("Seçilen personel bulunamadı.");
            }

            // Personel bu hizmeti vermeye yetkili mi? (Örn: Cilt bakım uzmanından saç kesimi randevusu alınamaz)
            var canProvideService = await _context.StaffServiceTypes.AnyAsync(x =>
                x.StaffId == dto.StaffId &&
                x.ServiceTypeId == dto.ServiceTypeId
            );
            if (!canProvideService)
            {
                throw new ConflictException("Seçilen personel bu hizmeti vermemektedir.");
            }

            // --- 2. TARİH VE ZAMAN KONTROLLERİ ---
            // Geçmiş bir tarihe randevu alınamaz.
            if (dto.AppointmentDate <= DateTime.Now)
            {
                throw new ConflictException("Geçmiş bir tarihe randevu oluşturulamaz.");
            }

            // --- 3. ÇALIŞMA SAATLERİ (MESAİ) KONTROLÜ ---
            // Müşterinin seçtiği gün (Pazartesi, Salı vb.) personelin mesaisi var mı?
            var workingHour = await _context.StaffWorkingHours
                .FirstOrDefaultAsync(w =>
                    w.StaffId == dto.StaffId &&
                    w.DayOfWeek == dto.AppointmentDate.DayOfWeek
                );

            // Personel o gün izinliyse (kayıt yoksa) işlem durdurulur.
            if (workingHour == null)
            {
                throw new ConflictException("Personel seçilen gün çalışmamaktadır.");
            }

            // Randevunun başlangıç saati ve hizmet süresi eklenince oluşan bitiş saati.
            var appointmentStartTime = dto.AppointmentDate.TimeOfDay;
            var appointmentEndTime = appointmentStartTime.Add(TimeSpan.FromMinutes(serviceType.DurationMinutes));

            // Bu saat aralığı personelin günlük mesai sınırları içinde kalıyor mu?
            if (appointmentStartTime < workingHour.StartTime || appointmentEndTime > workingHour.EndTime)
            {
                throw new ConflictException("Randevu personelin çalışma saatleri dışında.");
            }

            // --- 4. RANDEVU ÇAKIŞMA (CONFLICT) KONTROLÜ ---
            var newStart = dto.AppointmentDate;
            var newEnd = newStart.AddMinutes(serviceType.DurationMinutes);

            // Çok kritik: Seçilen saat aralığında personelin önceden alınmış iptal EDİLMEMİŞ başka bir randevusu var mı?
            // "Kesişme (Overlap)" formülü: Mevcut randevunun bitişi yeni başlangıçtan büyükse VE mevcut başlangıç yeni bitişten küçükse çakışma vardır.
            var hasConflict = await _context.Appointments
                .Include(a => a.ServiceType)
                .AnyAsync(a =>
                    a.StaffId == dto.StaffId &&
                    a.Status != "Cancelled" &&
                    a.ServiceType != null &&
                    newStart < a.AppointmentDate.AddMinutes(a.ServiceType.DurationMinutes) &&
                    newEnd > a.AppointmentDate
                );

            if (hasConflict)
            {
                throw new ConflictException("Seçilen personelin bu tarih ve saatte başka bir randevusu bulunmaktadır.");
            }

            // Müşterinin (UserId) aynı tarih ve saatte (çakışan) başka bir randevusu var mı?
            var hasCustomerConflict = await _context.Appointments
                .Include(a => a.ServiceType)
                .AnyAsync(a =>
                    a.UserId == userId &&
                    a.Status != "Cancelled" &&
                    a.ServiceType != null &&
                    newStart < a.AppointmentDate.AddMinutes(a.ServiceType.DurationMinutes) &&
                    newEnd > a.AppointmentDate
                );

            if (hasCustomerConflict)
            {
                throw new ConflictException("Bu tarih ve saatte zaten başka bir randevunuz bulunmaktadır. Lütfen farklı bir saat seçiniz.");
            }

            // --- 5. KAYIT OLUŞTURMA ---
            // Bütün güvenlik duvarlarından geçtiyse DTO'dan Entity'ye dönüşüm yapıyoruz.
            var appointment = _mapper.Map<Appointment>(dto);

            // Yeni alınan randevular her zaman "Bekliyor (Pending)" statüsünde başlar.
            appointment.Status = "Pending";
            appointment.CreatedAt = DateTime.Now;
            appointment.UserId = userId; // Hangi hesabın bu randevuyu aldığı kaydedilir.

            _context.Appointments.Add(appointment);
            
            // İşlemleri veritabanına fiziksel olarak yazar.
            await _unitOfWork.SaveChangesAsync();
            
            // --- 6. BİLDİRİM (NOTIFICATION) ---
            // Anlık SignalR Bildirimi
            await _hubContext.Clients.All.SendAsync("ReceiveNewAppointment", appointment.CustomerName, appointment.AppointmentDate.ToString("dd.MM.yyyy HH:mm"));

            // Müşteriye randevu talebinin alındığına dair e-posta göndeririz.
            await _emailService.SendEmailAsync(
                appointment.CustomerEmail, 
                "Randevu Talebiniz Alındı",
                $"<p>Merhaba {appointment.CustomerName},</p>" +
                $"<p><strong>{appointment.AppointmentDate.ToString("dd.MM.yyyy HH:mm")}</strong> tarihi için randevu talebiniz alınmıştır. Randevunuz onaylandığında size tekrar bilgi verilecektir.</p>"
            );

            // İşlem bittiğinde oluşturulan ID vb. bilgileri Ön Yüze döneriz.
            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        public async Task<List<AppointmentResponseDto>> GetMyAppointmentsAsync(
            int userId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<AppointmentResponseDto>>(appointments);

            foreach (var dto in dtos)
            {
                dto.IsReviewed = await _context.Reviews.AnyAsync(r => r.AppointmentId == dto.Id);
            }

            return dtos;
        }
        public async Task<List<AppointmentResponseDto>> GetAllAsync()
        {
            var appointments = await _context.Appointments
    .Include(a => a.ServiceType)
    .Include(a => a.Staff)
    .ToListAsync();

            return _mapper.Map<List<AppointmentResponseDto>>(appointments);
        }
        public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
        {
            var appointment = await _context.Appointments
    .Include(a => a.ServiceType)
    .Include(a => a.Staff)
    .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return null;
            }

            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        public async Task<AppointmentResponseDto?> UpdateAsync(
     int id,
     UpdateAppointmentDto dto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return null;
            }

            var serviceType = await _context.ServiceTypes
                .FindAsync(dto.ServiceTypeId);

            if (serviceType == null)
            {
                throw new NotFoundException(
                    "Seçilen hizmet bulunamadı."
                );
            }
            if (dto.AppointmentDate <= DateTime.Now)
            {
                throw new ConflictException(
                    "Randevu tarihi geçmiş bir tarih olamaz."
                );
            }

            var staff = await _context.Staffs
                .FindAsync(dto.StaffId);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Seçilen personel bulunamadı."
                );
            }

            var canProvideService = await _context.StaffServiceTypes.AnyAsync(x =>
                x.StaffId == dto.StaffId &&
                x.ServiceTypeId == dto.ServiceTypeId
            );

            if (!canProvideService)
            {
                throw new ConflictException(
                    "Seçilen personel bu hizmeti vermemektedir."
                );
            }

            var workingHour = await _context.StaffWorkingHours
                .FirstOrDefaultAsync(w =>
                    w.StaffId == dto.StaffId &&
                    w.DayOfWeek == dto.AppointmentDate.DayOfWeek
                );

            if (workingHour == null)
            {
                throw new ConflictException(
                    "Personel seçilen gün çalışmamaktadır."
                );
            }

            var appointmentStartTime = dto.AppointmentDate.TimeOfDay;

            var appointmentEndTime =
                appointmentStartTime.Add(
                    TimeSpan.FromMinutes(serviceType.DurationMinutes)
                );

            if (appointmentStartTime < workingHour.StartTime ||
                appointmentEndTime > workingHour.EndTime)
            {
                throw new ConflictException(
                    "Randevu personelin çalışma saatleri dışında."
                );
            }

            var newStart = dto.AppointmentDate;

            var newEnd = newStart.AddMinutes(
                serviceType.DurationMinutes
            );

            var hasConflict = await _context.Appointments
                .Include(a => a.ServiceType)
                .AnyAsync(a =>
                    a.Id != id &&
                    a.StaffId == dto.StaffId &&
                    a.Status != "Cancelled" &&
                    a.ServiceType != null &&
                    newStart < a.AppointmentDate.AddMinutes(
                        a.ServiceType.DurationMinutes
                    ) &&
                    newEnd > a.AppointmentDate
                );

            if (hasConflict)
            {
                throw new ConflictException(
                    "Seçilen personelin bu saat aralığında başka bir randevusu bulunmaktadır."
                );
            }

            _mapper.Map(dto, appointment);

            await _unitOfWork.SaveChangesAsync();

            appointment.ServiceType = serviceType;
            appointment.Staff = staff;

            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return false;
            }

            _context.Appointments.Remove(appointment);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<AppointmentResponseDto> UpdateStatusAsync(
            int id,
            UpdateAppointmentStatusDto dto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                throw new NotFoundException(
                    "Randevu bulunamadı."
                );
            }

            var allowedStatuses = new[]
            {
        "Pending",
        "Confirmed",
        "Completed",
        "Cancelled"
    };

            if (!allowedStatuses.Contains(dto.Status))
            {
                throw new ConflictException(
                    "Geçersiz randevu durumu."
                );
            }

            if (dto.Status == "Completed" && appointment.AppointmentDate > DateTime.Now)
            {
                throw new ConflictException("Gelecekteki bir randevu tamamlanamaz. Lütfen saatinin geçmesini bekleyin.");
            }

            appointment.Status = dto.Status;

            await _unitOfWork.SaveChangesAsync();

            // Email Notification to Customer based on new status
            string subject = "";
            string body = "";
            
            if (dto.Status == "Confirmed")
            {
                subject = "Randevunuz Onaylandı";
                body = $"<p>Merhaba {appointment.CustomerName},</p><p><strong>{appointment.AppointmentDate.ToString("dd.MM.yyyy HH:mm")}</strong> tarihindeki randevunuz onaylanmıştır.</p>";
            }
            else if (dto.Status == "Cancelled")
            {
                subject = "Randevunuz İptal Edildi";
                body = $"<p>Merhaba {appointment.CustomerName},</p><p><strong>{appointment.AppointmentDate.ToString("dd.MM.yyyy HH:mm")}</strong> tarihindeki randevunuz maalesef iptal edilmiştir.</p>";
            }
            
            if (!string.IsNullOrEmpty(subject))
            {
                await _emailService.SendEmailAsync(appointment.CustomerEmail, subject, body);
            }

            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        public async Task<AppointmentResponseDto> CancelMyAppointmentAsync(
            int appointmentId,
            int userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    a.UserId == userId
                );

            if (appointment == null)
            {
                throw new NotFoundException(
                    "Randevu bulunamadı veya bu randevu size ait değil."
                );
            }

            if (appointment.Status == "Cancelled")
            {
                throw new ConflictException(
                    "Randevu zaten iptal edilmiş."
                );
            }

            if (appointment.Status == "Completed")
            {
                throw new ConflictException(
                    "Tamamlanmış bir randevu iptal edilemez."
                );
            }

            appointment.Status = "Cancelled";

            await _unitOfWork.SaveChangesAsync();

            // Email Notification to Staff
            if (appointment.Staff != null && !string.IsNullOrEmpty(appointment.Staff.Email))
            {
                await _emailService.SendEmailAsync(appointment.Staff.Email, "Randevu İptali (Müşteri Tarafından)",
                    $"<p>Merhaba {appointment.Staff.FullName},</p>" +
                    $"<p>{appointment.CustomerName} adlı müşteri, <strong>{appointment.AppointmentDate.ToString("dd.MM.yyyy HH:mm")}</strong> tarihindeki randevusunu iptal etti.</p>");
            }

            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        public async Task<List<AppointmentResponseDto>> GetByStaffAsync(int staffId)
        {
            var staff = await _context.Staffs.FindAsync(staffId);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Personel bulunamadı."
                );
            }

            var appointments = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .Where(a => a.StaffId == staffId)
                .ToListAsync();

            return _mapper.Map<List<AppointmentResponseDto>>(appointments);
        }
        public async Task<List<AppointmentResponseDto>> GetByDateAsync(DateTime date)
        {
            var appointments = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .Where(a => a.AppointmentDate.Date == date.Date)
                .ToListAsync();

            return _mapper.Map<List<AppointmentResponseDto>>(appointments);
        }
        public async Task<List<AppointmentResponseDto>> GetByStatusAsync(string status)
        {
            var allowedStatuses = new[]
            {
        "Pending",
        "Confirmed",
        "Completed",
        "Cancelled"
    };

            if (!allowedStatuses.Contains(status))
            {
                throw new ConflictException(
                    "Geçersiz randevu durumu."
                );
            }

            var appointments = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .Where(a => a.Status == status)
                .ToListAsync();

            return _mapper.Map<List<AppointmentResponseDto>>(appointments);
        }
        public async Task<PagedResultDto<AppointmentResponseDto>> FilterAsync(
            int? staffId,
            DateTime? date,
            string? status,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize,
            string? search)
        {
            if (page < 1)
            {
                throw new ConflictException(
                    "Sayfa numarası 1 veya daha büyük olmalıdır."
                );
            }

            if (pageSize < 1)
            {
                throw new ConflictException(
                    "Sayfa boyutu 1 veya daha büyük olmalıdır."
                );
            }
            var query = _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .AsQueryable();
            if (startDate.HasValue &&
        endDate.HasValue &&
        startDate.Value.Date > endDate.Value.Date)
            {
                throw new ConflictException(
                    "Başlangıç tarihi bitiş tarihinden sonra olamaz."
                );
            }

            if (staffId.HasValue)
            {
                query = query.Where(a => a.StaffId == staffId.Value);
            }

            if (date.HasValue)
            {
                query = query.Where(
                    a => a.AppointmentDate.Date == date.Value.Date
                );
            }
            if (startDate.HasValue)
            {
                query = query.Where(
                    a => a.AppointmentDate >= startDate.Value.Date
                );
            }

            if (endDate.HasValue)
            {
                var nextDay = endDate.Value.Date.AddDays(1);

                query = query.Where(
                    a => a.AppointmentDate < nextDay
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var allowedStatuses = new[]
                {
        "Pending",
        "Confirmed",
        "Completed",
        "Cancelled"
    };

                if (!allowedStatuses.Contains(status))
                {
                    throw new ConflictException(
                        "Geçersiz randevu durumu."
                    );
                }

                query = query.Where(a => a.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.CustomerName.Contains(search) ||
                    a.CustomerEmail.Contains(search)
                );
            }
            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<AppointmentResponseDto>>(appointments);

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize
            );

            return new PagedResultDto<AppointmentResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };


        }
        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(
    int staffId,
    int serviceTypeId,
    DateTime date)
        {
            var serviceType = await _context.ServiceTypes
                .FindAsync(serviceTypeId);

            if (serviceType == null)
            {
                throw new NotFoundException(
                    "Hizmet bulunamadı."
                );
            }

            var staff = await _context.Staffs
                .FindAsync(staffId);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Hizmet veren bulunamadı."
                );
            }

            var canProvideService =
                await _context.StaffServiceTypes.AnyAsync(x =>
                    x.StaffId == staffId &&
                    x.ServiceTypeId == serviceTypeId
                );

            if (!canProvideService)
            {
                throw new ConflictException(
                    "Seçilen kişi bu hizmeti vermemektedir."
                );
            }

            var workingHour =
                await _context.StaffWorkingHours
                    .FirstOrDefaultAsync(w =>
                        w.StaffId == staffId &&
                        w.DayOfWeek == date.DayOfWeek
                    );

            if (workingHour == null)
            {
                return new List<AvailableSlotDto>();
            }

            // O güne ait aktif randevuları başlangıç + süre olarak çek
            var appointments =
                await _context.Appointments
                    .Where(a =>
                        a.StaffId == staffId &&
                        a.AppointmentDate.Date == date.Date &&
                        a.Status != "Cancelled"
                    )
                    .Select(a => new
                    {
                        StartTime = a.AppointmentDate,

                        DurationMinutes =
                            a.ServiceType != null
                                ? a.ServiceType.DurationMinutes
                                : 0
                    })
                    .ToListAsync();

            var availableSlots =
                new List<AvailableSlotDto>();

            var currentTime =
                date.Date.Add(workingHour.StartTime);

            var workingEnd =
                date.Date.Add(workingHour.EndTime);

            while (
                currentTime.AddMinutes(
                    serviceType.DurationMinutes
                ) <= workingEnd)
            {
                var slotStart = currentTime;

                var slotEnd =
                    slotStart.AddMinutes(
                        serviceType.DurationMinutes
                    );

                // Var olan herhangi bir randevuyla zaman aralığı çakışıyor mu?
                var hasConflict =
                    appointments.Any(a =>
                    {
                        var existingStart =
                            a.StartTime;

                        var existingEnd =
                            existingStart.AddMinutes(
                                a.DurationMinutes
                            );

                        return
                            slotStart < existingEnd &&
                            slotEnd > existingStart;
                    });

                if (!hasConflict &&
                    slotStart > DateTime.Now)
                {
                    availableSlots.Add(
                        new AvailableSlotDto
                        {
                            StartTime = slotStart,
                            EndTime = slotEnd
                        }
                    );
                }

                currentTime =
                    currentTime.AddMinutes(
                        serviceType.DurationMinutes
                    );
            }

            return availableSlots;
        }
        public async Task<DashboardDto> GetDashboardAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayAppointments = await _context.Appointments
                .CountAsync(a =>
                    a.AppointmentDate >= today &&
                    a.AppointmentDate < tomorrow &&
                    a.Status != "Cancelled"
                );

            var pendingAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Pending");

            var confirmedAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Confirmed");

            var completedAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Completed");

            var cancelledAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Cancelled");

            var totalAppointments = await _context.Appointments
                .CountAsync();
            var todayAppointmentList = await _context.Appointments
        .Include(a => a.ServiceType)
        .Include(a => a.Staff)
        .Where(a =>
            a.AppointmentDate >= today &&
            a.AppointmentDate < tomorrow &&
            a.Status != "Cancelled"
        )
        .OrderBy(a => a.AppointmentDate)
        .ToListAsync();

            return new DashboardDto
            {
                TodayAppointments = todayAppointments,
                PendingAppointments = pendingAppointments,
                ConfirmedAppointments = confirmedAppointments,
                CompletedAppointments = completedAppointments,
                CancelledAppointments = cancelledAppointments,
                TotalAppointments = totalAppointments,

                TodayAppointmentList =
                _mapper.Map<List<AppointmentResponseDto>>(todayAppointmentList)
            };
        }
        public async Task<List<AppointmentResponseDto>> GetMyStaffAppointmentsAsync(
            int userId)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Bu kullanıcıya bağlı personel kaydı bulunamadı."
                );
            }

            var appointments = await _context.Appointments
                .Where(a => a.StaffId == staff.Id)
                .Include(a => a.ServiceType)
                .Include(a => a.Staff)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return _mapper.Map<List<AppointmentResponseDto>>(appointments);
        }
        public async Task<int> GetStaffIdByUserIdAsync(int userId)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Bu kullanıcıya bağlı personel kaydı bulunamadı."
                );
            }

            return staff.Id;
        }


    }
}