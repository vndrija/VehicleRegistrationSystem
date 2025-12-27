using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleService.DTOs;
using VehicleService.Enums;
using VehicleService.Models;

namespace VehicleService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VehicleTransfersController : ControllerBase
{
    private readonly AppDbContext _db;

    public VehicleTransfersController(AppDbContext db)
    {
        _db = db;
    }

    // POST: api/VehicleTransfers
    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateVehicleTransferDto dto)
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        var userId = User.FindFirst("sub")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        // Validation: Check vehicle exists
        var vehicle = await _db.Vehicles.FindAsync(dto.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        // Validation: Only current owner can create transfer
        if (vehicle.OwnerId != userId)
        {
            return Forbid();
        }

        // Validation: Cannot transfer to yourself
        if (dto.ToUserId == userId)
        {
            return BadRequest(new { message = "Cannot transfer vehicle to yourself" });
        }

        // Validation: Check if target user exists
        // Note: In a real system, you'd call AuthService to verify user exists
        if (string.IsNullOrWhiteSpace(dto.ToUserId))
        {
            return BadRequest(new { message = "Target user ID is required" });
        }

        // Validation: Check if there's already a pending transfer for this vehicle
        var existingPendingTransfer = await _db.VehicleTransfers
            .FirstOrDefaultAsync(t => t.VehicleId == dto.VehicleId && t.Status == VehicleTransferStatus.Pending);

        if (existingPendingTransfer != null)
        {
            return BadRequest(new { message = "A pending transfer request already exists for this vehicle" });
        }

        // Create transfer request
        var transfer = new VehicleTransfer
        {
            VehicleId = dto.VehicleId,
            FromUserId = userId,
            ToUserId = dto.ToUserId,
            Status = VehicleTransferStatus.Pending,
            CreatedAt = DateTime.Now
        };

        _db.VehicleTransfers.Add(transfer);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Transfer request created successfully",
            data = new
            {
                transfer.Id,
                transfer.VehicleId,
                transfer.FromUserId,
                transfer.ToUserId,
                transfer.Status,
                transfer.CreatedAt
            }
        });
    }

    // GET: api/VehicleTransfers/my-requests
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyTransfers()
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        var userId = User.FindFirst("sub")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;

        // Get transfers where user is either sender or receiver
        var transfers = await _db.VehicleTransfers
            .Include(t => t.Vehicle)
            .Where(t => t.FromUserId == userId || t.ToUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(new { message = "Transfers retrieved successfully", data = transfers });
    }

    // GET: api/VehicleTransfers/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transfer = await _db.VehicleTransfers
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer == null)
        {
            return NotFound(new { message = "Transfer request not found" });
        }

        var userId = User.FindFirst("sub")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;

        // Only sender or receiver can view the transfer
        if (transfer.FromUserId != userId && transfer.ToUserId != userId)
        {
            return Forbid();
        }

        return Ok(new { message = "Transfer retrieved successfully", data = transfer });
    }

    // POST: api/VehicleTransfers/{id}/respond
    [HttpPost("{id}/respond")]
    public async Task<IActionResult> RespondToTransfer(int id, [FromBody] RespondToTransferDto dto)
    {
        var transfer = await _db.VehicleTransfers
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer == null)
        {
            return NotFound(new { message = "Transfer request not found" });
        }

        var userId = User.FindFirst("sub")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;

        // Validation: Only the target user can respond
        if (transfer.ToUserId != userId)
        {
            return Forbid();
        }

        // Validation: Only pending transfers can be responded to
        if (transfer.Status != VehicleTransferStatus.Pending)
        {
            return BadRequest(new { message = "Only pending transfer requests can be responded to" });
        }

        if (dto.Accept)
        {
            // Accept the transfer
            transfer.Status = VehicleTransferStatus.Accepted;
            transfer.RespondedAt = DateTime.Now;

            // Close the previous owner's ownership record
            var previousOwnership = await _db.VehicleOwnershipHistory
                .FirstOrDefaultAsync(h => h.VehicleId == transfer.VehicleId && h.ToDate == null);

            if (previousOwnership != null)
            {
                previousOwnership.ToDate = DateTime.Now;
            }

            // Get the new owner's username from JWT claims
            var newOwnerName = User.FindFirst("name")?.Value
                                ?? User.Identity?.Name
                                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                                ?? transfer.ToUserId; // Fallback to userId if username not found

            // Create new ownership record for the new owner
            var newOwnership = new VehicleOwnershipHistory
            {
                VehicleId = transfer.VehicleId,
                OwnerId = transfer.ToUserId,
                OwnerName = newOwnerName,
                FromDate = DateTime.Now,
                ToDate = null // Current owner, no end date
            };
            _db.VehicleOwnershipHistory.Add(newOwnership);

            // Update vehicle owner
            if (transfer.Vehicle != null)
            {
                transfer.Vehicle.OwnerId = transfer.ToUserId;
                transfer.Vehicle.OwnerName = newOwnerName;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Transfer accepted successfully. Vehicle ownership updated.",
                data = transfer
            });
        }
        else
        {
            // Reject the transfer
            transfer.Status = VehicleTransferStatus.Rejected;
            transfer.RespondedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Transfer rejected",
                data = transfer
            });
        }
    }

    // GET: api/VehicleTransfers/pending-received
    [HttpGet("pending-received")]
    public async Task<IActionResult> GetPendingReceivedTransfers()
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        var userId = User.FindFirst("sub")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;

        // Get pending transfers where current user is the receiver
        var transfers = await _db.VehicleTransfers
            .Include(t => t.Vehicle)
            .Where(t => t.ToUserId == userId && t.Status == VehicleTransferStatus.Pending)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(new { message = "Pending received transfers retrieved successfully", data = transfers });
    }
}
