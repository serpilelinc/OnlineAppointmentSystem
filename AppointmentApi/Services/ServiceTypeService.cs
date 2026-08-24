using AppointmentApi.Data;
using AppointmentApi.DTOs;
using AppointmentApi.Exceptions;
using AppointmentApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApi.Services
{
    public class ServiceTypeService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ServiceTypeService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceTypeResponseDto> CreateAsync(
            CreateServiceTypeDto dto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == dto.CategoryId);

            if (category == null)
            {
                throw new NotFoundException(
                    "Seçilen kategori bulunamadı."
                );
            }

            var serviceType =
                _mapper.Map<AppointmentServiceType>(dto);

            serviceType.CategoryId = category.Id;
            serviceType.Category = category;

            _context.ServiceTypes.Add(serviceType);

            await _context.SaveChangesAsync();

            var result =
                _mapper.Map<ServiceTypeResponseDto>(serviceType);

            result.CategoryId = category.Id;
            result.CategoryName = category.Name;

            return result;
        }

        public async Task<List<ServiceTypeResponseDto>> GetAllAsync()
        {
            var serviceTypes = await _context.ServiceTypes
                .Include(x => x.Category)
                .OrderBy(x => x.Category != null
                    ? x.Category.Name
                    : "")
                .ThenBy(x => x.Name)
                .ToListAsync();

            return serviceTypes
                .Select(x => new ServiceTypeResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    DurationMinutes = x.DurationMinutes,
                    Price = x.Price,

                    CategoryId = x.CategoryId ?? 0,

                    CategoryName =
                        x.Category != null
                            ? x.Category.Name
                            : "Kategorisiz"
                })
                .ToList();
        }
        public async Task<List<ServiceTypeResponseDto>> GetByCategoryAsync(
    int categoryId)
        {
            var categoryExists =
                await _context.Categories
                    .AnyAsync(x => x.Id == categoryId);

            if (!categoryExists)
            {
                throw new NotFoundException(
                    "Kategori bulunamadı."
                );
            }

            var serviceTypes =
                await _context.ServiceTypes
                    .Include(x => x.Category)
                    .Where(x => x.CategoryId == categoryId)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            return serviceTypes
                .Select(x => new ServiceTypeResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    DurationMinutes = x.DurationMinutes,
                    Price = x.Price,
                    CategoryId = x.CategoryId ?? 0,
                    CategoryName =
                        x.Category != null
                            ? x.Category.Name
                            : "Kategorisiz"
                })
                .ToList();
        }

        public async Task<ServiceTypeResponseDto> GetByIdAsync(
            int id)
        {
            var serviceType = await _context.ServiceTypes
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (serviceType == null)
            {
                throw new NotFoundException(
                    "Hizmet bulunamadı."
                );
            }

            return new ServiceTypeResponseDto
            {
                Id = serviceType.Id,
                Name = serviceType.Name,
                Description = serviceType.Description,
                DurationMinutes = serviceType.DurationMinutes,
                Price = serviceType.Price,

                CategoryId =
                    serviceType.CategoryId ?? 0,

                CategoryName =
                    serviceType.Category != null
                        ? serviceType.Category.Name
                        : "Kategorisiz"
            };
        }
        public async Task<ServiceTypeResponseDto> UpdateAsync(
    int id,
    UpdateServiceTypeDto dto)
        {
            var serviceType = await _context.ServiceTypes
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (serviceType == null)
            {
                throw new NotFoundException(
                    "Hizmet bulunamadı."
                );
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == dto.CategoryId);

            if (category == null)
            {
                throw new NotFoundException(
                    "Seçilen kategori bulunamadı."
                );
            }

            serviceType.Name = dto.Name.Trim();
            serviceType.Description = dto.Description.Trim();
            serviceType.DurationMinutes = dto.DurationMinutes;
            serviceType.Price = dto.Price;
            serviceType.CategoryId = category.Id;
            serviceType.Category = category;

            await _context.SaveChangesAsync();

            return new ServiceTypeResponseDto
            {
                Id = serviceType.Id,
                Name = serviceType.Name,
                Description = serviceType.Description,
                DurationMinutes = serviceType.DurationMinutes,
                Price = serviceType.Price,
                CategoryId = category.Id,
                CategoryName = category.Name
            };
        }

        public async Task DeleteAsync(int id)
        {
            var serviceType = await _context.ServiceTypes
                .FindAsync(id);

            if (serviceType == null)
            {
                throw new NotFoundException(
                    "Hizmet bulunamadı."
                );
            }

            _context.ServiceTypes.Remove(serviceType);

            await _context.SaveChangesAsync();
        }
    }
}