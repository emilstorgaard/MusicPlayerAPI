﻿namespace MusicPlayerAPI.Models.Dtos;

public class UserResDto
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
