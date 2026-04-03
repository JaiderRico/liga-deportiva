using SportsLeague.Domain.Enums;

namespace SportsLeague.API.DTOs.Request;

public class SponsorRequestDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public SponsorCategory Category { get; set; }
}