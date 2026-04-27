using System.Net;
using System.Net.Http.Json;
using Shared.Models;
using Xunit;

namespace server.Data;

public sealed class ReportsCrudTests : IClassFixture<ApiFactory>
{
  private readonly HttpClient _client;

  public ReportsCrudTests(ApiFactory factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Report_Crud_Works()
  {
    var report = new Report
    {
      submitted_by_user_id = 1,
      title = "Integration test report",
      report_text = "Created by automated test",
      status = ReportStatus.submitted,
      submitted_at = DateTime.UtcNow
    };

    // Create
    var postResponse = await _client.PostAsJsonAsync("/api/reports", report);
    Console.WriteLine($"POST /api/reports response: {postResponse.StatusCode}");
    Console.WriteLine($"Expected response content: {HttpStatusCode.Created}");
    Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

    var created = await postResponse.Content.ReadFromJsonAsync<Report>();
    Assert.NotNull(created);
    Assert.True(created!.report_id > 0);

    var id = created.report_id;

    // Read one
    var getResponse = await _client.GetAsync($"/api/reports/{id}");
    Console.WriteLine($"GET /api/reports/{id} response: {getResponse.StatusCode}");
    Console.WriteLine($"Expected response content: {HttpStatusCode.OK}");
    Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    var fetched = await getResponse.Content.ReadFromJsonAsync<Report>();
    Assert.NotNull(fetched);
    Assert.Equal("Integration test report", fetched!.title);

    // Update
    fetched.title = "Updated integration test report";
    fetched.status = ReportStatus.under_review;

    var putResponse = await _client.PutAsJsonAsync($"/api/reports/{id}", fetched);
    Console.WriteLine($"PUT /api/reports/{id} response: {putResponse.StatusCode}");
    Console.WriteLine($"Expected response content: {HttpStatusCode.NoContent}");
    Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

    var updated = await _client.GetFromJsonAsync<Report>($"/api/reports/{id}");
    Assert.NotNull(updated);
    Assert.Equal("Updated integration test report", updated!.title);
    Assert.Equal(ReportStatus.under_review, updated.status);

    // Delete
    var deleteResponse = await _client.DeleteAsync($"/api/reports/{id}");
    Console.WriteLine($"DELETE /api/reports/{id} response: {deleteResponse.StatusCode}");
    Console.WriteLine($"Expected response content: {HttpStatusCode.NoContent}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    // Confirm delete
    var afterDeleteResponse = await _client.GetAsync($"/api/reports/{id}");
    Console.WriteLine($"GET /api/reports/{id} response after delete: {afterDeleteResponse.StatusCode}");
    Console.WriteLine($"Expected response content: {HttpStatusCode.NotFound}");
    Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
  }
}