using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCarApp.Api.Data;
using MyCarApp.Api.Models;
using System.Globalization;

namespace MyCarApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly AppDbContext _db;

    public ImportController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/import/{vehicleId}/last
    // Returns info about the last import for the Undo button
    [HttpGet("{vehicleId}/last")]
    public async Task<IActionResult> GetLastImport(int vehicleId)
    {
        var batch = await _db.ImportBatches
            .Where(b => b.VehicleId == vehicleId && b.IsLatest)
            .FirstOrDefaultAsync();

        if (batch == null)
            return NotFound();

        return Ok(new
        {
            batch.Id,
            batch.FileName,
            batch.ImportedAt,
            batch.RowCount
        });
    }

    // POST api/import/{vehicleId}
    // Accepts a CSV file, validates, and imports
    [HttpPost("{vehicleId}")]
    public async Task<IActionResult> Import(int vehicleId, IFormFile file)
    {
        // Verify vehicle belongs to current user
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId);

        if (vehicle == null)
            return NotFound("Vehicle not found.");

        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only CSV files are supported.");

        // Check if this filename has already been imported for this vehicle
        var alreadyImported = await _db.ImportBatches
            .AnyAsync(b => b.VehicleId == vehicleId && b.FileName == file.FileName);

        if (alreadyImported)
            return BadRequest($"The file '{file.FileName}' has already been imported. Please use a different file.");

        // Read and parse the CSV
        List<string[]> rows = new();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            // Skip header line
            var header = await reader.ReadLineAsync();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = ParseCsvLine(line);

                // Validate column count — must be exactly 8 (Date, Time, + 6 fields)
                if (cols.Length != 8)
                    return BadRequest($"Invalid format: expected 8 columns but found {cols.Length}. Please check your file.");

                rows.Add(cols);
            }
        }

        if (rows.Count == 0)
            return BadRequest("The file contains no data rows.");

        // Get existing entries for duplicate detection
        var existingEntries = await _db.LogEntries
            .Where(l => l.VehicleId == vehicleId)
            .Select(l => new { l.DateTime, l.OdometerKm })
            .ToListAsync();

        // Parse rows and check for duplicates
        var newEntries = new List<LogEntry>();
        var duplicates = new List<string>();

        foreach (var cols in rows)
        {
            // Parse date + time from first two columns (matches export format)
            if (!DateTime.TryParseExact(
                    $"{cols[0]} {cols[1]}",
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dateTime))
            {
                return BadRequest($"Invalid date/time format in row: '{cols[0]} {cols[1]}'. Expected dd/MM/yyyy HH:mm.");
            }

            if (!decimal.TryParse(cols[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var odometerKm))
                return BadRequest($"Invalid odometer value: '{cols[2]}'.");

            // Check for duplicate (same DateTime + OdometerKm for this vehicle)
            var isDuplicate = existingEntries
                .Any(e => e.DateTime == dateTime && e.OdometerKm == odometerKm);

            if (isDuplicate)
                duplicates.Add($"{cols[0]} {cols[1]} / {cols[2]} km");

            var fuelLoaded = cols[3].Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase);

            decimal? fuelLiters = decimal.TryParse(cols[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var fl) ? fl : null;
            decimal? fuelPricePerLiter = decimal.TryParse(cols[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var fpl) ? fpl : null;
            decimal? fuelTotalPaid = decimal.TryParse(cols[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var ftp) ? ftp : null;
            var stationName = cols[7].Trim('"', ' ');

            newEntries.Add(new LogEntry
            {
                VehicleId = vehicleId,
                DateTime = dateTime,
                OdometerKm = odometerKm,
                FuelLoaded = fuelLoaded,
                FuelLiters = fuelLiters,
                FuelPricePerLiter = fuelPricePerLiter,
                FuelTotalPaid = fuelTotalPaid,
                PetrolStationName = string.IsNullOrEmpty(stationName) ? null : stationName
            });
        }

        // Reject entire file if any duplicates found
        if (duplicates.Any())
        {
            var dupList = string.Join("; ", duplicates.Take(5));
            var more = duplicates.Count > 5 ? $" ... and {duplicates.Count - 5} more" : "";
            return BadRequest($"Import rejected: {duplicates.Count} duplicate(s) found: {dupList}{more}. Please fix your file and try again.");
        }

        // Mark previous latest batch as no longer latest
        var previousBatch = await _db.ImportBatches
            .Where(b => b.VehicleId == vehicleId && b.IsLatest)
            .FirstOrDefaultAsync();

        if (previousBatch != null)
            previousBatch.IsLatest = false;

        // Create new import batch
        var batch = new ImportBatch
        {
            VehicleId = vehicleId,
            FileName = file.FileName,
            ImportedAt = DateTime.UtcNow,
            RowCount = newEntries.Count,
            IsLatest = true
        };

        _db.ImportBatches.Add(batch);
        await _db.SaveChangesAsync(); // Save to get batch.Id

        // Assign batch ID to all entries and save
        foreach (var entry in newEntries)
            entry.ImportBatchId = batch.Id;

        _db.LogEntries.AddRange(newEntries);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = $"✅ Import complete — {newEntries.Count} records loaded from '{file.FileName}'.",
            rowCount = newEntries.Count,
            batchId = batch.Id
        });
    }

    // DELETE api/import/{vehicleId}/last
    // Removes the last import batch and its log entries
    [HttpDelete("{vehicleId}/last")]
    public async Task<IActionResult> UndoLastImport(int vehicleId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId);

        if (vehicle == null)
            return NotFound("Vehicle not found.");

        var batch = await _db.ImportBatches
            .Where(b => b.VehicleId == vehicleId && b.IsLatest)
            .FirstOrDefaultAsync();

        if (batch == null)
            return NotFound("No import to undo.");

        // Delete all log entries from this batch
        var entries = await _db.LogEntries
            .Where(l => l.ImportBatchId == batch.Id)
            .ToListAsync();

        _db.LogEntries.RemoveRange(entries);
        _db.ImportBatches.Remove(batch);

        // Restore previous batch as latest (if one exists)
        var previousBatch = await _db.ImportBatches
            .Where(b => b.VehicleId == vehicleId)
            .OrderByDescending(b => b.ImportedAt)
            .FirstOrDefaultAsync();

        if (previousBatch != null)
            previousBatch.IsLatest = true;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = $"✅ Import reversed — {entries.Count} records removed from '{batch.FileName}'.",
            rowCount = entries.Count
        });
    }

    // Helper: parse a CSV line respecting quoted fields
    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }
}