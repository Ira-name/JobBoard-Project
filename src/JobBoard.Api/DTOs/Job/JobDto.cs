using JobBoard.Api.Models;

public record JobDto(
    Guid Id,
    Guid CompanyId,
    string Title,
    string Description,
    string Location,
    decimal SalaryMin,
    decimal SalaryMax,
    JobType Type,
    DateTime PostedAt,
    DateTime ExpiresAt,
    bool IsActive
)
{
    public static JobDto FromModel(JobPosting job) =>
        new(
            job.Id,
            job.CompanyId,
            job.Title,
            job.Description,
            job.Location,
            job.SalaryMin,
            job.SalaryMax,
            job.Type,
            job.PostedAt,
            job.ExpiresAt,
            job.IsActive
        );
}