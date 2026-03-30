namespace SportsLeague.API.DTOs.Response;

public class SponsorResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Aquí mapearemos el string del Enum
}