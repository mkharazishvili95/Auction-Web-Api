using Auction_Web_Api.Product;
using Auction_Web_Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction_Web_Api_Tests.FakeServices
{
    public class FakeProductService : IProductService
    {
        private readonly List<Product> _products = new List<Product>()
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

        public Task<IEnumerable<Product>> GetAllProducts()
        {
            try
            {
                var productList = _products.ToList();
                return Task.FromResult<IEnumerable<Product>>(productList);
            }
            catch
            {
                return null;
            }
        }

        public Task<IEnumerable<Product>> GetAvailableProducts()
        {
            try
            {
                var availableProducts = _products.Where(p => p.IsAvailable == true);
                if (availableProducts.Any())
                {
                    return Task.FromResult<IEnumerable<Product>>(availableProducts);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public Task<Product> GetProductById(int productId)
        {
            try
            {
                var existingProduct = _products.FirstOrDefault(p => p.Id == productId);
                if(existingProduct == null)
                {
                    return null;
                }
                else
                {
                    return Task.FromResult(existingProduct);
                }
            }
            catch
            {
                return null;
            }
        }

        public Task<IEnumerable<Product>> GetProductsByCategoryId(int categoryId)
        {
            try
            {
                var getProductsByCategoryId = _products.Where(p => p.CategoryId == categoryId).ToList();
                if(getProductsByCategoryId == null)
                {
                    return null;
                }
                else
                {
                    return Task.FromResult<IEnumerable<Product>>(getProductsByCategoryId);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
