using AutoMapper;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Volunteers;
using System.Text.Json;

namespace RedCrossManager.Server.Mapping;

public class VolunteerMappingProfile : Profile
{
    public VolunteerMappingProfile()
    {
        CreateMap<RegisterVolunteerDto, Volunteer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AreasOfInterest, opt => opt.MapFrom(src => JsonSerializer.Serialize(src.AreasOfInterest)))
            .ForMember(dest => dest.Availability, opt => opt.MapFrom(src => JsonSerializer.Serialize(src.Availability)))
            .ForMember(dest => dest.IsMinor, opt => opt.MapFrom(src => CalculateIsMinor(src.DateOfBirth)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => VolunteerStatus.Pending))
            .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.SmsOptIn, opt => opt.MapFrom(_ => false));

        CreateMap<Volunteer, VolunteerDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }

    private static bool CalculateIsMinor(DateTime dateOfBirth)
    {
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;
        return age < 18;
    }
}
