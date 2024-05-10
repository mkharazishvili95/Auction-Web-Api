using Auction_Web_Api.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction_Web_Api.Identity
{
    public class UserRegisterModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int AddressId {  get; set; }
        [ForeignKey("AddressId")]
        public UserAddress UserAddress { get; set; } = new UserAddress();
    }
}
