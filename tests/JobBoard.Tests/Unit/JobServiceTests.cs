using JobBoard.Api.Models;
using JobBoard.Api.Repositories;
using JobBoard.Api.Services;
using MockQueryable.NSubstitute;
using NSubstitute;
using Shouldly;
using Xunit;

public class JobServiceTests
{
    private readonly IJobRepository _repo = Substitute.For<IJobRepository>();
    private readonly JobService _sut;

    public JobServiceTests()
    {
        _sut = new JobService(_repo);
    }

    [Fact]
    public async Task CreateAsync_ValidJob_SavesSuccessfully()
    {
        // Arrange
        var job = new JobPosting
        {
            SalaryMin = 1000,
            SalaryMax = 2000,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await _sut.CreateAsync(job);

        // Assert
        await _repo.Received(1).AddAsync(job);
        await _repo.Received(1).SaveChangesAsync();
        job.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAsync_WhenSalaryMinGreaterThanMax_ShouldThrowException()
    {
        // Arrange
        var job = new JobPosting
        {
            SalaryMin = 2000,
            SalaryMax = 1000,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => _sut.CreateAsync(job));
    }

    [Fact]
    public async Task CreateAsync_ExpiredDate_ThrowsException()
    {
        // Arrange
        var job = new JobPosting
        {
            SalaryMin = 1000,
            SalaryMax = 2000,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => _sut.CreateAsync(job));
    }
    
    [Fact]
    public async Task CreateAsync_EqualSalary_ThrowsException()
    {
        var job = new JobPosting
        {
            SalaryMin = 1000,
            SalaryMax = 1000,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        await Should.ThrowAsync<Exception>(() => _sut.CreateAsync(job));
    }

    [Fact]
    public async Task GetActiveJobsAsync_ExpiredJob_IsNotReturned()
    {
        // Arrange
        var jobs = new List<JobPosting>
        {
            new JobPosting
            {
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            }
        };
        var mock = jobs.AsQueryable().BuildMock();
        _repo.Query().Returns(mock);

        // Act
        var result = await _sut.GetActiveJobsAsync(null, null, null);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task CloseAsync_ValidJob_SetsInactive()
    {
        // Arrange
        var job = new JobPosting { Id = Guid.NewGuid(), IsActive = true };

        _repo.GetByIdAsync(job.Id).Returns(job);

        // Act
        await _sut.CloseAsync(job.Id);

        // Assert
        job.IsActive.ShouldBeFalse();
        await _repo.Received().UpdateAsync(
            Arg.Is<JobPosting>(j => j.IsActive == false));
        await _repo.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task CloseAsync_JobNotFound_ThrowsException()
    {
        // Arrange
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((JobPosting?)null);

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => _sut.CloseAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_InvalidSalary_ThrowsException()
    {
        // Arrange
        var job = new JobPosting
        {
            SalaryMin = 5000,
            SalaryMax = 1000
        };

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => _sut.UpdateAsync(job));
    }
    
    [Fact]
    public async Task CreateAsync_InvalidSalary_DoesNotSave()
    {
        // Arrange
        var job = new JobPosting
        {
            SalaryMin = 2000,
            SalaryMax = 1000,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await Should.ThrowAsync<Exception>(() => _sut.CreateAsync(job));

        // Assert
        await _repo.DidNotReceive().AddAsync(Arg.Any<JobPosting>());
        await _repo.DidNotReceive().SaveChangesAsync();
    }
    [Theory]
    [InlineData(1000, 2000)]
    [InlineData(1, 9999)]
    public async Task CreateAsync_ValidSalary_Works(decimal min, decimal max)
    {
        // Arrange
        var job = new JobPosting
        {
            SalaryMin = min,
            SalaryMax = max,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await _sut.CreateAsync(job);

        // Assert
        await _repo.Received().AddAsync(job);
    }
    [Fact]
    public async Task UpdateAsync_ValidJob_UpdatesSuccessfully()
    {
        // Arrange
        var job = new JobPosting
        {
            SalaryMin = 1000,
            SalaryMax = 2000,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await _sut.UpdateAsync(job);

        // Assert
        await _repo.Received().UpdateAsync(job);
        await _repo.Received().SaveChangesAsync();
    }
    [Fact]
    public async Task GetActiveJobsAsync_ValidJob_ReturnsJob()
    {
        // Arrange
        var jobs = new List<JobPosting>
        {
            new JobPosting
            {
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            }
        };
        var mock = jobs.AsQueryable().BuildMock();

        _repo.Query().Returns(mock);

        // Act
        var result = await _sut.GetActiveJobsAsync(null, null, null);

        // Assert
        result.Count().ShouldBe(1);
    }
    
    
}