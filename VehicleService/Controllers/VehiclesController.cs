using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VehicleService.Models;
using VehicleService.DTOs;
using VehicleService.Services;
using VehicleService.Enums;

namespace VehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints require JWT authentication
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _db;
        //private readonly IVehicleService _vehicleService;

        public VehiclesController(AppDbContext db)
        {
            _db = db;
            //_vehicleService = vehicleService;
        }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Vehicles.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        return vehicle == null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVehicleRequest vehicle)
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        // Try common claim names for user id and name
        var ownerId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("id")?.Value;

        var ownerName = User.FindFirst("name")?.Value
                        ?? User.Identity?.Name
                        ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        var newVehicle = new Vehicle
        {
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            OwnerId = ownerId,
            OwnerName = ownerName,
            ExpirationDate = DateTime.Now.AddYears(1), // Default 1 year expiration
            Status = VehicleStatus.Unregistered // Explicitly set as Unregistered
        };

        _db.Vehicles.Add(newVehicle);
        await _db.SaveChangesAsync();

        // Create initial ownership history
        var initialOwnership = new VehicleOwnershipHistory
        {
            VehicleId = newVehicle.Id,
            OwnerId = ownerId,
            OwnerName = ownerName,
            FromDate = DateTime.Now,
            ToDate = null // Current owner, so no end date
        };
        _db.VehicleOwnershipHistory.Add(initialOwnership);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Vehicle created successfully",
            data = newVehicle
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateVehicleRequest vehicle)
    {
        var existing = await _db.Vehicles.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Make = vehicle.Make;
        existing.Model = vehicle.Model;
        existing.Year = vehicle.Year;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Vehicle updated successfully",
            data = existing
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("owner/{ownerName}")]
    public async Task<IActionResult> GetByOwnerName(string ownerName)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.OwnerName == ownerName)
            .ToListAsync();
        return Ok(new
        {
            message = "Vehicles retrieved successfully",
            data = vehicles
        });
    }

    [HttpGet("owner-id/{ownerId}")]
    public async Task<IActionResult> GetByOwnerId(string ownerId)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.OwnerId == ownerId)
            .ToListAsync();
        return Ok(new
        {
            message = "Vehicles retrieved successfully",
            data = vehicles
        });
    }

    [HttpGet("{id}/ownership-history")]
    public async Task<IActionResult> GetOwnershipHistory(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        var history = await _db.VehicleOwnershipHistory
            .Where(h => h.VehicleId == id)
            .OrderByDescending(h => h.FromDate)
            .ToListAsync();

        return Ok(new
        {
            message = "Ownership history retrieved successfully",
            data = history
        });
    }

}
}