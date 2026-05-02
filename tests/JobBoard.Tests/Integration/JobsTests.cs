using System.Net;
using System.Net.Http.Json;
using JobBoard.Api.Data;
using JobBoard.Api.DTOs;
using JobBoard.Api.Models;
using JobBoard.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace JobBoard.Tests.Integration;

public class JobsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public JobsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Create_ValidJob_Returns200()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;

        var job = new
        {
            CompanyId = companyId,
            Title = "Backend Developer",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };

        var response = await _client.PostAsJsonAsync("/api/jobs", job);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<JobDto>();

        created.ShouldNotBeNull();
        created.Title.ShouldBe("Backend Developer");
        created.IsActive.ShouldBeTrue();
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAll_FiltersByLocation_ReturnsCorrectJobs()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var companyId = db.Companies.First().Id;

        // Arrange
        var job1 = new CreateJobDto(
            companyId,
            "Job 1",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var job2 = new CreateJobDto(
            companyId,
            "Job 2",
            "Test",
            "Kyiv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var post1 = await _client.PostAsJsonAsync("/api/jobs", job1);
        post1.StatusCode.ShouldBe(HttpStatusCode.OK);

        var post2 = await _client.PostAsJsonAsync("/api/jobs", job2);
        post2.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response = await _client.GetAsync("/api/jobs?location=Lviv");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var jobs = await response.Content.ReadFromJsonAsync<List<JobDto>>();

        jobs.ShouldNotBeNull();
        jobs.ShouldNotBeEmpty();
        jobs.ShouldAllBe(j => j.Location == "Lviv");
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Apply_ToJob_Returns200_AndCreatesApplication()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;

        var job = new CreateJobDto(
            companyId,
            "QA Engineer",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", job);
        var createdJob = await jobResponse.Content.ReadFromJsonAsync<JobDto>();

        var applyDto = new ApplyDto(
            "Ira",
            "ira@test.com",
            "http://resume.com",
            "Hello"
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/jobs/{createdJob!.Id}/apply",
            applyDto
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var application = await response.Content.ReadFromJsonAsync<ApplicationDto>();

        application.ShouldNotBeNull();
        application.Email.ShouldBe("ira@test.com");
        application.Status.ShouldBe(ApplicationStatus.Pending);
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_Application_Status_Works()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;

        var job = new CreateJobDto(
            companyId,
            "Backend",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", job);
        var createdJob = await jobResponse.Content.ReadFromJsonAsync<JobDto>();

        var applyDto = new ApplyDto(
            "Ira",
            "ira@test.com",
            "url",
            "text"
        );

        var applyResponse = await _client.PostAsJsonAsync(
            $"/api/jobs/{createdJob!.Id}/apply",
            applyDto
        );

        var application = await applyResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var updateDto = new UpdateApplicationStatusDto(ApplicationStatus.Accepted);

        // Act
        var response = await _client.PatchAsJsonAsync(
            $"/api/applications/{application!.Id}/status",
            updateDto
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();

        var updated = db2.Applications.First(a => a.Id == application!.Id);

        updated.Status.ShouldBe(ApplicationStatus.Accepted);
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Cannot_Apply_To_Expired_Job()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;
        
        var job = new CreateJobDto(
            companyId,
            "Old Job",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(5)
        );

        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", job);
        jobResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var createdJob = await jobResponse.Content.ReadFromJsonAsync<JobDto>();
        
        var jobFromDb = db.JobPostings.First(j => j.Id == createdJob!.Id);
        jobFromDb.ExpiresAt = DateTime.UtcNow.AddDays(-1);

        await db.SaveChangesAsync();
        
        var applyDto = new ApplyDto("Ira", "ira@test.com", "url", "text");

        var response = await _client.PostAsJsonAsync(
            $"/api/jobs/{createdJob.Id}/apply",
            applyDto
        );
        
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAll_DoesNotReturnExpiredJobs()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;
        
        await _client.PostAsJsonAsync("/api/jobs", new CreateJobDto(
            companyId,
            "Active Job",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(5)
        ));
        
        await _client.PostAsJsonAsync("/api/jobs", new CreateJobDto(
            companyId,
            "Expired Job",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(-2)
        ));

        var response = await _client.GetAsync("/api/jobs");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var jobs = await response.Content.ReadFromJsonAsync<List<JobDto>>();

        jobs.ShouldNotBeNull();
        
        jobs.ShouldAllBe(j => j.IsActive);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Cannot_Apply_Twice_With_Same_Email()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;
        
        var job = new CreateJobDto(
            companyId,
            "Frontend",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", job);
        var createdJob = await jobResponse.Content.ReadFromJsonAsync<JobDto>();

        var applyDto = new ApplyDto(
            "Ira",
            "ira@test.com",
            "url",
            "text"
        );
        
        var first = await _client.PostAsJsonAsync(
            $"/api/jobs/{createdJob!.Id}/apply",
            applyDto
        );

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var second = await _client.PostAsJsonAsync(
            $"/api/jobs/{createdJob.Id}/apply",
            applyDto
        );
        
        second.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateJob_InvalidSalary_ReturnsBadRequest()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;

        var job = new CreateJobDto(
            companyId,
            "Bad Salary Job",
            "Test",
            "Lviv",
            3000, 
            1000, 
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var response = await _client.PostAsJsonAsync("/api/jobs", job);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_Status_For_NonExisting_Application_Returns404()
    {
        var fakeId = Guid.NewGuid();

        var updateDto = new UpdateApplicationStatusDto(ApplicationStatus.Accepted);

        var response = await _client.PatchAsJsonAsync(
            $"/api/applications/{fakeId}/status",
            updateDto
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Cannot_Apply_To_Inactive_Job()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = db.Companies.First().Id;

        var job = new CreateJobDto(
            companyId,
            "Inactive Job",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", job);
        var createdJob = await jobResponse.Content.ReadFromJsonAsync<JobDto>();
        
        var entity = db.JobPostings.First(j => j.Id == createdJob!.Id);
        entity.IsActive = false;
        await db.SaveChangesAsync();

        var applyDto = new ApplyDto("Ira", "ira@test.com", "url", "text");

        var response = await _client.PostAsJsonAsync(
            $"/api/jobs/{createdJob.Id}/apply",
            applyDto
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    
}