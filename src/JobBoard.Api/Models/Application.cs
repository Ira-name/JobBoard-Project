using System.ComponentModel.DataAnnotations;

namespace JobBoard.Api.Models;

public class Application
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public string CoverLetter { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public ApplicationStatus Status { get; set; }

    public JobPosting? JobPosting { get; set; }
}