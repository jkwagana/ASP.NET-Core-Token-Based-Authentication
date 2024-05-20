namespace HomeAPIs.Data.ViewModels
{
    public class AuthResultsVM
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }

    }
}
