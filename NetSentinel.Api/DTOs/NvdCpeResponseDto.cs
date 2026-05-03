
// Mapeia a resposta do endpoint de CPE Dictionary:
// GET https://services.nvd.nist.gov/rest/json/cpes/2.0?keywordSearch=...
//
// Estrutura JSON real da NVD CPE API:
// {
//   "resultsPerPage": 5,
//   "totalResults": 12,
//   "products": [                        ← "products", NÃO "vulnerabilities"
//     {
//       "cpe": {
//         "cpeName": "cpe:2.3:a:seattlelab:slmail:5.5:*:*:*:*:*:*:*",
//         "cpeNameId": "some-uuid"
//       }
//     }
//   ]
// }

using System.Text.Json.Serialization;

namespace NetSentinel.Api.DTOs;

public class NvdCpeResponseDto
{
    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("products")]
    public List<CpeProduct>? Products { get; set; }
}

public class CpeProduct
{
    [JsonPropertyName("cpe")]
    public CpeItem? Cpe { get; set; }
}

public class CpeItem
{
    // Ex: "cpe:2.3:a:seattlelab:slmail:5.5:*:*:*:*:*:*:*"
    [JsonPropertyName("cpeName")]
    public string? CpeName { get; set; }

    [JsonPropertyName("cpeNameId")]
    public string? CpeNameId { get; set; }
}