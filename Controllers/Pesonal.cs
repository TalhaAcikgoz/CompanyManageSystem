using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyIdentityApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

public class CVInfo
{
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }

        // Foreign key
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
}
public class GetUserModel
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
}

// TODO: dogum tarihlerii ekle
namespace MyIdentityApp.Controllers{
    [Route("api/[controller]")]
    public class PersonalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;//auth
        private readonly RoleManager<IdentityRole> _roleManager;

        public PersonalController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

    [HttpPost("addpersonel")]
    public async Task<IActionResult> CreatePersonal([FromBody] CreateUserModel model)
    {
        if (string.IsNullOrEmpty(User.Identity.Name))
        {
            return Unauthorized(new { message = "Oturum açmamış." });
        }
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
            FirstName = model.FirstName,
            LastName = model.LastName,
            BirthDate = model.BirthDate,
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
            if (userRole == null || !userRole.Any())
            {
                return BadRequest("Kullaniciya ait rol bulunamadi.");
            }
                // Kullanıcının CV bilgilerini alalım
            var cvInfos = await _context.CVInfos
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();
            var cvDictionary = cvInfos.ToDictionary(c => c.Key, c => c.Value);

            return Ok(new {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.UserName,
            BirthDate = user.BirthDate,
            Department = user.Department,
            Email = user.Email,
            Role = userRole[0],
            CompanyName = user.CompanyName,
            CVInfos = cvDictionary
        });
        }

        [HttpPut("updateprofile")]
        public async Task<IActionResult> UpdateProfile([FromBody] CreateUserModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(User.Identity.Name))
                    {
                        return Unauthorized(new { message = "Oturum açmamış." });
                    }
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return Unauthorized(new {message = "Kullanıcı bulunamadı."});
                }
                // 'Manager' rolündeki kullanıcılar için ek yetki kontrolleri
                if (User.IsInRole("Manager") || User.IsInRole("Personal"))
                {
                    if(User.IsInRole("Personal") && model.username != User.Identity.Name)
                    {
                        return Forbid("Bu profil güncelleme işlemi için yetkiniz yok.");
                    }
                    if (string.IsNullOrEmpty(model.username))
                    {
                        Console.WriteLine("Username is null or empty");
                        return BadRequest(new {message = "Kullanıcı adı sağlanmalıdır."});
                    }

                    var targetUser = await _userManager.FindByNameAsync(model.username);
                    if (targetUser == null || targetUser.CompanyName != user.CompanyName)
                    {
                        Console.WriteLine("User not found or wrong company name" + model.username);
                        return Forbid("Bu profil güncelleme işlemi için yetkiniz yok.");
                    }
                        if (model.FirstName != null)
                        {
                            targetUser.FirstName = model.FirstName;
                        }
                        if (model.LastName != null)
                        {
                            targetUser.LastName = model.LastName;
                        }
                        if (model.BirthDate != null)
                        {
                            targetUser.BirthDate = model.BirthDate;
                        }
                        if (model.Department != null)
                        {
                            targetUser.Department = model.Department;
                        }
                        if (model.email != null)
                        {
                            targetUser.Email = model.email;
                        }

                        var result = await _userManager.UpdateAsync(targetUser);
                        if (result.Succeeded)
                        {
                            Console.WriteLine("Profile updated successfully." + targetUser.UserName);
                            return Ok(new {message = "Profil bilgileri güncellendi."});
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            Console.WriteLine("Update errors: " + errors);
                            return BadRequest(new {message = "Profil bilgileri güncellenemedi: " + errors});
                        }
                        
                }
                return Forbid("Bu profil güncelleme işlemi için yetkiniz yok.");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return StatusCode(500, "Sunucu hatası.");
            }
        }

        [HttpPut("updatecv")]
        public async Task<IActionResult> UpdateCV([FromBody] Dictionary<string, string> cvData)
{
    try
    {
        if (string.IsNullOrEmpty(User.Identity?.Name))
        {
            return Unauthorized(new { message = "Oturum açmamış." });
        }

        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        if (user == null)
        {
            Console.WriteLine("User not found");
            return Unauthorized(new { message = "Kullanıcı bulunamadı." });
        }

        if (User.IsInRole("Manager") || User.IsInRole("Personal"))
        {
            if (User.IsInRole("Personal") && (!cvData.ContainsKey("username") || cvData["username"] != User.Identity.Name))
            {
                return Forbid("Bu profil güncelleme işlemi için yetkiniz yok.");
            }
            if (!cvData.ContainsKey("username") || string.IsNullOrEmpty(cvData["username"]))
            {
                Console.WriteLine("Username is null or empty");
                return BadRequest(new { message = "Kullanıcı adı sağlanmalıdır." });
            }

            var targetUser = await _userManager.FindByNameAsync(cvData["username"]);
            if (targetUser == null || targetUser.CompanyName != user.CompanyName)
            {
                Console.WriteLine("User not found or wrong company name" + cvData["username"]);
                return Forbid("Bu profil güncelleme işlemi için yetkiniz yok.");
            }

            if (cvData.Count > 1)
            {
                foreach (var item in cvData)
                {
                    if (item.Key == "username")
                    {
                        continue;
                    }

                    var cvInfo = await _context.CVInfos
                        .Where(c => c.ApplicationUserId == targetUser.Id && c.Key == item.Key)
                        .FirstOrDefaultAsync();

                    if (cvInfo == null)
                    {
                        cvInfo = new CVInfo
                        {
                            Key = item.Key,
                            Value = item.Value,
                            ApplicationUserId = targetUser.Id
                        };
                        await _context.CVInfos.AddAsync(cvInfo);
                    }
                    else
                    {
                        cvInfo.Value = item.Value;
                        _context.CVInfos.Update(cvInfo);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "CV bilgileri güncellendi." });
            }
        }

        return Forbid("Bu profil güncelleme işlemi için yetkiniz yok.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception: " + ex.Message);
        return StatusCode(500, "Sunucu hatası.");
    }
}


    }    
}