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

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Shared.Models.File> Files { get; set; }

    public DbSet<Report_File> ReportFiles { get; set; }

    public DbSet<Incident_File> IncidentFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.category_id);

            entity.Property(c => c.category_name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(c => c.category_name)
                .IsUnique();
        });

        modelBuilder.Entity<Severity>(entity =>
        {
            entity.ToTable("severity");
            entity.HasKey(s => s.severity_id);

            entity.Property(s => s.severity_name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(s => s.severity_name)
                .IsUnique();
        });

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

        modelBuilder.Entity<Shared.Models.File>(entity =>
        {
            entity.ToTable("files", table =>
            {
                table.HasCheckConstraint(
                    "ck_files_file_hash_sha256",
                    "CHAR_LENGTH(`file_hash`) = 64 AND `file_hash` REGEXP '^[0-9A-Fa-f]{64}$'");
            });
            entity.HasKey(f => f.file_id);
            entity.Property(f => f.file_name)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(f => f.file_path)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(f => f.file_hash)
                .IsRequired()
                .HasColumnType("char(64)")
                .HasMaxLength(64)
                .IsFixedLength();
            entity.Property(f => f.uploaded_at)
                .IsRequired();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments", table =>
            {
                table.HasTrigger("trg_comments_before_insert_prevent_resolved_incident");
            });
            entity.HasKey(c => c.comment_id);
            entity.Property(c => c.incident_id)
                .IsRequired();
            entity.Property(c => c.user_id)
                .IsRequired();
            entity.Property(c => c.comment_text)
                .IsRequired();
            entity.Property(c => c.created_at)
                .IsRequired();
            entity.Property(c => c.updated_at)
                .IsRequired();

            entity.HasIndex(c => c.incident_id)
                .HasDatabaseName("fk_comments_incident");
            entity.HasIndex(c => c.user_id)
                .HasDatabaseName("fk_comments_user");

            entity.HasOne(c => c.Incident)
                .WithMany()
                .HasForeignKey(c => c.incident_id)
                .HasPrincipalKey(i => i.incident_id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.user_id)
                .HasPrincipalKey(u => u.user_id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Report_File>(entity =>
        {
            entity.ToTable("report_files");
            entity.HasKey(rf => new { rf.report_id, rf.file_id });

            entity.Property(rf => rf.report_id)
                .IsRequired();
            entity.Property(rf => rf.file_id)
                .IsRequired();
            entity.HasIndex(rf => rf.file_id)
                .HasDatabaseName("fk_report_files_file");

            entity.HasOne(rf => rf.Report)
                .WithMany()
                .HasForeignKey(rf => rf.report_id)
                .HasPrincipalKey(r => r.report_id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rf => rf.File)
                .WithMany()
                .HasForeignKey(rf => rf.file_id)
                .HasPrincipalKey(f => f.file_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Incident_File>(entity =>
        {
            entity.ToTable("incident_files");
            entity.HasKey(ifile => new { ifile.incident_id, ifile.file_id });

            entity.Property(ifile => ifile.incident_id)
                .IsRequired();
            entity.Property(ifile => ifile.file_id)
                .IsRequired();
            entity.HasIndex(ifile => ifile.file_id)
                .HasDatabaseName("fk_incident_files_file");

            entity.HasOne(ifile => ifile.Incident)
                .WithMany()
                .HasForeignKey(ifile => ifile.incident_id)
                .HasPrincipalKey(i => i.incident_id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ifile => ifile.File)
                .WithMany()
                .HasForeignKey(ifile => ifile.file_id)
                .HasPrincipalKey(f => f.file_id)
                .OnDelete(DeleteBehavior.Cascade);
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
