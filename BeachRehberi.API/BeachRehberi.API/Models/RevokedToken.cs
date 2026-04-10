using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class RevokedToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
}
