using Auction_Web_Api.Product;
using Dapper;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Auction_Web_Api.Validation
{
    public class ProductValidator : AbstractValidator<Product.Product>
    {
        private readonly IConfiguration _configuration;
        public ProductValidator(IConfiguration configuration) 
        {
            _configuration = configuration;
            RuleFor(p => p.Name).NotEmpty().WithMessage("Enter Product Name!")
                .Length(3, 15).WithMessage("Product Name length should be more from 3 to 15 chars!");
            RuleFor(p => p.Description).NotEmpty().WithMessage("Enter Product Description!")
                .Length(5, 30).WithMessage("Product Description length should be from 5 to 30 chars!");
            RuleFor(u => u.StartingPrice).NotEmpty().WithMessage("Enter Starting Price!");
            RuleFor(u => u.StartTime).NotEmpty().WithMessage("Enter Starting Time!")
                .GreaterThan(DateTime.Now).WithMessage("Starting Time should be greater than Datetime Now!");
            RuleFor(u => u.EndTime).NotEmpty().WithMessage("Enter End Time!")
                .GreaterThanOrEqualTo(u => u.StartTime.AddDays(1)).WithMessage("End Time should be greater than Start Time minimal by 1 day!");
            RuleFor(u => u.CategoryId).NotEmpty().WithMessage("Enter Category Id")
                .Must(CategoryExists).WithMessage("Category is not exists by this Id. Try another!");
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        private bool CategoryExists(int categoryId)
        {
            using(var connection = Connection)
            {
                connection.OpenAsync();
                var existingCategory = connection.QueryAsync<int>("SELECT * FROM dbo.Categories WHERE Id = @Id", new { Id = categoryId });
                return existingCategory != null;
            }
        }
    }
}
