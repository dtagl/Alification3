using Alification.Data.Entities;
using Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Controller]
[Route("homepage")]
public class HomepageController:Controller
{
    
    private readonly TelegramAuthService _auth;
    private readonly MyContext _context;


    public HomepageController(TelegramAuthService auth,MyContext context)
    {
        _auth = auth;
        _context = context;
    }
    
    [HttpGet("homepage")]
    public async Task<List<Room>> Homepage([FromBody] long userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == userId);
        var company = _context.Companies.FirstOrDefault(c => c.Id == user.CompanyId);
        List<Room> rooms = _context.Rooms.Where(r=>r.CompanyId==company.Id).ToList();
        return rooms;
    }

    public async Task<List<Booking>> MyBookings([FromBody]long userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == userId);
        var bookings = user.Bookings.ToList();
        return bookings;
    }

    public async Task<List<Room>> FreeNow()
    {
        var now = DateTime.Now;
        List<Room> list=new List<Room>();
        //there should be logic that gives back rooms that are available now
        return list;
    }

    //this controller should return list of available for admin methods
    public async Task<List<string>> AdminFunctions([FromBody]long userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == userId);
        if (user.Role != Role.Admin) return new List<string>(){"Only for admins"};
        
        return null;
    }
}