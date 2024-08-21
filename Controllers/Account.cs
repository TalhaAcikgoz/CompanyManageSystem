using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MyIdentityApp.Data;

namespace MyIdentityApp.Controllers;

public class CreateUserModel
{
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

public class LoginModel
{
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

[Route("api/[controller]")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;//auth
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

    // Kullanıcı oluşturma
    [HttpPost("create")]
    //public async Task<IActionResult> CreateUser(string username, string email, string password, string role)
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
    {
        var user = new ApplicationUser { UserName = model.username, Email = model.email, CompanyName = model.CompanyName};
        var result = await _userManager.CreateAsync(user, model.password);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync(model.role))
            {
                var roleeror = await _userManager.DeleteAsync(user);
                return BadRequest("Role bulunamadi."+ " delete error: "+ roleeror.ToString());
            }
            await _userManager.AddToRoleAsync(user, model.role);
            return Ok("Kullanici basariyla oluşturuldu ve rol atandi.");
        }
        var resultErrors = await _userManager.DeleteAsync(user);

        return BadRequest("Kullanici oluşturulurken hata oluştu. "+ result.ToString()+ " delete error: "+ resultErrors.ToString());
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.username, model.password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Giriş başarılı." });
            }
            return BadRequest(new { Message = "Giriş başarısız." });
        }
        return BadRequest(new { Message = "Geçersiz giriş bilgileri." });
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "Çıkış başarılı." });
    }

}