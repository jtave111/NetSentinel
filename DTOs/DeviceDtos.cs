namespace NetSentinel.Api.DTOs;

public class CreateDeviceDto
{
    public string Hostname { get; set; } = string.Empty;
    public string Ipv4Address { get; set; } = string.Empty;
    public string Ipv6Address { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    
    public int? UserId { get; set; } 

    public string ? WindowsUsername { get; set; }
    
    public List<AppPayloadDto> InstalledApplications { get; set; } = new();
}

public class AppPayloadDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
}