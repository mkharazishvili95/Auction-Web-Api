namespace Auction_Web_Api.Models
{
    public class EmailUpdateModel
    {
        public string eMail { get; set; } = string.Empty;
        public string repeatEmail { get; set; } = string.Empty;
        public string newEmail { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string repeatPassword { get; set; } = string.Empty;
    }
}
