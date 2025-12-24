using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleService.Models;

namespace VehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
       private readonly AppDbContext _db;
    
    public VehiclesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Vehicles.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        return vehicle == null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Vehicle vehicle)
    {
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Vehicle vehicle)
    {
        var existing = await _db.Vehicles.FindAsync(id);
        if (existing == null) return NotFound();
        
        existing.RegistrationNumber = vehicle.RegistrationNumber;
        existing.Make = vehicle.Make;
        existing.Model = vehicle.Model;
        existing.Year = vehicle.Year;
        existing.OwnerName = vehicle.OwnerName;
        existing.ExpirationDate = vehicle.ExpirationDate;
        
        await _db.SaveChangesAsync();
        return NoContent();
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
    
    // ADVANCED FEATURE #1: Check expiring registrations
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring()
    {
        var threshold = DateTime.Now.AddDays(30);
        var expiring = await _db.Vehicles
            .Where(v => v.ExpirationDate <= threshold && v.ExpirationDate >= DateTime.Now)
            .ToListAsync();
        return Ok(expiring);
    }
}
}