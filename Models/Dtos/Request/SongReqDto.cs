﻿namespace MusicPlayerAPI.Models.Dtos.Request;

public class SongReqDto
{
    public required string Title { get; set; }
    public required string Artist { get; set; }
}
