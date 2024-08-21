using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyIdentityApp.Data;
using Microsoft.EntityFrameworkCore;


public class CreateUserModel
{
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}
// TODO: dogum tarihlerii ekle
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

    [HttpPost("addpersonel")]
    public async Task<IActionResult> CreatePersonal([FromBody] CreateUserModel model)
    {
        var manager = await _userManager.FindByNameAsync(User.Identity.Name);
        if (manager == null)
        {
            return Unauthorized(new { message = "his not auth" });
        }

        if (manager.CompanyName != model.CompanyName)
        {
            return BadRequest(new { message = "wrong company name" });
        }

        var user = new ApplicationUser
        {
            UserName = model.username,
            Email = model.email,
            CompanyName = model.CompanyName,
        };

        var result = await _userManager.CreateAsync(user, model.password);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync(model.role))
            {
                var roleError = await _userManager.DeleteAsync(user);
                return BadRequest(new { message = "Rol bulunamadi." });
            }
            await _userManager.AddToRoleAsync(user, model.role);
            return Ok(new { message = " User creation succssess " });
        }

        // Hata mesajlarını toplamak için
        var errorMessages = result.Errors.Select(e => e.Description).ToArray();
        return BadRequest(new { message = "user creation fail", errors = errorMessages });
    }

        [HttpGet("listpersonel")]
    public async Task<IActionResult> ListPersonel()
    {
        // Giriş yapmış olan şirket yöneticisini alalım
        var manager = await _userManager.FindByNameAsync(User.Identity.Name);
        if (manager == null)
        {
            return Unauthorized(new { message = "Oturum açmamış." });
        }

        // İlk olarak şirket adına göre filtreleme yapalım
        var usersInCompany = await _userManager.Users
            .Where(u => u.CompanyName == manager.CompanyName)
            .ToListAsync();
        Console.WriteLine("86 Personal.cs :" + usersInCompany.Count);

        // Şirket personelini listelemek için bir liste oluşturalım
        var personelList = new List<object>();

        // Filtrelenmiş kullanıcıları döngüyle kontrol edelim
        foreach (var user in usersInCompany)
        {
            if (await _userManager.IsInRoleAsync(user, "Personal"))
            {
                personelList.Add(new
                {
                    user.UserName,
                    user.Email
                });
            }
        }
        Console.WriteLine("104 Personal.cs :" + personelList.Count);

        return Ok(personelList);
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
        public async Task<IActionResult> UpdatePersonal() // burayida yaz
        {
            await Task.Delay(10);
            return Ok("Personal bilgileri");
        }
    }


    
    
}