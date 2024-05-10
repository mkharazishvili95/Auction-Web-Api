using Auction_Web_Api.BidingModel;
using Auction_Web_Api.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;

namespace Auction_Web_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public BidController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [Authorize]
        [HttpPost("MakeABid")]
        public async Task<IActionResult> MakeABid(Bid bidModel)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var userId = int.Parse(User.Identity.Name);
                    var existingBidder = await connection.QueryFirstOrDefaultAsync("SELECT * FROM dbo.Users WHERE Id = @Id", new {Id =  userId});
                    if(existingBidder == null)
                    {
                        return NotFound(new { Message = "User Not Found!" });
                    }
                    var existingProduct = await connection.QueryFirstOrDefaultAsync("SELECT * FROM dbo.Product WHERE Id = @Id", new { Id = bidModel.ProductId });
                    if(existingProduct == null)
                    {
                        return NotFound(new { Message = $"There is no any Product by ID: {bidModel.ProductId} to bid!" });
                    }
                    if(existingProduct.IsAvailable == false || existingProduct.EndTime < DateTime.Now || existingBidder.BuyerId != null)
                    {
                        return BadRequest(new { Message = "That product is not available now!" });
                    }
                    if (existingBidder.IsBlocked == true)
                    {
                        return BadRequest(new { Message = "You can not make any bid, because you are blocked!" });
                    }
                    if(existingBidder.Id == existingProduct.SellerId)
                    {
                        return BadRequest(new { Message = "You can not make any bid for your product!" });
                    }
                    if(bidModel.BiddingCurrentAmount <= existingProduct.CurrentPrice)
                    {
                        return BadRequest(new { Message = $"Current Amount should be greater than Current Price. Enter more than: {existingProduct.CurrentPrice} GEL as a bid." });
                    }
                    var bidderList = await connection.QueryAsync<Bidders>(
                        "SELECT * FROM dbo.Bidders");
                    var productList = await connection.QueryAsync<Product.Product>(
                        "SELECT * FROM dbo.Product");
                    var existingBidderOfThisProduct = await connection.QueryAsync(
                        "SELECT * FROM dbo.Bidders WHERE BidderId = @BidderId AND ProductId = @ProductId", new { BidderId = userId, ProductId = existingProduct.Id });
                    if (existingBidderOfThisProduct.Any())
                    {
                        var riseAmount = bidModel.BiddingCurrentAmount;
                        await connection.ExecuteAsync("UPDATE dbo.Bidders SET BiddingCurrentAmount = @UpdatedAmount WHERE BidderId = @BidderId AND ProductId = @ProductId", new
                        {
                            BidderId = existingBidder.Id,
                            UpdatedAmount = riseAmount,
                            ProductId = existingProduct.Id
                        });
                        await connection.ExecuteAsync("UPDATE dbo.Product SET CurrentPrice = @NewCurrentPrice WHERE Id = @Id", new
                        {
                            Id = existingProduct.Id,
                            NewCurrentPrice = bidModel.BiddingCurrentAmount
                        });
                        return Ok(new { Message = "You have successfully rised a bid!" });
                    }
                    else
                    {
                        await connection.ExecuteAsync("INSERT INTO dbo.Bidders (BidderId, BidderEmail, BiddingCurrentAmount, BiddingDate, ProductId) " +
                            "VALUES (@BidderId, @BidderEmail, @BiddingCurrentAmount, @BiddingDate, @ProductId)", new
                            {
                                BidderId = existingBidder.Id,
                                BidderEmail = existingBidder.Email,
                                BiddingCurrentAmount = bidModel.BiddingCurrentAmount,
                                BiddingDate = DateTime.Now,
                                ProductId = existingProduct.Id
                            });
                        await connection.ExecuteAsync("UPDATE dbo.Product SET CurrentPrice = @NewCurrentPrice WHERE Id = @Id", new 
                        {
                            Id = existingProduct.Id,
                            NewCurrentPrice = bidModel.BiddingCurrentAmount
                        });
                        return Ok(new { Message = "You have successfully made a bid!" });
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPost("RiseABidBy10")]
        public async Task<IActionResult> RiseABidByTen(int productId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingProduct = await connection.QueryFirstOrDefaultAsync<Product.Product>(
                        "SELECT * FROM dbo.Product WHERE Id = @Id", new { Id = productId });
                    if(existingProduct == null)
                    {
                        return NotFound(new { Message = $"There is no any product by ID: {productId}" });
                    }
                    var userId = int.Parse(User.Identity.Name);
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>(
                        "SELECT * FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                    if(existingUser == null)
                    {
                        return NotFound(new { Message = "User Not Found!" });
                    }
                    if(existingProduct.IsAvailable == false)
                    {
                        return BadRequest(new { Message = "Auction is not active now!" });
                    }
                    if(existingUser.IsBlocked)
                    {
                        return BadRequest(new { Message = "You can not make any bid, because you are blocked!" });
                    }
                    if (existingProduct.SellerId == userId)
                    {
                        return BadRequest(new { Message = "You can not make any bid with your product!" });
                    }
                    var existingProductCurrentPrice = existingProduct.CurrentPrice;
                    var risePriceBy10 = existingProductCurrentPrice += 10;
                    await connection.ExecuteAsync("UPDATE dbo.Product SET CurrentPrice = @CurrentPrice WHERE Id = @Id", new { Id = existingProduct.Id, CurrentPrice = risePriceBy10 });
                    var bidList = await connection.QueryAsync<int>(
                        "SELECT BidderId FROM dbo.Bidders WHERE ProductId = @ProductId", new { ProductId = productId});
                    if (bidList.Any())
                    {
                        await connection.ExecuteAsync("UPDATE dbo.Bidders SET BiddingCurrentAmount = @RiseAmount WHERE ProductId = @ProductId",
                            new {RiseAmount = risePriceBy10, ProductId =  productId });
                        return Ok(new { Message = "You have successfully rised a bid by 10 GEL." });
                    }
                    else
                    {
                        await connection.ExecuteAsync("INSERT INTO dbo.Bidders (BidderId, BidderEmail, BiddingCurrentAmount, BiddingDate, ProductId) " +
                            "VALUES (@BidderId, @BidderEmail, @BiddingCurrentAmount, @BiddingDate, @ProductId); SELECT SCOPE_IDENTITY();", new
                            {
                                BidderId = userId,
                                BidderEmail = existingUser.Email,
                                BiddingCurrentAmount = risePriceBy10,
                                BiddingDate = DateTime.Now,
                                ProductId = productId
                            });
                        return Ok(new { Message = "You have successfully rised a bid by 10 GEL." });
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
