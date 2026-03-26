using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.DTOs
{
    public class LoginDto 
    { 
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "GeĂ§ersiz e-posta formatÄą.")]
        public string Email { get; set; } = ""; 

        [Required(ErrorMessage = "Ĺifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Ĺifre en az 6 karakter olmalÄądÄąr.")]
        public string Password { get; set; } = ""; 
    }

    public class RegisterDto 
    { 
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = ""; 

        [Required]
        [MinLength(6)]
        [MaxLength(50)]
        public string Password { get; set; } = ""; 

        [Required]
        [MaxLength(50)]
        public string ContactName { get; set; } = ""; 

        [Required]
        public int BeachId { get; set; } 
    }
}
