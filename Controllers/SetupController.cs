using Authentication.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private  readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SetupController> _logger;

        public SetupController(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<SetupController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string name)
        {
            // Check if the roles exist
            var roleExist = await _roleManager.RoleExistsAsync(name);

            if (!roleExist)
            {
                // check if the role has been added successfully*
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(name));

                if(roleResult.Succeeded)
                {
                    _logger.LogInformation($"The Role {name} has not been added successfully");
                    return Ok(new
                    {
                        result = $"The Role {name} has been added successfully"
                    });
                }
                else
                {
                    _logger.LogInformation($"The Role {name} has been added successfully");
                    return BadRequest(new
                    {
                        error = $"The Role {name} has not been added successfully"
                    });
                }
                
            }
            return BadRequest(new { error = "Role already exist" });
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("AddUserToRole")]
        public async Task<IActionResult> AddUserToRole(string email,string roleName)
        {
            // Check if the user exist
            var user = await _userManager.FindByEmailAsync(email);

            if(user == null)
            {
                _logger.LogInformation($"The user with the {email} does not exist");
                return BadRequest(new { error = "User does not exist" });
            }

            // Check if the Role exist
            var roleExist = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                _logger.LogInformation($"The Role {roleName} does not exist");
                return BadRequest(new
                {
                    error = $"The Role {roleName} has not been added successfully exist"
                });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            
            //Check if the user is assigned to the role successfully
            if (result.Succeeded)
            {
                return Ok(new { result = "Success, user has been added to the role"});
            }
            else
            {
                _logger.LogInformation($"The user was not able to added to the role");
                return BadRequest(new { error = "The user was not able to added to the role" });
            }
        }

        [HttpGet]
        [Route("GetUserRoles")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            // Check if email is exist
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogInformation($"The user with the {email} does not exist");
                return BadRequest(new { error = "User does not exist" });
            }

            // return the roles
            var roles = await _userManager.GetRolesAsync(user);
            
            return Ok(roles);
        }

        [HttpPost]
        [Route("RemoveUserFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(string email,string roleName)
        {
            // Check if user is exist
            // Check if email is exist
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogInformation($"The user with the {email} does not exist");
                return BadRequest(new { error = "User does not exist" });
            }

            // Check if the Role exist
            var roleExist = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                _logger.LogInformation($"The Role {roleName} does not exist");
                return BadRequest(new
                {
                    error = $"The Role {roleName} has not been added successfully exist"
                });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return Ok(new { result = $"User {email} has been removed from role {roleName}" });
            }
            return BadRequest(new {error=$"Unable to remove User {email} from role {roleName}"});
        }
    }
}
