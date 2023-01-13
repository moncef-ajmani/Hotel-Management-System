using Authentication.Configurations;
using Authentication.Models;
using Authentication.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            // Validate the incoming request
            if (ModelState.IsValid)
            {

                // We need to check if the email already exist
                var user_exist = await _userManager.FindByEmailAsync(requestDto.Email);

                if(user_exist != null)
                {
                    return BadRequest(new AuthResult() {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Email already exist"
                        }
                    });
                }

                // create a user
                var new_user = new IdentityUser()
                {
                    Email = requestDto.Email,
                    UserName = requestDto.Email
                };
                var is_created = await _userManager.CreateAsync(new_user, requestDto.Password);
                Console.WriteLine("Test" + is_created);

                if (is_created.Succeeded)
                {
                    // Add the user to a role
                    await _userManager.AddToRoleAsync(new_user, "client"); 

                    // Generate the token
                    var token = await GenerateJwtToken(new_user);

                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest(new AuthResult()
                {
                    Errors = new List<string>()
                    {
                        "Server Error"
                    },
                    Result = false
                });
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto userLoginRequestDto)
        {
            if (ModelState.IsValid)
            {
                // Check if the user exist
                var existing_user = await _userManager.FindByEmailAsync(userLoginRequestDto.Email);

                if(existing_user == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors=new List<string>()
                        {
                            "Invalid Payload"
                        },
                        Result=false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existing_user, userLoginRequestDto.Password);

                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid credentials"
                        },
                        Result = false
                    });
                }
                var jwtToken = await GenerateJwtToken(existing_user);

                return Ok(new AuthResult()
                {
                    Token = jwtToken,
                    Result = true
                });
            }
            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                },
                Result = false
            });
        }
        // Get all valid claims from the corresponding user
        private async Task<List<Claim>> GetAllValidClaims(IdentityUser user)
        {
            var _options = new IdentityOptions();
            var claims = new List<Claim>
            {
                new Claim("id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
            };

            // Getting the claims that we have assigned to the user
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            // Get the user role and add it to the claims
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
               
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null) {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));

                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach(var roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            return claims; 
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            var claims = await GetAllValidClaims(user);

            // Token description
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
