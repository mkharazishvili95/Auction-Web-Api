using Auction_Web_Api.Identity;
using Auction_Web_Api.Product;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction_Web_Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age {  get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; }
        public bool IsBlocked {  get; set; }
        public int AddressId { get; set; }
        [ForeignKey("AddressId")]
        public UserAddress UserAddress { get; set; } = new UserAddress();
        public List<Product.Product> Products { get; set; }
        public User()
        {
            Role = Roles.User;
            Balance = 0;
            IsBlocked = false;
        }
    }
}
