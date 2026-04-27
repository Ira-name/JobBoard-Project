using JobBoard.Api.Models;

public record UpdateJobDto(
    string Title,
    string Description,
    string Location,
    decimal SalaryMin,
    decimal SalaryMax,
    JobType Type,
    DateTime ExpiresAt
)
{
    public void UpdateModel(JobPosting job)
    {
        job.Title = Title;
        job.Description = Description;
        job.Location = Location;
        job.SalaryMin = SalaryMin;
        job.SalaryMax = SalaryMax;
        job.Type = Type;
        job.ExpiresAt = ExpiresAt;
    }
}