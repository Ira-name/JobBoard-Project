using System.ComponentModel.DataAnnotations;

namespace JobBoard.Api.Models;

public class Company
{
    /*public Guid Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Url]
    public string Website { get; set; } = string.Empty;

    public string Industry { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;

    // Навігаційна властивість для зв'язку 1-до-багатьох
    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();*/
    
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;

    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}