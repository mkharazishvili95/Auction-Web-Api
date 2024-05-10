using System.ComponentModel.DataAnnotations;

namespace Auction_Web_Api.Models
{
    public class UserAddress
    {
        [Key]
        public int Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
