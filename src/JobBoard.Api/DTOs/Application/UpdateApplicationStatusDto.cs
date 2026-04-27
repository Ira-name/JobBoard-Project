using JobBoard.Api.Models;

namespace JobBoard.Api.DTOs;

public record UpdateApplicationStatusDto(
    ApplicationStatus Status
);