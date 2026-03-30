using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;
using NetSentinel.Api.Filters;
using Microsoft.AspNetCore.Authorization;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/manager/[controller]")] 
public class DeviceController : ControllerBase
{
    private readonly AppDbContext _context;

    public DeviceController(AppDbContext context)
    {
        _context = context;
    }

    [ApiKey]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateDeviceDto request)
    {   
        int? finalUserId = request.UserId; 

        if (!string.IsNullOrEmpty(request.WindowsUsername))
        {
            var matchUser = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == request.WindowsUsername || 
                (!string.IsNullOrEmpty(request.UserEmail) && u.Email == request.UserEmail));
                
            if(matchUser != null)
            {
                finalUserId = matchUser.Id;
            }
            else
            {   
                string finalName = (request.UserFullName == "Not_Identified" || string.IsNullOrEmpty(request.UserFullName)) 
                    ? request.WindowsUsername 
                    : request.UserFullName;

                string finalEmail = (request.UserEmail == "Not_Identified" || string.IsNullOrEmpty(request.UserEmail))
                    ? $"{request.WindowsUsername}@corpDomain.com" 
                    : request.UserEmail;

                //criando novo user TODO: talvez refatorar essa criada (politica de alterar senha ? )
                var newUser = new User
                {
                    Username = request.WindowsUsername, 
                    Name = finalName,                   
                    Email = finalEmail,                 
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"), 
                    Department = "Auto-Register",
                    RoleId = 2 
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                finalUserId = newUser.Id;
            }
        }
         
        // Busca o Dispositivo pelo Hostname ou MAC 
        var existingDevice = await _context.Devices
            .Include(d => d.InstalledApplications) 
            .FirstOrDefaultAsync(d => d.MacAddress == request.MacAddress || d.Hostname == request.Hostname);

        if (existingDevice != null)
        {
            // MODO UPDATE
           existingDevice.Ipv4Address = request.Ipv4Address ?? "unknown";
            existingDevice.Ipv6Address = request.Ipv6Address ?? "";  
            existingDevice.MacAddress = request.MacAddress ?? "00:00:00:00:00:00";
            existingDevice.OperatingSystem = request.OperatingSystem ?? "unknown";
            existingDevice.UserId = finalUserId; 
            existingDevice.LastSync = DateTime.UtcNow; 

            // apaga a lista velha de programas e salva a nova 
            _context.InstalledApplications.RemoveRange(existingDevice.InstalledApplications);
            existingDevice.InstalledApplications.Clear();

            if (request.InstalledApplications != null)
            {
                foreach (var appDto in request.InstalledApplications)
                {
                    existingDevice.InstalledApplications.Add(new InstalledApplication
                    {
                        Name = appDto.Name,
                        Version = appDto.Version,
                        Publisher = appDto.Publisher
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Device {existingDevice.Hostname} updated successfully", deviceId = existingDevice.Id });
        }
        else
        {
            // MODO INSERT
            var newDevice = new Device
            {
                Hostname = request.Hostname,
                Ipv4Address = request.Ipv4Address ?? "unknown", 
                Ipv6Address = request.Ipv6Address ?? "",             
                MacAddress = request.MacAddress ?? "00:00:00:00:00:00",
                OperatingSystem = request.OperatingSystem ?? "unknown",
                
                UserId = finalUserId, 
                FirstSync = DateTime.UtcNow, 
                LastSync = DateTime.UtcNow
            };
            if (request.InstalledApplications != null)
            {
                foreach (var appDto in request.InstalledApplications)
                {
                    newDevice.InstalledApplications.Add(new InstalledApplication
                    {
                        Name = appDto.Name,
                        Version = appDto.Version,
                        Publisher = appDto.Publisher
                    });
                }
            }

            _context.Devices.Add(newDevice);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Device {newDevice.Hostname} registered successfully", deviceId = newDevice.Id });
        }
    }

    // Listagem para o nextJs
    [Authorize]
    [HttpGet("list")] 
    public async Task<IActionResult> GetAllDevices()
    {
        return Ok(await _context.Devices.Include(d => d.InstalledApplications).ToListAsync());
    }   
}