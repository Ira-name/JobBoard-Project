using System.ComponentModel.DataAnnotations;

namespace JobBoard.Api.Models;

public class Application
{
    /*public Guid Id { get; set; }

    [Required]
    public Guid JobPostingId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ResumeUrl { get; set; } = string.Empty;

    [StringLength(2000)]
    public string CoverLetter { get; set; } = string.Empty;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    // Навігаційна властивість
    public JobPosting? JobPosting { get; set; }*/
    
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