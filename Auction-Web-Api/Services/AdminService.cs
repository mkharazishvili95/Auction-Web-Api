using Auction_Web_Api.BidingModel;
using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using Auction_Web_Api.Product;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.Common;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Auction_Web_Api.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<IEnumerable<User>> GetUsersByBalance(BalanceModel balance);
        Task<User> GetUserById(int userId);
        Task<IEnumerable<User>> GetUsersByCity(string city);
        Task<IEnumerable<User>> GetUsersByCountry(string country);
        Task<IEnumerable<User>> GetUsersByAge(AgeModel ageModel);
        Task<bool> BlockUser(int userId);
        Task<bool> UnblockUser(int userid);
        Task<IEnumerable<User>> GetBlockedUsers();
        Task<bool> DeleteUser(int userId);
        Task<bool> AddProductCategory(Categories newCategory);
        Task<bool> UpdateCategoryName(CategoryUpdateModel categoryUpdate);
        Task<IEnumerable<Categories>> GetAllCategories();
        Task<Categories> GetCategoryById(int categoryId);
        Task<bool> DeleteCategory(int categoryId);
        Task<bool> RefreshFinishedBids();
        Task<bool> RefreshFinishedBidById(int productId);
        Task<IEnumerable<Product.Product>> GetFinishedBids();
    }
    public class AdminService : IAdminService
    {
        private readonly IConfiguration _configuration;
        public AdminService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<bool> AddProductCategory(Categories newCategory)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getAllCategories = await connection.QueryAsync<Categories>("SELECT * FROM dbo.Categories");
                    if (string.IsNullOrEmpty(newCategory.Name) || string.IsNullOrWhiteSpace(newCategory.Name))
                    {
                        return false;
                    }
                    if (getAllCategories.Any(x => x.Name.ToUpper() == newCategory.Name.ToUpper()))
                    {
                        return false;
                    }
                    if(newCategory.Name.Length <= 0 || newCategory.Name.Length > 25)
                    {
                        return false;
                    }
                    else
                    {
                        await connection.ExecuteAsync("INSERT INTO dbo.Categories (Name) VALUES (@Name)", new
                        {
                            Name = newCategory.Name
                        });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        

        public async Task<bool> BlockUser(int userId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM dbo.Users WHERE Id = @Id",
                        new { Id = userId });
                    if(existingUser == null)
                    {
                        return false;
                    }
                    if (existingUser.Role == Roles.Admin)
                    {
                        return false;
                    }
                    if (existingUser.IsBlocked == true)
                    {
                        return false;
                    }
                    existingUser.IsBlocked = true;
                    await connection.ExecuteAsync("UPDATE dbo.Users SET IsBlocked = 1 WHERE Id = @Id", new { Id = userId });
                    await connection.ExecuteAsync("INSERT INTO dbo.BlockedUsers (Email, UserId, BlockedDate) VALUES " +
                        "(@Email, @UserId, @BlockedDate)", new
                        {
                            Email = existingUser.Email,
                            UserId = existingUser.Id,
                            BlockedDate = DateTime.Now
                        });
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategory(int categoryId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingCategory = await connection.QueryFirstOrDefaultAsync<Categories>("SELECT * FROM dbo.Categories WHERE Id = @Id", new
                    {
                        Id = categoryId
                    });
                    if(existingCategory == null)
                    {
                        return false;
                    }
                    else
                    {
                        await connection.ExecuteAsync("DELETE FROM dbo.Categories WHERE Id = @Id", new
                        {
                            Id = categoryId
                        });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUser(int userId)
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
                        return false;
                    }
                    if(existingUser.Role == Roles.Admin)
                    {
                        return false;
                    }
                    else
                    {
                        var isBlockedUserOrNot = existingUser.IsBlocked;
                        if (isBlockedUserOrNot == true)
                        {
                            await connection.QueryFirstOrDefaultAsync("DELETE FROM dbo.BlockedUsers WHERE UserId = @Id", new { Id = userId });
                        }
                        await connection.ExecuteAsync("DELETE FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                        await connection.ExecuteAsync("DELETE FROM dbo.UserAddress WHERE Id = @Id", new { Id = addressId });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        public async Task<IEnumerable<Product.Product>> GetFinishedBids()
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var finishedProducts = await connection.QueryAsync<Product.Product, Categories, Product.Product>(
                    "SELECT P.*, C.* FROM dbo.Product P JOIN dbo.Categories C ON C.Id = P.CategoryId WHERE EndTime < @DatetimeNow",
                    (product, category) =>
                    {
                        product.Category = category;
                        return product;
                    },
                    new { DatetimeNow = DateTime.UtcNow });

                    if (!finishedProducts.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return finishedProducts;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RefreshFinishedBids()
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getFinishedBidProducts = await connection.QueryAsync<Product.Product>(
                        "SELECT * FROM dbo.Product WHERE EndTime < @DateTimeNow", new { DateTimeNow = DateTime.UtcNow });
                    if(getFinishedBidProducts == null)
                    {
                        return false;
                    }
                    else
                    {
                        var maxBidders = await connection.QueryAsync<int>(
                            "SELECT TOP 1 BidderId FROM dbo.Bidders ORDER BY BiddingCurrentAmount DESC");
                        var maxBidder = maxBidders.FirstOrDefault();
                        if(maxBidders == null)
                        {
                            return false;
                        }
                        if (getFinishedBidProducts.Any(x => x.IsAvailable) == false)
                        {
                            return false;
                        }
                        await connection.ExecuteAsync("UPDATE dbo.Product SET IsAvailable = 0, BuyerId = @BuyerId WHERE EndTime < @DateTimeNow",new 
                        { 
                            BuyerId = maxBidders,
                            DaTetimeNow = DateTime.UtcNow
                        });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Categories>> GetAllCategories()
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getAllCategories = await connection.QueryAsync<Categories>("SELECT * FROM dbo.Categories");
                    if(getAllCategories == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getAllCategories;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var userList = await connection.QueryAsync<User,UserAddress,User>(
                        "SELECT P.Id, P.FirstName,P.LastName, P.IdentityNumber, P.ContactNumber, P.Age, P.Balance, P.Email, P.Role, P.AddressId, C.Id, C.Country, C.City FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId", 
                        (user,address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, splitOn:"Id");
                    if(userList == null)
                    {
                        return null;
                    }
                    else
                    {
                        return userList;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetBlockedUsers()
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getBlockedUsers = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.*, C.* FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE IsBlocked = 1",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, splitOn: "Id");
                    if(getBlockedUsers == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getBlockedUsers;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<Categories> GetCategoryById(int categoryId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingCategory = await connection.QueryFirstOrDefaultAsync<Categories>("SELECT * FROM dbo.Categories WHERE Id = @Id", new
                    {
                        Id = categoryId
                    });
                    if(existingCategory == null)
                    {
                        return null;
                    }
                    else
                    {
                        return existingCategory;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> GetUserById(int userId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUserById = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.Id, P.FirstName,P.LastName, P.IdentityNumber, P.ContactNumber, P.Age, P.Balance, P.Email, P.Role, P.AddressId, C.Id, C.Country, C.City FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE P.Id = @Id",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { Id = userId }, splitOn: "Id");
                    var existingUser = getUserById.FirstOrDefault();
                    if(existingUser == null)
                    {
                        return null;
                    }
                    else
                    {
                        return existingUser;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetUsersByAge(AgeModel ageModel)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUsersByAge = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.Id, P.FirstName,P.LastName, P.IdentityNumber, P.ContactNumber, P.Age, P.Balance, P.Email, P.Role, P.AddressId, C.Id, C.Country, C.City FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE Age BETWEEN @MinAge AND @MaxAge",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { MinAge = ageModel.MinAge, MaxAge = ageModel.MaxAge }, splitOn: "Id");
                    if(getUsersByAge == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getUsersByAge;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetUsersByBalance(BalanceModel balance)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUsersByBalance = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.*, C.* FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE Balance BETWEEN @MinBalance AND @MaxBalance",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { MinBalance = balance.MinBalance, MaxBalance = balance.MaxBalance }, splitOn: "Id");
                    if(getUsersByBalance == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getUsersByBalance;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetUsersByCity(string city)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUsersByCity = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.Id, P.FirstName,P.LastName, P.IdentityNumber, P.ContactNumber, P.Age, P.Balance, P.Email, P.Role, P.AddressId, C.Id, C.Country, C.City FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE City = @City",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { City = city}, splitOn: "Id");
                    if(getUsersByCity == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getUsersByCity;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        public async Task<IEnumerable<User>> GetUsersByCountry(string country)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getUsersByCountry = await connection.QueryAsync<User, UserAddress, User>(
                        "SELECT P.Id, P.FirstName,P.LastName, P.IdentityNumber, P.ContactNumber, P.Age, P.Balance, P.Email, P.Role, P.AddressId, C.Id, C.Country, C.City FROM dbo.Users P JOIN dbo.UserAddress C ON C.Id = P.AddressId WHERE Country = @Country",
                        (user, address) =>
                        {
                            user.UserAddress = address;
                            return user;
                        }, new { Country = country }, splitOn: "Id");
                    if(getUsersByCountry == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getUsersByCountry;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UnblockUser(int userid)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM dbo.Users WHERE Id = @Id",
                        new { Id = userid });
                    if (existingUser == null)
                    {
                        return false;
                    }
                    if (existingUser.Role == Roles.Admin)
                    {
                        return false;
                    }
                    if (existingUser.IsBlocked == false)
                    {
                        return false;
                    }
                    existingUser.IsBlocked = false;
                    await connection.ExecuteAsync("UPDATE dbo.Users SET IsBlocked = 0 WHERE Id = @Id", new { Id = userid });
                    var userInTheBlockList = await connection.QueryFirstOrDefaultAsync("SELECT * FROM dbo.BlockedUsers WHERE UserId = @Id", new
                    {
                        Id = userid
                    });
                    if(userInTheBlockList == null)
                    {
                        return false;
                    }
                    else
                    {
                        await connection.ExecuteAsync("DELETE FROM dbo.BlockedUsers WHERE UserId = @Id", new { Id = userid });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryName(CategoryUpdateModel categoryUpdate)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingCategory = await connection.QueryFirstOrDefaultAsync<Categories>("SELECT * FROM dbo.Categories WHERE Id = @Id", new
                    {
                        Id = categoryUpdate.CategoryId
                    });
                    if (existingCategory == null)
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(categoryUpdate.NewName) || string.IsNullOrWhiteSpace(categoryUpdate.NewName))
                    {
                        return false;
                    }
                    var categoryList = await connection.QueryAsync<Categories>("SELECT * FROM dbo.Categories");
                    if (categoryList.Any(x => x.Name.ToUpper() == categoryUpdate.NewName.ToUpper()))
                    {
                        return false;
                    }
                    if (categoryUpdate.NewName.Length <= 0 || categoryUpdate.NewName.Length > 25)
                    {
                        return false;
                    }
                    else
                    {
                        existingCategory.Name = categoryUpdate.NewName;
                        await connection.ExecuteAsync("UPDATE dbo.Categories SET Name = @NewName WHERE Id = @Id", new
                        {
                            Id = categoryUpdate.CategoryId,
                            NewName = categoryUpdate.NewName
                        });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RefreshFinishedBidById(int productId)
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
                        return false;
                    }
                    if (existingProduct.EndTime > DateTime.Now)
                    {
                        return false;
                    }
                    var existingBidder = await connection.QueryAsync<Bidders>("SELECT * FROM dbo.Bidders WHERE ProductId = @ProductId", new {ProductId =  productId});
                    var maxBidder = await connection.QueryFirstOrDefaultAsync<Bidders>("SELECT TOP 1 BidderId FROM dbo.Bidders ORDER BY BiddingCurrentAmount DESC");
                    if (existingBidder.Any() == false)
                    {
                        return false;
                    }
                    if(maxBidder == null)
                    {
                        return false;
                    }
                    if(existingProduct.IsAvailable == false)
                    {
                        return false;
                    }
                    else
                    {
                        var buyerId = await connection.QueryAsync<int>("SELECT TOP 1 BidderId FROM dbo.Bidders ORDER BY BiddingCurrentAmount DESC");
                        await connection.ExecuteAsync(
                            "UPDATE dbo.Product SET IsAvailable = 0, BuyerId = @BuyerId WHERE Id = @Id",
                            new { Id = productId, BuyerId = buyerId});
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
