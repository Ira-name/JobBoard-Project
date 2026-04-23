using JobBoard.Api.Models;
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
    public async Task<IActionResult> Apply(Guid jobId, Application application)
    {
        await _applicationService.ApplyAsync(jobId, application);
        return Ok(application);
    }

    // GET: /api/jobs/{jobId}/applications
    [HttpGet("jobs/{jobId}/applications")]
    public async Task<IActionResult> GetByJobId(Guid jobId)
    {
        var applications = await _applicationService.GetByJobIdAsync(jobId);
        return Ok(applications);
    }

    // PATCH: /api/applications/{id}/status
    [HttpPatch("applications/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ApplicationStatus status)
    {
        await _applicationService.UpdateStatusAsync(id, status);
        return NoContent();
    }
}