using Alification.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class MyContext:DbContext
{
    public MyContext(DbContextOptions<MyContext>options):base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Room> Rooms { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}