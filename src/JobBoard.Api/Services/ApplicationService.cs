using JobBoard.Api.Models;
using JobBoard.Api.Repositories;

namespace JobBoard.Api.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;

    public ApplicationService(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
    }

    public async Task ApplyAsync(Guid jobId, Application application)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null)
            throw new Exception("Job not found");

        // вакансія має бути активна
        if (!job.IsActive || job.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Cannot apply to inactive or expired job");

        // одна заявка на email
        var exists = await _applicationRepository.ExistsAsync(jobId, application.Email);

        if (exists)
            throw new Exception("Application with this email already exists");

        application.JobPostingId = jobId;
        application.AppliedAt = DateTime.UtcNow;
        application.Status = ApplicationStatus.Pending;

        await _applicationRepository.AddAsync(application);
        await _applicationRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<Application>> GetByJobIdAsync(Guid jobId)
    {
        return await _applicationRepository.GetByJobIdAsync(jobId);
    }

    public async Task UpdateStatusAsync(Guid applicationId, ApplicationStatus status)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);

        if (application == null)
            throw new Exception("Application not found");

        application.Status = status;

        await _applicationRepository.SaveChangesAsync();
    }
}