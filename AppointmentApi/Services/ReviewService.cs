using AppointmentApi.Data;
using AppointmentApi.DTOs;
using AppointmentApi.Exceptions;
using AppointmentApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApi.Services
{
    public class ReviewService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReviewService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto, int userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
            {
                throw new NotFoundException("Randevu bulunamadı.");
            }

            if (appointment.UserId != userId)
            {
                throw new ConflictException("Sadece kendi randevularınızı değerlendirebilirsiniz.");
            }

            if (appointment.Status != "Completed")
            {
                throw new ConflictException("Sadece tamamlanmış randevular değerlendirilebilir.");
            }

            var existingReview = await _context.Reviews
                .AnyAsync(r => r.AppointmentId == dto.AppointmentId);

            if (existingReview)
            {
                throw new ConflictException("Bu randevuyu zaten değerlendirdiniz.");
            }

            var review = _mapper.Map<Review>(dto);
            review.CustomerId = userId;
            review.StaffId = appointment.StaffId ?? 0;
            review.CreatedAt = DateTime.Now;
            
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // İlişkili verileri yükleyip DTO'ya çevirmek için Customer ve Staff'ı atayalım.
            var createdReview = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Staff)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            return _mapper.Map<ReviewResponseDto>(createdReview);
        }

        public async Task<List<ReviewResponseDto>> GetStaffReviewsAsync(int staffId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Staff)
                .Where(r => r.StaffId == staffId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ReviewResponseDto>>(reviews);
        }
    }
}
