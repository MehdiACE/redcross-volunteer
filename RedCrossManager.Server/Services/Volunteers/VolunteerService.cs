using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Auth;
using RedCrossManager.Server.DTOs.Volunteers;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Repositories;
using RedCrossManager.Server.Services.Auth;
using RedCrossManager.Server.Services.Notifications;

namespace RedCrossManager.Server.Services.Volunteers;

public interface IVolunteerService
{
    Task<LoginResponseDto> RegisterAsync(RegisterVolunteerDto dto, CancellationToken cancellationToken = default);
    Task<VolunteerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VolunteerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<VolunteerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VolunteerDto?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<VolunteerDto?> UpdateSmsOptInAsync(Guid id, bool smsOptIn, CancellationToken cancellationToken = default);
}

public class VolunteerService : IVolunteerService
{
    private readonly IVolunteerRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<VolunteerService> _logger;
    private readonly RedCrossDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public VolunteerService(
        IVolunteerRepository repository,
        IEmailService emailService,
        IMapper mapper,
        ILogger<VolunteerService> logger,
        RedCrossDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _repository = repository;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterVolunteerDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate email
        if (await _repository.EmailExistsAsync(dto.Email, cancellationToken))
        {
            throw new InvalidOperationException($"Volunteer with email {dto.Email} already exists.");
        }

        // Check if email is already used by another user
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"Email {dto.Email} is already registered.");
        }

        // Create User entity with hashed password
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        
        _dbContext.Users.Add(user);

        // Assign "Volunteer" role
        var volunteerRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Volunteer", cancellationToken);
        if (volunteerRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = volunteerRole.Id
            };
            _dbContext.UserRoles.Add(userRole);
        }
        else
        {
            _logger.LogWarning("Volunteer role not found in database. User created without role.");
        }

        // Create Volunteer entity and link to User
        var volunteer = _mapper.Map<Volunteer>(dto);
        volunteer.Id = Guid.NewGuid();
        volunteer.UserId = user.Id;

        var created = await _repository.AddAsync(volunteer, cancellationToken);
        
        // Send confirmation email
        await _emailService.SendConfirmationEmailAsync(
            created.Email,
            created.FirstName,
            created.LanguagePreference,
            cancellationToken);

        _logger.LogInformation("Volunteer registered: {VolunteerId}, Email: {Email}, IsMinor: {IsMinor}, UserId: {UserId}",
            created.Id, created.Email, created.IsMinor, user.Id);

        // Get user roles for JWT token
        var roles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        // Generate JWT token
        var token = _jwtTokenService.CreateToken(user, roles);
        
        return new LoginResponseDto(
            UserId: user.Id,
            AccessToken: token.Token,
            ExpiresAtUtc: token.ExpiresAtUtc,
            Roles: roles.AsReadOnly()
        );
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

    public async Task<List<VolunteerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var volunteers = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<VolunteerDto>>(volunteers);
    }

    public async Task<VolunteerDto?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var volunteer = await _repository.GetByIdAsync(id, cancellationToken);
        if (volunteer == null)
        {
            return null;
        }

        // Parse and validate status
        if (!Enum.TryParse<VolunteerStatus>(status, out var volunteerStatus))
        {
            var validStatuses = string.Join(", ", Enum.GetNames(typeof(VolunteerStatus)));
            throw new InvalidOperationException($"Invalid status: {status}. Valid statuses are: {validStatuses}");
        }

        volunteer.Status = volunteerStatus;
        await _repository.UpdateAsync(volunteer, cancellationToken);

        _logger.LogInformation("Volunteer status updated: {VolunteerId}, NewStatus: {Status}", id, status);

        return _mapper.Map<VolunteerDto>(volunteer);
    }

    public async Task<VolunteerDto?> UpdateSmsOptInAsync(Guid id, bool smsOptIn, CancellationToken cancellationToken = default)
    {
        var volunteer = await _repository.GetByIdAsync(id, cancellationToken);
        if (volunteer == null)
        {
            return null;
        }

        volunteer.SmsOptIn = smsOptIn;
        await _repository.UpdateAsync(volunteer, cancellationToken);

        _logger.LogInformation("Volunteer SMS opt-in updated: {VolunteerId}, SmsOptIn: {SmsOptIn}", id, smsOptIn);

        return _mapper.Map<VolunteerDto>(volunteer);
    }
}
