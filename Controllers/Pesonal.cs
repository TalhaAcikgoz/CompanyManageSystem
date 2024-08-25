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

[HttpPost("addleave")]
public async Task<IActionResult> AddLeave([FromBody] LeavePeriod leavePeriod, [FromQuery] string username)
{
    var user = await _userManager.FindByNameAsync(username);
    if (user == null)
    {
        return BadRequest(new { message = "Kullanıcı bulunamadı." });
    }

    leavePeriod.ApplicationUserId = user.Id;

    _context.LeavePeriods.Add(leavePeriod);
    await _context.SaveChangesAsync();
    if (user.LeavePeriods == null)
    {
        Console.WriteLine("Leave periods is null");
    }

    return Ok(new { message = "İzin başarıyla eklendi." });
}


[HttpPut("approveleave")]
public async Task<IActionResult> ApproveLeave(int leaveId)
{
    Console.WriteLine("Leave id: " + leaveId);
    var leavePeriod = await _context.LeavePeriods.FindAsync(leaveId);
    if (leavePeriod == null)
    {
        return NotFound(new { message = "İzin bulunamadı." });
    }

    leavePeriod.IsApproved = true;
    await _context.SaveChangesAsync();

    return Ok(new { message = "İzin başarıyla onaylandı." });
}

[HttpGet("getallleaves")]
public async Task<IActionResult> GetAllLeaves()
{
    var manager = await _userManager.FindByNameAsync(User.Identity.Name);
    if (manager == null)
    {
        return Unauthorized(new { message = "Oturum açmamış." });
    }
    var usersInCompany = await _userManager.Users
        .Where(u => u.CompanyName == manager.CompanyName)
        .ToListAsync();
    
    var personelList = new List<object>();
    Console.WriteLine("Users in company: " + usersInCompany.Count);
    foreach (var user in usersInCompany)
    {
        Console.WriteLine("User: " + user.UserName);
        if (await _userManager.IsInRoleAsync(user, "Personal"))
        {
            var leaves = await _context.LeavePeriods
                .Where(lp => lp.ApplicationUser.Id == user.Id)
                .Select(lp => new
                {
                    lp.Id,
                    lp.StartDate,
                    lp.EndDate,
                    lp.Reason,
                    lp.IsApproved,
                    lp.ApplicationUser.UserName
                })
                .ToListAsync();

            if (leaves.Any())
            {
                personelList.Add(new
                {
                    user.UserName,
                    Leaves = leaves
                });
            }
        }

    }
    return Ok(personelList);
}

[HttpGet("getleaves")]
public async Task<IActionResult> GetLeaves(string username)
{
    var user = await _userManager.FindByNameAsync(username);
    if (user == null)
    {
        return NotFound(new { message = "Kullanıcı bulunamadı." });
    }
    
    var leaves = await _context.LeavePeriods
                               .Where(lp => lp.ApplicationUser.Id == user.Id)
                               .Select(lp => new
                               {
                                   lp.StartDate,
                                   lp.EndDate,
                                   lp.Reason,
                                   lp.Id,
                                   lp.IsApproved,
                                   lp.ApplicationUser.UserName
                               })
                               .ToListAsync();

    if (!leaves.Any())
    {
        return Ok(new { message = "İzin bulunamadı." });
    }

    return Ok(leaves);
}

[HttpDelete("cancelleave")]
public async Task<IActionResult> CancelLeave(string username, int leaveId)
{
    try
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı." });
        }

        // LeavePeriods veritabanından sorgulandı
        var leavePeriod = await _context.LeavePeriods
                                        .Where(lp => lp.ApplicationUser.Id == user.Id && lp.Id == leaveId)
                                        .FirstOrDefaultAsync();

        if (leavePeriod == null)
        {
            return NotFound(new { message = "İzin bulunamadı." });
        }

        _context.LeavePeriods.Remove(leavePeriod);

        var result = await _context.SaveChangesAsync();
        if (result <= 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "İzin iptal edilirken bir hata oluştu." });
        }

        return Ok(new { message = "İzin başarıyla iptal edildi." });
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
    }
}


[HttpPost("addcost")]
public async Task<IActionResult> AddCost([FromBody] Cost cost)
{
    var user = await _userManager.FindByNameAsync(cost.Username);
    if (user == null)
    {
        return BadRequest(new { message = "Kullanıcı bulunamadı." });
    }

    cost.ApplicationUserId = user.Id;
    cost.Date = DateTime.Now;

    _context.Costs.Add(cost);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Masraf başarıyla eklendi." });
}

[HttpGet("getallcosts")]
public async Task<IActionResult> GetAllCosts()
{
    var manager = await _userManager.FindByNameAsync(User.Identity.Name);
    if (manager == null)
    {
        return Unauthorized(new { message = "Oturum açmamış." });
    }
    var usersInCompany = await _userManager.Users
        .Where(u => u.CompanyName == manager.CompanyName)
        .ToListAsync();
    
    var personelList = new List<object>();
    Console.WriteLine("Users in company: " + usersInCompany.Count);
    foreach (var user in usersInCompany)
    {
        Console.WriteLine("User: " + user.UserName);
        if (await _userManager.IsInRoleAsync(user, "Personal"))
        {
            var costs = await _context.Costs
                .Where(c => c.ApplicationUser.Id == user.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Reason,
                    c.Amount,
                    c.Date,
                    c.IsApproved,
                    c.ApplicationUser.UserName
                })
                .ToListAsync();

            if (costs.Any())
            {
                personelList.Add(new
                {
                    user.UserName,
                    Costs = costs
                });
            }
        }

    }
    return Ok(personelList);
}

[HttpGet("getcosts")]
public async Task<IActionResult> GetCosts(string username)
{
    var user = await _userManager.FindByNameAsync(username);
    if (user == null)
    {
        return NotFound(new { message = "Kullanıcı bulunamadı." });
    }

    var costs = await _context.Costs
                               .Where(c => c.ApplicationUser.Id == user.Id)
                               .Select(c => new
                               {
                                   c.Id,
                                   c.Reason,
                                   c.Amount,
                                   c.Date,
                                   c.IsApproved,
                                   c.ApplicationUser.UserName
                               })
                               .ToListAsync();

    if (!costs.Any())
    {
        return Ok(new { message = "Masraf bulunamadı." });
    }

    return Ok(costs);
}

[HttpPut("approvecost")]
public async Task<IActionResult> ApproveCost(int costId)
{
    var cost = await _context.Costs.FindAsync(costId);
    if (cost == null)
    {
        return NotFound(new { message = "Masraf bulunamadı." });
    }

    cost.IsApproved = true;
    await _context.SaveChangesAsync();

    return Ok(new { message = "Masraf başarıyla onaylandı." });
}

[HttpDelete("deletecost")]
public async Task<IActionResult> DeleteCost(int costId)
{
    var cost = await _context.Costs.FindAsync(costId);
    if (cost == null)
    {
        return NotFound(new { message = "Masraf bulunamadı." });
    }

    _context.Costs.Remove(cost);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Masraf başarıyla silindi." });
}

[HttpGet("upcoming-birthdays")]
public async Task<IActionResult> GetUpcomingBirthdays()
{
    var manager = await _userManager.FindByNameAsync(User.Identity.Name);
    if (manager == null)
    {
        return Unauthorized(new { message = "Oturum açmamış." });
    }
    var usersInCompany = await _userManager.Users
        .Where(u => u.CompanyName == manager.CompanyName)
        .ToListAsync();
    Console.WriteLine("Users in company: " + usersInCompany.Count);
    var today = DateTime.Today;
    var upcomingBirthdays = usersInCompany
        .Where(u => u.BirthDate.HasValue)
        .Select(u => {
            var birthDateThisYear = new DateTime(today.Year, u.BirthDate.Value.Month, u.BirthDate.Value.Day);
            if (birthDateThisYear < today)
            {
                // Eğer bu yılki doğum günü geçtiyse, bir sonraki yılki doğum gününü hesapla
                birthDateThisYear = birthDateThisYear.AddYears(1);
            }
            var daysUntilBirthday = (birthDateThisYear - today).Days;

            return new
            {
                u.UserName,
                BirthDate = u.BirthDate.Value.ToString("yyyy-MM-dd"),
                DaysUntilBirthday = daysUntilBirthday
            };
        })
        .Where(u => u.DaysUntilBirthday < 30)
        .OrderBy(u => u.DaysUntilBirthday)
        .ToList();



    return Ok(upcomingBirthdays);
}




    }    
}