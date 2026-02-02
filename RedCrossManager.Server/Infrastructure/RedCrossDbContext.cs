using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Infrastructure;

public class RedCrossDbContext : DbContext
{
    public RedCrossDbContext(DbContextOptions<RedCrossDbContext> options) : base(options)
    {
    }

    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<ParentalConsent> ParentalConsents => Set<ParentalConsent>();
    public DbSet<OnboardingStep> OnboardingSteps => Set<OnboardingStep>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<TrainingEnrollment> TrainingEnrollments => Set<TrainingEnrollment>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<CommunicationMessage> CommunicationMessages => Set<CommunicationMessage>();
    public DbSet<CommunicationRecipient> CommunicationRecipients => Set<CommunicationRecipient>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Volunteer configuration
        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.HasIndex(v => v.Email).IsUnique();
            entity.Property(v => v.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(v => v.LastName).IsRequired().HasMaxLength(100);
            entity.Property(v => v.Email).IsRequired().HasMaxLength(255);
            entity.Property(v => v.Phone).IsRequired().HasMaxLength(50);
            entity.Property(v => v.AddressStreet).IsRequired().HasMaxLength(255);
            entity.Property(v => v.AddressCity).IsRequired().HasMaxLength(100);
            entity.Property(v => v.AddressStateProvince).IsRequired().HasMaxLength(100);
            entity.Property(v => v.AddressPostalCode).IsRequired().HasMaxLength(20);
            entity.Property(v => v.AddressCountry).IsRequired().HasMaxLength(100);
            entity.Property(v => v.EmergencyContactName).IsRequired().HasMaxLength(200);
            entity.Property(v => v.EmergencyContactPhone).IsRequired().HasMaxLength(50);
            entity.Property(v => v.AreasOfInterest).IsRequired();
            entity.Property(v => v.Availability).IsRequired();
            entity.Property(v => v.LanguagePreference).IsRequired().HasMaxLength(10);
            
            // Optional foreign key to User
            entity.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ParentalConsent configuration
        modelBuilder.Entity<ParentalConsent>(entity =>
        {
            entity.HasKey(pc => pc.Id);
            entity.HasIndex(pc => pc.VolunteerId).IsUnique();
            entity.Property(pc => pc.GuardianName).IsRequired().HasMaxLength(200);
            entity.Property(pc => pc.GuardianEmail).IsRequired().HasMaxLength(255);
            entity.Property(pc => pc.GuardianPhone).IsRequired().HasMaxLength(50);
            entity.Property(pc => pc.ConsentFormUrl).HasMaxLength(1024);
            entity.Property(pc => pc.ReviewerNotes).HasMaxLength(2000);

            entity.HasOne(pc => pc.Volunteer)
                .WithOne(v => v.ParentalConsent)
                .HasForeignKey<ParentalConsent>(pc => pc.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OnboardingStep configuration
        modelBuilder.Entity<OnboardingStep>(entity =>
        {
            entity.HasKey(os => os.Id);
            entity.HasIndex(os => new { os.VolunteerId, os.StepType });
            entity.Property(os => os.ReviewerNotes).HasMaxLength(2000);

            entity.HasOne(os => os.Volunteer)
                .WithMany(v => v.OnboardingSteps)
                .HasForeignKey(os => os.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => new { d.VolunteerId, d.Category });
            entity.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            entity.Property(d => d.FileUrl).IsRequired().HasMaxLength(1024);
            entity.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(d => d.ReviewerNotes).HasMaxLength(2000);

            entity.HasOne(d => d.Volunteer)
                .WithMany(v => v.Documents)
                .HasForeignKey(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Training configuration
        modelBuilder.Entity<Training>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => new { t.StartDate, t.Status });
            entity.Property(t => t.Title).IsRequired().HasMaxLength(255);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(2000);
            entity.Property(t => t.Category).IsRequired().HasMaxLength(100);
            entity.Property(t => t.LocationName).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Status).IsRequired().HasMaxLength(50);
        });

        // TrainingEnrollment configuration
        modelBuilder.Entity<TrainingEnrollment>(entity =>
        {
            entity.HasKey(te => te.Id);
            entity.HasIndex(te => new { te.TrainingId, te.VolunteerId }).IsUnique();
            entity.Property(te => te.Status).IsRequired().HasMaxLength(50);
            entity.Property(te => te.CertificateNumber).HasMaxLength(100);

            entity.HasOne(te => te.Training)
                .WithMany(t => t.Enrollments)
                .HasForeignKey(te => te.TrainingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(te => te.Volunteer)
                .WithMany()
                .HasForeignKey(te => te.VolunteerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Certification configuration
        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => new { c.VolunteerId, c.Type, c.ExpiresAt });
            entity.Property(c => c.Issuer).IsRequired().HasMaxLength(255);

            entity.HasOne(c => c.Volunteer)
                .WithMany(v => v.Certifications)
                .HasForeignKey(c => c.VolunteerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Document)
                .WithMany()
                .HasForeignKey(c => c.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Mission configuration
        modelBuilder.Entity<Mission>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => new { m.StartAt, m.Published });
            entity.Property(m => m.Title).IsRequired().HasMaxLength(255);
            entity.Property(m => m.Description).IsRequired().HasMaxLength(2000);
            entity.Property(m => m.Location).IsRequired().HasMaxLength(500);
        });

        // Assignment configuration
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => new { a.MissionId, a.VolunteerId });
            entity.HasIndex(a => new { a.VolunteerId, a.Status });
            entity.Property(a => a.RoleDescription).HasMaxLength(500);
            entity.Property(a => a.Notes).HasMaxLength(2000);
            entity.Property(a => a.HoursWorked).HasColumnType("decimal(5,2)");

            entity.HasOne(a => a.Mission)
                .WithMany(m => m.Assignments)
                .HasForeignKey(a => a.MissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Volunteer)
                .WithMany(v => v.Assignments)
                .HasForeignKey(a => a.VolunteerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CommunicationMessage configuration
        modelBuilder.Entity<CommunicationMessage>(entity =>
        {
            entity.HasKey(cm => cm.Id);
            entity.HasIndex(cm => new { cm.Segment, cm.SentAt });
            entity.Property(cm => cm.Segment).IsRequired().HasMaxLength(100);
            entity.Property(cm => cm.Language).IsRequired().HasMaxLength(10);
            entity.Property(cm => cm.Subject).HasMaxLength(500);
            entity.Property(cm => cm.BodyTemplate).IsRequired();
        });

        // CommunicationRecipient configuration
        modelBuilder.Entity<CommunicationRecipient>(entity =>
        {
            entity.HasKey(cr => cr.Id);
            entity.HasIndex(cr => new { cr.MessageId, cr.VolunteerId });
            entity.HasIndex(cr => new { cr.DeliveryStatus, cr.Channel });
            entity.Property(cr => cr.GuardianEmail).HasMaxLength(255);
            entity.Property(cr => cr.GuardianPhone).HasMaxLength(50);
            entity.Property(cr => cr.LastError).HasMaxLength(2000);

            entity.HasOne(cr => cr.Message)
                .WithMany(cm => cm.Recipients)
                .HasForeignKey(cr => cr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasIndex(n => new { n.UserId, n.CreatedAt });
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(2000);
            entity.Property(n => n.ActionUrl).HasMaxLength(1024);

            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Volunteer)
                .WithMany()
                .HasForeignKey(n => n.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(u => u.IsActive).IsRequired();
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(255);
        });

        // UserRole configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
