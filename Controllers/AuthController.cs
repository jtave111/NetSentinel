using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
        
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Username == request.Username);

        if(user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
        {
           return  Unauthorized(new {erro = "Invalid username or password"});
        }    
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "Employee") 
        };
        //Assinando o Token 
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3), // Token vale por 3 horas
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = jwt, message = "Login successful" });
    }

    [HttpPost("users")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var role = await _context.Roles.FindAsync(request.RoleId);

        if(role == null) return NotFound(new {erro = "Role not found"});

        if(await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new{error = "This email is already registered"});

        }else if(await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest(new {error = "This username is already exists"});

        }

        string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Username = request.Username,
            Email = request.Email,
            Password = passwordHash, 
            Department = request.Department,
            RoleId = request.RoleId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();


        return Ok(new {message =  $"User {user.Username} created as successfully"});

    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
    {
        if(await _context.Roles.AnyAsync(r => r.Name == request.Name)) return BadRequest(new {error = "Duplicate role"});
    
        var role = new Role{Name = request.Name, Description = request.Description};
        

        _context.Roles.Add(role); 
        await _context.SaveChangesAsync();

        return Ok(new { message = "Role created successfully", roleId = role.Id });
    }
}