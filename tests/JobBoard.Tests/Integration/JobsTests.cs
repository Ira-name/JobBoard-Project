using System.Net;
using System.Net.Http.Json;
using JobBoard.Api.Data;
using JobBoard.Api.DTOs;
using JobBoard.Api.DTOs.Company;
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
    public async Task Create_ValidJob_Returns200()// Створення валідної вакансії повертає статус 200 OK
    {
        // Arrange 
        var company = new CreateCompanyDto(
            Name: "Test Company",
            Description: "Desc",
            Website: "https://test.com",
            Industry: "IT",
            LogoUrl: "logo.png"
        );

        var companyResponse = await _client.PostAsJsonAsync("/api/companies", company);
        companyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<CompanyDto>();
        createdCompany.ShouldNotBeNull();

        // Arrange
        var job = new
        {
            CompanyId = createdCompany.Id,
            Title = "Backend Developer",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/jobs", job);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<JobDto>();

        created.ShouldNotBeNull();
        created.Title.ShouldBe("Backend Developer");
        created.IsActive.ShouldBeTrue();
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAll_FiltersByLocation_ReturnsCorrectJobs()//Отримання всіх вакансій з фільтром по локації повертає правильні результати
    {
        // Arrange
        var company = new CreateCompanyDto(
            Name: "Test Company",
            Description: "Desc",
            Website: "https://test.com",
            Industry: "IT",
            LogoUrl: "logo.png"
        );

        var companyResponse = await _client.PostAsJsonAsync("/api/companies", company);
        companyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<CompanyDto>();
        createdCompany.ShouldNotBeNull();

        var companyId = createdCompany.Id;

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
    public async Task Apply_ToJob_Returns200_AndCreatesApplication()//Подання заявки на вакансію повертає 200 OK і створює запис Application
    {
        // Arrange
        var company = new CreateCompanyDto(
            Name: "Test Company",
            Description: "Desc",
            Website: "https://test.com",
            Industry: "IT",
            LogoUrl: "logo.png"
        );

        var companyResponse = await _client.PostAsJsonAsync("/api/companies", company);
        companyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<CompanyDto>();
        createdCompany.ShouldNotBeNull();

        // Arrange 
        var job = new CreateJobDto(
            createdCompany.Id,
            "QA Engineer",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", job);
        jobResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var createdJob = await jobResponse.Content.ReadFromJsonAsync<JobDto>();
        createdJob.ShouldNotBeNull();

        var applyDto = new ApplyDto(
            "Ira",
            "ira@test.com",
            "http://resume.com",
            "Hello"
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/jobs/{createdJob.Id}/apply",
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
    public async Task Update_Application_Status_Works()//“Оновлення статусу заявки працює коректно
    {
        // Arrange
        var company = new CreateCompanyDto(
            "Test Company",
            "Desc",
            "https://test.com",
            "IT",
            "logo.png"
        );

        var companyResponse = await _client.PostAsJsonAsync("/api/companies", company);
        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<CompanyDto>();

        // Arrange
        var job = new CreateJobDto(
            createdCompany!.Id,
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

        // Arrange
        var applyDto = new ApplyDto("Ira", "ira@test.com", "url", "text");

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

        // Assert (1) 
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Assert (2) 
        var getResponse = await _client.GetAsync($"/api/applications/{application.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updated = await getResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        updated.ShouldNotBeNull();
        updated.Status.ShouldBe(ApplicationStatus.Accepted);
    }
    
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Cannot_Apply_To_Expired_Job()//Не можна подати заявку на прострочену вакансію
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
    public async Task GetAll_DoesNotReturnExpiredJobs()//GET /api/jobs не повертає прострочені вакансії
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
    public async Task Cannot_Apply_Twice_With_Same_Email()//Не можна подати дві заявки з однаковим email
    {
        // Arrange
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", new CreateCompanyDto(
            Name: "Test Company",
            Description: "Test",
            Website: "https://test.com",
            Industry: "IT",
            LogoUrl: "logo.png"
        ));

        companyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var company = await companyResponse.Content.ReadFromJsonAsync<CompanyDto>();
        company.ShouldNotBeNull();

        // Arrange
        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", new CreateJobDto(
            company.Id,
            "Frontend",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        ));

        jobResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var job = await jobResponse.Content.ReadFromJsonAsync<JobDto>();
        job.ShouldNotBeNull();

        var applyDto = new ApplyDto(
            "Ira",
            "ira@test.com",
            "url",
            "text"
        );

        // Act
        var first = await _client.PostAsJsonAsync(
            $"/api/jobs/{job.Id}/apply",
            applyDto
        );

        // Assert
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var second = await _client.PostAsJsonAsync(
            $"/api/jobs/{job.Id}/apply",
            applyDto
        );

        // Assert
        second.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateJob_InvalidSalary_ReturnsBadRequest()//Створення вакансії з некоректною зарплатою повертає помилку
    {
        // Arrange 
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", new CreateCompanyDto(
            Name: "Test Company",
            Description: "Test",
            Website: "https://test.com",
            Industry: "IT",
            LogoUrl: "logo.png"
        ));

        companyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var company = await companyResponse.Content.ReadFromJsonAsync<CompanyDto>();
        company.ShouldNotBeNull();

        // Arrange 
        var job = new CreateJobDto(
            company.Id,
            "Bad Salary Job",
            "Test",
            "Lviv",
            3000,
            1000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/jobs", job);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_Status_For_NonExisting_Application_Returns404()//Оновлення статусу неіснуючої заявки повертає 404
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
    public async Task Cannot_Apply_To_Inactive_Job()//Не можна подати заявку на неактивну вакансію
    {
        // Arrange 
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", new CreateCompanyDto(
            "Test Company",
            "Test",
            "https://test.com",
            "IT",
            "logo.png"
        ));

        var company = await companyResponse.Content.ReadFromJsonAsync<CompanyDto>();

        // Arrange — job
        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", new CreateJobDto(
            company!.Id,
            "Inactive Job",
            "Test",
            "Lviv",
            1000,
            2000,
            JobType.FullTime,
            DateTime.UtcNow.AddDays(10)
        ));

        var job = await jobResponse.Content.ReadFromJsonAsync<JobDto>();
        
        var closeResponse = await _client.DeleteAsync($"/api/jobs/{job!.Id}");
        closeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act
        var applyDto = new ApplyDto("Ira", "ira@test.com", "url", "text");

        var response = await _client.PostAsJsonAsync(
            $"/api/jobs/{job.Id}/apply",
            applyDto
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
}