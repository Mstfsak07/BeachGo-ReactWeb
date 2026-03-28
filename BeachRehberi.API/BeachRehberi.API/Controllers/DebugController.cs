using BeachRehberi.API.Data;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly BeachDbContext _db;

    public DebugController(BeachDbContext db)
    {
        _db = db;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.BusinessUsers
            .Where(u => !u.IsDeleted)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                u.BeachId,
                u.ContactName,
                u.BusinessName,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt
            })
            .ToListAsync();

        return users.ToOkApiResponse();
    }
}