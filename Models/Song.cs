﻿using System.Text.Json.Serialization;

namespace MusicPlayerAPI.Models;

public class Song
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public required string AudioFilePath { get; set; }
    public required string CoverImagePath { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public required User User { get; set; }

    [JsonIgnore]
    public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    [JsonIgnore]
    public ICollection<LikedSong> LikedSongs { get; set; } = new List<LikedSong>();
}
