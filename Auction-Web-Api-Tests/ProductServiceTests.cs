using Auction_Web_Api.Helpers;
using Auction_Web_Api.Models;
using Auction_Web_Api.Product;
using Auction_Web_Api.Services;
using Auction_Web_Api_Tests.FakeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeServices
{
    [TestFixture]
    public class ProductServiceTests
    {
        private FakeProductService _productService;

        [SetUp]
        public void SetUp()
        {
            var products = new List<Product>
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
            _productService = new FakeProductService();
        }
        [Test]
        public async Task GetAllProducts_ReturnsAllProducts()
        {
            var result = await _productService.GetAllProducts();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Product>>(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAvailableProducts_ReturnsAvailableProducts()
        {
            var result = await _productService.GetAvailableProducts();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Product>>(result);
            Assert.IsTrue(result.All(p => p.IsAvailable));
        }

        [Test]
        public async Task GetProductById_ReturnsExistingProduct()
        {
            int existingProductId = 1;
            var result = await _productService.GetProductById(existingProductId);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Product>(result);
            Assert.AreEqual(existingProductId, result.Id);
        }

        [Test]
        public async Task GetProductsByCategoryId_ReturnsProductsOfCategory()
        {
            int categoryId = 1;
            var result = await _productService.GetProductsByCategoryId(categoryId);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Product>>(result);
            Assert.IsTrue(result.All(p => p.CategoryId == categoryId));
        }
    }
}
