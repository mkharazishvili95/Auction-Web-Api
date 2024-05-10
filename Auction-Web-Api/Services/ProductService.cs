using Auction_Web_Api.Product;
using Auction_Web_Api.Validation;
using Dapper;
using System.Data.Common;
using System.Data.SqlClient;

namespace Auction_Web_Api.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product.Product>>GetAllProducts();
        Task<IEnumerable<Product.Product>> GetAvailableProducts();
        Task<IEnumerable<Product.Product>> GetProductsByCategoryId(int categoryId);
        Task<Product.Product> GetProductById(int productId);
    }
    public class ProductService : IProductService
    {
        private readonly IConfiguration _configuration;
        private readonly ProductValidator _productValidator;
        public ProductService(IConfiguration configuration, ProductValidator productValidator)
        {
            _configuration = configuration;
            _productValidator = productValidator;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<Product.Product>> GetAllProducts()
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getAllProducts = await connection.QueryAsync<Product.Product, Categories, Product.Product>(
                    "SELECT P.*, C.* FROM dbo.Product P JOIN dbo.Categories C ON C.Id = P.CategoryId",
                    (product, category) =>
                    {
                        product.Category = category;
                        return product;
                    }, splitOn: "Id");
                    if (getAllProducts == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getAllProducts;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Product.Product>> GetAvailableProducts()
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var availableProducts = await connection.QueryAsync<Product.Product, Categories, Product.Product>(
                        "SELECT P.*, C.* FROM dbo.Product P JOIN dbo.Categories C ON C.Id = P.CategoryId WHERE P.IsAvailable = 1",
                        (product, category) =>
                        {
                            product.Category = category;
                            return product;
                        }, splitOn: "Id");
                    if(availableProducts == null)
                    {
                        return null;
                    }
                    else
                    {
                        return availableProducts;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<Product.Product> GetProductById(int productId)
        {
            try
            {
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getProductById = await connection.QueryAsync<Product.Product, Categories, Product.Product>(
                        "SELECT P.*, C.* FROM dbo.Product P JOIN dbo.Categories C ON C.Id = P.CategoryId WHERE P.Id = @Id",
                        (product, category) =>
                        {
                            product.Category = category;
                            return product;
                        }, new { Id = productId }, splitOn: "Id");
                    var existingProduct = getProductById.FirstOrDefault();
                    if(existingProduct == null)
                    {
                        return null;
                    }
                    else
                    {
                        return existingProduct;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        public async Task<IEnumerable<Product.Product>> GetProductsByCategoryId(int categoryId)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var getProductsByCategoryId = await connection.QueryAsync<Product.Product, Categories, Product.Product>
                        ("SELECT P.*, C.* FROM dbo.Product P JOIN dbo.Categories C ON C.Id = P.CategoryId WHERE P.CategoryId = @CategoryId",
                        (product, category) =>
                        {
                            product.Category = category;
                            return product;
                        }, new { CategoryId = categoryId }, splitOn: "Id");
                    if(getProductsByCategoryId == null)
                    {
                        return null;
                    }
                    else
                    {
                        return getProductsByCategoryId;
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
