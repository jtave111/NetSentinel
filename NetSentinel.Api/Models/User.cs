using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetSentinel.Api.Models;

[Table("tb_users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)] 
    public string Department { get; set; } = string.Empty; 

    [Required]
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public List<Device> Devices { get; set; } = new();
}