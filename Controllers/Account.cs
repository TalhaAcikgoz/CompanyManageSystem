using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MyIdentityApp.Data;
using Microsoft.EntityFrameworkCore;


namespace MyIdentityApp.Controllers;

public class CreateUserModel
{
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Department { get; set; } = string.Empty;
    public List<DateTime> LeaveDay { get; set; } = new List<DateTime>();
    public Dictionary<string, string> CVInfos { get; set; } = new Dictionary<string, string>();
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
        var existingUser = await _userManager.FindByNameAsync(model.username);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already has ben taken " });
        }
        var user = new ApplicationUser { UserName = model.username, 
            Email = model.email, 
            CompanyName = model.CompanyName,
            FirstName = model.FirstName,
            LastName = model.LastName,
            BirthDate = model.BirthDate,
            //CVInfos = model.CVInfos.Select(kv => new CVInfo { Key = kv.Key, Value = kv.Value }).ToList()
        };
        var result = await _userManager.CreateAsync(user, model.password);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync(model.role))
            {
                var roleeror = await _userManager.DeleteAsync(user);
                return BadRequest(new { message = "Role bulunamadi."+ " delete error: "+ roleeror.ToString() });
            }
            await _userManager.AddToRoleAsync(user, model.role);
            return Ok("Kullanici basariyla oluşturuldu ve rol atandi.");
        }
        var resultErrors = await _userManager.DeleteAsync(user);

        return BadRequest(new { message = "Kullanici oluşturulurken hata oluştu. "+ result.ToString()+ " delete error: "+ resultErrors.ToString() });
    }

/*     [HttpGet("getuser")]
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
    } */

    [HttpGet("getuser")]
    public async Task<IActionResult> GetUser()
    {
        
        if (string.IsNullOrEmpty(User.Identity.Name))
        {
            return Unauthorized(new { message = "Oturum açmamış." });
        }
        var username = User.Identity.Name;
        //Console.WriteLine(username);
        if (username == null)
        {
            return Unauthorized("Kullanıcı oturum açmamış.");
        }

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return BadRequest("Kullanıcı bulunamadı.");
        }

        var userRole = await _userManager.GetRolesAsync(user);
        if (userRole == null || !userRole.Any())
        {
            return BadRequest("Kullanıcıya ait rol bulunamadı.");
        }

        return Ok(new {
            Username = user.UserName,
            Email = user.Email,
            Role = userRole[0],
            CompanyName = user.CompanyName
        });
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


    [HttpGet("listpersonel")]
    public async Task<IActionResult> ListPersonel()
    {
        // Giriş yapmış olan şirket yöneticisini alalım
        if (string.IsNullOrEmpty(User.Identity.Name))
        {
            return Unauthorized(new { message = "Oturum açmamış." });
        }
        var manager = await _userManager.FindByNameAsync(User.Identity.Name);
        if (manager == null)
        {
            return Unauthorized(new { message = "Oturum açmamış." });
        }

        // İlk olarak şirket adına göre filtreleme yapalım
        var usersInCompany = await _userManager.Users
            .Where(u => u.CompanyName == manager.CompanyName)
            .ToListAsync();

        // Şirket personelini listelemek için bir liste oluşturalım
        var personelList = new List<object>();

        // Filtrelenmiş kullanıcıları döngüyle kontrol edelim
        foreach (var user in usersInCompany)
        {
            if (await _userManager.IsInRoleAsync(user, "Personel"))
            {
                personelList.Add(new
                {
                    user.UserName,
                    user.Email
                });
            }
        }

        return Ok(personelList);
    }


}