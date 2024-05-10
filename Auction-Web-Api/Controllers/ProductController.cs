using Auction_Web_Api.Models;
using Auction_Web_Api.Product;
using Auction_Web_Api.Services;
using Auction_Web_Api.TransactionModel;
using Auction_Web_Api.Validation;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Tweetinvi.Core.Parameters;

namespace Auction_Web_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ProductValidator _productValidator;
        private readonly IProductService _productService;
        public ProductController(IConfiguration configuration, ProductValidator productValidator, IProductService productService)
        {
            _configuration = configuration;
            _productValidator = productValidator;
            _productService = productService;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [Authorize]
        [HttpPost("AddNewProduct")]
        public async Task<IActionResult> AddNewProduct(Product.Product newProduct)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var userId = int.Parse(User.Identity.Name);
                    var seller = await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                    if(seller == null)
                    {
                        return NotFound(new { Message = "User Not Found!" });
                    }
                    if(seller.IsBlocked == true)
                    {
                        return BadRequest(new { Message = "You can not add any product, because you are blocked!" });
                    }
                    
                    else
                    {
                        var productValidatorResults = _productValidator.Validate(newProduct);
                        if (!productValidatorResults.IsValid)
                        {
                            return BadRequest(productValidatorResults.Errors);
                        }
                        else
                        {
                            newProduct.CurrentPrice = newProduct.StartingPrice;
                            newProduct.IsAvailable = true;
                            newProduct.SellerId = userId;
                            connection.ExecuteScalar<Product.Product>("INSERT INTO dbo.Product (Name, Description, StartingPrice, StartTime, EndTime, CategoryId, CurrentPrice, IsAvailable, SellerId) " +
                                "VALUES (@Name, @Description, @StartingPrice, @StartTime, @EndTime, @CategoryId, @CurrentPrice, 1, @SellerId)", new
                                {
                                    Name = newProduct.Name,
                                    Description = newProduct.Description,
                                    StartingPrice = newProduct.StartingPrice,
                                    StartTime = newProduct.StartTime,
                                    EndTime = newProduct.EndTime,
                                    CategoryId = newProduct.CategoryId,
                                    CurrentPrice = newProduct.StartingPrice,
                                    SellerId = userId
                                });
                            return Ok(new { Message = "The product has been successfully added to the auction." });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var getAllProducts = await _productService.GetAllProducts();
                if(getAllProducts == null || getAllProducts.Any() == false)
                {
                    return NotFound(new { Message = "Product list is empty!" });
                }
                else
                {
                    return Ok(getAllProducts);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetAvailableProducts")]
        public async Task<IActionResult> GetAvailableProducts()
        {
            try
            {
                var getAvailableProducts = await _productService.GetAvailableProducts();
                if(getAvailableProducts == null || !getAvailableProducts.Any())
                {
                    return NotFound(new { Message = "There is no any avilable product yet!" });
                }
                else
                {
                    return Ok(getAvailableProducts);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetProductsByCategoryId")]
        public async Task<IActionResult> GetProductsByCategoryId(int categoryId)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getProductsByCategoryId = await _productService.GetProductsByCategoryId(categoryId);
                    if(getProductsByCategoryId == null || getProductsByCategoryId.Any() == false)
                    {
                        return NotFound(new { Message = $"There is no any product by categoryId: {categoryId}" });
                    }
                    else
                    {
                        return Ok(getProductsByCategoryId);
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("GetProductById")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var existingProduct = await _productService.GetProductById(productId);
                if(existingProduct == null)
                {
                    return NotFound(new { Message = $"There is no any product by ID: {productId}" });
                }
                else
                {
                    return Ok(existingProduct);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetWinningProducts")]
        public async Task<IActionResult> GetWinningProducts()
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var userId = int.Parse(User.Identity.Name);
                    var getWinningProducts = await connection.QueryAsync<Product.Product, Categories, Product.Product>(
                        "SELECT P.*, C.* FROM dbo.Product P JOIN dbo.Categories C ON C.Id = P.CategoryId WHERE P.BuyerId = @BuyerId",
                        (product, category) =>
                        {
                            product.Category = category;
                            return product;
                        }, new { BuyerId = userId }, splitOn: "Id");
                    if(getWinningProducts.Any() == false)
                    {
                        return NotFound(new { Message = "There is no any product, that you have won!" });
                    }
                    else
                    {
                        return Ok(getWinningProducts);
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetWinningProductById")]
        public async Task<IActionResult> GetWinningProductById(int productId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var userId = int.Parse(User.Identity.Name);
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>(
                        "SELECT * FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                    if(existingUser == null)
                    {
                        return NotFound(new { Message = "User Not Found!" });
                    }
                    var getWinningProductById = await connection.QueryAsync<Product.Product, Categories, Product.Product>(
                        "SELECT P.*, C.* FROM dbo.Product P JOIN dbo.Categories C ON C.Id = P.CategoryId WHERE P.Id = @ProductId AND P.BuyerId = @BuyerId",
                        (product, category) =>
                        {
                            product.Category = category;
                            return product;
                        }, new { ProductId = productId, BuyerId =  userId}, splitOn: "Id");
                    if(getWinningProductById.Any() == false)
                    {
                        return NotFound(new { Message = $"There is no any Winning product by ID: {productId}" });
                    }
                    else
                    {
                        return Ok(getWinningProductById);
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPost("BuyWinningProduct")]
        public async Task<IActionResult> BuyWinningProduct(int productId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var userId = int.Parse(User.Identity.Name);
                    var existingBuyer = await connection.QueryFirstOrDefaultAsync<User>(
                        "SELECT * FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                    var winningAuction = await connection.QueryFirstOrDefaultAsync<Product.Product>(
                        "SELECT * FROM dbo.Product WHERE BuyerId = @BuyerId AND Id = @Id" , new { BuyerId = userId, Id = productId });
                    if(winningAuction == null)
                    {
                        return NotFound(new { Message = $"There is no any winning product by ID: {productId}" });
                    }
                    var transactionList = await connection.QueryAsync<int>(
                        "SELECT ProductId FROM dbo.Transactions WHERE ProductId = @ProductId", new { ProductId = winningAuction.Id });
                    if (transactionList.Any())
                    {
                        return BadRequest(new { Message = "You have already bought that product!" });
                    }
                    if(existingBuyer == null)
                    {
                        return NotFound(new { Message = "User Not Found!" });
                    }
                    if(existingBuyer.IsBlocked == true)
                    {
                        return BadRequest(new { Message = "You can not buy that product, because you are blocked!" });
                    }
                    var winningProductPrice = winningAuction.CurrentPrice;
                    var buyerBalance = existingBuyer.Balance;
                    if(buyerBalance <  winningProductPrice)
                    {
                        return BadRequest(new
                        {
                            Message = $"You can not buy that product, because you do not have anough money. That product costs: {winningProductPrice} GEL. and your balance is: {buyerBalance} GEL."
                        });
                    }
                    if (winningAuction.SellerId == userId)
                    {
                        return BadRequest(new { Message = "You can not buy your product!" });
                    }
                    else
                    {
                        await connection.ExecuteAsync(
                            "INSERT INTO dbo.Transactions (TransactionDate, SenderId, ReceiverId, ProductId, Amount) " +
                            "VALUES (@TransactionDate, @SenderId, @ReceiverId, @ProductId, @Amount); SELECT SCOPE_IDENTITY();", new
                            {
                                TransactionDate = DateTime.Now,
                                SenderId = userId,
                                ReceiverId = winningAuction.SellerId,
                                ProductId = winningAuction.Id,
                                Amount = winningAuction.CurrentPrice
                            });
                        var balanceResult = existingBuyer.Balance -= winningProductPrice;
                        await connection.ExecuteAsync("UPDATE dbo.Users SET Balance = @Balance WHERE Id = @Id", new { Id = userId, Balance = balanceResult });
                        var existingSeller = await connection.QueryFirstOrDefaultAsync<User>(
                            "SELECT * FROM dbo.Users WHERE Id = @Id", new { Id = winningAuction.SellerId });
                        if(existingSeller == null)
                        {
                            return NotFound(new {Message = "Seller does not exist!"});
                        }
                        var commission = (winningProductPrice * 15 / 100);
                        var reduceFifthPercent = (winningProductPrice) - (commission);
                        var sellerBalanceResult = existingSeller.Balance += reduceFifthPercent;
                        await connection.ExecuteAsync("UPDATE dbo.Users SET Balance = @Balance WHERE Id = @Id", new {Id  = existingSeller.Id, Balance = sellerBalanceResult});
                        var companyAccount = await connection.QueryAsync<CompanyAccount>(
                            "SELECT * FROM dbo.CompanyAccount");
                        var companyStatement = companyAccount.SingleOrDefault() ?? new CompanyAccount();
                        if(companyStatement == null)
                        {
                            return BadRequest(new { Message = "Error!" });
                        }
                        var updatedBalanceForTheCompany = companyStatement.Balance += commission;
                        await connection.ExecuteAsync("UPDATE dbo.CompanyAccount SET Balance = @UpdatedBalance", new { UpdatedBalance = updatedBalanceForTheCompany });
                        return Ok(new {Message = "You have successfully bought that product!"});
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
