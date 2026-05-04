using JobBoard.Api.Models;

namespace JobBoard.Api.Services;

public interface ICompanyService
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<IEnumerable<Company>> GetAllAsync();
    Task CreateAsync(Company company);
}