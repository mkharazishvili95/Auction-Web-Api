using Auction_Web_Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction_Web_Api.Product
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public int SellerId { get; set; }
        public int BuyerId {  get; set; }
        public int CategoryId {  get; set; }
        [ForeignKey("CategoryId")]
        public Categories Category { get; set; }
        public Product()
        {
            IsAvailable = true;
            CurrentPrice = StartingPrice;
            Name = string.Empty;
            Description = string.Empty;
        }
    }
}
