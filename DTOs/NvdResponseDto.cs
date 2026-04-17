// =============================================================================
// NETSENTINEL — NvdResponseDto.cs
// =============================================================================
// Mapeia a resposta do endpoint de CVEs:
// GET https://services.nvd.nist.gov/rest/json/cves/2.0?keywordSearch=...
// =============================================================================

using System.Text.Json.Serialization;

namespace NetSentinel.Api.DTOs;

public class NvdResponseDto
{
    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    // ✅ "vulnerabilities" — correto para o endpoint de CVEs
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
    // CVSS V3.1 — padrão moderno (pós 2019)
    [JsonPropertyName("cvssMetricV31")]
    public List<NvdCvssMetric>? CvssMetricV31 { get; set; }

    // CVSS V3.0 — fallback intermédio (2015-2019)
    [JsonPropertyName("cvssMetricV30")]
    public List<NvdCvssMetric>? CvssMetricV30 { get; set; }

    // CVSS V2 — software legado (pré-2015)
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
    public string? BaseSeverity { get; set; }
}