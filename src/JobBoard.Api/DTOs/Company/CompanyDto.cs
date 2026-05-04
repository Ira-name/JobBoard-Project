namespace JobBoard.Api.DTOs.Company;

public record CompanyDto(
    Guid Id,
    string Name,
    string Description,
    string Website,
    string Industry,
    string LogoUrl
)
{
    public static CompanyDto FromModel(Models.Company c) =>
        new(c.Id, c.Name, c.Description, c.Website, c.Industry, c.LogoUrl);
}