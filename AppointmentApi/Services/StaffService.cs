using AutoMapper;
using AppointmentApi.Data;
using AppointmentApi.DTOs;
using AppointmentApi.Exceptions;
using AppointmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApi.Services
{
    public class StaffService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public StaffService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StaffResponseDto> CreateAsync(CreateStaffDto dto)
        {
            var staff = _mapper.Map<Staff>(dto);

            _context.Staffs.Add(staff);

            await _context.SaveChangesAsync();

            return _mapper.Map<StaffResponseDto>(staff);
        }

        public async Task<List<StaffResponseDto>> GetAllAsync()
        {
            var staffs = await _context.Staffs.ToListAsync();

            return _mapper.Map<List<StaffResponseDto>>(staffs);
        }
        public async Task AssignServiceAsync(AssignServiceToStaffDto dto)
        {
            // Personel var mı?
            var staff = await _context.Staffs.FindAsync(dto.StaffId);

            if (staff == null)
            {
                throw new NotFoundException("Personel bulunamadı.");
            }

            // Hizmet var mı?
            var serviceType = await _context.ServiceTypes.FindAsync(dto.ServiceTypeId);

            if (serviceType == null)
            {
                throw new NotFoundException("Hizmet bulunamadı.");
            }

            // Bu hizmet bu personele daha önce atanmış mı?
            var alreadyAssigned = await _context.StaffServiceTypes.AnyAsync(x =>
                x.StaffId == dto.StaffId &&
                x.ServiceTypeId == dto.ServiceTypeId
            );

            if (alreadyAssigned)
            {
                throw new ConflictException(
                    "Bu hizmet zaten bu personele atanmış."
                );
            }

            var staffServiceType = new StaffServiceType
            {
                StaffId = dto.StaffId,
                ServiceTypeId = dto.ServiceTypeId
            };

            _context.StaffServiceTypes.Add(staffServiceType);

            await _context.SaveChangesAsync();
        }
        public async Task AddWorkingHourAsync(CreateStaffWorkingHourDto dto)
        {
            // Personel gerçekten var mı?
            var staff = await _context.Staffs.FindAsync(dto.StaffId);

            if (staff == null)
            {
                throw new NotFoundException("Personel bulunamadı.");
            }

            // Başlangıç saati bitiş saatinden önce olmalı
            if (dto.StartTime >= dto.EndTime)
            {
                throw new ConflictException(
                    "Başlangıç saati bitiş saatinden önce olmalıdır."
                );
            }
            var hasWorkingHourConflict =
            await _context.StaffWorkingHours.AnyAsync(w =>
                w.StaffId == dto.StaffId &&
                w.DayOfWeek == dto.DayOfWeek &&
                dto.StartTime < w.EndTime &&
                dto.EndTime > w.StartTime
            );

            if (hasWorkingHourConflict)
            {
                throw new ConflictException(
                    "Personelin bu gün için çakışan bir çalışma saati bulunmaktadır."
                );
            }

            var workingHour = new StaffWorkingHour
            {
                StaffId = dto.StaffId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            _context.StaffWorkingHours.Add(workingHour);

            await _context.SaveChangesAsync();
        }
        public async Task<List<StaffResponseDto>> GetByServiceTypeAsync(
            int serviceTypeId)
        {
            var serviceType = await _context.ServiceTypes
                .FindAsync(serviceTypeId);

            if (serviceType == null)
            {
                throw new NotFoundException(
                    "Hizmet bulunamadı."
                );
            }

            var staffs = await _context.StaffServiceTypes
                .Where(x => x.ServiceTypeId == serviceTypeId)
                .Include(x => x.Staff)
                .Select(x => x.Staff!)
                .ToListAsync();

            return _mapper.Map<List<StaffResponseDto>>(staffs);
        }
        public async Task<StaffResponseDto> GetByIdAsync(int id)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Personel bulunamadı."
                );
            }

            return _mapper.Map<StaffResponseDto>(staff);
        }
        public async Task<List<ServiceTypeResponseDto>> GetServicesByStaffAsync(
            int staffId)
        {
            var staff = await _context.Staffs
                .FindAsync(staffId);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Personel bulunamadı."
                );
            }

            var services = await _context.StaffServiceTypes
                .Where(x => x.StaffId == staffId)
                .Include(x => x.ServiceType)
                .Select(x => x.ServiceType!)
                .ToListAsync();

            return _mapper.Map<List<ServiceTypeResponseDto>>(services);
        }
        public async Task<List<StaffWorkingHour>> GetWorkingHoursByStaffAsync(
            int staffId)
        {
            var staff = await _context.Staffs
                .FindAsync(staffId);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Personel bulunamadı."
                );
            }

            return await _context.StaffWorkingHours
                .Where(x => x.StaffId == staffId)
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .ToListAsync();
        }
        public async Task<StaffResponseDto> UpdateAsync(
    int id,
    UpdateStaffDto dto)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Hizmet veren bulunamadı."
                );
            }

            staff.FullName = dto.FullName.Trim();
            staff.Email = dto.Email.Trim().ToLower();
            staff.Title = dto.Title.Trim();

            await _context.SaveChangesAsync();

            return _mapper.Map<StaffResponseDto>(staff);
        }


        public async Task RemoveServiceAsync(
            int staffId,
            int serviceTypeId)
        {
            var assignment =
                await _context.StaffServiceTypes
                    .FirstOrDefaultAsync(x =>
                        x.StaffId == staffId &&
                        x.ServiceTypeId == serviceTypeId
                    );

            if (assignment == null)
            {
                throw new NotFoundException(
                    "Bu hizmet, hizmet verene atanmış değil."
                );
            }

            _context.StaffServiceTypes.Remove(assignment);

            await _context.SaveChangesAsync();
        }
        public async Task DeleteWorkingHourAsync(int id)
        {
            var workingHour = await _context.StaffWorkingHours
                .FindAsync(id);

            if (workingHour == null)
            {
                throw new NotFoundException(
                    "Çalışma saati bulunamadı."
                );
            }

            _context.StaffWorkingHours.Remove(workingHour);

            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAllWorkingHoursByStaffAsync(int staffId)
        {
            var workingHours = await _context.StaffWorkingHours
                .Where(w => w.StaffId == staffId)
                .ToListAsync();

            if (workingHours.Any())
            {
                _context.StaffWorkingHours.RemoveRange(workingHours);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetStaffIdByUserIdAsync(int userId)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff == null)
            {
                throw new NotFoundException("Bu kullanıcıya bağlı personel kaydı bulunamadı.");
            }

            return staff.Id;
        }
        public async Task<List<StaffResponseDto>> GetAvailableStaffAsync(
    int serviceTypeId,
    DateTime date)
        {
            var staffs = await _context.Staffs
                .Where(staff =>
                    _context.StaffServiceTypes.Any(ss =>
                        ss.StaffId == staff.Id &&
                        ss.ServiceTypeId == serviceTypeId
                    )
                    &&
                    _context.StaffWorkingHours.Any(wh =>
                        wh.StaffId == staff.Id &&
                        wh.DayOfWeek == date.DayOfWeek
                    )
                )
                .ToListAsync();

            return _mapper.Map<List<StaffResponseDto>>(staffs);
        }
        public async Task DeleteAsync(int id)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (staff == null)
            {
                throw new NotFoundException(
                    "Hizmet veren bulunamadı."
                );
            }

            var hasAppointments =
                await _context.Appointments
                    .AnyAsync(x => x.StaffId == id);

            if (hasAppointments)
            {
                throw new ConflictException(
                    "Bu hizmet verene ait randevu kayıtları bulunduğu için silinemez."
                );
            }

            var serviceAssignments =
                await _context.StaffServiceTypes
                    .Where(x => x.StaffId == id)
                    .ToListAsync();

            if (serviceAssignments.Count > 0)
            {
                _context.StaffServiceTypes.RemoveRange(
                    serviceAssignments
                );
            }

            var workingHours =
                await _context.StaffWorkingHours
                    .Where(x => x.StaffId == id)
                    .ToListAsync();

            if (workingHours.Count > 0)
            {
                _context.StaffWorkingHours.RemoveRange(
                    workingHours
                );
            }

            if (staff.UserId.HasValue)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == staff.UserId.Value
                    );

                if (user != null)
                {
                    _context.Users.Remove(user);
                }
            }

            _context.Staffs.Remove(staff);

            await _context.SaveChangesAsync();
        }
    }
}