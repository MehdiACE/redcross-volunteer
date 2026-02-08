using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RedCrossManager.Server.DTOs.Documents;
using RedCrossManager.Server.Tests.Infrastructure;
using Xunit;

namespace RedCrossManager.Server.Tests.Integration;

public class DocumentsTests : IAsyncLifetime
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private string _volunteerToken = null!;
    private string _coordinatorToken = null!;
    private Guid _volunteerId;

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        await _factory.InitializeDatabaseAsync();

        var seedResult = await _factory.SeedTestUsersAsync();
        _volunteerToken = seedResult["volunteer"];
        _coordinatorToken = seedResult["coordinator"];
        _volunteerId = Guid.Parse(seedResult["volunteerId"]);
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task UploadVerifyAndListDocuments_WorksForVolunteerAndCoordinator()
    {
        var request = new UploadDocumentRequestDto
        {
            Category = "Identification",
            FileName = "id.pdf",
            ContentType = "application/pdf",
            SizeBytes = 1024
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _volunteerToken);
        var uploadUrlResponse = await _client.PostAsJsonAsync("/api/v1/documents/upload-url", request);

        Assert.Equal(HttpStatusCode.OK, uploadUrlResponse.StatusCode);

        var uploadPayload = await uploadUrlResponse.Content.ReadFromJsonAsync<UploadDocumentResponseDto>();
        Assert.NotNull(uploadPayload);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _coordinatorToken);
        var verifyResponse = await _client.PatchAsJsonAsync($"/api/v1/documents/{uploadPayload!.DocumentId}/verify", new VerifyDocumentDto
        {
            Status = "Approved",
            ReviewerNotes = "Looks good"
        });

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _volunteerToken);
        var listResponse = await _client.GetAsync($"/api/v1/documents/{_volunteerId}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var documents = await listResponse.Content.ReadFromJsonAsync<List<DocumentDto>>();
        Assert.NotNull(documents);
        Assert.Single(documents!);
        Assert.Equal("Approved", documents![0].VerificationStatus);
    }
}
