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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(r => r.report_id);
            entity.Property(r => r.title).IsRequired();
            entity.Property(r => r.report_text).IsRequired();
            entity.Property(r => r.status).HasConversion<string>().IsRequired().HasMaxLength(32);
        });
    }
}