namespace NetSentinel.Api.DTOs;

public class CreateDeviceDto
{
    public string Hostname { get; set; } = string.Empty;
    public string? WindowsUsername { get; set; }
    
    public string? UserFullName { get; set; } 
    public string? UserEmail { get; set; }

    public string? Ipv4Address { get; set; }
    public string? Ipv6Address { get; set; }
    public string? MacAddress { get; set; }
    public string? OperatingSystem { get; set; }
    public int? UserId { get; set; }
    
    public List<AppPayloadDto> InstalledApplications { get; set; } = new();
}

 class DeviceDto
{
    public int Id { get; set; }
    public string Hostname { get; set; }
    public string Ipv4Address { get; set; }
    public string Ipv6Address { get; set; }
    public string MacAddress { get; set; }
    public string OperatingSystem { get; set; }
    public DateTime FirstSync { get; set; }
    public DateTime LastSync { get; set; }
    public object InstalledApplications { get; set; }
}

internal class SoftwareVulnerabilityDto
{
    public int Id { get; set; }
    public string CveId { get; set; }
    public string Description { get; set; }
    public double CvssScore { get; set; }
    public string Severity { get; set; }
}
public class AppPayloadDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
}
internal class InstalledApplicationDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Publisher { get; set; }
    public List<SoftwareVulnerabilityDto> Vulnerabilities { get; set; }
}