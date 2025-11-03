using Alification.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

//this controller is for "first time" use
[Controller]
[Route("forst")]
public class FirstController:Controller
{
    private readonly TelegramAuthService _auth;
    private readonly MyContext _context;


    public FirstController(TelegramAuthService auth,MyContext context)
    {
        _auth = auth;
        _context = context;
    }
    
    

    //this method is for entrypage, it checks if user is already logined or not, to skip registration part and auth part
    [HttpPost("etry_page")]
    public async Task<IActionResult> FirstMenu([FromBody] Dictionary<string, string> data)
    {
        
        if (!_auth.Validate(data))
            return Unauthorized("Invalid Telegram data");
        if (!data.TryGetValue("id", out var idStr) || !long.TryParse(idStr, out var telegramId)) 
            return BadRequest("Missing Telegram user ID");
        
        var exist = await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        if (exist != null)
        {
            //if user already exist in db then skip this controller and redirect to homepage
        }
        
        return Ok();

    }
    
    
    //this for Create company function
    [HttpPost("create")]
    public async Task<IActionResult> CreteCompany([FromBody] Dictionary<string, string> data)
    {
        if (!_auth.Validate(data))
            return Unauthorized("Invalid Telegram data");
        // Parse user fields
        if (!data.TryGetValue("id", out var idStr) || !long.TryParse(idStr, out var telegramId))
            return BadRequest("Missing Telegram user ID");
        
        //all this should be checked for null;
        data.TryGetValue("username", out var username);
        data.TryGetValue("companyName", out var companyName);
        data.TryGetValue("password", out var password);
        //hashpassword method to hash password
        var hash = password;
        data.TryGetValue("workingStart", out var workingStart);
        DateTime.TryParse(workingStart, out var ws);
        data.TryGetValue("workingEnd", out var workingEnd);
        DateTime.TryParse(workingEnd, out var we);

        var comId = new Guid();
        var newconmany = new Company()
        {
            Id = comId,
            Name = companyName,
            PasswordHash = hash,
            WorkingStart = ws,
            WorkingEnd = we
        };
        var newuser = new User()
        {
            Id = new Guid(),
            TelegramId = telegramId,
            UserName = username,
            CompanyId = comId,
            Role = Role.Admin
        };
        _context.Users.Add(newuser);
        _context.Companies.Add(newconmany);
        _context.SaveChanges();
        
        //redirect to homepage
        return Ok();
    }

    
    //this for enter company 
    [HttpPost("enter")]
    public async Task<IActionResult> EnterCompany([FromBody] Dictionary<string, string> data)
    {
        if (!_auth.Validate(data))
            return Unauthorized("Invalid Telegram data");
        // Parse user fields
        if (!data.TryGetValue("id", out var idStr) || !long.TryParse(idStr, out var telegramId))
            return BadRequest("Missing Telegram user ID");
        //all this should be checked for null
        data.TryGetValue("username", out var username);
        data.TryGetValue("companyName", out var companyName);
        data.TryGetValue("password", out var password);
        //hashpassword method to check password

        var com = _context.Companies.FirstOrDefault(c => c.Name == companyName);
        //check companyinputs
        
        var newuser = new User()
        {
            Id = new Guid(),
            TelegramId = telegramId,
            UserName = username,
            CompanyId = com.Id,
            Role = Role.User
        };
        _context.Users.Add(newuser);
        _context.SaveChanges();
        
        //redirect to homepage
        return Ok();
    }
}

