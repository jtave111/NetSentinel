// PIPELINE AUTOMÁTICO DE DESCOBERTA DE VULNERABILIDADES:
//
//   [1] KEYWORD SEARCH — rápido, funciona para software moderno
//       GET ?keywordSearch={nome}+{versão}
//
//   [2] CPE LOOKUP — automático, funciona para software legado
//       2A → GET cpes/2.0?keywordSearch={nome}  (descobre o CPE oficial)
//       2B → GET cves/2.0?cpeName={cpe}         (busca CVEs pelo CPE)
//
// App sem nome? → Retorna lista vazia imediatamente

// App sem nome? → Retorna lista vazia imediatamente
//        ↓       
// ESTRATÉGIA 1 — keywordSearch
// Encontrou CVEs? → Retorna imediatamente 
//         ↓ Não encontrou
// ESTRATÉGIA 2 — CPE Lookup
// 2A → Descobre o CPE oficial do software
// 2B → Usa o CPE para buscar CVEs
// Encontrou CVEs? → Retorna 
// Não encontrou?  → Retorna lista vazia 


// Não é necessário manter nenhum dicionário manual.
// O sistema descobre CPEs e CVEs automaticamente via NVD API.

using System.Text.Json;
using System.Text.RegularExpressions;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.Services;

public class NvdIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NvdIntegrationService> _logger;
    private readonly string _apiKey;

    // client para endpoint de CPEs (URL base diferente)
    private readonly HttpClient _cpeHttpClient;

    public NvdIntegrationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NvdIntegrationService> logger)
    {
        _httpClient = httpClient;
        _logger     = logger;
        _apiKey     = configuration["NvdApi:ApiKey"] ?? string.Empty;

        // cliente principal  CVEs
        _httpClient.BaseAddress = new Uri(
            configuration["NvdApi:BaseUrl"]
            ?? "https://services.nvd.nist.gov/rest/json/cves/2.0"
        );
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // cliente secundário  CPE Dictionary (endpoint diferente)
        _cpeHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://services.nvd.nist.gov/rest/json/cpes/2.0"),
            Timeout     = TimeSpan.FromSeconds(30)
        };
    }


    public async Task<List<SoftwareVulnerability>> ScanApplicationAsync(InstalledApplication app)
    {
        var vulnerabilities = new List<SoftwareVulnerability>();

        if (string.IsNullOrWhiteSpace(app.Name)) return vulnerabilities;

        _logger.LogInformation(
            "[NVD] A analisar: '{Name}' v{Version}",
            app.Name, app.Version ?? "N/A"
        );

        // ESTRATÉGIA 1: keywordSearch 
        vulnerabilities = await SearchByKeywordAsync(app);

        if (vulnerabilities.Any())
        {
            _logger.LogInformation(
                "[NVD] '{Name}' → {Count} CVEs encontradas via keywordSearch.",
                app.Name, vulnerabilities.Count
            );
            SetApplicationId(vulnerabilities, app.Id);
            return vulnerabilities;
        }

        // ── ESTRATÉGIA 2: CPE Lookup automático 
        _logger.LogWarning(
            "[NVD] keywordSearch sem resultados para '{Name}'. A tentar CPE lookup...",
            app.Name
        );

        var cpe = await FindCpeAsync(app.Name, app.Version);

        if (cpe != null)
        {
            _logger.LogInformation(
                "[NVD] CPE encontrado para '{Name}': {Cpe}",
                app.Name, cpe
            );

            vulnerabilities = await SearchByCpeAsync(cpe);

            if (vulnerabilities.Any())
            {
                _logger.LogInformation(
                    "[NVD] '{Name}' → {Count} CVEs encontradas via CPE lookup.",
                    app.Name, vulnerabilities.Count
                );
                SetApplicationId(vulnerabilities, app.Id);
                return vulnerabilities;
            }
        }

        _logger.LogWarning(
            "[NVD] Nenhuma CVE encontrada para '{Name}' v{Version}.",
            app.Name, app.Version ?? "N/A"
        );

        return vulnerabilities;
    }

    
    // ESTRATÉGIA 1 — KEYWORD SEARCH

    private async Task<List<SoftwareVulnerability>> SearchByKeywordAsync(InstalledApplication app)
    {
        string cleanName = Regex.Replace(app.Name, @"\s*\$.*?\$", "").Trim();
        var nameParts    = cleanName.Split(' ');
        string coreName  = nameParts.Length > 1
            ? $"{nameParts[0]} {nameParts[1]}"
            : nameParts[0];

        string searchName    = coreName.Replace(" ", "+");
        string searchVersion = (app.Version ?? "").Replace(" ", "+");

        var queries = new List<string?>
        {
            !string.IsNullOrEmpty(searchVersion)
                ? $"?keywordSearch={searchName}+{searchVersion}"
                : null,
            $"?keywordSearch={searchName}",
        }
        .Where(q => q != null)
        .Distinct()
        .ToList();

        foreach (var query in queries)
        {
            var nvdData = await SendCveRequestAsync(query!);

            if (nvdData?.Vulnerabilities != null && nvdData.Vulnerabilities.Any())
                return ParseVulnerabilities(nvdData);

            await Task.Delay(GetDelay());
        }

        return new List<SoftwareVulnerability>();
    }

    // ESTRATÉGIA 2A — DESCOBRE O CPE AUTOMATICAMENTE

    private async Task<string?> FindCpeAsync(string softwareName, string? version)
    {
        try
        {
            string cleanName  = Regex.Replace(softwareName, @"\s*\$.*?\$", "").Trim();
            string searchName = cleanName.Replace(" ", "+");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"?keywordSearch={searchName}&resultsPerPage=5"
            );

            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.Add("apiKey", _apiKey);

            var response = await _cpeHttpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[NVD-CPE] HTTP {Status} ao buscar CPE para '{Name}'",
                    response.StatusCode, softwareName
                );
                return null;
            }

            var json    = await response.Content.ReadAsStringAsync();
            var cpeData = JsonSerializer.Deserialize<NvdCpeResponseDto>(json);

            if (cpeData?.Products == null || !cpeData.Products.Any())
            {
                _logger.LogWarning(
                    "[NVD-CPE] Nenhum CPE encontrado para '{Name}'",
                    softwareName
                );
                return null;
            }

            // Se temos versão, tenta encontrar o CPE mais específico
            if (!string.IsNullOrEmpty(version))
            {
                string majorVersion = version.Split('.')[0];

                var versionMatch = cpeData.Products.FirstOrDefault(p =>
                    p.Cpe?.CpeName?.Contains(
                        majorVersion,
                        StringComparison.OrdinalIgnoreCase
                    ) == true
                );

                if (versionMatch?.Cpe?.CpeName != null)
                    return versionMatch.Cpe.CpeName;
            }

            // retorna o primeiro CPE mais relevante segundo a NVD
            return cpeData.Products.FirstOrDefault()?.Cpe?.CpeName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NVD-CPE] Erro ao buscar CPE para '{Name}'", softwareName);
            return null;
        }
        finally
        {
            await Task.Delay(GetDelay());
        }
    }

    // ESTRATÉGIA 2B — BUSCA CVEs PELO CPE ENCONTRADO

    private async Task<List<SoftwareVulnerability>> SearchByCpeAsync(string cpeName)
    {
        var nvdData = await SendCveRequestAsync(
            $"?cpeName={Uri.EscapeDataString(cpeName)}"
        );

        await Task.Delay(GetDelay());

        return nvdData != null
            ? ParseVulnerabilities(nvdData)
            : new List<SoftwareVulnerability>();
    }

    // HELPERS PRIVADOS
    private async Task<NvdResponseDto?> SendCveRequestAsync(string relativeUrl)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.Add("apiKey", _apiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[NVD] HTTP {Status} para: {Url}",
                    response.StatusCode, relativeUrl
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NvdResponseDto>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NVD] Erro no request: {Url}", relativeUrl);
            return null;
        }
    }

    private List<SoftwareVulnerability> ParseVulnerabilities(NvdResponseDto nvdData)
    {
        var results = new List<SoftwareVulnerability>();

        if (nvdData?.Vulnerabilities == null) return results;

        foreach (var item in nvdData.Vulnerabilities.Take(10))
        {
            if (item.Cve == null) continue;

            if (results.Any(v => v.CveId == item.Cve.Id)) continue;

            var desc = item.Cve.Descriptions?
                .FirstOrDefault(d => d.Lang == "en")?.Value
                ?? "Sem descrição disponível";

            var (score, severity) = ExtractCvssScore(item.Cve.Metrics);

            results.Add(new SoftwareVulnerability
            {
                CveId       = item.Cve.Id,
                Description = desc,
                CvssScore   = score,
                Severity    = severity,
            });
        }

        return results;
    }

    private static (double Score, string Severity) ExtractCvssScore(NvdMetrics? metrics)
    {
        // CVSS V3.1 — padrão moderno
        if (metrics?.CvssMetricV31?.Count > 0)
        {
            var data = metrics.CvssMetricV31[0].CvssData;
            return (data?.BaseScore ?? 0.0, data?.BaseSeverity ?? "LOW");
        }

        // CVSS V3.0 — fallback intermédio
        if (metrics?.CvssMetricV30?.Count > 0)
        {
            var data = metrics.CvssMetricV30[0].CvssData;
            return (data?.BaseScore ?? 0.0, data?.BaseSeverity ?? "LOW");
        }

        // CVSS V2 — software legado (pré-2015)
        if (metrics?.CvssMetricV2?.Count > 0)
        {
            var score = metrics.CvssMetricV2[0].CvssData?.BaseScore ?? 0.0;

            var severity = score switch
            {
                >= 9.0 => "CRITICAL",
                >= 7.0 => "HIGH",
                >= 4.0 => "MEDIUM",
                _      => "LOW"
            };

            return (score, severity);
        }

        return (0.0, "LOW");
    }

    private static void SetApplicationId(List<SoftwareVulnerability> vulns, int appId)
    {
        foreach (var v in vulns)
            v.InstalledApplicationId = appId;
    }

    private int GetDelay() =>
        string.IsNullOrEmpty(_apiKey) ? 6000 : 1000;
}