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

public class AppPayloadDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
}