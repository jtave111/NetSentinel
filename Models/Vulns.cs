using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetSentinel.Api.Models;

[Table("tb_vulnerabilities")]
public class Vulnerability
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)] 
    public string Cve { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(5000)] 
    public string Description { get; set; } = string.Empty;

    public List<Device> Devices { get; set; } = new();
}