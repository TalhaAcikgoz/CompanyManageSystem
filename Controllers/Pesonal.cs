using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyIdentityApp.Data;
using Microsoft.EntityFrameworkCore;


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

    private string GenerateRandomPassword()
{
    var options = new PasswordOptions
    {
        RequiredLength = 8,
        RequiredUniqueChars = 1,
        RequireDigit = true,
        RequireLowercase = true,
        RequireNonAlphanumeric = true,
        RequireUppercase = true,
    };

    string[] randomChars = new[] {
        "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // Uppercase 
        "abcdefghijkmnopqrstuvwxyz",    // Lowercase
        "0123456789",                   // Digits
        "!@$?_-"                        // Non-alphanumeric
    };

    Random rand = new Random(Environment.TickCount);
    List<char> chars = new List<char>();

    if (options.RequireUppercase)
        chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);

    if (options.RequireLowercase)
        chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);

    if (options.RequireDigit)
        chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);

    if (options.RequireNonAlphanumeric)
        chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);

    for (int i = chars.Count; i < options.RequiredLength || chars.Distinct().Count() < options.RequiredUniqueChars; i++)
    {
        string rcs = randomChars[rand.Next(0, randomChars.Length)];
        chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
    }

    return new string(chars.ToArray());
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
        var password = GenerateRandomPassword();
        var user = new ApplicationUser
        {
            UserName = model.username,
            Email = model.email,
            CompanyName = model.CompanyName,
            FirstName = model.FirstName,
            LastName = model.LastName,
            BirthDate = model.BirthDate,
            Department = model.Department,
            maxLeaveDays = model.maxLeaveDays
        };
        Console.WriteLine("Random pass: " + password);
        model.password = password;
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync(model.role))
            {
                var roleError = await _userManager.DeleteAsync(user);
                return BadRequest(new { message = "Rol bulunamadi." });
            }
            await _userManager.AddToRoleAsync(user, model.role);
            var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
            await emailService.SendEmailAsync(model.email, "Your account password", $"Your userName is:{model.username}\nYour password is: {password}");

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

public async Task<string> GetManagerEmailAsync(string username)
{
    var user = await _userManager.FindByNameAsync(username); // Kullanıcıyı asenkron olarak al
    if (user == null)
    {
        return null; // Kullanıcı bulunamazsa null dön
    }
    
    var company = user.CompanyName;
    Console.WriteLine("Company: " + company);
    
    var usersInCompany = await _userManager.Users
        .Where(u => u.CompanyName == company)
        .ToListAsync();
    
    var managerEmail = ""; // Manager'ın e-posta adresini saklamak için değişken
    foreach (var u in usersInCompany)
    {
        if (await _userManager.IsInRoleAsync(u, "Manager"))
        {
            managerEmail = u.Email;
            break; // Bir tane manager bulduktan sonra döngüden çık
        }
    }
    if (managerEmail == "")
    {
        Console.WriteLine("Manager not found in getfunct.");
        return null; // Manager bulunamazsa null dön
    }
    Console.WriteLine("Manager email: " + managerEmail);
    return managerEmail; // E-posta adresini döndür
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
                    Console.WriteLine("User found: " + model.password);
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
                        if (model.password != null)
                        {   
                            Console.WriteLine("Password reset" + targetUser.UserName + " " + model.password );
                            var token = await _userManager.GeneratePasswordResetTokenAsync(targetUser);
                            var result1 = await _userManager.ResetPasswordAsync(targetUser, token, model.password);
                            if (!result1.Succeeded)
                            {
                                var errors = string.Join(", ", result1.Errors.Select(e => e.Description));
                                Console.WriteLine("Password reset errors: " + errors);
                                return BadRequest(new {message = "Şifre sıfırlama hatası: " + errors});
                            }
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

    if (user.maxLeaveDays < (leavePeriod.EndDate - leavePeriod.StartDate).Days)
    {
        return BadRequest(new { message = "Kullanıcının izin hakkı yetersiz." });
    }
    if (leavePeriod.StartDate > leavePeriod.EndDate)
    {
        return BadRequest(new { message = "Başlangıç tarihi bitiş tarihinden büyük olamaz." });
    }

    _context.LeavePeriods.Add(leavePeriod);
    await _context.SaveChangesAsync();
    if (user.LeavePeriods == null)
    {
        Console.WriteLine("Leave periods is null");
    }
    var managerEmail = await GetManagerEmailAsync(username);
    if (managerEmail != null)
    {
        Console.WriteLine("Manager email: " + managerEmail);
    }
    else
    {
        Console.WriteLine("Manager email not found.");
    }
    var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
    await emailService.SendEmailAsync(managerEmail, "Izin istegi", $"The employee named - {user.UserName} - wants to take leave between {leavePeriod.StartDate} - {leavePeriod.EndDate}.\nReason: {leavePeriod.Reason}\nPlease approve or reject the leave request.\n\nBest regards,\nHR Department");

    return Ok(new { message = "İzin başarıyla eklendi." });
}


[HttpPut("approveleave")]
public async Task<IActionResult> ApproveLeave(int leaveId)
{
    var leavePeriod = await _context.LeavePeriods.FindAsync(leaveId);
    if (leavePeriod == null)
    {
        return NotFound(new { message = "İzin bulunamadı." });
    }
    var user = await _userManager.FindByIdAsync(leavePeriod.ApplicationUserId);
    if (user == null)
    {
        return NotFound(new { message = "Kullanıcı bulunamadı." });
    }
    Console.WriteLine("leavedays: ", (leavePeriod.EndDate - leavePeriod.StartDate).Days);
    user.maxLeaveDays -= (leavePeriod.EndDate - leavePeriod.StartDate).Days + 1;
    _context.Users.Update(user);

    leavePeriod.IsApproved = true;
    await _context.SaveChangesAsync();
    var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
    await emailService.SendEmailAsync(user.Email, "Izin istegi Onaylandi", $"Approved the director's request for leave from {leavePeriod.StartDate} - {leavePeriod.EndDate}.\nReason: {leavePeriod.Reason}\n Best regards,\nHR Department");


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
                    lp.ApplicationUser.UserName,
                    lp.ApplicationUser.maxLeaveDays
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
                                   lp.ApplicationUser.UserName,
                                   lp.ApplicationUser.maxLeaveDays
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
        if (leavePeriod.IsApproved)
        {
            return BadRequest(new { message = "Onaylanmış izin iptal edilemez." });
        }

        _context.LeavePeriods.Remove(leavePeriod);

        var result = await _context.SaveChangesAsync();
        if (result <= 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "İzin iptal edilirken bir hata oluştu." });
        }

        var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
        await emailService.SendEmailAsync(user.Email, "Izin istegi Reddedildi", $"Your leave request from {leavePeriod.StartDate} - {leavePeriod.EndDate} is canceled.\nLeave Reason: {leavePeriod.Reason}\n Best regards,\nHR Department");

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
    var managerEmail = await GetManagerEmailAsync(user.UserName);
    if (managerEmail != null)
    {
        Console.WriteLine("Manager email: " + managerEmail);
    }
    else
    {
        Console.WriteLine("Manager email not found.");
    }
    var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
    await emailService.SendEmailAsync(managerEmail, "Expense Request", $"The employee - {user.UserName} - made an expense entry worth {cost.Amount} TL for {cost.Reason}.\n\nPlease approve or reject the expense entry.\n\nBest regards,\nHR Department");

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
    var user = await _userManager.FindByIdAsync(cost.ApplicationUserId);
    if (user == null)
    {
        return NotFound(new { message = "Kullanıcı bulunamadı." });
    }

    cost.IsApproved = true;
    await _context.SaveChangesAsync();

    var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
    await emailService.SendEmailAsync(user.Email, "Your expense request has been approved", $"Approved the director's request for expense {cost.Amount} TL.\nExpense Reason: {cost.Reason}\n\nBest regards,\nHR Department");


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

    var user = await _userManager.FindByIdAsync(cost.ApplicationUserId);
    if (user == null)
    {
        return NotFound(new { message = "Kullanıcı bulunamadı." });
    }

    var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
    await emailService.SendEmailAsync(user.Email, "Your expense claim has been rejected", $"Your expense request {cost.Amount} TL is rejected.\nExpense Reason: {cost.Reason}\n\nBest regards,\nHR Department");

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

    var logFilePath = "birthday-check.log";
    var todays = DateTime.Today;
    if (System.IO.File.Exists(logFilePath))
    {
        var logContent = await System.IO.File.ReadAllTextAsync(logFilePath);
            if (DateTime.TryParse(logContent, out var lastRunDate))
            {
                if (lastRunDate.Date == today)
                {
                    // Eğer log dosyasında bugün tarihi varsa, işlem yapılmış demektir
                    Console.WriteLine("Bugün işlem yapıldı.");
                    return Ok(upcomingBirthdays);

                }
            }
    }
    if (upcomingBirthdays.Count > 0)
    {
        var allUsersEmails = usersInCompany.Select(u => u.Email).ToList();
        var managerEmails = new List<string>();
        foreach (var user in usersInCompany)
        {
            if (await _userManager.IsInRoleAsync(user, "Manager"))
            {
                managerEmails.Add(user.Email);
            }
        }
        var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
        foreach (var birthday in upcomingBirthdays)
        {
            var subject = $"Upcoming Birthday Reminder: {birthday.UserName}";
            var body = $"The birthday of the employee named {birthday.UserName} is approaching. The birthday will be on {birthday.BirthDate}. There are {birthday.DaysUntilBirthday} days left until the birthday.";
            foreach (var email in allUsersEmails)
            {
                if (birthday.DaysUntilBirthday < 7)
                {
                    await emailService.SendEmailAsync(email, subject, body);
                }
            }
        }
        await System.IO.File.WriteAllTextAsync(logFilePath, today.ToString("yyyy-MM-dd"));
    }

    return Ok(upcomingBirthdays);
}

    }    
}