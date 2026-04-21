using JobBoard.Api.Models;

namespace JobBoard.Api.Repositories;

public interface IJobRepository
{
    Task<JobPosting?> GetByIdAsync(Guid id);
    Task<IEnumerable<JobPosting>> GetAllAsync();
    Task AddAsync(JobPosting job);
    Task UpdateAsync(JobPosting job);
    Task SaveChangesAsync();
}