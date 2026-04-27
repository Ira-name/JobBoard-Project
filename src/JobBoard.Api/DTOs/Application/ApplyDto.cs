using JobBoard.Api.Models;

public record ApplyDto(
    string ApplicantName,
    string Email,
    string ResumeUrl,
    string CoverLetter
)
{
    public Application ToModel() =>
        new Application
        {
            ApplicantName = ApplicantName,
            Email = Email,
            ResumeUrl = ResumeUrl,
            CoverLetter = CoverLetter
        };
}