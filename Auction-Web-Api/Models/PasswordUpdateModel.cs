namespace Auction_Web_Api.Models
{
    public class PasswordUpdateModel
    {
        public string eMail { get; set; } = string.Empty;
        public string repeatEmail { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string repeatPassword { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
    }
}
