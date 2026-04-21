using Microsoft.EntityFrameworkCore;
using JobBoard.Api.Models;

namespace JobBoard.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<Application> Applications => Set<Application>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company → JobPosting (1:N)
        modelBuilder.Entity<JobPosting>()
            .HasOne(j => j.Company)
            .WithMany(c => c.JobPostings)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobPosting → Application (1:N)
        modelBuilder.Entity<Application>()
            .HasOne(a => a.JobPosting)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Унікальність: одна заявка на email + вакансію
        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.JobPostingId, a.Email })
            .IsUnique();
    }
}