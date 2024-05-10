using Auction_Web_Api.Helpers;
using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using Auction_Web_Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeServices
{
    public class FakeUserService : IUserService
    {
        public Task<User> Login(UserLoginModel loginModel)
        {
            var fakeUser = new User
            {
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = "599999999",
                Email = "misho123@gmail.com",
                IdentityNumber = "01010101010",
                Password = PasswordHashing.HashPassword("misho123"),
                UserAddress = new UserAddress { Country = "Georgia", City = "Tbilisi" }

            };

            return Task.FromResult(fakeUser);
        }

        public Task<User> Register(User newUser)
        {
            var fakeUser = new User
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Age = newUser.Age,
                ContactNumber = newUser.ContactNumber,
                Email = newUser.Email,
                IdentityNumber = newUser.IdentityNumber,
                Password = PasswordHashing.HashPassword("misho123"),
                UserAddress = newUser.UserAddress
            };

            return Task.FromResult(fakeUser);
        }
    }
}
