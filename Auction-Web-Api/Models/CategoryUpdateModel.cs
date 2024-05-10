namespace Auction_Web_Api.Models
{
    public class CategoryUpdateModel
    {
        public int CategoryId { get; set; }
        public string NewName { get; set; } = string.Empty;
    }
}
