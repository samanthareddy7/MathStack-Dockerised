using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Microsoft.EntityFrameworkCore;
using MathApi.Models; 

namespace MathApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MathController : ControllerBase
{
    private readonly MathDbContext _context;

    // This is "Dependency Injection" - it brings your DB connection into the controller
    public MathController(MathDbContext context)
    {
        _context = context;
    }

    [HttpPost("PostCalculate")]
    [Authorize] // <--- LOCK: Only users with a valid JWT can enter this method
    public async Task<IActionResult> PostCalculate(MathCalculation mathCalculation)
    {
        // 1. EXTRACT IDENTITY: We get the UserId directly from the JWT "Badge" 
        // This is much more secure than passing it in the JSON body.
        var Token = User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(Token))
        {
            return Unauthorized(new Error("User ID not found in token!"));
        }

        // 2. VALIDATION: Check if the numbers or operation are missing
        if (mathCalculation.FirstNumber == null || mathCalculation.SecondNumber == null || mathCalculation.Operation == 0) 
        {
            return BadRequest(new Error("Math equation not complete!"));
        }

        // 3. CALCULATION LOGIC: Performing the math based on the operation code
        switch (mathCalculation.Operation)
        {
            case 1: // Addition
                mathCalculation.Result = mathCalculation.FirstNumber + mathCalculation.SecondNumber;
                break;
            case 2: // Subtraction
                mathCalculation.Result = mathCalculation.FirstNumber - mathCalculation.SecondNumber;
                break;
            case 3: // Multiplication
                mathCalculation.Result = mathCalculation.FirstNumber * mathCalculation.SecondNumber;
                break;
            default: // Division (and others)
                if (mathCalculation.SecondNumber == 0) return BadRequest(new Error("Cannot divide by zero!"));
                mathCalculation.Result = mathCalculation.FirstNumber / mathCalculation.SecondNumber;
                break;
        }

        try
        {
            // 4. FACTORY SYNC: Ensure the model is updated with the Token ID from the JWT
            mathCalculation.FirebaseUuid = Token;

            // 5. SAVE: Push the calculation to your Docker SQL Database
            _context.MathCalculations.Add(mathCalculation);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(new Error("Database Error: " + ex.Message));
        }

        // 6. RETURN: Success (201 Created) with the final object including the result
        return Created(mathCalculation.CalculationId.ToString(), mathCalculation);
    }

    [HttpGet("GetHistory")]
    [Authorize] // <--- LOCK: Protects the user's history from being seen by others
    public async Task<IActionResult> GetHistory()
    {
        // 1. IDENTITY CHECK: Get the ID of the person currently logged in
        var Token = User.FindFirst("UserId")?.Value;

        // 2. FETCH: Query the database for records matching ONLY this user
        List<MathCalculation> historyItems = await _context.MathCalculations
            .Where(m => m.FirebaseUuid == Token)
            .ToListAsync();

        // 3. RESPONSE: Return 200 OK with the list, or 404 if they have no history
        if (historyItems.Count > 0)
        {
            return Ok(historyItems);
        } 
        else
        {
            return NotFound(new Error("No history found for this user!"));
        }
    }

    [HttpDelete("DeleteHistory")]
    [Authorize] // <--- LOCK: Only the owner can delete their own records
    public async Task<IActionResult> DeleteHistory()
    {            
        // 1. IDENTITY CHECK: Ensure we know who is requesting the deletion
        var Token = User.FindFirst("UserId")?.Value;

        // 2. FIND: Get all items belonging to this specific user
        var removableItems = await _context.MathCalculations
            .Where(m => m.FirebaseUuid == Token)
            .ToListAsync();

        // 3. REMOVE: If items exist, delete them from the database
       if (removableItems.Any())
{
    _context.MathCalculations.RemoveRange(removableItems);
    await _context.SaveChangesAsync();
    return Ok(removableItems); // <-- Fixes the array assertion constraint
}
        else
        {
            return NotFound(new Error("No history exists to delete!"));
        }
    }
}