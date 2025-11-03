// File: Controllers/AdminController.cs
using Api.Data;
using Api.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly MyContext _context;
    public AdminController(MyContext context) => _context = context;

    private async Task<bool> IsAdmin(Guid requesterId)
    {
        var u = await _context.Users.FindAsync(requesterId);
        return u != null && u.Role == Role.Admin;
    }

    // ---------------------------
    // ✅ Статистика компании
    // ---------------------------
    [HttpGet("stats/{companyId:guid}")]
    public async Task<IActionResult> GetStats(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var company = await _context.Companies.FindAsync(companyId);
        if (company == null) return NotFound("Company not found.");

        var now = DateTime.UtcNow;
        var totalRooms = await _context.Rooms.CountAsync(r => r.CompanyId == companyId);
        var totalUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId);
        var totalBookings = await _context.Bookings.Include(b => b.Room)
            .CountAsync(b => b.Room.CompanyId == companyId);
        var activeBookings = await _context.Bookings.Include(b => b.Room)
            .CountAsync(b => b.Room.CompanyId == companyId && b.EndAt > now);

        return Ok(new
        {
            company.Name,
            totalRooms,
            totalUsers,
            totalBookings,
            activeBookings
        });
    }

    // ---------------------------
    // ✅ CRUD: Комнаты
    // ---------------------------
    [HttpPost("room")]
    public async Task<IActionResult> CreateRoom([FromBody] AdminCreateRoomDto dto)
    {
        if (!await IsAdmin(dto.RequesterId)) return Forbid();

        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Description = dto.Description,
            CompanyId = dto.CompanyId
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return Ok(room);
    }

    [HttpPut("room/{roomId:guid}")]
    public async Task<IActionResult> UpdateRoom(Guid roomId, [FromBody] AdminUpdateRoomDto dto)
    {
        if (!await IsAdmin(dto.RequesterId)) return Forbid();

        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null) return NotFound();

        room.Name = dto.Name ?? room.Name;
        room.Capacity = dto.Capacity ?? room.Capacity;
        room.Description = dto.Description ?? room.Description;

        await _context.SaveChangesAsync();
        return Ok(room);
    }

    [HttpDelete("room/{roomId:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null) return NotFound();

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("rooms/{companyId:guid}")]
    public async Task<IActionResult> GetRooms(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var rooms = await _context.Rooms.Where(r => r.CompanyId == companyId).ToListAsync();
        return Ok(rooms);
    }

    // ---------------------------
    // ✅ CRUD: Пользователи
    // ---------------------------
    [HttpGet("users/{companyId:guid}")]
    public async Task<IActionResult> GetUsers(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var users = await _context.Users.Where(u => u.CompanyId == companyId)
            .Select(u => new { u.Id, u.UserName, u.TelegramId, u.Role })
            .ToListAsync();
        return Ok(users);
    }

    [HttpDelete("user/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("user/{userId:guid}/role")]
    public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeRoleDto dto)
    {
        if (!await IsAdmin(dto.RequesterId)) return Forbid();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.Role = dto.NewRole;
        await _context.SaveChangesAsync();
        return Ok(new { user.Id, user.UserName, user.Role });
    }

    // ---------------------------
    // ✅ CRUD: Брони
    // ---------------------------
    [HttpGet("bookings/{companyId:guid}")]
    public async Task<IActionResult> GetCompanyBookings(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Where(b => b.Room.CompanyId == companyId)
            .Select(b => new
            {
                b.Id,
                Room = b.Room.Name,
                User = b.User.UserName,
                b.StartAt,
                b.EndAt
            })
            .OrderByDescending(b => b.StartAt)
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpDelete("booking/{bookingId:guid}")]
    public async Task<IActionResult> DeleteBooking(Guid bookingId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return NotFound();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // ---------------------------
    // ✅ Изменить пароль компании
    // ---------------------------
    [HttpPut("company/{companyId:guid}/password")]
    public async Task<IActionResult> ChangePassword(Guid companyId, [FromBody] ChangePasswordDto dto)
    {
        if (!await IsAdmin(dto.RequesterId)) return Forbid();

        var company = await _context.Companies.FindAsync(companyId);
        if (company == null) return NotFound();

        company.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();
        return Ok("Password updated");
    }

    // ---------------------------
    // ✅ Настройка рабочего времени
    // ---------------------------
    [HttpPut("company/{companyId:guid}/workhours")]
    public async Task<IActionResult> SetWorkHours(Guid companyId, [FromBody] WorkHoursDto dto)
    {
        if (!await IsAdmin(dto.RequesterId)) return Forbid();

        var company = await _context.Companies.FindAsync(companyId);
        if (company == null) return NotFound();

        // TODO: можно добавить в Company поля StartHour/EndHour
        return Ok(new { companyId, dto.StartHour, dto.EndHour });
    }
}

// DTOs (уникальные имена, без конфликтов)
public record AdminCreateRoomDto(Guid CompanyId, Guid RequesterId, string Name, int Capacity, string Description);
public record AdminUpdateRoomDto(Guid RequesterId, string? Name, int? Capacity, string? Description);
public record ChangeRoleDto(Guid RequesterId, Role NewRole);
public record ChangePasswordDto(Guid RequesterId, string NewPassword);
public record WorkHoursDto(Guid RequesterId, int StartHour, int EndHour);
