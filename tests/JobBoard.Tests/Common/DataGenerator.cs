using AutoFixture;
using JobBoard.Api.Data;
using JobBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Tests.Common;

public static class DataGenerator
{
    private static readonly Fixture Fixture = CreateFixture();
    private static readonly Random Random = new();

    public static async Task PopulateTestData(AppDbContext db)
    {
        if (await db.JobPostings.AnyAsync())
            return;

        // Companies
        var companies = GenerateCompanies(200);
        await db.Companies.AddRangeAsync(companies);
        await db.SaveChangesAsync();

        var companyIds = companies.Select(c => c.Id).ToList();

        // JobPostings
        var jobs = GenerateJobs(10_000, companyIds);
        await db.JobPostings.AddRangeAsync(jobs);
        await db.SaveChangesAsync();

        var jobIds = jobs.Select(j => j.Id).ToList();

        // Applications
        var applications = GenerateApplications(5_000, jobIds);
        await db.Applications.AddRangeAsync(applications);

        await db.SaveChangesAsync();
    }
    
    // Companies
    private static List<Company> GenerateCompanies(int count)
    {
        return Fixture.Build<Company>()
            .Without(c => c.JobPostings)
            .With(c => c.Name, () => $"Company {Guid.NewGuid().ToString()[..8]}")
            .CreateMany(count)
            .ToList();
    }

    // Jobs
    private static List<JobPosting> GenerateJobs(int count, List<Guid> companyIds)
    {
        var jobs = new List<JobPosting>();

        for (int i = 0; i < count; i++)
        {
            var salaryMin = Random.Next(800, 5000);
            var salaryMax = salaryMin + Random.Next(500, 7000);

            jobs.Add(new JobPosting
            {
                Id = Guid.NewGuid(),
                CompanyId = companyIds[Random.Next(companyIds.Count)],

                Title = $"Job {i}",
                Description = "Auto generated job for testing",

                Location = GetRandomLocation(),
                SalaryMin = salaryMin,
                SalaryMax = salaryMax,

                Type = GetRandomJobType(),

                PostedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 60)),
                ExpiresAt = DateTime.UtcNow.AddDays(Random.Next(5, 90)),

                IsActive = true
            });
        }

        return jobs;
    }
    
    // Applications
    private static List<Application> GenerateApplications(int count, List<Guid> jobIds)
    {
        var applications = new List<Application>();

        for (int i = 0; i < count; i++)
        {
            applications.Add(new Application
            {
                Id = Guid.NewGuid(),
                JobPostingId = jobIds[Random.Next(jobIds.Count)],

                ApplicantName = $"User {i}",
                Email = $"user{i}@test.com",

                ResumeUrl = "https://example.com/resume.pdf",
                CoverLetter = "Auto generated cover letter",

                AppliedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 30)),

                Status = GetRandomStatus()
            });
        }

        return applications;
    }
    
    private static string GetRandomLocation()
    {
        var locations = new[] { "Kyiv", "Lviv", "Odesa", "Dnipro", "Kharkiv" };
        return locations[Random.Next(locations.Length)];
    }

    private static JobType GetRandomJobType()
    {
        var values = Enum.GetValues<JobType>();
        return values[Random.Next(values.Length)];
    }

    private static ApplicationStatus GetRandomStatus()
    {
        var values = Enum.GetValues<ApplicationStatus>();
        return values[Random.Next(values.Length)];
    }

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}