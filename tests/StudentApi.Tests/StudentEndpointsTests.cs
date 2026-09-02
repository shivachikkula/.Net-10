using System.Net;
using System.Net.Http.Json;
using StudentApi.Api.Dtos;

namespace StudentApi.Tests;

public class StudentEndpointsTests : IClassFixture<StudentApiFactory>
{
    private readonly HttpClient _client;

    public StudentEndpointsTests(StudentApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateStudentRequest NewStudentRequest(string? email = null) => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Email = email ?? $"ada.{Guid.NewGuid():N}@example.com",
        DateOfBirth = new DateOnly(2000, 12, 10),
        Department = "Mathematics",
        Gpa = 3.9m,
    };

    [Fact]
    public async Task CreateStudent_ReturnsCreated_WithLocationAndBody()
    {
        var request = NewStudentRequest();

        var response = await _client.PostAsJsonAsync("/api/students", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("Ada Lovelace", created.FullName);
        Assert.Equal(request.Email, created.Email);
    }

    [Fact]
    public async Task CreateStudent_DuplicateEmail_ReturnsConflict()
    {
        var request = NewStudentRequest();
        await _client.PostAsJsonAsync("/api/students", request);

        var response = await _client.PostAsJsonAsync("/api/students", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_InvalidPayload_ReturnsValidationProblem()
    {
        var invalid = NewStudentRequest() with { Email = "not-an-email" };

        var response = await _client.PostAsJsonAsync("/api/students", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStudentById_AfterCreate_ReturnsSameStudent()
    {
        var created = await CreateAsync(NewStudentRequest());

        var response = await _client.GetAsync($"/api/students/{created.Id}");
        var fetched = await response.Content.ReadFromJsonAsync<StudentResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(created.Email, fetched.Email);
    }

    [Fact]
    public async Task GetStudentById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/students/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllStudents_ReturnsCreatedStudent()
    {
        var created = await CreateAsync(NewStudentRequest());

        var response = await _client.GetAsync("/api/students");
        var students = await response.Content.ReadFromJsonAsync<List<StudentResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(students!, s => s.Id == created.Id);
    }

    [Fact]
    public async Task UpdateStudent_ExistingId_UpdatesFieldsAndTimestamp()
    {
        var created = await CreateAsync(NewStudentRequest());

        var update = new UpdateStudentRequest
        {
            FirstName = "Augusta",
            LastName = "King",
            Email = created.Email,
            DateOfBirth = created.DateOfBirth,
            Department = "Computer Science",
            Gpa = 4.0m,
        };

        var response = await _client.PutAsJsonAsync($"/api/students/{created.Id}", update);
        var updated = await response.Content.ReadFromJsonAsync<StudentResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Augusta King", updated!.FullName);
        Assert.Equal("Computer Science", updated.Department);
        Assert.NotNull(updated.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateStudent_UnknownId_ReturnsNotFound()
    {
        var update = NewStudentRequest();
        var payload = new UpdateStudentRequest
        {
            FirstName = update.FirstName,
            LastName = update.LastName,
            Email = update.Email,
            DateOfBirth = update.DateOfBirth,
            Department = update.Department,
            Gpa = update.Gpa,
        };

        var response = await _client.PutAsJsonAsync("/api/students/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteStudent_ExistingId_RemovesStudent()
    {
        var created = await CreateAsync(NewStudentRequest());

        var response = await _client.DeleteAsync($"/api/students/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/students/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteStudent_UnknownId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/students/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<StudentResponse> CreateAsync(CreateStudentRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/students", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StudentResponse>())!;
    }
}
