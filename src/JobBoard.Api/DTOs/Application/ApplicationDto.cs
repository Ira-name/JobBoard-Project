using JobBoard.Api.Models;

public record ApplicationDto(
    Guid Id,
    Guid JobPostingId,
    string ApplicantName,
    string Email,
    DateTime AppliedAt,
    ApplicationStatus Status
)
{
    public static ApplicationDto FromModel(Application app) =>
        new(
            app.Id,
            app.JobPostingId,
            app.ApplicantName,
            app.Email,
            app.AppliedAt,
            app.Status
        );
}