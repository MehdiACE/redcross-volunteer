using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Repositories;
using RedCrossManager.Server.Services.Volunteers;
using RedCrossManager.Server.Services.Notifications;
using RedCrossManager.Server.Services.Onboarding;
using RedCrossManager.Server.Services.Consents;
using RedCrossManager.Server.Services.Auth;
using RedCrossManager.Server.Services.Dashboard;
using RedCrossManager.Server.Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog logging
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
builder.Services.AddScoped<LoggingActionFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LoggingActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<RedCrossDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IConsentService, ConsentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

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
using (var scope = app.Services.CreateScope())
{
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

// Enable CORS and auth
app.UseCors("AngularClient");
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

// Expose Program type for WebApplicationFactory in tests
public partial class Program { }
