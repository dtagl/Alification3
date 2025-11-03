

namespace Alification.Data.Entities;

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public string Description { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    // 96-slot availability is computed per date, not stored in DB
    public ICollection<Booking> Bookings { get; set; }
}