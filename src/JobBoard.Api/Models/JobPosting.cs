using System.ComponentModel.DataAnnotations;

namespace JobBoard.Api.Models;

public class JobPosting
{
    /* public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal SalaryMin { get; set; }

    [Range(0, 1000000)]
    public decimal SalaryMax { get; set; }

    [Required]
    public JobType Type { get; set; }

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Навігаційні властивості
    public Company? Company { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();*/
    
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
    public JobType Type { get; set; }
    public DateTime PostedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }

    public Company? Company { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    
    
}