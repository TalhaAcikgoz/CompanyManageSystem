using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyIdentityApp.Data;

public class CreateUserModel
{
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

namespace MyIdentityApp.Controllers{

    [Route("api/[controller]")]
    public class PersonalController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;//auth
        private readonly RoleManager<IdentityRole> _roleManager;

        public PersonalController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        //         var user = new ApplicationUser { UserName = model.username, Email = model.email, CompanyName = model.CompanyName};
        // var result = await _userManager.CreateAsync(user, model.password);
        // if (result.Succeeded)
        // {
        //     if (!await _roleManager.RoleExistsAsync(model.role))
        //     {
        //         var roleeror = await _userManager.DeleteAsync(user);
        //         return BadRequest("Role bulunamadi."+ " delete error: "+ roleeror.ToString());
        //     }
        //     await _userManager.AddToRoleAsync(user, model.role);
        //     return Ok("Kullanici basariyla oluşturuldu ve rol atandi.");
        // }
        // var resultErrors = await _userManager.DeleteAsync(user);

        // return BadRequest("Kullanici oluşturulurken hata oluştu. "+ result.ToString()+ " delete error: "+ resultErrors.ToString());

        [HttpPost("create")]
        public async Task<IActionResult> CreatePersonal()
        {
            await Task.Delay(10);
            return Ok("Personal oluşturuldu");
        }

        [HttpGet("getuser")]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest("Kullanici bulunamadi.");
            }
            var userRole = await _userManager.GetRolesAsync(user);
            if (userRole == null)
            {
                return BadRequest("Kullaniciya ait rol bulunamadi.");
            }
            return Ok(user.UserName +'\n'+ user.Email+'\n'+ userRole[0]);
        }

        [HttpPut("updatepersonal")]
        public async Task<IActionResult> UpdatePersonal()
        {
            await Task.Delay(10);
            return Ok("Personal bilgileri");
        }
    }
}