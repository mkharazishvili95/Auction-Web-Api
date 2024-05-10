using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction_Web_Api_Tests.FakeServices
{
    public class FakeTokenGenerator : ITokenGenerator
    {
        public string GenerateToken(User user)
        {
            string fakeToken = "Fake Token Generator =)";
            return fakeToken;
        }
    }
}
