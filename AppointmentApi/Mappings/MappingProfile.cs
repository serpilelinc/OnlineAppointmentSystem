using AutoMapper;
using AppointmentApi.DTOs;
using AppointmentApi.Models;

namespace AppointmentApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateAppointmentDto, Appointment>();

            CreateMap<UpdateAppointmentDto, Appointment>();

            CreateMap<Appointment, AppointmentResponseDto>()
    .ForMember(
        dest => dest.ServiceTypeName,
        opt => opt.MapFrom(src => src.ServiceType != null
            ? src.ServiceType.Name
            : string.Empty)
    )
    .ForMember(
        dest => dest.StaffName,
        opt => opt.MapFrom(src => src.Staff != null
            ? src.Staff.FullName
            : string.Empty)
    );
            CreateMap<CreateServiceTypeDto, AppointmentServiceType>();
            CreateMap<AppointmentServiceType, ServiceTypeResponseDto>();

            CreateMap<CreateStaffDto, Staff>();
            CreateMap<Staff, StaffResponseDto>();

            CreateMap<CreateReviewDto, Review>();
            CreateMap<Review, ReviewResponseDto>()
                .ForMember(
                    dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : string.Empty)
                )
                .ForMember(
                    dest => dest.StaffName,
                    opt => opt.MapFrom(src => src.Staff != null ? src.Staff.FullName : string.Empty)
                );
        }
    }
}