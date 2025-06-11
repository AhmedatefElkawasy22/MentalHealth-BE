using AutoMapper;
using MentalHealth.DTOs;
using MentalHealth_BackEnd.DTO.Appointment;
using MentalHealth_BackEnd.DTO.Specialization;
using MentalHealth_BackEnd.DTO.Therapist;
using MentalHealth_BackEnd.Models;

namespace MentalHealth_BackEnd.helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            #region Specialization DTO
            CreateMap<Specialization, GetSpecializations>();
            CreateMap<AddSpecialization, Specialization>();
            CreateMap<UpdateSpecialization, Specialization>();
            #endregion

            #region Therabist
            CreateMap<TherapistAndDoctor, TherapistDto>();

            #endregion

            #region Visitor
            CreateMap<Visitor, VisitorDto>();
            CreateMap<VisitorDto, Visitor>();

            #endregion

            #region Appointement
            CreateMap<AppointmentCreateDto, Appointment>();
            CreateMap<AppointmentUpdateDto, Appointment>();
            CreateMap<Appointment, AppointmentReadDto>()
                .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.TherapistName, opt => opt.MapFrom(src => src.TherapistAndDoctor.Name));

            #endregion

            #region Appointement Status 
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.TherapistName, opt => opt.MapFrom(src => src.TherapistAndDoctor.Name));

            #endregion
        }
    }
}
