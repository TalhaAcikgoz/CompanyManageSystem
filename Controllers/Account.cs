using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MyIdentityApp.Data;

namespace MyIdentityApp.Controllers;

// [Route("api/[controller]")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Kullanıcı oluşturma
    // [HttpPost("create")]
    public async Task<IActionResult> CreateUser(string username, string email, string password, string role)
    {
        var user = new ApplicationUser { UserName = username, Email = email};
        Console.WriteLine("User oluşturuldu. " + user.UserName);
        var result = await _userManager.CreateAsync(user, password);
        Console.WriteLine(result.ToString());
        if (result.Succeeded)
        {
            Console.WriteLine("Kullanici oluşturuldu.");
            // Rolün var olup olmadığını kontrol etme
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleeror = await _userManager.DeleteAsync(user);
                Console.WriteLine("Role bulunamadi.");
                return BadRequest("Role bulunamadi."+ " delete error: "+ roleeror.ToString());
            }
            Console.WriteLine("Role exist.");

            // Kullanıcıya rol atama
            await _userManager.AddToRoleAsync(user, role);
            return Ok("Kullanici basariyla oluşturuldu ve rol atandi.");
        }
        var resultErrors = await _userManager.DeleteAsync(user);

        return BadRequest("Kullanici oluşturulurken hata oluştu. "+ result.ToString()+ " delete error: "+ resultErrors.ToString());
    }

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
}