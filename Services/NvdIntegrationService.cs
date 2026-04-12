using System.Text.Json;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.Services;

//TODO pradonizar os loggers 
public class NvdIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public NvdIntegrationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        httpClient.BaseAddress = new Uri(configuration["NvdApi:BaseUrl"] ?? "https://services.nvd.nist.gov/rest/json/cves/2.0");
        _apiKey = configuration["NvdApi:ApiKey"] ?? string.Empty;

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }


    public async Task<List<SoftwareVulnerability>> ScanApplicationAsync(InstalledApplication app)
    {
        var vulnerabilities = new List<SoftwareVulnerability>();

        if (string.IsNullOrEmpty(app.Name) || string.IsNullOrEmpty(app.Version))  return vulnerabilities;

        string cleanName = System.Text.RegularExpressions.Regex.Replace(app.Name, @"\s*\(.*?\)", "").Trim();
        var nameParts = cleanName.Split(' ');
        string cleanVersion = app.Version.Replace(" ", "+");
        string coreName = nameParts.Length > 1 ? $"{nameParts[0]} {nameParts[1]}" : nameParts[0];
        string searchName = coreName.Replace(" ", "+");
        string searchVersion = app.Version.Replace(" ", "+");

        //CPE
        string requestUrl = $"?keywordSearch={searchName}+{searchVersion}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        if (!string.IsNullOrEmpty(_apiKey))
        {
            request.Headers.Add("apiKey", _apiKey);
        }


        try
        {

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {  
                var jsonResult = await response.Content.ReadAsStringAsync();
                var nvdData = JsonSerializer.Deserialize<NvdResponseDto>(jsonResult);
                
                
                foreach(var item in nvdData.Vulnerabilities.Take(5))
                {
                    if (item.Cve != null)
                    {
                        var desc = item.Cve.Descriptions?.FirstOrDefault(d => d.Lang == "en")?.Value ?? "Sem descrição";
                        
                        // tenta pegar a nota CVSS V3.1, se não achar tenta a V2
                        double score = 0.0;
                        string severity = "LOW";

                        if (item.Cve.Metrics?.CvssMetricV31?.Count > 0)
                        {
                            score = item.Cve.Metrics.CvssMetricV31[0].CvssData?.BaseScore ?? 0.0;
                            severity = item.Cve.Metrics.CvssMetricV31[0].CvssData?.BaseSeverity ?? "LOW";
                        }
                        else if (item.Cve.Metrics?.CvssMetricV2?.Count > 0)
                        {
                            score = item.Cve.Metrics.CvssMetricV2[0].CvssData?.BaseScore ?? 0.0;
                            severity = item.Cve.Metrics.CvssMetricV2[0].CvssData?.BaseSeverity ?? "LOW";
                        }

                        vulnerabilities.Add(new SoftwareVulnerability
                        {
                            CveId = item.Cve.Id,
                            Description = desc,
                            CvssScore = score,
                            Severity = severity,
                            InstalledApplicationId = app.Id 
                        });
                    }
                }
            }
            else
            {
                Console.WriteLine($"[AVISO] NVD retornou status {response.StatusCode} para {app.Name}");
            }
            
        }catch(Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao consultar NVD para {app.Name}: {ex.Message}");
        
        }

        int delayMiliseconds = string.IsNullOrEmpty(_apiKey) ? 6000 : 1000; 
        await Task.Delay(delayMiliseconds);

        return vulnerabilities;
    }
}