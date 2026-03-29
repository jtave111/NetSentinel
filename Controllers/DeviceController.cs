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
        int ? finalUSerID = request.UserId;

        if (!string.IsNullOrEmpty(request.WindowsUsername))
        {
            var matchUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.WindowsUsername);
                
            if(matchUser != null)
            {
                
                finalUSerID = matchUser.Id;
            }
            else
            {   //TODO: Criar funcao de pre cadastro melhro se der tempo  
                var newUser = new User
                {
                    Username = request.WindowsUsername,
                    Name = request.WindowsUsername, // Usa o próprio username como nome inicial
                    Email = $"{request.WindowsUsername}@corpDomain.com", 
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"), 
                    Department = "Auto-Registrado",
                    RoleId = 2 
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                finalUSerID = newUser.Id;
                
            }
        }
         
        // Busca o Dispositivo pelo Hostname $env:COMPUTERNAME ou mac 
        var existingDevice = await _context.Devices
            .Include(d => d.InstalledApplications) 
            .FirstOrDefaultAsync(d => d.MacAddress == request.MacAddress || d.Hostname == request.Hostname);

        if (existingDevice != null)
        {
            // MODO UPDATE
            existingDevice.Ipv4Address = request.Ipv4Address;
            existingDevice.Ipv6Address = request.Ipv6Address;
            existingDevice.MacAddress = request.MacAddress;
            existingDevice.OperatingSystem = request.OperatingSystem;
            existingDevice.UserId = finalUSerID;
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
                Ipv4Address = request.Ipv4Address,
                Ipv6Address = request.Ipv6Address,
                MacAddress = request.MacAddress,
                OperatingSystem = request.OperatingSystem,
                UserId = finalUSerID, 
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


    [Authorize]
    public async Task<IActionResult> GetAllDevices()
    {

        return Ok (await _context.Devices.Include(d => d.InstalledApplications).ToListAsync());
        
    }   
}