using JobBoard.Api.Models;

namespace JobBoard.Api.Services;

public interface IApplicationService
{
    Task ApplyAsync(Guid jobId, Application application);
    Task<IEnumerable<Application>> GetByJobIdAsync(Guid jobId);
    Task UpdateStatusAsync(Guid applicationId, ApplicationStatus status);
}