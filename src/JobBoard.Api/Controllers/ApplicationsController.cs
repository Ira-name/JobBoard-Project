using JobBoard.Api.DTOs;
using JobBoard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    // POST: /api/jobs/{jobId}/apply
    [HttpPost("jobs/{jobId}/apply")]
    public async Task<IActionResult> Apply(Guid jobId, [FromBody] ApplyDto dto)
    {
        var application = dto.ToModel();
        await _applicationService.ApplyAsync(jobId, application);
        return Ok(ApplicationDto.FromModel(application));
    }

    // GET: /api/jobs/{jobId}/applications
    [HttpGet("jobs/{jobId}/applications")]
    public async Task<IActionResult> GetByJobId(Guid jobId)
    {
        var applications = await _applicationService.GetByJobIdAsync(jobId);
        return Ok(applications.Select(ApplicationDto.FromModel));
    }

    // PATCH: /api/applications/{id}/status
    [HttpPatch("applications/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusDto dto)
    {
        await _applicationService.UpdateStatusAsync(id, dto.Status);

        return NoContent();
    }
    [HttpGet("applications/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var app = await _applicationService.GetByIdAsync(id);

        if (app == null)
            return NotFound();

        return Ok(ApplicationDto.FromModel(app));
    }
}