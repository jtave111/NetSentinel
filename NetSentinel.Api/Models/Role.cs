using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetSentinel.Api.Models;

[Table("tb_roles")]
public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; 

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty; 

    public List<User> Users { get; set; } = new();
}