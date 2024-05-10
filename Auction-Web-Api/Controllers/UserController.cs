using Auction_Web_Api.Helpers;
using Auction_Web_Api.Identity;
using Auction_Web_Api.Models;
using Auction_Web_Api.Services;
using Auction_Web_Api.Validation;
using Dapper;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tweetinvi.Core.Models;
using Tweetinvi.Models;
using Tweetinvi.Models.V2;

namespace Auction_Web_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly NewUserValidator _userValidator;
        private readonly IUserService _service;
        private readonly AppSettings _appSettings;
        public UserController(IConfiguration configuration, NewUserValidator userValidator, IUserService service, AppSettings appSettings)
        {
            _configuration = configuration;
            _userValidator = userValidator;
            _service = service;
            _appSettings = appSettings;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        [HttpPost("RegisterForm")]
        public async Task<IActionResult> Register(Models.User newUser)
        {
            try
            {
                var validatorResults = await _userValidator.ValidateAsync(newUser);
                if (!validatorResults.IsValid)
                {
                    return BadRequest(validatorResults.Errors);
                }
                else
                {
                    await _service.Register(newUser);
                    return Ok(new { SuccessMessage = $"User: {newUser.Email.ToUpper()} has successfully registered!" });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("LoginForm")]
        public async Task<IActionResult> Login(UserLoginModel loginModel)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var existingUser = await _service.Login(loginModel);

                    if (string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
                    {
                        return BadRequest(new { Error = "Enter your Email and Password!" });
                    }

                    if (loginModel.Email.ToUpper() != existingUser?.Email.ToUpper())
                    {
                        return BadRequest(new { Error = "Email or Password is incorrect!" });
                    }

                    if (PasswordHashing.HashPassword(loginModel.Password) != existingUser?.Password)
                    {
                        return BadRequest(new { Error = "Email or Password is incorrect!" });
                    }

                    if (loginModel.Email.ToUpper() == existingUser?.Email.ToUpper() && PasswordHashing.HashPassword(loginModel.Password) == existingUser?.Password)
                    {
                        var newLogg = await connection.ExecuteAsync(
                            @"INSERT INTO Loggs (LoggedUser, UserId, LoggedDate) VALUES (@LoggedUser, @UserId, @LoggedDate);",
                            new
                            {
                                LoggedUser = existingUser.Email,
                                UserId = existingUser.Id,
                                LoggedDate = DateTime.Now
                            });

                        var tokenString = GenerateToken(existingUser);

                        return Ok(new
                        {
                            Message = "You have successfully Logged!",
                            FirstName = existingUser.FirstName,
                            Email = existingUser.Email,
                            Role = existingUser.Role,
                            Token = tokenString
                        });
                    }
                    return BadRequest(new { Error = "Login Error!" });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetMyBalance")]
        public async Task<IActionResult> GetMyBalance()
        {
            try
            {
                var userId = int.Parse(User.Identity.Name);
                using(var connection = Connection)
                {
                    await connection.OpenAsync();
                    var existingUserBalance = await connection.QueryFirstOrDefaultAsync("SELECT Balance FROM dbo.Users WHERE Id = @Id", new
                    {
                        Id = userId
                    });
                    return Ok(existingUserBalance);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("UpdateEmailAddress")]
        public async Task<IActionResult> UpdateEmailAddress([FromBody] EmailUpdateModel emailUpdateModel)
        {
            try
            {
                var userId = int.Parse(User.Identity.Name);
                using(var connection = Connection)
                {
                    var existingUser = await connection.QueryFirstOrDefaultAsync
                        ("SELECT * FROM dbo.Users WHERE Id = @Id", new {Id = userId});
                    var existingUserPassword = await connection.QueryFirstOrDefaultAsync
                        ("SELECT Password FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                    if(existingUser == null || emailUpdateModel.eMail.ToUpper() != existingUser.Email.ToUpper())
                    {
                        return NotFound(new { Message = "User Not Found!" });
                    }
                    if(string.IsNullOrEmpty(emailUpdateModel.eMail) || string.IsNullOrEmpty(emailUpdateModel.repeatEmail) || string.IsNullOrEmpty(emailUpdateModel.newEmail) || string.IsNullOrEmpty(emailUpdateModel.password) ||
                        string.IsNullOrEmpty(emailUpdateModel.repeatPassword))
                    {
                        return BadRequest(new { Message = "Fill all the lines, please!" });
                    }
                    if(emailUpdateModel.eMail.ToUpper() != emailUpdateModel.repeatEmail.ToUpper())
                    {
                        return BadRequest(new { Message = "Email and repeat Email should be the same!" });
                    }
                    if(PasswordHashing.HashPassword(emailUpdateModel.password) != existingUser.Password || PasswordHashing.HashPassword(emailUpdateModel.password) != existingUser.Password)
                    {
                        return BadRequest(new { Message = "Password is incorrect!" });
                    }
                    if(emailUpdateModel.newEmail.ToUpper() == emailUpdateModel.eMail.ToUpper() || emailUpdateModel.newEmail.ToUpper() == emailUpdateModel.repeatEmail.ToUpper())
                    {
                        return BadRequest(new { Message = "New Email should be different than recent!" });
                    }
                    var listOfEmails = await connection.QueryAsync("SELECT Email FROM dbo.Users");
                    if (listOfEmails.Contains(emailUpdateModel.newEmail))
                    {
                        return BadRequest(new { Message = "Email already exists. Try another!" });
                    }
                    if (emailUpdateModel.newEmail.Contains("@") == false || emailUpdateModel.newEmail.Contains(".") == false || emailUpdateModel.newEmail.Length < 6)
                    {
                        return BadRequest(new { Message = "Enter Valid Email Address!" });
                    }
                    if(existingUser.IsBlocked == true)
                    {
                        return BadRequest(new { Message = "You can not change Email, because you are blocked!" });
                    }
                    else
                    {
                        existingUser.Email = emailUpdateModel.newEmail;
                        await connection.ExecuteAsync("UPDATE dbo.Users SET Email = @Email WHERE Id = @Id", new
                        {
                            Id = userId,
                            Email = emailUpdateModel.newEmail
                        });
                        return Ok(new {Message =  "Email Address has successfully updated in the database!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordUpdateModel passwordUpdateModel)
        {
            try
            {
                using (var connection = Connection)
                {
                    await connection.OpenAsync();
                    var userId = int.Parse(User.Identity.Name);
                    var existingUser = await connection.QueryFirstOrDefaultAsync
                        ("SELECT * FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                    var existingUserPassword = await connection.QueryFirstOrDefaultAsync
                        ("SELECT Password FROM dbo.Users WHERE Id = @Id", new { Id = userId });
                    if (existingUser == null || passwordUpdateModel.eMail.ToUpper() != existingUser.Email.ToUpper())
                    {
                        return NotFound(new { Message = "User Not Found!" });
                    }
                    if (string.IsNullOrEmpty(passwordUpdateModel.eMail) || string.IsNullOrEmpty(passwordUpdateModel.repeatEmail) || string.IsNullOrEmpty(passwordUpdateModel.password) || string.IsNullOrEmpty(passwordUpdateModel.repeatPassword) ||
                        string.IsNullOrEmpty(passwordUpdateModel.newPassword))
                    {
                        return BadRequest(new { Message = "Fill all the lines, please!" });
                    }
                    if (passwordUpdateModel.eMail.ToUpper() != passwordUpdateModel.repeatEmail.ToUpper())
                    {
                        return BadRequest(new { Message = "Email and repeat Email should be the same!" });
                    }
                    if (PasswordHashing.HashPassword(passwordUpdateModel.password) != existingUser.Password || PasswordHashing.HashPassword(passwordUpdateModel.repeatPassword) != existingUser.Password)
                    {
                        return BadRequest(new { Message = "Password is incorrect!" });
                    }
                    if (PasswordHashing.HashPassword(passwordUpdateModel.newPassword) == existingUser.Password)
                    {
                        return BadRequest(new { Message = "New Password should be different than recent!" });
                    }
                    if (passwordUpdateModel.newPassword.Length < 6 || passwordUpdateModel.newPassword.Length > 15)
                    {
                        return BadRequest(new { Message = "New password length should be from 6 to 15 chars or numbers!" });
                    }
                    if (existingUser.IsBlocked == true)
                    {
                        return BadRequest(new { Message = "You can not change the Password, because you are blocked!" });
                    }
                    else
                    {
                        existingUser.Password = PasswordHashing.HashPassword(passwordUpdateModel.newPassword);
                        await connection.ExecuteAsync("UPDATE dbo.Users SET Password = @Password WHERE Id = @Id", new
                        {
                            Id = userId,
                            Password = PasswordHashing.HashPassword(passwordUpdateModel.newPassword)
                        });
                        return Ok(new { Message = "Password has successfully updated in the database!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

            private object GenerateToken(Models.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Email.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(365),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}
