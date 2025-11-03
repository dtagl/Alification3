using Alification.Data.Entities;
using Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


//this controller is for booking(main) function
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
    
    
    
    //this is for getting list of time slots in that company in that room, should show only future slots(not to show yesterday in list)
    /*
     list will look like:
     
     27-june
     28-june
     30-june
     ...
     
     and if user chooses date then there will be shown this timeslots:
     1-room
     27-june
     9:00/9:15/9:30/9:45
     -------------------
     10:00/10:15/10:30/10:45
     -------------------
     ...
     */
    
    [HttpGet("room/{RoomId}")]
    public async Task<Dictionary<DateTime,bool>> GetTimespans(Guid @RoomId,[FromBody] DateTime date)
    {
        //shoud be updated and corected+added datecheck
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

    //this is for register booking
    [HttpGet("register")]
    public async Task RegisterBooking([FromBody] Guid RoomId,[FromBody] Guid UserId, [FromBody] DateTime time)
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

    //this is for showing info of users in booked timespans
    public class GetBookedDto
    {
        public string UserName { get; set; }
        public long TelegramId { get; set; }
    }
    [HttpGet("booked")]
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

    
    //this is for deleting booking
    [HttpDelete("delete_booking")]
    public async Task<IActionResult> DeleteBook([FromBody] Guid RoomId,[FromBody] Guid UserId, [FromBody] DateTime time)
    {
        var book = _context.Bookings.FirstOrDefault(b => b.UserId == UserId && b.RoomId == RoomId && b.StartAt == time);

        _context.Bookings.Remove(book);
        _context.SaveChanges();
        return Ok(); 
    }
    
    
    

    
    
}