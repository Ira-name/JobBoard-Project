using JobBoard.Api.Models;
using JobBoard.Api.Repositories;
using JobBoard.Api.Services;
using NSubstitute;
using Shouldly;
using Xunit;

public class ApplicationServiceTests
{
    private readonly IApplicationRepository _appRepo = Substitute.For<IApplicationRepository>();
    private readonly IJobRepository _jobRepo = Substitute.For<IJobRepository>();

    private readonly ApplicationService _sut;

    public ApplicationServiceTests()
    {
        _sut = new ApplicationService(_appRepo, _jobRepo);
    }

    [Fact]
    public async Task ApplyAsync_ValidApplication_SavesSuccessfully()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        _jobRepo.GetByIdAsync(jobId).Returns(new JobPosting
        {
            Id = jobId,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });

        _appRepo.ExistsAsync(jobId, "test@mail.com").Returns(false);

        var app = new Application
        {
            Email = "test@mail.com"
        };

        // Act
        await _sut.ApplyAsync(jobId, app);

        // Assert
        await _appRepo.Received().AddAsync(app);
        await _appRepo.Received().SaveChangesAsync();
        app.Status.ShouldBe(ApplicationStatus.Pending);
    }

    [Fact]
    public async Task ApplyAsync_JobNotFound_ThrowsException()
    {
        // Arrange
        _jobRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((JobPosting?)null);

        // Act & Assert
        await Should.ThrowAsync<Exception>(() =>
            _sut.ApplyAsync(Guid.NewGuid(), new Application()));
    }

    [Fact]
    public async Task ApplyAsync_InactiveJob_ThrowsException()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        _jobRepo.GetByIdAsync(jobId).Returns(new JobPosting
        {
            IsActive = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });

        // Act & Assert
        await Should.ThrowAsync<Exception>(() =>
            _sut.ApplyAsync(jobId, new Application()));
    }

    [Fact]
    public async Task ApplyAsync_ExpiredJob_ThrowsException()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        _jobRepo.GetByIdAsync(jobId).Returns(new JobPosting
        {
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        });

        // Act & Assert
        await Should.ThrowAsync<Exception>(() =>
            _sut.ApplyAsync(jobId, new Application()));
    }

    [Fact]
    public async Task ApplyAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        _jobRepo.GetByIdAsync(jobId).Returns(new JobPosting
        {
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });

        _appRepo.ExistsAsync(jobId, "test@mail.com").Returns(true);

        var app = new Application
        {
            Email = "test@mail.com"
        };

        // Act & Assert
        await Should.ThrowAsync<Exception>(() =>
            _sut.ApplyAsync(jobId, app));
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidApplication_UpdatesStatus()
    {
        // Arrange
        var app = new Application { Id = Guid.NewGuid() };

        _appRepo.GetByIdAsync(app.Id).Returns(app);

        // Act
        await _sut.UpdateStatusAsync(app.Id, ApplicationStatus.Accepted);

        // Assert
        app.Status.ShouldBe(ApplicationStatus.Accepted);
        await _appRepo.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateStatusAsync_NotFound_ThrowsException()
    {
        // Arrange
        _appRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Application?)null);

        // Act & Assert
        await Should.ThrowAsync<Exception>(() =>
            _sut.UpdateStatusAsync(Guid.NewGuid(), ApplicationStatus.Accepted));
    }

    [Fact]
    public async Task ApplyAsync_SetsAppliedAt()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        _jobRepo.GetByIdAsync(jobId).Returns(new JobPosting
        {
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });

        _appRepo.ExistsAsync(jobId, "test@mail.com").Returns(false);

        var app = new Application
        {
            Email = "test@mail.com"
        };

        // Act
        await _sut.ApplyAsync(jobId, app);

        // Assert
        app.AppliedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateStatusAsync_OnlyStatusChanges()
    {
        var app = new Application
        {
            Id = Guid.NewGuid(),
            ApplicantName = "Test",
            Email = "test@mail.com",
            Status = ApplicationStatus.Pending
        };

        _appRepo.GetByIdAsync(app.Id).Returns(app);

        await _sut.UpdateStatusAsync(app.Id, ApplicationStatus.Accepted);

        app.Status.ShouldBe(ApplicationStatus.Accepted);
        app.Email.ShouldBe("test@mail.com");
        app.ApplicantName.ShouldBe("Test");
    }
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyAsync_SetsCorrectJobPostingId()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _jobRepo.GetByIdAsync(jobId).Returns(new JobPosting { IsActive = true, ExpiresAt = DateTime.UtcNow.AddDays(1) });
        _appRepo.ExistsAsync(jobId, Arg.Any<string>()).Returns(false);
        var app = new Application { Email = "test@mail.com" };

        // Act
        await _sut.ApplyAsync(jobId, app);

        // Assert
        app.JobPostingId.ShouldBe(jobId);
    }
}