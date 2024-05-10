using Auction_Web_Api.BidingModel;
using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using Auction_Web_Api.Product;
using Auction_Web_Api.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Auction_Web_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAdminService _service;
        public AdminController(IConfiguration configuration, IAdminService service)
        {
            _configuration = configuration;
            _service = service;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var userList = await _service.GetAllUsers();
                if(userList == null || userList.Any() == false)
                {
                    return NotFound(new { Message = $"There is no any User in the database yet!" });
                }
                else
                {
                    return Ok(userList);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var existingUser = await _service.GetUserById(userId);
                if(existingUser == null)
                {
                    return NotFound(new { Message = $"There is no any User by ID: {userId}" });
                }
                else
                {
                    return Ok(existingUser);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetUsersByCountry")]
        public async Task<IActionResult> GetUsersByCountry(string country)
        {
            try
            {
                var getUsersByCountry = await _service.GetUsersByCountry(country);
                if(getUsersByCountry == null || getUsersByCountry.Any() == false)
                {
                    return NotFound(new { Message = $"There is no any User from the country of: {country.ToUpper()}" });
                }
                else
                {
                    return Ok(getUsersByCountry);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetUsersByCity")]
        public async Task<IActionResult> GetUsersByCity(string city)
        {
            try
            {
                var getUsersByCity = await _service.GetUsersByCity(city);
                if(getUsersByCity == null || getUsersByCity.Any() == false)
                {
                    return NotFound(new { Message = $"There is no any User from the city of: {city.ToUpper()}" });
                }
                else
                {
                    return Ok(getUsersByCity);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetUsersByBalance")]
        public async Task<IActionResult> GetUsersByBalance(BalanceModel balance)
        {
            try
            {
                var getUsersByBalance = await _service.GetUsersByBalance(balance);
                if(getUsersByBalance == null || getUsersByBalance.Any() == false)
                {
                    return NotFound(new { Message = $"There is no any User, that balance is between {balance.MinBalance} and {balance.MaxBalance} GEL." });
                }
                else
                {
                    return Ok(getUsersByBalance);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetUsersByAge")]
        public async Task<IActionResult> GetUsersByAge([FromBody]AgeModel ageModel)
        {
            try
            {
                var getUsersByAge = await _service.GetUsersByAge(ageModel);
                if(getUsersByAge == null || getUsersByAge.Any() == false)
                {
                    return NotFound(new { Message = $"There is no any User, that age is between: {ageModel.MinAge} and {ageModel.MaxAge}" });
                }
                else
                {
                    return Ok(getUsersByAge);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("BlockUser")]
       public async Task<IActionResult> BlockUser(int userId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUserById = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.*, C.* FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE P.Id = @Id",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { Id = userId }, splitOn: "Id");

                    var existingUser = getUserById.FirstOrDefault();
                    if (existingUser == null)
                    {
                        return NotFound(new { Message = $"There is no any User by ID: {userId} to block!" });
                    }
                    if (existingUser.Role == Roles.Admin)
                    {
                        return BadRequest(new { Message = "You have no permission to block another Admin!" });
                    }
                    if (existingUser.IsBlocked == true)
                    {
                        return BadRequest(new { Message = "That User is already blocked!" });
                    }
                    else
                    {
                        await _service.BlockUser(userId);
                        return Ok(new { Message = $"User: {existingUser.Email.ToUpper()} has successfully blocked by Admin!" });
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("UnBlockUser")]
        public async Task<IActionResult> UnBlockUser(int userId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUserById = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.*, C.* FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE P.Id = @Id",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { Id = userId }, splitOn: "Id");

                    var existingUser = getUserById.FirstOrDefault();
                    if (existingUser == null)
                    {
                        return NotFound(new { Message = $"There is no any User by ID: {userId} to block!" });
                    }
                    if (existingUser.Role == Roles.Admin)
                    {
                        return BadRequest(new { Message = "You have no permission to block or unblock another Admin!" });
                    }
                    if (existingUser.IsBlocked == false)
                    {
                        return BadRequest(new { Message = "That User is already unblocked!" });
                    }
                    else
                    {
                        await _service.UnblockUser(userId);
                        return Ok(new { Message = $"User: {existingUser.Email.ToUpper()} has successfully unblocked by Admin!" });
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetBlockedUsers")]
        public async Task<IActionResult> GetBlockedUsers()
        {
            try
            {
                var getBlockedUsers = await _service.GetBlockedUsers();
                if(getBlockedUsers == null || getBlockedUsers.Any() == false)
                {
                    return NotFound(new { Message = "There is no any Blocked User in the database!" });
                }
                else
                {
                    return Ok(getBlockedUsers);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUserById = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.*, C.* FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE P.Id = @Id",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { Id = userId }, splitOn: "Id");
                    var existingUser = getUserById.FirstOrDefault();
                    var addressId = existingUser.UserAddress.Id;
                    if(existingUser == null)
                    {
                        return NotFound(new { Message = $"There is no any User by ID: {userId} to delete!" });
                    }
                    if(existingUser.Role == Roles.Admin)
                    {
                        return BadRequest(new { Message = "You do not have permission to delete another Admin!" });
                    }
                    else
                    {
                        await _service.DeleteUser(userId);
                        return Ok(new { Message = "User has successfully deleted from the database!" });
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("AddNewCategory")]
        public async Task<IActionResult> AddProductCategory(Categories newCategory)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getAllCategories = await connection.QueryAsync<Categories>("SELECT * FROM dbo.Categories");
                    if (string.IsNullOrEmpty(newCategory.Name) || string.IsNullOrWhiteSpace(newCategory.Name))
                    {
                        return BadRequest(new { Message = "Enter Product Name!" });
                    }
                    if (getAllCategories.Any(x => x.Name.ToUpper() == newCategory.Name.ToUpper()))
                    {
                        return BadRequest(new { Message = "Category with this Name already exists. Try another!" });
                    }
                    if (newCategory.Name.Length <= 0 || newCategory.Name.Length > 25)
                    {
                        return BadRequest(new { Message = "Category Name length should be from 1 to 25 chars!" });
                    }
                    else
                    {
                        await _service.AddProductCategory(newCategory);
                        return Ok(new { Message = "Category has been successfully added!" });
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var getAllCategories = await _service.GetAllCategories();
                if(getAllCategories == null || getAllCategories.Any() == false)
                {
                    return NotFound(new { Message = "Category list is empty!" });
                }
                else
                {
                    return Ok(getAllCategories);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            try
            {
                var existingCategory = await _service.GetCategoryById(categoryId);
                if(existingCategory == null)
                {
                    return NotFound(new { Message = $"There is no any Category by ID: {categoryId}" });
                }
                else
                {
                    return Ok(existingCategory);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> UpdateCategoryName(CategoryUpdateModel categoryUpdate)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingCategory = await connection.QueryFirstOrDefaultAsync<Categories>("SELECT * FROM dbo.Categories WHERE Id = @Id", new
                    {
                        Id = categoryUpdate.CategoryId
                    });
                    if (existingCategory == null)
                    {
                        return NotFound(new { Message = $"There is no any Category by ID: {categoryUpdate.CategoryId}" });
                    }
                    if (string.IsNullOrEmpty(categoryUpdate.NewName) || string.IsNullOrWhiteSpace(categoryUpdate.NewName))
                    {
                        return BadRequest(new { Message = "Enter Category Name without space!" });
                    }
                    var categoryList = await connection.QueryAsync<Categories>("SELECT * FROM dbo.Categories");
                    if (categoryList.Any(x => x.Name.ToUpper() == categoryUpdate.NewName.ToUpper()))
                    {
                        return BadRequest(new { Message = "Category name already exists. Try another!" });
                    }
                    if (categoryUpdate.NewName.Length <= 0 || categoryUpdate.NewName.Length > 25)
                    {
                        return BadRequest(new { Message = "Category Name length should be from 1 to 25 chars!" });
                    }
                    else
                    {
                        existingCategory.Name = categoryUpdate.NewName;
                        await connection.ExecuteAsync("UPDATE dbo.Categories SET Name = @NewName WHERE Id = @Id", new
                        {
                            Id = categoryUpdate.CategoryId,
                            NewName = categoryUpdate.NewName
                        });
                        return Ok(new { Message = "Category name has been successfully updated!" });
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var existingCategory = await _service.GetCategoryById(categoryId);
                if(existingCategory == null)
                {
                    return NotFound(new { Message = $"There is no any Category by ID: {categoryId} to delete!" });
                }
                else
                {
                    await _service.DeleteCategory(categoryId);
                    return Ok(new { Message = "Category has been successfully deleted!" });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetFinishedBids")]
        public async Task<IActionResult> GetFinishedBids()
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var finishedProducts = await _service.GetFinishedBids();

                    if (!finishedProducts.Any())
                    {
                        return NotFound(new { Message = "There is no any finished bids in the database!" });
                    }
                    else
                    {
                        return Ok(finishedProducts);
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("RefreshFinishedBidById")]
        public async Task<IActionResult> RefreshFinishedBidById(int productId)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingProduct = await connection.QueryFirstOrDefaultAsync<Product.Product>(
                        "SELECT * FROM dbo.Product WHERE Id = @ProductId", new { ProductId = productId });
                    if (existingProduct == null)
                    {
                        return NotFound(new { Message = $"There is no any Product by ID: {productId}" });
                    }
                    if (existingProduct.EndTime > DateTime.Now)
                    {
                        return NotFound(new { Message = $"There is no any finished product by ID: {productId}" });
                    }
                    var maxBidder = await connection.QueryAsync<Bidders>(
                        "SELECT TOP 1 BidderId FROM dbo.Bidders WHERE ProductId = @ProductId ORDER BY BiddingCurrentAmount DESC",
                        new { ProductId = existingProduct.Id });

                    if (maxBidder == null)
                    {
                        return BadRequest(new { Message = "That product hasn't winner!" });
                    }
                    if(existingProduct.IsAvailable == false)
                    {
                        return BadRequest(new { Message = "That product is not available now!" });
                    }
                    else
                    {
                        await _service.RefreshFinishedBidById(productId);
                        return Ok(new { Message = "Something has been refreshed in the database!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("RefreshFinishedBids")]
        public async Task<IActionResult> RefreshFinishedBids()
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getFinishedBidProducts = await connection.QueryAsync<Product.Product>(
                        "SELECT * FROM dbo.Product WHERE EndTime < @DateTimeNow", new { DateTimeNow = DateTime.UtcNow });
                    if (getFinishedBidProducts == null)
                    {
                        return NotFound(new { Message = "There is no any finished bids!" });
                    }
                    else
                    {
                        var maxBidders = await connection.QueryAsync<int>(
                            "SELECT TOP 1 BidderId FROM dbo.Bidders ORDER BY BiddingCurrentAmount DESC");
                        var maxBidder = maxBidders.FirstOrDefault();
                        if (maxBidders == null)
                        {
                            return BadRequest(new { Message = "There is no any winner for the any product!" });
                        }
                        if(getFinishedBidProducts.Any(x => x.IsAvailable) == false)
                        {
                            return BadRequest(new { Message = "There is no any available product in the database!" });
                        }
                        await _service.RefreshFinishedBids();
                        return Ok(new { Message = "Something has successfully changed in the database!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
