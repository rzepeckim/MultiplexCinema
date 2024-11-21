using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiplexCinema.Models;
using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Auditorium> Auditoriums { get; set; }
    public DbSet<Screening> Screenings { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    // Method to update screening status asynchronously
    public async Task UpdateScreeningStatusAsync(int screeningId)
    {
        try
        {
            await Database.ExecuteSqlRawAsync("EXEC UpdateScreeningStatus @ScreeningId", 
                new SqlParameter("@ScreeningId", screeningId));
        }
        catch (SqlException ex)
        {
            // Log exception (consider using a logging framework like Serilog or NLog)
            throw new InvalidOperationException("Error executing stored procedure", ex);
        }
    }

    // Fluent API configuration for the models
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Ensure the base method is called to avoid issues with Identity

        // Configure relationships
        builder.Entity<Ticket>()
            .HasOne(t => t.Screening)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);

        builder.Entity<Screening>()
            .HasOne(s => s.Movie)
            .WithMany()
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Screening>()
            .HasOne(s => s.Auditorium)
            .WithMany()
            .HasForeignKey(s => s.AuditoriumId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seeding data

        // Movies
        builder.Entity<Movie>().HasData(
            new Movie { Id = 1, Title = "Skazani na Shawshank", Genre = "Dramat", Duration = 142, Description = "Dwóch więźniów nawiązuje więź przez wiele lat, znajdując pociechę i ostateczne odkupienie przez akty wspólnej przyzwoitości." },
            new Movie { Id = 2, Title = "Ojciec chrzestny", Genre = "Kryminalny, Dramat", Duration = 175, Description = "Starzejący się patriarcha dynastii przestępczej przekazuje kontrolę nad swoją tajną imperium swojemu niechętnemu synowi." },
            new Movie { Id = 3, Title = "Mroczny Rycerz", Genre = "Akcja, Kryminał, Dramat", Duration = 152, Description = "Kiedy zagrożenie znane jako Joker pojawia się z przeszłości, wywołuje chaos w Gotham." },
            new Movie { Id = 4, Title = "Pulp Fiction", Genre = "Kryminalny, Dramat", Duration = 154, Description = "Życie dwóch płatnych morderców, boksera, żony gangstera i pary złodziei w czterech opowieściach o przemocy i odkupieniu." },
            new Movie { Id = 5, Title = "Forrest Gump", Genre = "Dramat, Romans", Duration = 142, Description = "Prezydentury Kennedy'ego i Johnsona, wojna w Wietnamie, skandal Watergate i inne wydarzenia historyczne z perspektywy mężczyzny z niezwykłą historią." }
        );

        // Auditoriums
        builder.Entity<Auditorium>().HasData(
            new Auditorium { Id = 1, Name = "Sala 1", SeatCount = 100 },
            new Auditorium { Id = 2, Name = "Sala 2", SeatCount = 150 }
        );

        // Screenings
        builder.Entity<Screening>().HasData(
            new Screening { Id = 1, MovieId = 1, AuditoriumId = 1, ScreeningTime = DateTime.Now.AddHours(10) },
            new Screening { Id = 2, MovieId = 2, AuditoriumId = 2, ScreeningTime = DateTime.Now.AddHours(12) }
        );

        // Identity Roles
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "role1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "role2", Name = "User", NormalizedName = "USER" }
        );

        // Optionally, seed an Admin user for easy setup (can be customized further)
        var hasher = new PasswordHasher<ApplicationUser>();
        builder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = "admin-user-id",
                UserName = "admin@cinema.com",
                NormalizedUserName = "ADMIN@CINEMA.COM",
                Email = "admin@cinema.com",
                NormalizedEmail = "ADMIN@CINEMA.COM",
                PasswordHash = hasher.HashPassword(null, "Admin@123")
            }
        );
    }
}
