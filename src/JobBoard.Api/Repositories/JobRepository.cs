using JobBoard.Api.Data;
using JobBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Api.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobPosting?> GetByIdAsync(Guid id)
    {
        return await _context.JobPostings
            .Include(j => j.Company)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<JobPosting>> GetAllAsync()
    {
        return await _context.JobPostings
            .Include(j => j.Company)
            .ToListAsync();
    }

    public async Task AddAsync(JobPosting job)
    {
        await _context.JobPostings.AddAsync(job);
    }

    public Task UpdateAsync(JobPosting job)
    {
        _context.JobPostings.Update(job);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}