
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;

namespace NetSentinel.Api.Workers;


public class DeviceStatusWorker : BackgroundService
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceStatusWorker> _logger;

    public DeviceStatusWorker(IServiceProvider serviceProvider, ILogger<DeviceStatusWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("[SENTINELA] Worker de Status de Dispositivos iniciado.");

    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await ChekedDEvicesStatusAsync(stoppingToken);
            
            await Task.Delay(TimeSpan.FromSeconds(50), stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SENTINELA] Erro crítico ao verificar status dos dispositivos.");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}



    public async Task ChekedDEvicesStatusAsync(CancellationToken stoppingToken)
    {
        //TODO: implementar lógica de checagem de status dos dispositivos (ping, etc) e atualizar campo IsActive, faixa de rede ? dhcp ? 
        _logger.LogInformation("[SENTINELA] Iniciando verificação de hosts ativos");
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var devices = await  context.Devices.ToListAsync(stoppingToken);

        var timeoutLimit = DateTime.UtcNow.AddMinutes(-2);
        int statusChangedCount = 0;

        foreach(var device in devices)
        {
            
            bool isCurrentlyActive = device.LastSync  >= timeoutLimit;
            
            
            if(device.IsActive != isCurrentlyActive)
            {
                
                device.IsActive = isCurrentlyActive;
                statusChangedCount++;

                var statusLog = isCurrentlyActive ? "ONLINE" : "OFFLINE";
                _logger.LogWarning($"[ALERTA] Dispositivo {device.Hostname} alterou o status para: {statusLog}");
            }
        }

        if(statusChangedCount > 0)
        {
           await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation($"[SENTINELA] Verificação finalizada, modificação feitas nos hosts.");

        }
        else
        {
            
            _logger.LogInformation($"[SENTINELA] Verificação finalizada, nenhuma modificação feitas nos hosts.");
        }


    }
}