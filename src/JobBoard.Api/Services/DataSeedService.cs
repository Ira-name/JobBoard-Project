using AutoFixture;
using JobBoard.Api.Data;
using JobBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Api.Services;

public interface IDataSeedService
{
    Task SeedAsync();
}

public class DataSeedService : IDataSeedService
{
    private readonly AppDbContext _context;
    private readonly Fixture _fixture;

    // Константи для обсягу даних
    private const int COMPANY_COUNT = 100;
    private const int JOBS_PER_COMPANY = 80; // Разом ~8000 вакансій
    private const int APPS_PER_JOB_AVG = 3;  // Разом ~3000 відгуків

    private readonly string[] CITIES = ["Kyiv", "Lviv", "Odesa", "Dnipro", "Kharkiv", "Rivne", "Vinnytsia"];
    private readonly string[] INDUSTRIES = ["IT", "Marketing", "Finance", "Healthcare", "Education"];

    public DataSeedService(AppDbContext context)
    {
        _context = context;
        _fixture = new Fixture();
        // Запобігаємо циклічним посиланням
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public async Task SeedAsync()
    {
        _context.Applications.RemoveRange(_context.Applications);
        _context.JobPostings.RemoveRange(_context.JobPostings);
        _context.Companies.RemoveRange(_context.Companies);
        // Тимчасові логи для перевірки
        Console.WriteLine("SEED STARTED");
        Console.WriteLine($"Current JobPostings count: {await _context.JobPostings.CountAsync()}");
        // Перевірка, чи база вже не порожня (використовуємо JobPostings)
        if (await _context.JobPostings.AnyAsync())
        {
            Console.WriteLine("Data already exists. Skipping seeding.");
            return;
        }

        try
        {
            // 1. Генеруємо Компанії
            var companies = GenerateCompanies(COMPANY_COUNT);
            _context.Companies.AddRange(companies);
            await _context.SaveChangesAsync();

            // 2. Генеруємо Вакансії
            var jobs = GenerateJobs(companies);
            _context.JobPostings.AddRange(jobs);
            await _context.SaveChangesAsync();

            // 3. Генеруємо Відгуки (Applications)
            var applications = GenerateApplications(jobs);
            _context.Applications.AddRange(applications);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seeding error: {ex.Message}");
            throw;
        }
    }

    private List<Company> GenerateCompanies(int count)
    {
        var companies = new List<Company>();
        for (int i = 0; i < count; i++)
        {
            companies.Add(new Company
            {
                Id = Guid.NewGuid(),
                Name = $"Enterprise Solutions {i}",
                Description = "Professional services and innovation provider.",
                Website = $"https://enterprise{i}.com",
                Industry = INDUSTRIES[Random.Shared.Next(INDUSTRIES.Length)],
                LogoUrl = $"https://assets.dev/logo{i}.png"
            });
        }
        return companies;
    }

    private List<JobPosting> GenerateJobs(List<Company> companies)
    {
        var jobs = new List<JobPosting>();
        var jobTypes = Enum.GetValues<JobType>();

        foreach (var company in companies)
        {
            for (int i = 0; i < JOBS_PER_COMPANY; i++)
            {
                var job = _fixture.Build<JobPosting>()
                    .Without(j => j.Id)
                    .Without(j => j.Company)
                    .Without(j => j.Applications)
                    .With(j => j.CompanyId, company.Id)
                    .With(j => j.Location, CITIES[Random.Shared.Next(CITIES.Length)])
                    .With(j => j.SalaryMin, (decimal)Random.Shared.Next(800, 2000))
                    .With(j => j.SalaryMax, (decimal)Random.Shared.Next(2100, 6000))
                    .With(j => j.Type, jobTypes[Random.Shared.Next(jobTypes.Length)])
                    .With(j => j.IsActive, true)
                    .With(j => j.PostedAt, DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 60)))
                    .With(j => j.ExpiresAt, DateTime.UtcNow.AddDays(Random.Shared.Next(30, 90)))
                    .Create();
                jobs.Add(job);
            }
        }
        return jobs;
    }

    private List<Application> GenerateApplications(List<JobPosting> jobs)
    {
        var applications = new List<Application>();
        var statuses = Enum.GetValues<ApplicationStatus>();
        int totalApps = jobs.Count * APPS_PER_JOB_AVG;

        for (int i = 0; i < totalApps; i++)
        {
            var randomJob = jobs[Random.Shared.Next(jobs.Count)];
            applications.Add(new Application
            {
                Id = Guid.NewGuid(),
                JobPostingId = randomJob.Id,
                ApplicantName = $"Applicant {i}",
                Email = $"candidate{i}@example.com",
                ResumeUrl = $"https://storage.dev/resumes/cv-{i}.pdf",
                CoverLetter = "I am applying for this position because I have relevant skills.",
                AppliedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 10)),
                Status = statuses[Random.Shared.Next(statuses.Length)]
            });
        }
        return applications;
    }
    
}