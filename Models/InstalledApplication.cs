using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetSentinel.Api.Models;

[Table("tb_installed_applications")]
public class InstalledApplication
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)] 
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)] 
    public string Version { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Publisher { get; set; } = string.Empty; 

    [Required]
    public int DeviceId { get; set; }
    public Device? Device { get; set; }

}