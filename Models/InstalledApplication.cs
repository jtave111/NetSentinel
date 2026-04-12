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

    public ICollection<SoftwareVulnerability> SoftwareVulnerabilities { get; set; } = new List<SoftwareVulnerability>();

}
[Table("tb_software_vulnerabilities")]
public class SoftwareVulnerability
{
    public int Id { get; set; }
    public string CveId { get; set; } = string.Empty; // Ex: CVE-2024-1234
    public string Description { get; set; } = string.Empty;
    public double CvssScore { get; set; } // Ex: 9.8 (Crítico)
    public string Severity { get; set; } = string.Empty; // CRITICAL, HIGH, MEDIUM
    public string ResolutionMode { get; set; } = string.Empty; 

    public int InstalledApplicationId { get; set; }
    
    public InstalledApplication ?  InstalledApplication { get; set; } 
}