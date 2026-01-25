using AutoMapper;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Volunteers;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Volunteers;

public interface IVolunteerService
{
    Task<VolunteerDto> RegisterAsync(RegisterVolunteerDto dto, CancellationToken cancellationToken = default);
    Task<VolunteerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VolunteerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public class VolunteerService : IVolunteerService
{
    private readonly IVolunteerRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<VolunteerService> _logger;

    public VolunteerService(
        IVolunteerRepository repository,
        IMapper mapper,
        ILogger<VolunteerService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<VolunteerDto> RegisterAsync(RegisterVolunteerDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate email
        if (await _repository.EmailExistsAsync(dto.Email, cancellationToken))
        {
            throw new InvalidOperationException($"Volunteer with email {dto.Email} already exists.");
        }

        var volunteer = _mapper.Map<Volunteer>(dto);
        volunteer.Id = Guid.NewGuid();

        var created = await _repository.AddAsync(volunteer, cancellationToken);
        
        _logger.LogInformation("Volunteer registered: {VolunteerId}, Email: {Email}, IsMinor: {IsMinor}",
            created.Id, created.Email, created.IsMinor);

        return _mapper.Map<VolunteerDto>(created);
    }

    public async Task<VolunteerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var volunteer = await _repository.GetByIdAsync(id, cancellationToken);
        return volunteer == null ? null : _mapper.Map<VolunteerDto>(volunteer);
    }

    public async Task<VolunteerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var volunteer = await _repository.GetByEmailAsync(email, cancellationToken);
        return volunteer == null ? null : _mapper.Map<VolunteerDto>(volunteer);
    }
}
