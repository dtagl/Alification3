using Alification.Data.Entities;
using Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


//this controller is for homepage
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
    
    //this is for giving list of rooms in company
    [HttpGet("homepage")]
    public async Task<List<Room>> Homepage([FromBody] long userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == userId);
        var company = _context.Companies.FirstOrDefault(c => c.Id == user.CompanyId);
        List<Room> rooms = _context.Rooms.Where(r=>r.CompanyId==company.Id).ToList();
        return rooms;
    }

    //this is for seeing my booking for all time
    [HttpGet]
    public async Task<List<Booking>> MyBookings([FromBody]long userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == userId);
        var bookings = user.Bookings.ToList();
        return bookings;
    }

    //this is for showing rooms that are now available
    [HttpGet("now_available")]
    public async Task<List<Room>> FreeNow()
    {
        var now = DateTime.Now;
        List<Room> list=new List<Room>();
        //there should be logic that gives back rooms that are available now
        return list;
    }

    
    //this is for showing admin functions
    [HttpGet("admin_functions")]
    //this controller should return list of available for admin methods
    public async Task<List<string>> AdminFunctions([FromBody]long userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == userId);
        if (user.Role != Role.Admin) return new List<string>(){"Only for admins"};
        
        return null;
    }
}