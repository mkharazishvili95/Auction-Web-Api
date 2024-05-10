using System.ComponentModel.DataAnnotations;

namespace Auction_Web_Api.Identity
{
    public class Loggs
    {
        [Key]
        public int Id {  get; set; }
        public string LoggedUser { get; set; } = string.Empty;
        public int UserId {  get; set; }
        public DateTime LoggedDate { get; set; } = DateTime.Now;
    }
}
