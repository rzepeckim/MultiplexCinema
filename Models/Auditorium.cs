namespace MultiplexCinema.Models;
using System.ComponentModel.DataAnnotations;

public class Auditorium
{
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    [Range(1, 500)]
    public int SeatCount { get; set; }
}
