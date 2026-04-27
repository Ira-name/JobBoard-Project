using System.ComponentModel.DataAnnotations;

namespace JobBoard.Api.Models;

public class JobPosting
{
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