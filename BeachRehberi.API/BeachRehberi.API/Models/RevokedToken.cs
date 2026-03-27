using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class RevokedToken
{
    [Key]
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}
