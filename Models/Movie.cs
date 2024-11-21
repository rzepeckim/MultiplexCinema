namespace MultiplexCinema.Models;
using System.ComponentModel.DataAnnotations;

public class Movie
{
    public int Id { get; set; }
    [Required]
    public required string Title { get; set; }
    [Required]
    public required string Description { get; set; }
    [Range(1, 300)]
    public int Duration { get; set; } 
    [Required]
    public required string Genre { get; set; }
}
