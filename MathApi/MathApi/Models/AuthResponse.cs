using System.ComponentModel.DataAnnotations;

namespace MathApi.Models
{
    public class AuthResponse
    {
        [Required]
        public string Token { get; set; }        
        
        [Required]
        public string UserId { get; set; }

        public AuthResponse(string token, string currentUserId)
        {
            this.Token = token;
            this.UserId = currentUserId;
        }
    }
}