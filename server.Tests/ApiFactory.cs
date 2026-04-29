using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySql.EntityFrameworkCore.Extensions;
using Shared.Models;

namespace server.Data;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("MySqlTest");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddEnvironmentVariables();
            config.AddUserSecrets<Program>(optional: true);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            using var tempProvider = services.BuildServiceProvider();
            var configuration = tempProvider.GetRequiredService<IConfiguration>();

            var connectionString =
                configuration.GetConnectionString("MySqlTest")
                ?? Environment.GetEnvironmentVariable("TEST_MYSQL_CONNECTION_STRING")
                ?? throw new InvalidOperationException(
                    "MySqlTest connection string is missing. Set ConnectionStrings:MySqlTest or TEST_MYSQL_CONNECTION_STRING.");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySQL(connectionString);
            });

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Roles.Add(new Role
            {
                role_id = 1,
                role_name = "user"
            });

            db.Users.Add(new User
            {
                user_id = 1,
                role_id = 1,
                first_name = "Integration",
                last_name = "Test",
                username = "integration_test_user",
                email = "integration.test@example.com",
                password_hash = "test-password-hash",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            db.SaveChanges();
        });
    }
}
