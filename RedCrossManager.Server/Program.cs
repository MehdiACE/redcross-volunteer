using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Repositories;
using RedCrossManager.Server.Services.Volunteers;
using RedCrossManager.Server.Services.Notifications;
using RedCrossManager.Server.Services.Messages;
using RedCrossManager.Server.Services.Onboarding;
using RedCrossManager.Server.Services.Consents;
using RedCrossManager.Server.Services.Auth;
using RedCrossManager.Server.Services.Dashboard;
using RedCrossManager.Server.Services.Trainings;
using RedCrossManager.Server.Services.Certificates;
using RedCrossManager.Server.Services.Communications;
using RedCrossManager.Server.Services.Missions;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Services.Documents;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Serilog logging
builder.Services.AddHttpContextAccessor();
builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.With(new PiiSafeLogEnricher(services.GetRequiredService<IHttpContextAccessor>()))
    .Enrich.WithProperty("Application", "RedCrossManager.Server"));

// Add services to the container.
builder.Services.AddScoped<LoggingActionFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LoggingActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<RedCrossDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
builder.Services.AddHealthChecks();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// JWT options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Auth services
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Repositories
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddScoped<IOnboardingStepRepository, OnboardingStepRepository>();
builder.Services.AddScoped<IParentalConsentRepository, ParentalConsentRepository>();
builder.Services.AddScoped<ITrainingRepository, TrainingRepository>();
builder.Services.AddScoped<ITrainingEnrollmentRepository, TrainingEnrollmentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ICommunicationRepository, CommunicationRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IConsentService, ConsentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<IAssignmentValidator, AssignmentValidator>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Communication providers (email/SMS)
builder.Services.AddScoped<IEmailProvider, SendGridEmailProvider>();
builder.Services.AddScoped<ISmsProvider, AzureSmsProvider>();

// CORS for Angular client
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentication & Authorization
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Volunteer", p => p.RequireRole("Volunteer", "Coordinator", "Admin"));
    options.AddPolicy("Coordinator", p => p.RequireRole("Coordinator", "Admin"));
    options.AddPolicy("Admin", p => p.RequireRole("Admin"));
});

var app = builder.Build();

// Seed database with roles
if (!app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
    await DatabaseSeeder.SeedRolesAsync(dbContext);
    await DatabaseSeeder.SeedAdminUserAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseSerilogRequestLogging();

// Enable CORS and auth
app.UseCors("AngularClient");
app.UseAuthentication();
app.UseAuthorization();

// Serve static files (e.g., uploaded documents)
app.UseStaticFiles();

// Map controllers
app.MapControllers();

app.Run();

// Expose Program type for WebApplicationFactory in tests
public partial class Program { }
