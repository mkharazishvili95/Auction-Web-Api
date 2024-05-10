using System.ComponentModel.DataAnnotations;

namespace Auction_Web_Api.BidingModel
{
    public class Bidders
    {
        [Key]
        public int Id { get; set; }
        public int BidderId {  get; set; }
        public string BidderEmail { get; set; } = string.Empty;
        public decimal BiddingCurrentAmount { get; set; }
        public DateTime BiddingDate { get; set; } = DateTime.Now;
        public int ProductId {  get; set; }
    }
}
