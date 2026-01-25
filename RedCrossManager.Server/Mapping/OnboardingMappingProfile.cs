using AutoMapper;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Onboarding;
using RedCrossManager.Server.DTOs.Consents;

namespace RedCrossManager.Server.Mapping;

public class OnboardingMappingProfile : Profile
{
    public OnboardingMappingProfile()
    {
        CreateMap<OnboardingStep, OnboardingStepDto>()
            .ForMember(dest => dest.StepType, opt => opt.MapFrom(src => src.StepType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<ParentalConsent, ParentalConsentDto>()
            .ForMember(dest => dest.ConsentStatus, opt => opt.MapFrom(src => src.ConsentStatus.ToString()))
            .ForMember(dest => dest.IdentityVerificationStatus, opt => opt.MapFrom(src => src.IdentityVerificationStatus.ToString()));
    }
}
