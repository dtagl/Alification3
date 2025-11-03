using Alification.Data.Entities;
using Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Controller]
[Route("room")]
public class RoomController:Controller
{
    private readonly TelegramAuthService _auth;
    private readonly MyContext _context;


    public RoomController(TelegramAuthService auth,MyContext context)
    {
        _auth = auth;
        _context = context;
    }
    public async Task<Dictionary<DateTime,bool>> GetTimespans([FromBody]Guid RoomId)
    {
        var room = _context.Rooms.FirstOrDefault(r => r.Id == RoomId);
        var company = _context.Companies.Where(c => c.Id == room.CompanyId).FirstOrDefault();

        var bookings = _context.Bookings;
        
        var a = company.WorkingStart;
        var b = company.WorkingEnd;

        var workinghours = b - a;

        var list = new Dictionary<DateTime, bool>();
        for (DateTime i = a; i < b; a += TimeSpan.FromMinutes(15))
        {
            //there should be method to check if time is not in bookings if no then false if booked then true
            list.Add(i,false);
        }
        return list;
    }

    public async Task GetEmpty([FromBody] Guid RoomId,[FromBody] Guid UserId, [FromBody] DateTime time)
    {
        var booking = new Booking()
        {
            Id =new Guid(),
            RoomId = RoomId,
            UserId = UserId,
            StartAt = time, 
            EndAt = time+TimeSpan.FromMinutes(15)
        };
        _context.Bookings.Add(booking);
        _context.SaveChanges();
    }

    public class GetBookedDto
    {
        public string UserName { get; set; }
        public long TelegramId { get; set; }
    }
    public async Task<GetBookedDto> GetBooked([FromBody] DateTime time)
    {
        var book = _context.Bookings.FirstOrDefault(b => b.StartAt == time);
        var user = _context.Users.FirstOrDefault(u => u.Id == book.UserId);
        return new GetBookedDto()
        {
            UserName = user.UserName,
            TelegramId = user.TelegramId
        };
    }

    public async Task<IActionResult> DeleteBook([FromBody] Guid RoomId,[FromBody] Guid UserId, [FromBody] DateTime time)
    {
        var book = _context.Bookings.FirstOrDefault(b => b.UserId == UserId && b.RoomId == RoomId && b.StartAt == time);

        _context.Bookings.Remove(book);
        _context.SaveChanges();
        return Ok(); 
    }
    
    
    

    
    
}