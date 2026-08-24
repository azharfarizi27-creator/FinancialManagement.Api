using System.Security.Claims;
using FinancialManagement.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagement.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Sesi login tidak valid atau user ID tidak ditemukan.");
        }

        return userId;
    }
}
