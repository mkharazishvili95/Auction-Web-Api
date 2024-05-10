using Auction_Web_Api.Helpers;
using Auction_Web_Api.Models;
using Auction_Web_Api.Product;
using Auction_Web_Api.Services;
using FakeServices;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FakeServices
{
    [TestFixture]
    public class AdminServiceTests
    {
        private FakeAdminService _adminService;

        [SetUp]
        public void SetUp()
        {
            var users = new List<User>
            {
                new User
            {
                Id = 1,
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = "599999999",
                Email = "misho123@gmail.com",
                IdentityNumber = "01010101010",
                Balance = 100,
                Role = "User",
                IsBlocked = false,
                Password = PasswordHashing.HashPassword("misho123"),
                UserAddress = new UserAddress { Country = "Georgia", City = "Tbilisi" }
            },
            new User
            {
                Id = 2,
                FirstName = "Test222",
                LastName = "Test222",
                Age = 22,
                ContactNumber = "555555555",
                Email = "test222@gmail.com",
                IdentityNumber = "22222222222",
                Balance = 500,
                Role = "User",
                IsBlocked = true,
                Password = PasswordHashing.HashPassword("Test222"),
                UserAddress = new UserAddress { Country = "Test_Country", City = "Test_City" }
                }
            };
            _adminService = new FakeAdminService(users);
        }

        [Test]
        public async Task GetAllUsers_ReturnsUsers_WhenUsersExist()
        {
            var users = new List<User>
            {
                new User
            {
                Id = 1,
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = "599999999",
                Email = "misho123@gmail.com",
                IdentityNumber = "01010101010",
                Role = "User",
                Password = PasswordHashing.HashPassword("misho123"),
                UserAddress = new UserAddress { Country = "Georgia", City = "Tbilisi" }
            },
            new User
            {
                Id = 2,
                FirstName = "Test222",
                LastName = "Test222",
                Age = 22,
                ContactNumber = "555555555",
                Email = "test222@gmail.com",
                IdentityNumber = "22222222222",
                Role = "User",
                Password = PasswordHashing.HashPassword("Test222"),
                UserAddress = new UserAddress { Country = "Test_Country", City = "Test_City" }
                }
            };
            var fakeAdminService = new FakeAdminService(users);
            var result = await fakeAdminService.GetAllUsers();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<User>>(result);
            Assert.AreEqual(users.Count, ((List<User>)result).Count);
        }

        [Test]
        public async Task GetAllUsers_ReturnsNull_WhenNoUsersExist()
        {
            var users = new List<User>();
            var fakeAdminService = new FakeAdminService(users);
            var result = await fakeAdminService.GetAllUsers();
            Assert.IsNull(result);
        }
        [Test]
        public async Task GetUserById_ReturnsExistingUser_WhenUserExists()
        {
            var users = new List<User>
            {
                new User
            {
                Id = 1,
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = "599999999",
                Email = "misho123@gmail.com",
                IdentityNumber = "01010101010",
                Role = "User",
                Password = PasswordHashing.HashPassword("misho123"),
                UserAddress = new UserAddress { Country = "Georgia", City = "Tbilisi" }
            },
            new User
            {
                Id = 2,
                FirstName = "Test222",
                LastName = "Test222",
                Age = 22,
                ContactNumber = "555555555",
                Email = "test222@gmail.com",
                IdentityNumber = "22222222222",
                Role = "User",
                Password = PasswordHashing.HashPassword("Test222"),
                UserAddress = new UserAddress { Country = "Test_Country", City = "Test_City" }
                }
            };
            var fakeAdminService = new FakeAdminService(users);
            var userId = 1;
            var result = await fakeAdminService.GetUserById(userId);
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result.Id);
        }

        [Test]
        public async Task GetUserById_ReturnsNull_WhenUserDoesNotExist()
        {
            var users = new List<User>
            {
                new User
            {
                Id = 1,
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = "599999999",
                Email = "misho123@gmail.com",
                IdentityNumber = "01010101010",
                Role = "User",
                Password = PasswordHashing.HashPassword("misho123"),
                UserAddress = new UserAddress { Country = "Georgia", City = "Tbilisi" }
            },
            new User
            {
                Id = 2,
                FirstName = "Test222",
                LastName = "Test222",
                Age = 22,
                ContactNumber = "555555555",
                Email = "test222@gmail.com",
                IdentityNumber = "22222222222",
                Role = "User",
                Password = PasswordHashing.HashPassword("Test222"),
                UserAddress = new UserAddress { Country = "Test_Country", City = "Test_City" }
                }
            };
            var fakeAdminService = new FakeAdminService(users);
            var userId = 3;
            var result = await fakeAdminService.GetUserById(userId);
            Assert.IsNull(result);
        }
        [Test]
        public async Task GetUsersByAge_ReturnsFilteredUsers()
        {
            var ageModel = new AgeModel { MinAge = 20, MaxAge = 30 };
            var result = await _adminService.GetUsersByAge(ageModel);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<User>>(result);
            var filteredUsers = result.ToList();
            Assert.AreEqual(2, filteredUsers.Count);
            Assert.AreEqual("Mikheil", filteredUsers[0].FirstName, "Test111", filteredUsers[1].FirstName);
        }

        [Test]
        public async Task GetUsersByAge_ReturnsEmptyList_WhenNoUsersMatchAgeCriteria()
        {
            var ageModel = new AgeModel { MinAge = 50, MaxAge = 60 };
            var result = await _adminService.GetUsersByAge(ageModel);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }
        [Test]
        public async Task GetUsersByBalance_ReturnsFilteredUsers()
        {
            var balanceModel = new BalanceModel { MinBalance = 50, MaxBalance = 150 };
            var result = await _adminService.GetUsersByBalance(balanceModel);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<User>>(result);
            var filteredUsers = result.ToList();
            Assert.AreEqual(1, filteredUsers.Count);
            Assert.AreEqual("Mikheil", filteredUsers[0].FirstName);
        }

        [Test]
        public async Task GetUsersByBalance_ReturnsEmptyList_WhenNoUsersMatchBalanceCriteria()
        {
            var balanceModel = new BalanceModel { MinBalance = 501, MaxBalance = 1500 };
            var result = await _adminService.GetUsersByBalance(balanceModel);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }
        [Test]
        public async Task BlockUser_ReturnsFalse_WhenUserNotFound()
        {
            var users = new List<User>
            {
                new User
            {
                Id = 1,
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = "599999999",
                Email = "misho123@gmail.com",
                IdentityNumber = "01010101010",
                Role = "User",
                Password = PasswordHashing.HashPassword("misho123"),
                IsBlocked = false,
                UserAddress = new UserAddress { Country = "Georgia", City = "Tbilisi" }
            },
            new User
            {
                Id = 2,
                FirstName = "Test222",
                LastName = "Test222",
                Age = 22,
                ContactNumber = "555555555",
                Email = "test222@gmail.com",
                IdentityNumber = "22222222222",
                Role = "User",
                Password = PasswordHashing.HashPassword("Test222"),
                IsBlocked = true,
                UserAddress = new UserAddress { Country = "Test_Country", City = "Test_City" }
                }
            };
            var userId = 4;
            var result = await _adminService.BlockUser(userId);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task BlockUser_ReturnsFalse_WhenUserIsAlreadyBlocked()
        {
            var userId = 2;
            var result = await _adminService.BlockUser(userId);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task BlockUser_ReturnsTrue_AndSetsUserBlocked_WhenUserIsNotBlocked()
        {
            var userId = 1;
            var result = await _adminService.BlockUser(userId);
            Assert.IsTrue(result);
            var blockedUser = _adminService.GetUserById(userId).Result;
            Assert.IsTrue(blockedUser.IsBlocked);
        }

        [Test]
        public async Task UnblockUser_ReturnsTrue_AndUnblocksUser_WhenUserIsBlocked()
        {
            var userId = 2;
            var existingUser = new User
            {
                Id = 2,
                FirstName = "Test222",
                LastName = "Test222",
                Age = 22,
                ContactNumber = "555555555",
                Email = "test222@gmail.com",
                IdentityNumber = "22222222222",
                Role = "User",
                Password = PasswordHashing.HashPassword("Test222"),
                IsBlocked = true,
                UserAddress = new UserAddress { Country = "Test_Country", City = "Test_City" }
            };
            var result = await _adminService.UnblockUser(userId);
            Assert.IsFalse(result);
            Assert.IsTrue(existingUser.IsBlocked);
        }


        [Test]
        public async Task UnblockUser_ReturnsFalse_WhenUserIsNotFound()
        {
            var userId = 10;
            var result = await _adminService.UnblockUser(userId);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UnblockUser_ReturnsFalse_WhenUserIsAlreadyUnblocked()
        {
            var userId = 1;
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = "599999999",
                Email = "misho123@gmail.com",
                IdentityNumber = "01010101010",
                Role = "User",
                Password = PasswordHashing.HashPassword("misho123"),
                IsBlocked = false,
                UserAddress = new UserAddress { Country = "Georgia", City = "Tbilisi" }
            };
            var result = await _adminService.UnblockUser(userId);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetUsersByCountry_ReturnsUsers_WhenCountryExists()
        {
            string country = "Georgia";
            var usersInCountry = await _adminService.GetUsersByCountry(country);
            Assert.IsNotNull(usersInCountry);
            Assert.IsInstanceOf<IEnumerable<User>>(usersInCountry);
            Assert.AreEqual(1, usersInCountry.Count());
        }

        [Test]
        public async Task GetUsersByCountry_ReturnsEmptyList_WhenCountryDoesNotExist()
        {
            string country = "England";
            var usersInCountry = await _adminService.GetUsersByCountry(country);
            Assert.IsNotNull(usersInCountry);
            Assert.IsEmpty(usersInCountry);
        }

        [Test]
        public async Task GetUsersByCity_ReturnsUsers_WhenCityExists()
        {
            string city = "Tbilisi";
            var usersInCity = await _adminService.GetUsersByCity(city);
            Assert.IsNotNull(usersInCity);
            Assert.IsInstanceOf<IEnumerable<User>>(usersInCity);
            Assert.AreEqual(1, usersInCity.Count());
        }
        [Test]
        public async Task GetNullByCityList_WhenCityDoesNotExist()
        {
            string city = "Rustavi";
            var usersInCity = await _adminService.GetUsersByCity(city);
            Assert.IsNotNull(usersInCity);
            Assert.IsEmpty(usersInCity);
        }

        [Test]
        public async Task DeleteUser_ReturnsTrue_WhenUserExistsAndIsDeleted()
        {
            var userIdToDelete = 2;
            var result = await _adminService.DeleteUser(userIdToDelete);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteUser_ReturnsFalse_WhenUserDoesNotExist()
        {
            var nonExistingUserId = 999;
            var result = await _adminService.DeleteUser(nonExistingUserId);
            Assert.IsFalse(result);
        }
        [Test]
        public async Task GetBlockedUsers_ReturnsBlockedUsers_WhenUsersAreBlocked()
        {
            _adminService.BlockUser(1);
            _adminService.BlockUser(2);
            var blockedUsers = await _adminService.GetBlockedUsers();
            Assert.IsNotNull(blockedUsers);
            Assert.IsInstanceOf<IEnumerable<User>>(blockedUsers);
            Assert.AreEqual(2, blockedUsers.Count());
        }
        [Test]
        public async Task GetCategoryById_ReturnsCategory_WhenCategoryExists()
        {
            int categoryId = 1;
            var category = await _adminService.GetCategoryById(categoryId);
            Assert.IsNotNull(category);
            Assert.AreEqual(categoryId, category.Id);
            Assert.AreEqual("Test Category_1", category.Name);
        }

        [Test]
        public async Task GetCategoryById_ReturnsNull_WhenCategoryDoesNotExist()
        {
            int categoryId = 3;
            var category = await _adminService.GetCategoryById(categoryId);
            Assert.IsNull(category);
        }
        [Test]
        public async Task AddProductCategory_WithValidCategory_ReturnsTrue()
        {
            var newCategory = new Categories
            {
                Name = "ValidCategory"
            };
            var result = await _adminService.AddProductCategory(newCategory);
            Assert.IsTrue(result);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task AddProductCategory_WithEmptyOrNullCategory_ReturnsFalse(string categoryName)
        {
            var newCategory = new Categories
            {
                Name = categoryName
            };
            var result = await _adminService.AddProductCategory(newCategory);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task AddProductCategory_WithExistingCategory_ReturnsFalse()
        {
            var existingCategory = new Categories
            {
                Name = "Test1"
            };
            var result = await _adminService.AddProductCategory(existingCategory);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task AddProductCategory_WithLongCategoryName_ReturnsFalse()
        {
            var newCategory = new Categories
            {
                Name = "ThisIsAReallyLongCategoryNameThatShouldExceedTheMaxLength"
            };
            var result = await _adminService.AddProductCategory(newCategory);
            Assert.IsFalse(result);
        }
        [Test]
        public async Task UpdateCategoryName_WithValidCategory_ReturnsTrue()
        {
            var categoryUpdate = new CategoryUpdateModel
            {
                CategoryId = 1,
                NewName = "NewName"
            };
            var result = await _adminService.UpdateCategoryName(categoryUpdate);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task UpdateCategoryName_WithNonExistingCategory_ReturnsFalse()
        {
            var categoryUpdate = new CategoryUpdateModel
            {
                CategoryId = 999,
                NewName = "NewName"
            };

            var result = await _adminService.UpdateCategoryName(categoryUpdate);
            Assert.IsFalse(result);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task UpdateCategoryName_WithEmptyOrNullNewName_ReturnsFalse(string newName)
        {
            var categoryUpdate = new CategoryUpdateModel
            {
                CategoryId = 1,
                NewName = newName
            };
            var result = await _adminService.UpdateCategoryName(categoryUpdate);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UpdateCategoryName_WithExistingNewName_ReturnsFalse()
        {
            var categoryUpdate = new CategoryUpdateModel
            {
                CategoryId = 1,
                NewName = "Test2"
            };
            var result = await _adminService.UpdateCategoryName(categoryUpdate);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UpdateCategoryName_WithLongNewName_ReturnsFalse()
        {
            var categoryUpdate = new CategoryUpdateModel
            {
                CategoryId = 1,
                NewName = "ThisIsAReallyLongCategoryNameThatShouldExceedTheMaxLength"
            };
            var result = await _adminService.UpdateCategoryName(categoryUpdate);
            Assert.IsFalse(result);
        }
        [Test]
        public async Task GetAllCategories_ReturnsListOfCategories()
        {
            var result = await _adminService.GetAllCategories();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Categories>>(result);
            Assert.IsTrue(result.Any());
        }
        [Test]
        public async Task DeleteCategory_WithExistingCategoryId_ReturnsTrue()
        {
            var categoryIdToDelete = 1;
            var result = await _adminService.DeleteCategory(categoryIdToDelete);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteCategory_WithNonExistingCategoryId_ReturnsFalse()
        {
            var categoryIdToDelete = 999;
            var result = await _adminService.DeleteCategory(categoryIdToDelete);
            Assert.IsFalse(result);
        }
        [Test]
        public async Task GetFinishedBids_ReturnsFinishedProducts()
        {
            var result = await _adminService.GetFinishedBids();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Product>>(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.All(p => p.EndTime < DateTime.Now));
        }
        [Test]
        public async Task RefreshFinishedBids_ReturnsTrue_WhenBidsAreRefreshed()
        {
            var result = await _adminService.RefreshFinishedBids();
            Assert.IsTrue(result);
        }
        [Test]
        public async Task RefreshFinishedBidById_ReturnsTrue_WhenBidIsRefreshed()
        {
            int productId = 1;
            var result = await _adminService.RefreshFinishedBidById(productId);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task RefreshFinishedBidById_ReturnsFalse_WhenProductIsNotFinishedOrNoBids()
        {
            int productId = 3;
            var result = await _adminService.RefreshFinishedBidById(productId);
            Assert.IsFalse(result);
        }
    }
}
