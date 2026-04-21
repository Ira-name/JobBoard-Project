using JobBoard.Api.Data;
using JobBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Api.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _context;

    public ApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Application application)
    {
        await _context.Applications.AddAsync(application);
    }

    public async Task<IEnumerable<Application>> GetByJobIdAsync(Guid jobId)
    {
        return await _context.Applications
            .Where(a => a.JobPostingId == jobId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid jobId, string email)
    {
        return await _context.Applications
            .AnyAsync(a => a.JobPostingId == jobId && a.Email == email);
    }

    public async Task<Application?> GetByIdAsync(Guid id)
    {
        return await _context.Applications
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}