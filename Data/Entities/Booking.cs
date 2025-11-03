namespace Alification.Data.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid RoomId { get; set; }
    public Room Room { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int TimespanId { get; set; } // starts with 0, 0-95 for 15-min slots
}