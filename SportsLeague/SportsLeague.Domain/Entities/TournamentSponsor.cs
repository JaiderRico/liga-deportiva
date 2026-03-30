namespace SportsLeague.Domain.Entities
{
    public class TournamentSponsor : AuditBase
    {
        // Claves Foráneas (FK)
        public int TournamentId { get; set; }
        public int SponsorId { get; set; }

        // Datos propios de la relación (Atributos de la tabla intermedia)
        public decimal ContractAmount { get; set; }
        public DateTime JoinedAt { get; set; }

        // Propiedades de Navegación
        public Tournament Tournament { get; set; } = null!;
        public Sponsor Sponsor { get; set; } = null!;
    }
}