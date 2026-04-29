using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Report> Reports { get; set; }
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Severity> Severities { get; set; }
    public DbSet<Category> Categories { get; set; }
    


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.user_id);
            entity.HasIndex(u => u.username).IsUnique();
            entity.HasIndex(u => u.email).IsUnique();
            entity.Property(u => u.role_id)
                .IsRequired();
            entity.Property(u => u.first_name)
                .IsRequired();
            entity.Property(u => u.last_name)
                .IsRequired();
            entity.Property(u => u.username)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(u => u.email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(u => u.password_hash)
                .IsRequired();
            entity.Property(u => u.created_at)
                .IsRequired();
            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(u => u.role_id)
                .HasPrincipalKey(r => r.role_id);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(r => r.report_id);
            entity.Property(r => r.title)
                .IsRequired();
            entity.Property(r => r.report_text)
                .IsRequired();
            entity.Property(r => r.status)
                .HasConversion<string>()
                .IsRequired()
                .HasColumnType("enum('submitted','under_review','linked','closed','rejected')");
            entity.Property(r => r.submitted_at)
                .IsRequired();
            entity.Property(r => r.updated_at)
                .IsRequired();
            entity.HasOne(r => r.SubmittedByUser)
                .WithMany(u => u.CreatedReports)
                .HasForeignKey(r => r.submitted_by_user_id)
                .HasPrincipalKey(u => u.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Incident)
                .WithMany(i => i.Reports)
                .HasForeignKey(r => r.incident_id)
                .HasPrincipalKey(i => i.incident_id)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.ToTable("incidents");
            entity.HasKey(i => i.incident_id);

            entity.Property(i => i.incident_title)
                .HasColumnName("incident_title")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(i => i.incident_description)
                .HasColumnName("incident_description")
                .IsRequired(); 

            entity.Property(i => i.CreatedByUserId)
                .IsRequired();

            entity.Property(i => i.created_at)
                .IsRequired();

            entity.Property(i => i.updated_at)
                .IsRequired();

            entity.Property(i => i.resolved_at)
                .IsRequired(false);
                
            entity.HasOne(i => i.CreatedByUser)
                .WithMany(u => u.CreatedIncidents)
                .HasForeignKey(i => i.CreatedByUserId)
                .HasPrincipalKey(u => u.user_id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(r => r.role_id);
            entity.Property(r => r.role_name)
                .IsRequired()
                .HasMaxLength(50);
        });
    }
}
