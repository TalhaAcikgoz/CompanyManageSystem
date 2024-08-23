using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyIdentityApp.Data;
using Microsoft.EntityFrameworkCore;



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

    [HttpPost("addpersonel")] // TODO burda kalmistin
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
        DateTime? birthDate = null;
        Console.WriteLine(model.BirthDate);
        var user = new ApplicationUser
        {
            UserName = model.username,
            Email = model.email,
            CompanyName = model.CompanyName,
            FirstName = model.FirstName,
            LastName = model.LastName,
            BirthDate = birthDate,
            Department = model.Department,
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

        return Ok(personelList);
    }

        [HttpGet("getpersonaldetails")]
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
            Console.WriteLine(user.BirthDate);
            return Ok(new {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.UserName,
            BirthDate = user.BirthDate,
            Department = user.Department,
            Email = user.Email,
            Role = userRole[0],
            CompanyName = user.CompanyName,
        });
        }

        [HttpPut("updatepersonal")]
        public async Task<IActionResult> UpdatePersonal() // burayida yaz
        {
            await Task.Delay(10);
            return Ok("Personal bilgileri");
        }

        // Example method to update CV information


        [HttpPut("updatecv")]
        public async Task<IActionResult> UpdateCV([FromBody] Dictionary<string, string> cvData)
        {
            var username = User.Identity?.Name;
            if (username == null)
            {
                return Unauthorized("User not logged in.");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update the CV information
            if (user.CVInfos == null)
            {
                user.CVInfos = new List<CVInfo>();
            }

            // Existing CVInfo'yu güncelle veya yenilerini ekle
            foreach (var entry in cvData)
            {
                var existingCVInfo = user.CVInfos.FirstOrDefault(c => c.Key == entry.Key);
                if (existingCVInfo != null)
                {
                    existingCVInfo.Value = entry.Value; // Mevcut kaydı güncelle
                }
                else
                {
                    user.CVInfos.Add(new CVInfo
                    {
                        Key = entry.Key,
                        Value = entry.Value,
                        ApplicationUserId = user.Id
                    }); // Yeni bir kayıt ekle
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok("CV information updated successfully.");
            }

            return BadRequest("Error updating CV information.");
        }



        

    }    
}