using AppointmentApi.Data;
using AppointmentApi.DTOs;
using AppointmentApi.Models;
using Microsoft.EntityFrameworkCore;
using AppointmentApi.UnitOfWork;

namespace AppointmentApi.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(AppDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryResponseDto> CreateAsync(
            CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim()
            };

            _context.Categories.Add(category);

            await _unitOfWork.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(x => x.Name)
                .Select(x => new CategoryResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                })
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                throw new Exception(
                    "Kategori bulunamadı."
                );
            }

            var hasService = await _context.ServiceTypes
                .AnyAsync(x => x.CategoryId == id);

            if (hasService)
            {
                throw new Exception(
                    "Bu kategoriye bağlı hizmetler bulunduğu için kategori silinemez."
                );
            }

            _context.Categories.Remove(category);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}