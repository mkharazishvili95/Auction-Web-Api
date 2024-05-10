using Auction_Web_Api.Models;
using FluentValidation;

namespace Auction_Web_Api.Validation
{
    public class UserAddressValidator : AbstractValidator<UserAddress>
    {
        public UserAddressValidator() 
        {
            RuleFor(a => a.Country).NotEmpty().WithMessage("Enter your Country!");
            RuleFor(a => a.City).NotEmpty().WithMessage("Enter your City!");
        }
    }
}
