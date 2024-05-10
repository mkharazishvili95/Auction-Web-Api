using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Auction_Web_Api.TransactionModel
{
    public class Transactions
    {
        [Key]
        public int Id { get; set; }
        public DateTime TransactionDate {  get; set; } = DateTime.Now;
        // Buyer
        public int SenderId {  get; set; }
        // Seller
        public int ReceiverId {  get; set; }
        public int ProductId {  get; set; }
        public decimal Amount {  get; set; }
    }
}
