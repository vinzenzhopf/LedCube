using System.Net.Sockets;
using LedCube.Core.Common.Config;
using LedCube.Core.CubeData.Repository;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LedCube.Streamer.Service;

public class UdpStreamerService : BackgroundService
{
    private ILogger Logger { get; }
    private readonly ICubeConfigRepository _cubeConfigRepository;
    private readonly ICubeRepository _cubeRepository;

    private UdpClient _udpClient;
    private CancellationTokenSource _receiverCancellationTks;
    private Task _receiverTask;

    public UdpStreamerService(ILoggerFactory loggerFactory, ICubeConfigRepository cubeConfigRepository, ICubeRepository cubeRepository)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        _cubeConfigRepository = cubeConfigRepository;
        _cubeRepository = cubeRepository;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _udpClient = new UdpClient(4242);
            
            
            _receiverCancellationTks = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            // _receiverTask = Task.Run(() => ReceiveMessagesAsync(_receiverCancellationTks.Token));
            
            
        }

        return Task.CompletedTask;
    }
}