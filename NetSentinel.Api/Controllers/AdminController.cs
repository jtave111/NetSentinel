 using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/manager/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpDelete("reset-vms")]
    public async Task<IActionResult> ResetVirtualMachines()
    {
        await _context.InstalledApplications.ExecuteDeleteAsync();

        await _context.Devices.ExecuteDeleteAsync();

        await _context.SoftwareVulnerabilities.ExecuteDeleteAsync();

        return Ok(new { message = "[SENTINELA] Reset concluído. Base de dispositivos e softwares limpa. Usuários mantidos." });
    }
}