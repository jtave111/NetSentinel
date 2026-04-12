using System.Text.Json.Serialization;

namespace NetSentinel.Api.DTOs;

public class NvdResponseDto
{
    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public List<NvdVulnerabilityWrapper>? Vulnerabilities { get; set; }
}

public class NvdVulnerabilityWrapper
{
    [JsonPropertyName("cve")]
    public NvdCveItem? Cve { get; set; }
}

public class NvdCveItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("descriptions")]
    public List<NvdDescription>? Descriptions { get; set; }

    [JsonPropertyName("metrics")]
    public NvdMetrics? Metrics { get; set; }
}

public class NvdDescription
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class NvdMetrics
{
    [JsonPropertyName("cvssMetricV31")]
    public List<NvdCvssMetric>? CvssMetricV31 { get; set; }
    
    [JsonPropertyName("cvssMetricV2")]
    public List<NvdCvssMetric>? CvssMetricV2 { get; set; }
}

public class NvdCvssMetric
{
    [JsonPropertyName("cvssData")]
    public NvdCvssData? CvssData { get; set; }
}

public class NvdCvssData
{
    [JsonPropertyName("baseScore")]
    public double BaseScore { get; set; }

    [JsonPropertyName("baseSeverity")]
    public string BaseSeverity { get; set; } = string.Empty;
}