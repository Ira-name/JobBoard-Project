using JobBoard.Api.Models;
using JobBoard.Api.Repositories;

namespace JobBoard.Api.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repo;

    public CompanyService(ICompanyRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Company?> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task CreateAsync(Company company)
    {
        if (string.IsNullOrWhiteSpace(company.Name))
            throw new ArgumentException("Name is required");

        await _repo.AddAsync(company);
        await _repo.SaveChangesAsync();
    }
}