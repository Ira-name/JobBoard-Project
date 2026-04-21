using JobBoard.Api.Models;
using JobBoard.Api.Repositories;

namespace JobBoard.Api.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<IEnumerable<JobPosting>> GetActiveJobsAsync(string? location, JobType? type, decimal? minSalary)
    {
        var jobs = await _jobRepository.GetAllAsync();

        // деактивує прострочені
        foreach (var job in jobs)
        {
            if (job.ExpiresAt < DateTime.UtcNow)
                job.IsActive = false;
        }

        return jobs
            .Where(j => j.IsActive)
            .Where(j => location == null || j.Location == location)
            .Where(j => type == null || j.Type == type)
            .Where(j => minSalary == null || j.SalaryMin >= minSalary);
    }

    public async Task<JobPosting?> GetByIdAsync(Guid id)
    {
        return await _jobRepository.GetByIdAsync(id);
    }

    public async Task CreateAsync(JobPosting job)
    {
        if (job.SalaryMin >= job.SalaryMax)
            throw new Exception("SalaryMin must be less than SalaryMax");

        if (job.ExpiresAt <= DateTime.UtcNow)
            throw new Exception("ExpiresAt must be in the future");

        job.PostedAt = DateTime.UtcNow;
        job.IsActive = true;

        await _jobRepository.AddAsync(job);
        await _jobRepository.SaveChangesAsync();
    }

    public async Task UpdateAsync(JobPosting job)
    {
        if (job.SalaryMin >= job.SalaryMax)
            throw new Exception("Invalid salary range");

        await _jobRepository.UpdateAsync(job);
        await _jobRepository.SaveChangesAsync();
    }

    public async Task CloseAsync(Guid id)
    {
        var job = await _jobRepository.GetByIdAsync(id);

        if (job == null)
            throw new Exception("Job not found");

        job.IsActive = false;

        await _jobRepository.UpdateAsync(job);
        await _jobRepository.SaveChangesAsync();
    }
}