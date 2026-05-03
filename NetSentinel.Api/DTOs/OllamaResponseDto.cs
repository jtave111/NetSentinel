using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetSentinel.Api.DTOs
{
    public class OllamaResponseDto
    {
        
        [JsonPropertyName("response")]
        public string ResponseContent { get; set; } = string.Empty;
    }

    public class VulnerabilityReport
{
    [JsonPropertyName("cves")]
    public List<CveDto> Cves { get; set; } = new();
}

}