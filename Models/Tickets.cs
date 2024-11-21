using System.ComponentModel.DataAnnotations;

namespace MultiplexCinema.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        // Klucz obcy do Screening
        [Required]
        public int ScreeningId { get; set; }

        // Nawias do Screening (relacja)
        [Required]
        public Screening Screening { get; set; } = null!;

        // Numer miejsca
        [Range(1, 500)]
        public int SeatNumber { get; set; }

        // Cena biletu
        [Range(0, 1000)]
        public decimal Price { get; set; }

        // Klucz obcy do użytkownika
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Rodzaj biletu")]
        public required string TicketType { get; set; } = string.Empty; // "Normalny" lub "Ulgowy"

        // Nawias do ApplicationUser (relacja)
        public ApplicationUser? User { get; set; } // Nullable, ponieważ użytkownik może być opcjonalny
    }
}