using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shared.Models;
using Xunit;

namespace server.Data;

public sealed class IntegrationCoverageTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public IntegrationCoverageTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Auth_RequiresToken_And_ReturnsCurrentUser()
    {
        var anonymousClient = _factory.CreateClient();
        var unauthorized = await anonymousClient.GetAsync("/api/auth/current-user");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var client = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(client, "user", "User123!");

        var response = await client.GetAsync("/api/auth/current-user");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal("user", document.RootElement.GetProperty("username").GetString());
    }

    [Fact]
    public async Task Incidents_Search_Filter_Paginate_And_Delete_Work()
    {
        var client = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(client, "analyst", "Analyst123!");

        var lookup = await GetLookups(client);
        var incident = await CreateIncident(client, lookup.CategoryIds["malware"], lookup.SeverityIds["low"], "Delete me incident");

        var searchResponse = await client.GetAsync("/api/incidents?search=VPN&resolved=false&page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        using var document = await JsonDocument.ParseAsync(await searchResponse.Content.ReadAsStreamAsync());
        Assert.Equal(1, document.RootElement.GetProperty("pageSize").GetInt32());
        Assert.True(document.RootElement.GetProperty("totalCount").GetInt32() >= 1);

        var deleteResponse = await client.DeleteAsync($"/api/incidents/{incident.incident_id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await client.GetAsync($"/api/incidents/{incident.incident_id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task Comments_Crud_Works()
    {
        var client = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(client, "analyst", "Analyst123!");

        var lookup = await GetLookups(client);
        var incident = await CreateIncident(client, lookup.CategoryIds["phishing"], lookup.SeverityIds["medium"], "Comment CRUD incident");

        var createResponse = await client.PostAsJsonAsync("/api/comments", new Comment
        {
            incident_id = incident.incident_id,
            comment_text = "First note"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<Comment>();
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync($"/api/comments/{created!.comment_id}", new Comment
        {
            comment_text = "Updated note"
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var detail = await client.GetAsync($"/api/incidents/{incident.incident_id}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var detailText = await detail.Content.ReadAsStringAsync();
        Assert.Contains("Updated note", detailText);

        var deleteResponse = await client.DeleteAsync($"/api/comments/{created.comment_id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Files_Upload_Download_Link_And_Remove_Work()
    {
        var client = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(client, "analyst", "Analyst123!");

        var lookup = await GetLookups(client);
        var incident = await CreateIncident(client, lookup.CategoryIds["suspicious file"], lookup.SeverityIds["high"], "File workflow incident");
        var fileContents = $"demo evidence {Guid.NewGuid():N}";
        var fileId = await UploadFile(client, $"/api/files/upload?incidentId={incident.incident_id}", "evidence.txt", fileContents);

        var incidentDetail = await client.GetAsync($"/api/incidents/{incident.incident_id}");
        Assert.Equal(HttpStatusCode.OK, incidentDetail.StatusCode);
        Assert.Contains("evidence.txt", await incidentDetail.Content.ReadAsStringAsync());

        var download = await client.GetAsync($"/api/files/{fileId}/download");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(fileContents, await download.Content.ReadAsStringAsync());

        var remove = await client.DeleteAsync($"/api/IncidentFiles/{incident.incident_id}/{fileId}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
    }

    [Fact]
    public async Task Admin_Crud_For_Users_Roles_Categories_And_Severity_Works()
    {
        var anonymous = _factory.CreateClient();
        var username = $"new_user_{Guid.NewGuid():N}"[..20];
        var register = await anonymous.PostAsJsonAsync("/api/auth/register", new
        {
            first_name = "New",
            last_name = "User",
            username,
            email = $"{username}@example.com",
            password = "User123!"
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var admin = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(admin, "admin", "Admin123!");

        var roleName = $"temp-role-{Guid.NewGuid():N}"[..20];
        var createRole = await admin.PostAsJsonAsync("/api/roles", new { role_name = roleName });
        Assert.Equal(HttpStatusCode.Created, createRole.StatusCode);
        var roleId = await ReadUInt(createRole, "role_id");
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/roles/{roleId}")).StatusCode);

        var categoryName = $"temp-category-{Guid.NewGuid():N}"[..24];
        var createCategory = await admin.PostAsJsonAsync("/api/categories", new { category_name = categoryName });
        Assert.Equal(HttpStatusCode.Created, createCategory.StatusCode);
        var categoryId = await ReadUInt(createCategory, "category_id");
        Assert.Equal(HttpStatusCode.NoContent, (await admin.PutAsJsonAsync($"/api/categories/{categoryId}", new { category_name = $"{categoryName}-u" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/categories/{categoryId}")).StatusCode);

        var severityName = $"temp-severity-{Guid.NewGuid():N}"[..24];
        var createSeverity = await admin.PostAsJsonAsync("/api/severity", new { severity_name = severityName });
        Assert.Equal(HttpStatusCode.Created, createSeverity.StatusCode);
        var severityId = await ReadUInt(createSeverity, "severity_id");
        Assert.Equal(HttpStatusCode.NoContent, (await admin.PutAsJsonAsync($"/api/severity/{severityId}", new { severity_name = $"{severityName}-u" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/severity/{severityId}")).StatusCode);

        var usersResponse = await admin.GetAsync("/api/auth/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);
        var usersJson = await usersResponse.Content.ReadAsStringAsync();
        Assert.Contains(username, usersJson);

        using var usersDocument = JsonDocument.Parse(usersJson);
        var userId = usersDocument.RootElement.EnumerateArray()
            .First(u => u.GetProperty("username").GetString() == username)
            .GetProperty("user_id")
            .GetUInt32();

        Assert.Equal(HttpStatusCode.NoContent, (await admin.PutAsJsonAsync($"/api/auth/users/{userId}", new { first_name = "Updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/auth/users/{userId}")).StatusCode);
    }

    [Fact]
    public async Task Dashboard_Endpoints_Return_Data()
    {
        var client = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(client, "analyst", "Analyst123!");

        foreach (var endpoint in new[]
        {
            "/api/dashboard/incidents-by-severity",
            "/api/dashboard/incidents-by-category",
            "/api/dashboard/reports-by-status",
            "/api/dashboard/user-report-counts",
            "/api/dashboard/average-time-to-resolution",
            "/api/dashboard/file-count-per-incident"
        })
        {
            var response = await client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()));
        }
    }

    [Fact]
    public async Task Report_To_Incident_Workflow_Copies_Report_Files()
    {
        var user = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(user, "user", "User123!");

        var reportResponse = await user.PostAsJsonAsync("/api/reports", new Report
        {
            title = "Workflow report",
            report_text = "Report with a file to copy."
        });
        Assert.Equal(HttpStatusCode.Created, reportResponse.StatusCode);
        var report = await reportResponse.Content.ReadFromJsonAsync<Report>();
        Assert.NotNull(report);

        var fileId = await UploadFile(user, $"/api/files/upload?reportId={report!.report_id}", "workflow.txt", $"workflow evidence {Guid.NewGuid():N}");

        var analyst = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(analyst, "analyst", "Analyst123!");
        var lookup = await GetLookups(analyst);
        var incident = await CreateIncident(analyst, lookup.CategoryIds["phishing"], lookup.SeverityIds["high"], "Workflow incident");

        var linkResponse = await analyst.PostAsync($"/api/reports/{report.report_id}/link-incident/{incident.incident_id}?copyFiles=true", null);
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        var incidentDetail = await analyst.GetAsync($"/api/incidents/{incident.incident_id}");
        Assert.Equal(HttpStatusCode.OK, incidentDetail.StatusCode);
        var detailText = await incidentDetail.Content.ReadAsStringAsync();
        Assert.Contains("workflow.txt", detailText);
        Assert.Contains(fileId.ToString(), detailText);
    }

    private static async Task<(Dictionary<string, uint> CategoryIds, Dictionary<string, uint> SeverityIds)> GetLookups(HttpClient client)
    {
        var categories = await client.GetFromJsonAsync<JsonElement[]>("/api/categories") ?? [];
        var severities = await client.GetFromJsonAsync<JsonElement[]>("/api/severity") ?? [];

        return (
            categories
                .GroupBy(c => c.GetProperty("category_name").GetString()!)
                .ToDictionary(g => g.Key, g => g.First().GetProperty("category_id").GetUInt32()),
            severities
                .GroupBy(s => s.GetProperty("severity_name").GetString()!)
                .ToDictionary(g => g.Key, g => g.First().GetProperty("severity_id").GetUInt32()));
    }

    private static async Task<Incident> CreateIncident(HttpClient client, uint categoryId, uint severityId, string title)
    {
        var response = await client.PostAsJsonAsync("/api/incidents", new Incident
        {
            category_id = categoryId,
            severity_id = severityId,
            incident_title = title,
            incident_description = $"{title} description"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var incident = await response.Content.ReadFromJsonAsync<Incident>();
        Assert.NotNull(incident);
        return incident!;
    }

    private static async Task<uint> UploadFile(HttpClient client, string url, string fileName, string contents)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new StringContent(contents);
        content.Add(fileContent, "uploadedFile", fileName);

        var response = await client.PostAsync(url, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadUInt(response, "file_id");
    }

    private static async Task<uint> ReadUInt(HttpResponseMessage response, string propertyName)
    {
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty(propertyName).GetUInt32();
    }
}
