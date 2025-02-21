using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistSong> PlaylistSongs { get; set; }
    public DbSet<LikedSong> LikedSongs { get; set; }
    public DbSet<LikedPlaylist> LikedPlaylists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the many-to-many relationship between Song and Playlist
        modelBuilder.Entity<PlaylistSong>()
            .HasKey(ps => new { ps.PlaylistId, ps.SongId });

        modelBuilder.Entity<PlaylistSong>()
            .HasOne(ps => ps.Playlist)
            .WithMany(p => p.PlaylistSongs)
            .HasForeignKey(ps => ps.PlaylistId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PlaylistSong>()
            .HasOne(ps => ps.Song)
            .WithMany(s => s.PlaylistSongs)
            .HasForeignKey(ps => ps.SongId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the many-to-many relationship between User and Song through LikedSong
        modelBuilder.Entity<LikedSong>()
            .HasKey(ls => new { ls.UserId, ls.SongId });

        modelBuilder.Entity<LikedSong>()
            .HasOne(ls => ls.User)
            .WithMany(u => u.LikedSongs)
            .HasForeignKey(ls => ls.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<LikedSong>()
            .HasOne(ls => ls.Song)
            .WithMany(s => s.LikedSongs)
            .HasForeignKey(ls => ls.SongId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the many-to-many relationship between User and Playlist through LikedPlaylist
        modelBuilder.Entity<LikedPlaylist>()
            .HasKey(lp => new { lp.UserId, lp.PlaylistId });

        modelBuilder.Entity<LikedPlaylist>()
            .HasOne(lp => lp.User)
            .WithMany(u => u.LikedPlaylists)
            .HasForeignKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<LikedPlaylist>()
            .HasOne(lp => lp.Playlist)
            .WithMany(p => p.LikedPlaylists)
            .HasForeignKey(lp => lp.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Default value configuration for CreatedAt and UpdatedAt property
        modelBuilder.Entity<User>()
            .Property(s => s.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<User>()
            .Property(s => s.UpdatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Song>()
            .Property(s => s.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<Song>()
            .Property(s => s.UpdatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Playlist>()
            .Property(p => p.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<Playlist>()
            .Property(p => p.UpdatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<LikedSong>()
            .Property(ls => ls.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<LikedPlaylist>()
            .Property(ls => ls.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<PlaylistSong>()
            .Property(ps => ps.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
