using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetSentinel.Api.Models;

[Table("tb_devices")]
public class Device
{
    [Key]
    public int Id { get; set; }

    [Required] 
    [MaxLength(100)]
    public string Hostname { get; set; } = string.Empty;
    
    [MaxLength(15)] 
    public string Ipv4Address { get; set;} = string.Empty;

    [MaxLength(39)]
    public string Ipv6Address { get; set;} = string.Empty;

    [MaxLength(17)] 
    public string MacAddress { get; set;} = string.Empty;

    [MaxLength(100)]
    public string OperatingSystem { get; set; } = string.Empty;
    
    public DateTime LastSync { get; set; }
    public DateTime FirstSync { get; set; }

    public bool IsActive { get; set; } = true;
    
    public int? UserId { get; set; }
    public User? User { get; set; }
    
    public List<InstalledApplication> InstalledApplications { get; set; } = new();

}