namespace JobBoard.Api.DTOs.Company;

public record CreateCompanyDto(
    string Name,
    string Description,
    string Website,
    string Industry,
    string LogoUrl
)
{
    public Models.Company ToModel() => new Models.Company
    {
        Id = Guid.NewGuid(),
        Name = Name,
        Description = Description,
        Website = Website,
        Industry = Industry,
        LogoUrl = LogoUrl
    };
}