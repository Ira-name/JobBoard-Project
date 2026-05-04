using JobBoard.Api.DTOs;
using JobBoard.Api.DTOs.Company;
using JobBoard.Api.Models;
using JobBoard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;

    public CompaniesController(ICompanyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companies = await _service.GetAllAsync();
        return Ok(companies.Select(CompanyDto.FromModel));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var company = await _service.GetByIdAsync(id);

        if (company == null)
            return NotFound();

        return Ok(CompanyDto.FromModel(company));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCompanyDto dto)
    {
        var company = dto.ToModel();
        await _service.CreateAsync(company);

        return Ok(CompanyDto.FromModel(company));
    }
}