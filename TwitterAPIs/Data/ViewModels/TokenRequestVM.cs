using System.ComponentModel.DataAnnotations;

namespace HomeAPIs.Data.ViewModels
{
    public class TokenRequestVM
    {

        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
