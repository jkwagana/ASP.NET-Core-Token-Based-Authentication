using HomeAPIs.Data.Helpers;
using HomeAPIs.Data.Models;
using HomeAPIs.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TwitterAPIs.Data;
using TwitterAPIs.Data.Models;

namespace TwitterAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;



        public AuthenticationController(UserManager<ApplicationUser> UserManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext Context,
            IConfiguration configuration,
            TokenValidationParameters tokenValidationParameters)
        {
            _UserManager = UserManager;
            _roleManager = roleManager;
            _appDbContext = Context;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
        }
        [HttpPost("register-user")]

        public async Task<IActionResult> Register([FromBody] RegisterVm registerVM)



        {
            if (!ModelState.IsValid)
            {

                return BadRequest("Please, Provide all the reguired fields");
            }
            var userExists = await _UserManager.FindByEmailAsync(registerVM.EmailAddress);
            if (userExists != null)
            {
                return BadRequest($"User {registerVM.EmailAddress} Already exists");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Email = registerVM.EmailAddress,
                UserName = registerVM.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),

            };
            var result = await _UserManager.CreateAsync(newUser, registerVM.Password);
            if (result.Succeeded)
            {
                //add user role

                switch (registerVM.Role)
                {
                    case UserRoles.Manager:
                        await _UserManager.AddToRoleAsync(newUser, UserRoles.Manager);
                        break;


                    case UserRoles.Student:
                        await _UserManager.AddToRoleAsync(newUser, UserRoles.Student);
                        break;
                    default:
                        break;

                }

                return Ok("user created");

            }
            {

                return StatusCode(201);
            }
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Registration Failed", errors });
            }


        }


        [HttpPost("login-user")]

        public async Task<IActionResult> Login([FromBody] LoginVm loginVm)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all the require fields");
            }
            var userExists = await _UserManager.FindByEmailAsync(loginVm.EmailAddress);
            if (userExists != null & await _UserManager.CheckPasswordAsync(userExists, loginVm.Password))
            {
                var tokenValue = await GenerateJWTokenAsync(userExists, null);


                return Ok(tokenValue);
            }
            else
            {
                return Unauthorized();

            }

        }

        [HttpPost("refresh-user")]

        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVM tokenRequestVM)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all the require fields");
            }
            var result = await VerifyAndGenerateTokenAsync(tokenRequestVM);

            return Ok(result);



        }

        private async Task<AuthResultsVM> VerifyAndGenerateTokenAsync(TokenRequestVM tokenRequestVM)
        {
            var JwtTokenHandler = new JwtSecurityTokenHandler();

            var storedToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequestVM.RefreshToken);
            var DbUser = await _UserManager.FindByIdAsync(storedToken.UserId);

            try
            {
                var tokenCheckResult = JwtTokenHandler.ValidateToken(tokenRequestVM.Token,
                    _tokenValidationParameters, out var validatedToken);

                return await GenerateJWTokenAsync(DbUser, storedToken);
            }
            catch (SecurityTokenExpiredException)
            {
                if (storedToken.DataExpiry >= DateTime.UtcNow)
                {
                    return await GenerateJWTokenAsync(DbUser, storedToken);
                }
                else
                {
                    return await GenerateJWTokenAsync(DbUser, null);

                }
            }
        }

        private async Task<AuthResultsVM> GenerateJWTokenAsync(ApplicationUser user, RefreshToken rToken)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),


            };

            //add user role claims
            var userRoles = await _UserManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }





            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
               audience: _configuration["JWT:Audience"],
               expires: DateTime.UtcNow.AddMinutes(1),
               claims: authClaims,
               signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));


            var JwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            if (rToken != null)
            {
                var rTokenResponse = new AuthResultsVM()
                {

                    Token = JwtToken,
                    RefreshToken = rToken.Token,
                    ExpiresAt = token.ValidTo

                };
                return rTokenResponse;
            }
            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = user.Id,
                DateAdded = DateTime.UtcNow,
                DataExpiry = DateTime.UtcNow.AddMonths(6),
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
            };
            await _appDbContext.RefreshTokens.AddAsync(refreshToken);
            await _appDbContext.SaveChangesAsync();

            var response = new AuthResultsVM()
            {


                Token = JwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = token.ValidTo
            };
            return response;


        }
    }



}
