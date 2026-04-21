using JobBoard.Api.Models;

namespace JobBoard.Api.Repositories;

public interface  IApplicationRepository
{
    Task AddAsync(Application application);
    Task<IEnumerable<Application>> GetByJobIdAsync(Guid jobId);
    Task<bool> ExistsAsync(Guid jobId, string email);
    Task<Application?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}