using Auction_Web_Api.BidingModel;
using Auction_Web_Api.Helpers;
using Auction_Web_Api.Models;
using Auction_Web_Api.Product;
using Auction_Web_Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FakeServices
{
    public class FakeAdminService : IAdminService
    {
        private readonly List<User> _users = new List<User>
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

        public FakeAdminService(List<User> users)
        {
            _users = users;
        }

        public async Task<bool> AddProductCategory(Categories newCategory)
        {
            var getAllCategories = new List<Categories>()
            {
                new Categories
                {
                    Id = 1,
                    Name = "Test1"
                },
                new Categories
                {
                     Id = 2,
                     Name = "Test2"
                }
        };
            if (string.IsNullOrEmpty(newCategory.Name) || string.IsNullOrWhiteSpace(newCategory.Name))
            {
                return false;
            }
            if (getAllCategories.Any(x => x.Name.ToUpper() == newCategory.Name.ToUpper()))
            {
                return false;
            }
            if (newCategory.Name.Length <= 0 || newCategory.Name.Length > 25)
            {
                return false;
            }
            else
            {
                var addNewCategory = new Categories()
                {
                    Name = newCategory.Name
                };
                return true;
            }
        }

        public async Task<bool> BlockUser(int userId)
        {
            try
            {
                var existingUser = _users.FirstOrDefault(user => user.Id == userId);
                if (existingUser == null)
                {
                    return false;
                }
                if (existingUser.IsBlocked)
                {
                    return false;
                }
                existingUser.IsBlocked = true;
                return true;
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
                var categoryList = new List<Categories>()
            {
                new Categories
                {
                    Id = 1,
                    Name = "Test1"
                },
                new Categories
                {
                     Id = 2,
                     Name = "Test2"
                }
            };
                var existingCategory = categoryList.FirstOrDefault(c => c.Id == categoryId);
                if (existingCategory == null)
                {
                    return false;
                }
                else
                {
                    categoryList.Remove(existingCategory);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var existingUser = _users.FirstOrDefault(user => user.Id == userId);
            if (existingUser == null)
            {
                return false;
            }
            else
            {
                _users.Remove(existingUser);
                return true;
            }
        }

        public Task<IEnumerable<Categories>> GetAllCategories()
        {
            try
            {
                var getAllCategories = new List<Categories>()
            {
                new Categories
                {
                    Id = 1,
                    Name = "Test1"
                },
                new Categories
                {
                     Id = 2,
                     Name = "Test2"
                }
            };
                if (getAllCategories.Any())
                {
                    return Task.FromResult<IEnumerable<Categories>>(getAllCategories);
                }
                else
                {
                    return Task.FromResult<IEnumerable<Categories>>(null);
                }
            }
            catch
            {
                return null;
            }
        }

        public Task<IEnumerable<User>> GetAllUsers()
        {
            var userList = _users.ToList();
            if (userList.Count > 0)
            {
                return Task.FromResult<IEnumerable<User>>(userList);
            }
            else
            {
                return Task.FromResult<IEnumerable<User>>(null);
            }
        }

        public Task<IEnumerable<User>> GetBlockedUsers()
        {
            var userList = new List<User>()
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
                IsBlocked = true,
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
            },

            new User
            {
                Id = 3,
                FirstName = "Test333",
                LastName = "Test333",
                Age = 22,
                ContactNumber = "555555552",
                Email = "test333@gmail.com",
                IdentityNumber = "33322222222",
                Role = "User",
                Password = PasswordHashing.HashPassword("Test333"),
                IsBlocked = false,
                UserAddress = new UserAddress { Country = "Test_Country", City = "Test_City" }
            }
            };
            var blockedUsers = userList.Where(user => user.IsBlocked).ToList();
            if (blockedUsers.Any())
            {
                return Task.FromResult<IEnumerable<User>>(blockedUsers);
            }
            else
            {
                return Task.FromResult<IEnumerable<User>>(null);
            }
        }

        public Task<Categories> GetCategoryById(int categoryId)
        {
            var categories = new List<Categories>()
            {
                new Categories
                {
                    Id = 1,
                    Name = "Test Category_1"
                },
                new Categories
                {
                    Id = 2,
                    Name = "Test Category_2"
                },
            };
            var getCategoryById = categories.FirstOrDefault(c => c.Id == categoryId);
            if (getCategoryById == null)
            {
                return Task.FromResult<Categories>(null);
            }
            else
            {
                return Task.FromResult(getCategoryById);
            }
        }

        public Task<IEnumerable<Product>> GetFinishedBids()
        {
            try
            {
                var bidList = new List<Bidders>()
                {
                    new Bidders
                    {
                        Id = 1,
                        BidderId = 1,
                        BidderEmail = "Misho999@gmail.com",
                        BiddingCurrentAmount = 999,
                        BiddingDate = DateTime.Parse("2024-01-10T12:00:00"),
                        ProductId = 1
                    },
                    new Bidders
                    {
                        Id = 2,
                        BidderId = 2,
                        BidderEmail = "Test222@gmail.com",
                        BiddingCurrentAmount = 5000,
                        BiddingDate = DateTime.Parse("2024-02-10T12:00:00"),
                        ProductId = 1
                    },
                };
                var productList = new List<Product>()
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Test1",
                        Description = "Test1",
                        StartingPrice = 10,
                        CurrentPrice = 1500,
                        StartTime = DateTime.Parse("2023-01-10T12:00:00"),
                        EndTime = DateTime.Parse("2023-05-10T12:00:00"),
                        IsAvailable = true,
                        SellerId = 10,
                        CategoryId = 1
                    },

                    new Product
                    {
                        Id = 2,
                        Name = "Test2",
                        Description = "Test1",
                        StartingPrice = 20,
                        CurrentPrice = 200,
                        StartTime = DateTime.Parse("2024-01-10T12:00:00"),
                        EndTime = DateTime.Parse("2025-05-10T12:00:00"),
                        IsAvailable = false,
                        SellerId = 10,
                        BuyerId = 2,
                        CategoryId = 1
                    }
                };
                var finishedProducts = productList.Where(p => p.EndTime < DateTime.Now).ToList();
                if (finishedProducts.Any() == false)
                {
                    return null;
                }
                else
                {
                    return Task.FromResult<IEnumerable<Product>>(finishedProducts);
                }
            }
            catch
            {
                return null;
            }
        }

        public Task<User> GetUserById(int userId)
        {
            var existingUser = _users.Where(x => x.Id == userId);
            if (existingUser != null)
            {
                return Task.FromResult<User>(existingUser.FirstOrDefault());
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetUsersByAge(AgeModel ageModel)
        {
            var filteredUsers = _users.Where(user => user.Age >= ageModel.MinAge && user.Age <= ageModel.MaxAge).ToList();
            return await Task.FromResult<IEnumerable<User>>(filteredUsers);
        }

        public async Task<IEnumerable<User>> GetUsersByBalance(BalanceModel balance)
        {
            var filteredUsersByBalance = _users.Where(user => user.Balance >= balance.MinBalance && user.Balance <= balance.MaxBalance).ToList();
            return await Task.FromResult<IEnumerable<User>>(filteredUsersByBalance);
        }

        public Task<IEnumerable<User>> GetUsersByCity(string city)
        {
            try
            {
                var usersInCity = _users.Where(user => user.UserAddress.City.Equals(city, StringComparison.OrdinalIgnoreCase)).ToList();
                return Task.FromResult<IEnumerable<User>>(usersInCity);
            }
            catch
            {
                return Task.FromResult<IEnumerable<User>>(null);
            }
        }

        public Task<IEnumerable<User>> GetUsersByCountry(string country)
        {
            try
            {
                var usersInCountry = _users.Where(user => user.UserAddress.Country.Equals(country, StringComparison.OrdinalIgnoreCase)).ToList();
                return Task.FromResult<IEnumerable<User>>(usersInCountry);
            }
            catch
            {
                return Task.FromResult<IEnumerable<User>>(null);
            }
        }

        public async Task<bool> RefreshFinishedBidById(int productId)
        {
            try
            {
                var bidList = new List<Bidders>()
        {
            new Bidders
            {
                Id = 1,
                BidderId = 1,
                BidderEmail = "Misho999@gmail.com",
                BiddingCurrentAmount = 999,
                BiddingDate = DateTime.Parse("2024-01-10T12:00:00"),
                ProductId = 1
            },
            new Bidders
            {
                Id = 2,
                BidderId = 2,
                BidderEmail = "Test222@gmail.com",
                BiddingCurrentAmount = 5000,
                BiddingDate = DateTime.Parse("2024-02-10T12:00:00"),
                ProductId = 1
            },
        };

                var productList = new List<Product>()
        {
            new Product
            {
                Id = 1,
                Name = "Test1",
                Description = "Test1",
                StartingPrice = 10,
                CurrentPrice = 1500,
                StartTime = DateTime.Parse("2023-01-10T12:00:00"),
                EndTime = DateTime.Parse("2023-05-10T12:00:00"),
                IsAvailable = true,
                SellerId = 10,
                CategoryId = 1
            },
            new Product
            {
                Id = 2,
                Name = "Test2",
                Description = "Test1",
                StartingPrice = 20,
                CurrentPrice = 200,
                StartTime = DateTime.Parse("2024-01-10T12:00:00"),
                EndTime = DateTime.Parse("2025-05-10T12:00:00"),
                IsAvailable = false,
                SellerId = 10,
                BuyerId = 2,
                CategoryId = 1
            },
        };

                var finishedProduct = productList.FirstOrDefault(p => p.Id == productId && p.EndTime < DateTime.Now);

                if (finishedProduct == null)
                {
                    return false;
                }
                else
                {
                    var maxBid = bidList.Where(b => b.ProductId == productId).OrderByDescending(b => b.BiddingCurrentAmount).FirstOrDefault();
                    if (maxBid == null)
                    {
                        return false;
                    }
                    finishedProduct.IsAvailable = false;
                    finishedProduct.BuyerId = maxBid.BidderId;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RefreshFinishedBids()
        {
            try
            {
                var bidList = new List<Bidders>()
        {
            new Bidders
            {
                Id = 1,
                BidderId = 1,
                BidderEmail = "Misho999@gmail.com",
                BiddingCurrentAmount = 999,
                BiddingDate = DateTime.Parse("2024-01-10T12:00:00"),
                ProductId = 1
            },
            new Bidders
            {
                Id = 2,
                BidderId = 2,
                BidderEmail = "Test222@gmail.com",
                BiddingCurrentAmount = 5000,
                BiddingDate = DateTime.Parse("2024-02-10T12:00:00"),
                ProductId = 1
            },
        };

                var productList = new List<Product>()
        {
            new Product
            {
                Id = 1,
                Name = "Test1",
                Description = "Test1",
                StartingPrice = 10,
                CurrentPrice = 1500,
                StartTime = DateTime.Parse("2023-01-10T12:00:00"),
                EndTime = DateTime.Parse("2023-05-10T12:00:00"),
                IsAvailable = true,
                SellerId = 10,
                CategoryId = 1
            },
            new Product
            {
                Id = 2,
                Name = "Test2",
                Description = "Test1",
                StartingPrice = 20,
                CurrentPrice = 200,
                StartTime = DateTime.Parse("2024-01-10T12:00:00"),
                EndTime = DateTime.Parse("2025-05-10T12:00:00"),
                IsAvailable = false,
                SellerId = 10,
                BuyerId = 2,
                CategoryId = 1
            },
        };

                var finishedProducts = productList.Where(p => p.EndTime < DateTime.Now).ToList();

                if (finishedProducts == null || !finishedProducts.Any())
                {
                    return false;
                }
                else
                {
                    foreach (var product in finishedProducts)
                    {
                        var maxBid = bidList.Where(b => b.ProductId == product.Id).OrderByDescending(b => b.BiddingCurrentAmount).FirstOrDefault();
                        if (maxBid == null)
                        {
                            return false;
                        }
                        if (!product.IsAvailable)
                        {
                        }
                        product.IsAvailable = false;
                        product.BuyerId = maxBid.BidderId;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> UnblockUser(int userid)
        {
            try
            {
                var existingUser = _users.FirstOrDefault(user => user.Id == userid);
                if (existingUser == null)
                {
                    return false;
                }
                if (existingUser.IsBlocked)
                {
                    return false;
                }
                else
                {
                    existingUser.IsBlocked = false;
                    return false;
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
                var getAllCategories = new List<Categories>()
            {
                new Categories
                {
                    Id = 1,
                    Name = "Test1"
                },
                new Categories
                {
                     Id = 2,
                     Name = "Test2"
                }
        };
                var existingCategory = getAllCategories.FirstOrDefault(c => c.Id == categoryUpdate.CategoryId);
                if (existingCategory == null)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(categoryUpdate.NewName) || string.IsNullOrWhiteSpace(categoryUpdate.NewName))
                {
                    return false;
                }
                if (getAllCategories.Any(x => x.Name.ToUpper() == categoryUpdate.NewName.ToUpper()))
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

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
