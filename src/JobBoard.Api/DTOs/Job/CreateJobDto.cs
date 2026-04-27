using JobBoard.Api.Models;

public record CreateJobDto(
    Guid CompanyId,
    string Title,
    string Description,
    string Location,
    decimal SalaryMin,
    decimal SalaryMax,
    JobType Type,
    DateTime ExpiresAt
)
{
    public JobPosting ToModel() =>
        new JobPosting
        {
            CompanyId = CompanyId,
            Title = Title,
            Description = Description,
            Location = Location,
            SalaryMin = SalaryMin,
            SalaryMax = SalaryMax,
            Type = Type,
            ExpiresAt = ExpiresAt
        };
}