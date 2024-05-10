using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using Auction_Web_Api_Tests.FakeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeServices
{
    [TestFixture]
    public class TokenGeneratorTests
    {
        private ITokenGenerator _fakeTokenGenerator;

        [SetUp]
        public void Setup()
        {
            _fakeTokenGenerator = new FakeTokenGenerator();
        }

        [Test]
        public void GenerateToken_ReturnsFakeToken()
        {
            var user = new User
            {
                Id = 1,
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 29,
                Email = "Misho123@gmail.com",
                Role = "User"
            };
            var generatedToken = _fakeTokenGenerator.GenerateToken(user);
            Assert.IsNotNull(generatedToken);
        }
    }
}
