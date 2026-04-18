using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MyCarApp.Api.Data;
using MyCarApp.Api.Models;

namespace MyCarApp.Api.Controllers;

[ApiController]
[Route("api/vehicles/{vehicleId}/logs")]
[Authorize]
public class LogEntriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public LogEntriesController(AppDbContext db)
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

        var logs = await _db.LogEntries
            .Where(l => l.VehicleId == vehicleId)
            .OrderByDescending(l => l.DateTime)
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("{logId}")]
    public async Task<IActionResult> GetById(int vehicleId, int logId)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = await _db.LogEntries
            .FirstOrDefaultAsync(l => l.Id == logId && l.VehicleId == vehicleId);

        if (log == null) return NotFound();
        return Ok(log);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int vehicleId, [FromBody] LogEntryDto dto)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = new LogEntry
        {
            VehicleId = vehicleId,
            //DateTime = dto.DateTime,
            DateTime = dto.DateTime.ToUniversalTime(),
            OdometerKm = dto.OdometerKm,
            FuelLoaded = dto.FuelLoaded,
            FuelLiters = dto.FuelLiters,
            FuelPricePerLiter = dto.FuelPricePerLiter,
            FuelTotalPaid = dto.FuelTotalPaid,
            PetrolStationName = dto.PetrolStationName
        };

        _db.LogEntries.Add(log);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { vehicleId, logId = log.Id }, log);
    }

    [HttpPut("{logId}")]
    public async Task<IActionResult> Update(int vehicleId, int logId, [FromBody] LogEntryDto dto)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = await _db.LogEntries
            .FirstOrDefaultAsync(l => l.Id == logId && l.VehicleId == vehicleId);

        if (log == null) return NotFound();

        //log.DateTime = dto.DateTime;
        log.DateTime = dto.DateTime.ToUniversalTime();
        log.OdometerKm = dto.OdometerKm;
        log.FuelLoaded = dto.FuelLoaded;
        log.FuelLiters = dto.FuelLiters;
        log.FuelPricePerLiter = dto.FuelPricePerLiter;
        log.FuelTotalPaid = dto.FuelTotalPaid;
        log.PetrolStationName = dto.PetrolStationName;

        await _db.SaveChangesAsync();
        return Ok(log);
    }

    [HttpDelete("{logId}")]
    public async Task<IActionResult> Delete(int vehicleId, int logId)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = await _db.LogEntries
            .FirstOrDefaultAsync(l => l.Id == logId && l.VehicleId == vehicleId);

        if (log == null) return NotFound();

        _db.LogEntries.Remove(log);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record LogEntryDto(
    DateTime DateTime,
    decimal OdometerKm,
    bool FuelLoaded,
    decimal? FuelLiters,
    decimal? FuelPricePerLiter,
    decimal? FuelTotalPaid,
    string? PetrolStationName
);