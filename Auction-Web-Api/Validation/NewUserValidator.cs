using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using Dapper;
using FluentValidation;
using System.Data.Common;
using System.Data.SqlClient;

namespace Auction_Web_Api.Validation
{
    public class NewUserValidator : AbstractValidator<User>
    {
        private readonly IConfiguration _configuration;
        public NewUserValidator(IConfiguration configuration) 
        {
            _configuration = configuration;

            RuleFor(u => u.FirstName).NotEmpty().WithMessage("Enter your FirstName!");
            RuleFor(u => u.LastName).NotEmpty().WithMessage("Enter your LastName!");
            RuleFor(u => u.Age).NotEmpty().WithMessage("Enter your Age!")
                .GreaterThanOrEqualTo(18).WithMessage("Your Age should be more or equal to 18!")
                .LessThanOrEqualTo(65).WithMessage("Your Age should be less or equal to 65!");
            RuleFor(u => u.IdentityNumber).NotEmpty().WithMessage("Enter your Identity Number!")
                .Length(10, 20).WithMessage("Your Identity Number length should be from 10 to 20 numbers!")
                .Must(DifferentIdentityNumber).WithMessage("Identity Number has already registered. Try another!");
            RuleFor(u => u.ContactNumber).NotEmpty().WithMessage("Enter your Contact Number!")
                .Length(9, 20).WithMessage("Enter your Valid Contact Number without +995!")
                .Must(DifferentContactNumber).WithMessage("Contact Number has already registered. Try another!");
            RuleFor(u => u.Email).NotEmpty().WithMessage("Enter your Email address!")
                .Must(DifferentEmail).WithMessage("Email already exists. Try another!")
                .EmailAddress().WithMessage("Enter your Valid Email address!");
            RuleFor(u => u.Password).NotEmpty().WithMessage("Enter your Password!")
                .Length(6, 15).WithMessage("Password length should be from 6 to 15 chars or numbers!");
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        private bool DifferentEmail(string email)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var result = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Users WHERE Email = @Email", new { Email = email });
                return result == 0;
            }
        }
        private bool DifferentIdentityNumber(string identity)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var result = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Users WHERE IdentityNumber = @IdentityNumber", new { IdentityNumber = identity });
                return result == 0;
            }
        }
        private bool DifferentContactNumber(string contactNumber) 
        {
            using (var connection = Connection)
            {
                connection.OpenAsync();
                var result = connection.QueryFirstOrDefault<int>("SELECT COUNT (*) FROM Users WHERE ContactNumber = @ContactNumber", new
                {
                    ContactNumber = contactNumber
                });
                return result == 0;
            }
        }
    }
}

/*
  



  {
  "id": 0,
  "firstName": "Misho",
  "lastName": "Kharazishvili",
  "age": 28,
  "identityNumber": "10101010101",
  "contactNumber": "598336060",
  "balance": 0,
  "email": "Misho999@gmail.com",
  "password": "Misho999",
  "addressId": 0,
  "userAddress": {
    "id": 0,
    "country": "Georgia",
    "city": "Tbilisi"
  }
}



 */
