using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.API.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ITournamentRepository tournamentRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
        var sponsor = await _sponsorRepository.GetByIdAsync(id);
        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);
        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
            throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");

        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El formato del email de contacto no es válido");

        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existing = await _sponsorRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");

        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name, id))
            throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");

        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El formato del email de contacto no es válido");

        existing.Name = sponsor.Name;
        existing.ContactEmail = sponsor.ContactEmail;
        existing.Phone = sponsor.Phone;
        existing.Category = sponsor.Category;

        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _sponsorRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");

        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(existing.Id);
    }

    public async Task<TournamentSponsor> LinkToTournamentAsync(int sponsorId, TournamentSponsor tournamentSponsor)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");

        var tournament = await _tournamentRepository.GetByIdAsync(tournamentSponsor.TournamentId);
        if (tournament == null)
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentSponsor.TournamentId}");

        var existing = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(
            tournamentSponsor.TournamentId, sponsorId);
        if (existing != null)
            throw new InvalidOperationException(
                $"El patrocinador '{sponsor.Name}' ya está vinculado al torneo '{tournament.Name}'");

        if (tournamentSponsor.ContractAmount <= 0)
            throw new InvalidOperationException("El monto del contrato debe ser mayor a 0");

        tournamentSponsor.SponsorId = sponsorId;
        tournamentSponsor.JoinedAt = DateTime.UtcNow;

        _logger.LogInformation("Linking sponsor {SponsorId} to tournament {TournamentId}", sponsorId, tournamentSponsor.TournamentId);
        return await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
    }

    public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");

        return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
    }

    public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)
    {
        var existing = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existing == null)
            throw new KeyNotFoundException(
                $"No se encontró la vinculación entre el patrocinador {sponsorId} y el torneo {tournamentId}");

        _logger.LogInformation("Unlinking sponsor {SponsorId} from tournament {TournamentId}", sponsorId, tournamentId);
        await _tournamentSponsorRepository.DeleteAsync(existing.Id);
    }

    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}