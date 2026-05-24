using System.Text.Json;
using NetSentinel.Api.DTOs;

namespace NetSentinel.Api.Services;

public class NvdService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NvdService> _logger;
    private readonly string _baseUrl;
    private readonly string? _apiKey;

    public NvdService(HttpClient httpClient, ILogger<NvdService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["NvdApi:BaseUrl"] ?? "https://services.nvd.nist.gov/rest/json/cves/2.0";
        _apiKey = configuration["NvdApi:ApiKey"];
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NetSentinel/1.0");
    }

    public async Task<List<NvdCveItem>> GetCvesAsync(string appName, string appVersion)
    {
        try
        {
            var keyword = Uri.EscapeDataString($"{appName} {appVersion}");
            var url = $"{_baseUrl}?keywordSearch={keyword}&resultsPerPage=10";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.Add("apiKey", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[NVD] Status {Status} para {App} {Version}", (int)response.StatusCode, appName, appVersion);
                return new List<NvdCveItem>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var nvdResponse = JsonSerializer.Deserialize<NvdResponseDto>(json);

            return nvdResponse?.Vulnerabilities?
                .Where(v => v.Cve != null)
                .Select(v => v.Cve!)
                .ToList() ?? new List<NvdCveItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NVD] Erro ao buscar CVEs para {App} {Version}", appName, appVersion);
            return new List<NvdCveItem>();
        }
    }
}
