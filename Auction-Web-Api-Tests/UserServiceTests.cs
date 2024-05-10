using Auction_Web_Api.Helpers;
using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeServices
{
    [TestFixture]
    public class UserServiceTests
    {
        private FakeUserService fakeUserService;

        [SetUp]
        public void Setup()
        {
            fakeUserService = new FakeUserService();
        }

        [Test]
        public async Task RegisterNewUser_ValidModel_ReturnsUser()
        {
            var userRegisterModel = new UserRegisterModel
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

            var result = await fakeUserService.Register(userRegisterModel.ToUser());
            Assert.NotNull(result);
            Assert.AreEqual(userRegisterModel.FirstName, result.FirstName);
            Assert.AreEqual(userRegisterModel.LastName, result.LastName);
            Assert.AreEqual(userRegisterModel.Age, result.Age);
            Assert.AreEqual(userRegisterModel.ContactNumber, result.ContactNumber);
            Assert.AreEqual(userRegisterModel.Email, result.Email);
            Assert.AreEqual(userRegisterModel.UserAddress.Country, result.UserAddress.Country);
            Assert.AreEqual(userRegisterModel.UserAddress.City, result.UserAddress.City);
        }

        [Test]
        public async Task LoginUser_ValidCredentials_ReturnsUser()
        {
            var userLoginModel = new UserLoginModel
            {
                Email = "misho123@gmail.com",
                Password = PasswordHashing.HashPassword("misho123"),
            };

            var result = await fakeUserService.Login(userLoginModel);
            Assert.NotNull(result);
            Assert.AreEqual("Mikheil", result.FirstName);
            Assert.AreEqual("Kharazishvili", result.LastName);
            Assert.AreEqual(28, result.Age);
            Assert.AreEqual("599999999", result.ContactNumber);
            Assert.AreEqual("misho123@gmail.com", result.Email);
            Assert.AreEqual("Georgia", result.UserAddress.Country);
            Assert.AreEqual("Tbilisi", result.UserAddress.City);
        }
    }
}
public static class ModelExtensions
{
    public static User ToUser(this UserRegisterModel model)
    {
        return new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Age = model.Age,
            ContactNumber = model.ContactNumber,
            Email = model.Email,
            IdentityNumber = model.IdentityNumber,
            Password = model.Password,
            UserAddress = model.UserAddress
        };
    }
}

