using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;

namespace NetSentinel.Api.Controllers;
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    public UserController(AppDbContext context, IConfiguration config
    )
       {
        _context = context;
        _config = config;

    } 

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        //pega o UserID do token JWT e busca as informações do usuário no banco de dados para retornar um UserDto.
        var useridClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
      
        if(useridClaim == null) return Unauthorized(new {error = "User ID claim não encontrado"});

        int userId = int.Parse(useridClaim);

        var currentUSer = await _context.Users.
            Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        

        var userDto = new UserDto{
            Name = currentUSer?.Name ?? "Unknown",
            Username = currentUSer?.Username ?? "Unknown",
            Email = currentUSer?.Email ?? "Unknown",
            Department = currentUSer?.Department ?? "Unknown"
        };
       
        return Ok(userDto);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.Include(u => u.Role).ToListAsync();
       
        var userDtos = users.Select(u => new UserDto
        {   Id = u.Id,
            Name       = u.Name,
            Username   = u.Username,
            Email      = u.Email,
            Department = u.Department,
            RoleId     = u.RoleId,
            Role       = u.Role != null ? new RoleDto
            {
                Id          = u.Role.Id,
                Name        = u.Role.Name,
                Description = u.Role.Description
            } : null
            
        }).ToList();
        
        return Ok(userDtos);
    }
} 