
namespace NetSentinel.Api.Workers;


public class DevicesStatusWorker : BackgroundService
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceStatusWorker> _logger;

    public DevicesStatusWorker(IServiceProvider serviceProvider, ILogger<DeviceStatusWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[SENTINELA] Worker de Status de Dispositivos iniciado. Rodando em background");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ChekedDEvicesStatusAsync(stoppingToken);
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
        
        throw new NotImplementedException();
    }



    public async Task ChekedDEvicesStatusAsync(CancellationToken stoppingToken)
    {
        //TODO: implementar lógica de checagem de status dos dispositivos (ping, etc) e atualizar campo IsActive, faixa de rede ? dhcp ? 
    }
}