using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MyCarApp.Api.Data;
using MyCarApp.Api.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace MyCarApp.Api.Controllers;

[ApiController]
[Route("api/vehicles/{vehicleId}/servicelogs")]
[Authorize]
public class ServiceLogsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly Cloudinary _cloudinary;

    public ServiceLogsController(AppDbContext db, Cloudinary cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
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

        var logs = await _db.ServiceLogs
            .Include(l => l.ServiceLogItems)
            .Include(l => l.ServiceDocuments)
            .Where(l => l.VehicleId == vehicleId)
            .OrderByDescending(l => l.ServiceDate)
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int vehicleId, int id)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = await _db.ServiceLogs
            .Include(l => l.ServiceLogItems)
            .Include(l => l.ServiceDocuments)
            .FirstOrDefaultAsync(l => l.Id == id && l.VehicleId == vehicleId);

        if (log == null) return NotFound();
        return Ok(log);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int vehicleId, [FromBody] ServiceLogDto dto)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = new ServiceLog
        {
            VehicleId = vehicleId,
            ServiceDate = dto.ServiceDate,
            OdometerKm = dto.OdometerKm,
            Notes = dto.Notes,
            ServiceLogItems = dto.Items.Select(i => new ServiceLogItem
            {
                ServiceItemId = i.ServiceItemId,
                Done = i.Done
            }).ToList()
        };

        _db.ServiceLogs.Add(log);

        // Update LastServiceDate and LastServiceKm for done items
        foreach (var item in dto.Items.Where(i => i.Done))
        {
            var serviceItem = await _db.ServiceItems
                .FirstOrDefaultAsync(s => s.Id == item.ServiceItemId);
            if (serviceItem != null)
            {
                serviceItem.LastServiceDate = dto.ServiceDate;
                serviceItem.LastServiceKm = dto.OdometerKm;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(log);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int vehicleId, int id)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = await _db.ServiceLogs
            .Include(l => l.ServiceDocuments)
            .FirstOrDefaultAsync(l => l.Id == id && l.VehicleId == vehicleId);

        if (log == null) return NotFound();

        // Delete documents from Cloudinary
        foreach (var doc in log.ServiceDocuments)
        {
            var publicId = doc.FileUrl.Split('/').Last().Split('.').First();
            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }

        _db.ServiceLogs.Remove(log);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Upload document to a service log
    [HttpPost("{id}/documents")]
    public async Task<IActionResult> UploadDocument(int vehicleId, int id, IFormFile file)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var log = await _db.ServiceLogs
            .FirstOrDefaultAsync(l => l.Id == id && l.VehicleId == vehicleId);
        if (log == null) return NotFound();

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest("Only JPG, PNG and PDF files are allowed.");

        // Upload to Cloudinary
        using var stream = file.OpenReadStream();
        var uploadParams = file.ContentType == "application/pdf"
            ? (RawUploadParams)new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"mycarapp/vehicle_{vehicleId}/service_{id}"
            }
            : new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"mycarapp/vehicle_{vehicleId}/service_{id}"
            };

        UploadResult uploadResult;
        if (file.ContentType == "application/pdf")
            uploadResult = await _cloudinary.UploadAsync((RawUploadParams)uploadParams);
        else
            uploadResult = await _cloudinary.UploadAsync((ImageUploadParams)uploadParams);

        if (uploadResult.Error != null)
            return BadRequest($"Upload failed: {uploadResult.Error.Message}");

        var document = new ServiceDocument
        {
            ServiceLogId = id,
            FileName = file.FileName,
            FileUrl = uploadResult.SecureUrl.ToString(),
            FileType = file.ContentType,
            UploadedAt = DateTime.Now
        };

        _db.ServiceDocuments.Add(document);
        await _db.SaveChangesAsync();

        return Ok(document);
    }

    // Delete a document
    [HttpDelete("{id}/documents/{docId}")]
    public async Task<IActionResult> DeleteDocument(int vehicleId, int id, int docId)
    {
        var vehicle = await GetUserVehicle(vehicleId);
        if (vehicle == null) return NotFound();

        var doc = await _db.ServiceDocuments
            .FirstOrDefaultAsync(d => d.Id == docId && d.ServiceLogId == id);
        if (doc == null) return NotFound();

        // Delete from Cloudinary
        var publicId = $"mycarapp/vehicle_{vehicleId}/service_{id}/" +
            doc.FileUrl.Split('/').Last().Split('.').First();
        await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        _db.ServiceDocuments.Remove(doc);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record ServiceLogDto(
    DateTime ServiceDate,
    decimal OdometerKm,
    string? Notes,
    List<ServiceLogItemDto> Items
);

public record ServiceLogItemDto(
    int ServiceItemId,
    bool Done
);