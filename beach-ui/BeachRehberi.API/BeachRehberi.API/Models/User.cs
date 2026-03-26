using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public string BusinessName { get; set; }
        
        public string Role { get; set; } = "BusinessOwner"; // Default rol
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
