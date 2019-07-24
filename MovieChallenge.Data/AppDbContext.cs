using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MovieChallenge.Data.Domain;

namespace MovieChallenge.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Serie> Series { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<MovieRating> MovieRatings { get; set; }
        public DbSet<SerieRating> SerieRatings { get; set; }
        public DbSet<EpisodeRating> EpisodeRatings { get; set; }

        //public DbSet<BaseRating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Movie>().HasKey(i => i.imdbID);
            builder.Entity<Movie>().Property(i => i.imdbID).ValueGeneratedNever();
            builder.Entity<MovieRating>().HasOne(m=>m.Movie).WithMany(r=>r.Ratings);

            builder.Entity<Serie>().HasKey(i => i.imdbID);
            builder.Entity<Serie>().Property(i => i.imdbID).ValueGeneratedNever();
            builder.Entity<SerieRating>().HasOne(s => s.Serie).WithMany(r => r.Ratings);

            builder.Entity<Episode>().HasKey(i => i.imdbID);
            builder.Entity<Episode>().Property(i => i.imdbID).ValueGeneratedNever();
            builder.Entity<EpisodeRating>().HasOne(e => e.Episode).WithMany(r => r.Ratings);

            base.OnModelCreating(builder);
        }
    }
}
