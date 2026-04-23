using JobBoard.Api.Models;
using JobBoard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    // GET: api/jobs
    [HttpGet]
    public async Task<IActionResult> GetAll(string? location, JobType? type, decimal? minSalary)
    {
        var jobs = await _jobService.GetActiveJobsAsync(location, type, minSalary);
        return Ok(jobs);
    }

    // GET: api/jobs/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);

        if (job == null)
            return NotFound();

        return Ok(job);
    }

    // POST: api/jobs
    [HttpPost]
    public async Task<IActionResult> Create(JobPosting job)
    {
        await _jobService.CreateAsync(job);
        return Ok(job);
    }

    // PUT: api/jobs/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, JobPosting job)
    {
        job.Id = id;
        await _jobService.UpdateAsync(job);
        return NoContent();
    }

    // DELETE: api/jobs/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Close(Guid id)
    {
        await _jobService.CloseAsync(id);
        return NoContent();
    }
}