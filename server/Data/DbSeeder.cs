using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { role_name = "user" },
                new Role { role_name = "analyst" },
                new Role { role_name = "admin" }
            );
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { category_name = "malware" },
                new Category { category_name = "phishing" },
                new Category { category_name = "unauthorized access" },
                new Category { category_name = "data leak" },
                new Category { category_name = "suspicious file" },
                new Category { category_name = "ransomware" },
                new Category { category_name = "virus" },
                new Category { category_name = "spyware" },
                new Category { category_name = "adware" },
                new Category { category_name = "trojan" },
                new Category { category_name = "zero-day exploit" },
                new Category { category_name = "worm" },
                new Category { category_name = "other" }
            );
        }

        if (!await db.Severities.AnyAsync())
        {
            db.Severities.AddRange(
                new Severity { severity_name = "low" },
                new Severity { severity_name = "medium" },
                new Severity { severity_name = "high" },
                new Severity { severity_name = "critical" }
            );
        }

        await db.SaveChangesAsync();
    }
}