using Auction_Web_Api.Helpers;
using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using Auction_Web_Api.Validation;
using Dapper;
using FluentValidation;
using System.Data.Common;
using System.Data.SqlClient;

namespace Auction_Web_Api.Services
{
    public interface IUserService
    {
        Task<User> Register(User newUser);
        Task<User> Login(UserLoginModel loginModel);
    }
    public class UserService : IUserService
    {
        private readonly NewUserValidator _newValidator;
        private readonly IConfiguration _configuration;
        public UserService(NewUserValidator newValidator, IConfiguration configuration)
        {
            _newValidator = newValidator;
            _configuration = configuration;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<User> Login(UserLoginModel loginModel)
        {
            try
            {
                if (string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
                {
                    return null;
                }
                using (var connection = Connection)
                {
                    connection.Open();
                    var selectQuery = "SELECT * FROM Users WHERE Email = @Email;";
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>(selectQuery, new { Email = loginModel.Email });
                    if (existingUser == null)
                    {
                        return null;
                    }
                    if (PasswordHashing.HashPassword(loginModel.Password) != existingUser.Password)
                    {
                        return null;
                    }
                    if (loginModel.Email.ToUpper() != existingUser.Email.ToUpper())
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

        public async Task<User> Register(User newUser)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var validatorResults = await _newValidator.ValidateAsync(newUser);
                    if (!validatorResults.IsValid)
                    {
                        return null;
                    }
                    else
                    {
                        var addressId = connection.ExecuteScalar<int>("INSERT INTO dbo.UserAddress (Country, City) VALUES (@Country, @City); SELECT SCOPE_IDENTITY();", new
                        {
                            Country = newUser.UserAddress.Country,
                            City = newUser.UserAddress.City
                        });
                        connection.ExecuteScalar("INSERT INTO dbo.Users (FirstName, LastName, Age, IdentityNumber, ContactNumber, Balance, Email, Password, Role, IsBlocked, AddressId) " +
                            "VALUES (@FirstName, @LastName, @Age, @IdentityNumber, @ContactNumber, 0, @Email, @Password, @Role, 0, @AddressId)", new
                            {
                                FirstName = newUser.FirstName,
                                LastName = newUser.LastName,
                                Age = newUser.Age,
                                IdentityNumber = newUser.IdentityNumber,
                                ContactNumber = newUser.ContactNumber,
                                Email = newUser.Email,
                                Password = PasswordHashing.HashPassword(newUser.Password),
                                Role = Roles.User,
                                AddressId = addressId
                            });
                        newUser.UserAddress.Id = addressId;
                        return newUser;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
