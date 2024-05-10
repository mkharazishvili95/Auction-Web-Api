using System.ComponentModel.DataAnnotations;

namespace Auction_Web_Api.Product
{
    public class Categories
    {
        [Key]
        public int Id {  get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
