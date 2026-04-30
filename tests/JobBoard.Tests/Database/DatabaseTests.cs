using JobBoard.Api.Data;
using JobBoard.Api.Models;
using JobBoard.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace JobBoard.Tests.Database;

public class DatabaseTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DatabaseTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Unique_Application_Email_Per_Job_Is_Enforced()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var job = db.JobPostings.First();

        db.Applications.Add(new Application
        {
            JobPostingId = job.Id,
            ApplicantName = "Test 1",
            Email = "dup@test.com",
            ResumeUrl = "url",
            CoverLetter = "text",
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        });

        await db.SaveChangesAsync();

        db.Applications.Add(new Application
        {
            JobPostingId = job.Id,
            ApplicantName = "Test 2",
            Email = "dup@test.com", // ❗ дубль
            ResumeUrl = "url",
            CoverLetter = "text",
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        });

        await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Query_Expired_Jobs_Returns_Correct_Ones()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var company = db.Companies.First();

        var activeJob = new JobPosting
        {
            CompanyId = company.Id,
            Title = "Active",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };

        var expiredJob = new JobPosting
        {
            CompanyId = company.Id,
            Title = "Expired",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };

        db.JobPostings.AddRange(activeJob, expiredJob);
        await db.SaveChangesAsync();

        var expired = db.JobPostings
            .Where(j => j.ExpiresAt < DateTime.UtcNow)
            .ToList();

        expired.ShouldNotBeEmpty();
        expired.ShouldAllBe(j => j.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public async Task Cascade_Delete_Company_Removes_Jobs_And_Applications()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var company = new Company
        {
            Name = "Test Company",
            Description = "Test",
            Website = "test.com",
            Industry = "IT",
            LogoUrl = "logo"
        };

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var job = new JobPosting
        {
            CompanyId = company.Id,
            Title = "Job",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };

        db.JobPostings.Add(job);
        await db.SaveChangesAsync();

        var application = new Application
        {
            JobPostingId = job.Id,
            ApplicantName = "Ira",
            Email = "ira@test.com",
            ResumeUrl = "url",
            CoverLetter = "text",
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        };

        db.Applications.Add(application);
        await db.SaveChangesAsync();

        // Act
        db.Companies.Remove(company);
        await db.SaveChangesAsync();

        // Assert
        db.JobPostings.ShouldNotContain(j => j.CompanyId == company.Id);
        db.Applications.ShouldNotContain(a => a.JobPostingId == job.Id);
    }
    [Fact]
    public async Task Cannot_Create_Application_With_Invalid_JobId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var application = new Application
        {
            JobPostingId = Guid.NewGuid(), // ❗ не існує
            ApplicantName = "Test",
            Email = "test@test.com",
            ResumeUrl = "url",
            CoverLetter = "text",
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        };

        db.Applications.Add(application);

        await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact]
    public async Task Cascade_Delete_Job_Removes_Applications()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var company = db.Companies.First();

        var job = new JobPosting
        {
            CompanyId = company.Id,
            Title = "Test Job",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };

        db.JobPostings.Add(job);
        await db.SaveChangesAsync();

        db.Applications.Add(new Application
        {
            JobPostingId = job.Id,
            ApplicantName = "Ira",
            Email = "ira@test.com",
            ResumeUrl = "url",
            CoverLetter = "text",
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        });

        await db.SaveChangesAsync();

        // Act
        db.JobPostings.Remove(job);
        await db.SaveChangesAsync();

        // Assert
        var applications = db.Applications
            .Where(a => a.JobPostingId == job.Id)
            .ToList();

        applications.ShouldBeEmpty();
    }
    [Fact]
    public async Task Query_Active_Jobs_Returns_Only_Valid_Ones()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var company = db.Companies.First();

        var active = new JobPosting
        {
            CompanyId = company.Id,
            Title = "Active",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };

        var inactive = new JobPosting
        {
            CompanyId = company.Id,
            Title = "Inactive",
            Description = "Test",
            Location = "Lviv",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsActive = false
        };

        db.JobPostings.AddRange(active, inactive);
        await db.SaveChangesAsync();

        var result = db.JobPostings
            .Where(j => j.IsActive && j.ExpiresAt > DateTime.UtcNow)
            .ToList();

        result.ShouldNotBeEmpty();
        result.ShouldAllBe(j => j.IsActive && j.ExpiresAt > DateTime.UtcNow);
    }
}