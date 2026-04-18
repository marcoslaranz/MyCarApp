using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MyCarApp.Api.Data;
using MyCarApp.Api.Models;

namespace MyCarApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly AppDbContext _db;

    public VehiclesController(AppDbContext db)
    {
        _db = db;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.UserId == GetUserId())
            .ToListAsync();
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == GetUserId());

        if (vehicle == null) return NotFound();
        return Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleDto dto)
    {
        var vehicle = new Vehicle
        {
            UserId = GetUserId(),
            Name = dto.Name,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            LicensePlate = dto.LicensePlate
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] VehicleDto dto)
    {
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == GetUserId());

        if (vehicle == null) return NotFound();

        vehicle.Name = dto.Name;
        vehicle.Make = dto.Make;
        vehicle.Model = dto.Model;
        vehicle.Year = dto.Year;
        vehicle.LicensePlate = dto.LicensePlate;

        await _db.SaveChangesAsync();
        return Ok(vehicle);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == GetUserId());

        if (vehicle == null) return NotFound();

        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record VehicleDto(string Name, string Make, string Model, int Year, string LicensePlate);