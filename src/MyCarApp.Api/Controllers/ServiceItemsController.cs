using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MyCarApp.Api.Data;
using MyCarApp.Api.Models;

namespace MyCarApp.Api.Controllers;

[ApiController]
[Route("api/vehicles/{vehicleId}/serviceitems")]
[Authorize]
public class ServiceItemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServiceItemsController(AppDbContext db)
    {
        _db = db;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    private async Task<Vehicle?> GetUserVehicle(int vehicleId) =>
        await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == GetUserId());

    [HttpGet]
    public async Task<IActionResult> GetAll(int vehicleId)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var items = await _db.ServiceItems
            .Where(s => s.VehicleId == vehicleId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int vehicleId, [FromBody] ServiceItemDto dto)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var item = new ServiceItem
        {
            VehicleId = vehicleId,
            Name = dto.Name,
            LastServiceDate = dto.LastServiceDate,
            LastServiceKm = dto.LastServiceKm,
            IntervalKm = dto.IntervalKm,
            IntervalMonths = dto.IntervalMonths,
            Notes = dto.Notes
        };

        _db.ServiceItems.Add(item);
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int vehicleId, int id, [FromBody] ServiceItemDto dto)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var item = await _db.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == id && s.VehicleId == vehicleId);
        if (item == null) return NotFound();

        item.Name = dto.Name;
        item.LastServiceDate = dto.LastServiceDate;
        item.LastServiceKm = dto.LastServiceKm;
        item.IntervalKm = dto.IntervalKm;
        item.IntervalMonths = dto.IntervalMonths;
        item.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int vehicleId, int id)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var item = await _db.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == id && s.VehicleId == vehicleId);
        if (item == null) return NotFound();

        _db.ServiceItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record ServiceItemDto(
    string Name,
    DateTime? LastServiceDate,
    decimal? LastServiceKm,
    int? IntervalKm,
    int? IntervalMonths,
    string? Notes
);