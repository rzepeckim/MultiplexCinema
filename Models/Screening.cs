using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MultiplexCinema.Models
{
    public class Screening
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; } // Foreign key reference to Movie

        public Movie Movie { get; set; } = null!; // Navigation property

        [Required]
        public DateTime ScreeningTime { get; set; }

        [Required]
        public int AuditoriumId { get; set; } // Foreign key reference to Auditorium

        public Auditorium Auditorium { get; set; } = null!; // Navigation property
        public string Status { get; set; } = "Nie Rozpoczęty";

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public Screening()
        {
            
            Status = "Nie Rozpoczęty";  
        }
    }
}