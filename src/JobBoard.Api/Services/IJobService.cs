using JobBoard.Api.Models;

namespace JobBoard.Api.Services;

public interface IJobService
{
    Task<IEnumerable<JobPosting>> GetActiveJobsAsync(string? location, JobType? type, decimal? minSalary);
    Task<JobPosting?> GetByIdAsync(Guid id);
    Task CreateAsync(JobPosting job);
    Task UpdateAsync(JobPosting job);
    Task CloseAsync(Guid id);
}