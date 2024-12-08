using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Song> Songs { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Default value configuration for CreatedAt property in Playlist
            modelBuilder.Entity<Playlist>()
                .Property(p => p.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure the many-to-many relationship using the PlaylistSongs table
            // Composite key for PlaylistSongs
            modelBuilder.Entity<PlaylistSong>()
                .HasKey(ps => new { ps.PlaylistId, ps.SongId }); // Composite key

            // Define relationships for PlaylistSongs
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistSongs)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Cascade); ;
        }
    }
}
