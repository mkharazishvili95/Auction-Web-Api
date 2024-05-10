using System.ComponentModel.DataAnnotations;

namespace Auction_Web_Api.Models
{
    public class CompanyAccount
    {
        [Key]
        public int Id { get; set; }
        public decimal Balance {  get; set; }
    }
}
