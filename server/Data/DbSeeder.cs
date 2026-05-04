using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Shared.Models;

namespace server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        await EnsureRoleAsync(db, "user");
        await EnsureRoleAsync(db, "analyst");
        await EnsureRoleAsync(db, "admin");

        foreach (var category in new[]
        {
            "malware",
            "phishing",
            "unauthorized access",
            "data leak",
            "suspicious file",
            "ransomware",
            "virus",
            "spyware",
            "adware",
            "trojan",
            "zero-day exploit",
            "worm",
            "other"
        })
        {
            await EnsureCategoryAsync(db, category);
        }

        foreach (var severity in new[] { "low", "medium", "high", "critical" })
        {
            await EnsureSeverityAsync(db, severity);
        }

        await db.SaveChangesAsync();

        await SeedDemoDataAsync(db, environment.ContentRootPath);
    }

    private static async Task SeedDemoDataAsync(ApplicationDbContext db, string contentRootPath)
    {
        var roles = (await db.Roles.ToListAsync())
            .GroupBy(r => r.role_name.ToLower())
            .ToDictionary(g => g.Key, g => g.First().role_id);
        var categories = (await db.Categories.ToListAsync())
            .GroupBy(c => c.category_name.ToLower())
            .ToDictionary(g => g.Key, g => g.First().category_id);
        var severities = (await db.Severities.ToListAsync())
            .GroupBy(s => s.severity_name.ToLower())
            .ToDictionary(g => g.Key, g => g.First().severity_id);

        var admin = await EnsureUserAsync(db, roles["admin"], "Admin", "User", "admin", "admin@example.com", "Admin123!");
        var analyst = await EnsureUserAsync(db, roles["analyst"], "Avery", "Analyst", "analyst", "analyst@example.com", "Analyst123!");
        var user = await EnsureUserAsync(db, roles["user"], "Uma", "User", "user", "user@example.com", "User123!");

        var evidenceFile = await EnsureDemoFileAsync(
            db,
            contentRootPath,
            "phishing-email.eml",
            "From: security-alert@example.test\nSubject: Password reset required\n\nDemo phishing email evidence.\n");
        var logFile = await EnsureDemoFileAsync(
            db,
            contentRootPath,
            "vpn-auth-log.txt",
            "2026-04-28T14:03:22Z failed login demo-user from 203.0.113.14\n");
        var screenshotFile = await EnsureDemoFileAsync(
            db,
            contentRootPath,
            "ransom-note.txt",
            "Demo ransom note placeholder for incident evidence.\n");

        var report1 = await EnsureReportAsync(
            db,
            user.user_id,
            "Suspicious password reset email",
            "A user received an email asking them to reset their password through an unknown link.",
            ReportStatus.linked);
        var report2 = await EnsureReportAsync(
            db,
            user.user_id,
            "Repeated VPN login failures",
            "Several failed VPN login attempts were observed against a single account.",
            ReportStatus.under_review);

        var incident1 = await EnsureIncidentAsync(
            db,
            analyst.user_id,
            categories["phishing"],
            severities["high"],
            "Credential phishing campaign",
            "Multiple users reported password reset emails pointing to an external collection site.",
            null);
        var incident2 = await EnsureIncidentAsync(
            db,
            analyst.user_id,
            categories["unauthorized access"],
            severities["medium"],
            "VPN brute-force activity",
            "Authentication logs show repeated failed VPN attempts against one account.",
            null);
        var incident3 = await EnsureIncidentAsync(
            db,
            admin.user_id,
            categories["ransomware"],
            severities["critical"],
            "Ransomware note discovered",
            "A demo ransomware note was uploaded for triage and containment practice.",
            DateTime.UtcNow.AddDays(-1));

        report1.incident_id = incident1.incident_id;
        report1.status = ReportStatus.linked;
        report1.updated_at = DateTime.UtcNow;

        await EnsureCommentAsync(db, incident1.incident_id, analyst.user_id, "Initial triage completed. Blocking indicators and preserving email evidence.");
        await EnsureCommentAsync(db, incident1.incident_id, admin.user_id, "Escalated to admin review for tenant-wide mailbox search.");
        await EnsureCommentAsync(db, incident2.incident_id, analyst.user_id, "Waiting for additional logs before closing this investigation.");

        await EnsureReportFileLinkAsync(db, report1.report_id, evidenceFile.file_id);
        await EnsureReportFileLinkAsync(db, report2.report_id, logFile.file_id);
        await EnsureIncidentFileLinkAsync(db, incident1.incident_id, evidenceFile.file_id);
        await EnsureIncidentFileLinkAsync(db, incident2.incident_id, logFile.file_id);
        await EnsureIncidentFileLinkAsync(db, incident3.incident_id, screenshotFile.file_id);

        await db.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(ApplicationDbContext db, string name)
    {
        if (!await db.Roles.AnyAsync(r => r.role_name == name))
        {
            db.Roles.Add(new Role { role_name = name });
        }
    }

    private static async Task EnsureCategoryAsync(ApplicationDbContext db, string name)
    {
        if (!await db.Categories.AnyAsync(c => c.category_name == name))
        {
            db.Categories.Add(new Category { category_name = name });
        }
    }

    private static async Task EnsureSeverityAsync(ApplicationDbContext db, string name)
    {
        if (!await db.Severities.AnyAsync(s => s.severity_name == name))
        {
            db.Severities.Add(new Severity { severity_name = name });
        }
    }

    private static async Task<User> EnsureUserAsync(
        ApplicationDbContext db,
        uint roleId,
        string firstName,
        string lastName,
        string username,
        string email,
        string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
        if (user != null)
        {
            return user;
        }

        user = new User
        {
            role_id = roleId,
            first_name = firstName,
            last_name = lastName,
            username = username,
            email = email,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        user.password_hash = new PasswordHasher<User>().HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task<Report> EnsureReportAsync(
        ApplicationDbContext db,
        uint userId,
        string title,
        string text,
        ReportStatus status)
    {
        var report = await db.Reports.FirstOrDefaultAsync(r => r.title == title);
        if (report != null)
        {
            return report;
        }

        report = new Report
        {
            submitted_by_user_id = userId,
            title = title,
            report_text = text,
            status = status,
            submitted_at = DateTime.UtcNow.AddDays(-4),
            updated_at = DateTime.UtcNow.AddDays(-3)
        };
        db.Reports.Add(report);
        await db.SaveChangesAsync();
        return report;
    }

    private static async Task<Incident> EnsureIncidentAsync(
        ApplicationDbContext db,
        uint userId,
        uint categoryId,
        uint severityId,
        string title,
        string description,
        DateTime? resolvedAt)
    {
        var incident = await db.Incidents.FirstOrDefaultAsync(i => i.incident_title == title);
        if (incident != null)
        {
            return incident;
        }

        incident = new Incident
        {
            CreatedByUserId = userId,
            category_id = categoryId,
            severity_id = severityId,
            incident_title = title,
            incident_description = description,
            created_at = DateTime.UtcNow.AddDays(-3),
            updated_at = resolvedAt ?? DateTime.UtcNow.AddDays(-1),
            resolved_at = resolvedAt
        };
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();
        return incident;
    }

    private static async Task EnsureCommentAsync(ApplicationDbContext db, uint incidentId, uint userId, string text)
    {
        var exists = await db.Comments.AnyAsync(c =>
            c.incident_id == incidentId &&
            c.user_id == userId &&
            c.comment_text == text);
        if (exists)
        {
            return;
        }

        db.Comments.Add(new Comment
        {
            incident_id = incidentId,
            user_id = userId,
            comment_text = text,
            created_at = DateTime.UtcNow.AddDays(-2),
            updated_at = DateTime.UtcNow.AddDays(-2)
        });
    }

    private static async Task<Shared.Models.File> EnsureDemoFileAsync(
        ApplicationDbContext db,
        string contentRootPath,
        string fileName,
        string contents)
    {
        var uploadRoot = Path.Combine(contentRootPath, "files");
        Directory.CreateDirectory(uploadRoot);

        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(contents)))
            .ToLowerInvariant();
        var storedFileName = $"demo_{hash}_{fileName}";
        var storedPath = Path.Combine(uploadRoot, storedFileName);

        if (!System.IO.File.Exists(storedPath))
        {
            try
            {
                await System.IO.File.WriteAllTextAsync(storedPath, contents);
            }
            catch (IOException) when (System.IO.File.Exists(storedPath))
            {
            }
        }

        var relativePath = $"files/{storedFileName}";
        var file = await db.Files.FirstOrDefaultAsync(f => f.file_hash == hash && f.file_name == fileName);
        if (file != null)
        {
            return file;
        }

        file = new Shared.Models.File
        {
            file_name = fileName,
            file_path = relativePath,
            file_hash = hash,
            uploaded_at = DateTime.UtcNow.AddDays(-3)
        };
        db.Files.Add(file);
        await db.SaveChangesAsync();
        return file;
    }

    private static async Task EnsureReportFileLinkAsync(ApplicationDbContext db, uint reportId, uint fileId)
    {
        if (!await db.ReportFiles.AnyAsync(rf => rf.report_id == reportId && rf.file_id == fileId))
        {
            db.ReportFiles.Add(new Report_File { report_id = reportId, file_id = fileId });
        }
    }

    private static async Task EnsureIncidentFileLinkAsync(ApplicationDbContext db, uint incidentId, uint fileId)
    {
        if (!await db.IncidentFiles.AnyAsync(ifile => ifile.incident_id == incidentId && ifile.file_id == fileId))
        {
            db.IncidentFiles.Add(new Incident_File { incident_id = incidentId, file_id = fileId });
        }
    }
}
