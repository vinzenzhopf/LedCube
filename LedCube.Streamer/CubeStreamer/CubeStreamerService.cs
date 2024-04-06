using System.Diagnostics;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.Common.Config.Config;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Streamer.Datagram;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LedCube.Streamer.CubeStreamer;

[ObservableObject]
public partial class CubeStreamerService : BackgroundService, ICubeStreamer
{
    private readonly ILogger _logger;
    private readonly IUdpCubeCommunication _communication;
    private readonly ICubeConfigRepository _cubeConfigRepository;
    private readonly ICubeRepository _cubeRepository;
    private readonly ICubeStreamingStatusMutable _cubeStreamingStatus;
    
    private readonly PeriodicTimer _updateTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
    private PeriodicTimer? _frameTimer = null;
    
    public bool FrameTransmissionEnabled { get; set; }
    
    private CancellationTokenSource? _streamerStoppingToken;
    private ushort _frameCounter;

    [ObservableProperty]
    private StreamerSettings _settings = StreamerSettings.Default;

    [ObservableProperty]
    private StreamingState _streamingState;
    
    public string? CurrentAnimation { get; private set; }
    public TimeSpan FrameTime { get; set; } = TimeSpan.FromMilliseconds(200);
    private TimeSpan _activeFrameTime;
    
    public ushort FrameCounter => _frameCounter;

    public CubeStreamerService(ILoggerFactory loggerFactory, 
        IUdpCubeCommunication communication, 
        ICubeConfigRepository cubeConfigRepository, 
        ICubeRepository cubeRepository, 
        ICubeStreamingStatusMutable cubeStreamingStatus)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _communication = communication;
        _cubeConfigRepository = cubeConfigRepository;
        _cubeRepository = cubeRepository;
        _cubeStreamingStatus = cubeStreamingStatus;
    }
    
    public async Task<bool> ConnectAsync(int localPort, IPAddress localAddress, HostAndPort hostAndPort, CancellationToken token)
    {
        if (_communication.IsConnected)
        {
            await DisconnectAsync(token);
        }
        await _communication.ReStartListeningAsync(localPort, localAddress, token);
        await _communication.ConnectAsync(hostAndPort, token);
        
        //Try to send info with retry
        var retryCounter = 0;
        while (!token.IsCancellationRequested && retryCounter < 5)
        {
            try
            {
                var response = await _communication.SendStatusRequestAsync(Settings.StreamerVersion, token);
                if (response is not null)
                {
                    var ticks = response?.PingTicks;
                    var ping = response?.Ping;
                    _logger.LogDebug("Connecting: StatusResponse Ping={ping} Ticks={ticks}, TicksPerSecond={tps}", ping, ticks, Stopwatch.Frequency);
                    if (response?.Ping < 0.5)
                    {
                        StreamingState = StreamingState.Stopped;
                        return true;
                    }
                }
                retryCounter++;
            }
            catch (Exception e)
            {
                retryCounter++;
                _logger.LogWarning(e, "Exception while connection. Connection attempt {attempt}", retryCounter);
            }
        }
        await DisconnectAsync(token);
        return false;
    }

    public async Task DisconnectAsync(CancellationToken token)
    {
        StreamingState = StreamingState.Disconnected;
        if (_communication.IsConnected)
        {
            await _communication.DisconnectAsync(token);
            await _communication.StopListeningAsync(token);
        }
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Settings.AlwaysSendFrames)
        {
            FrameTransmissionEnabled = true;
        }
        
        var updateLoop = Task.Run(async () => await UpdateLoop(stoppingToken), stoppingToken);
        var frameLoop = Task.Run(async () => await FrameLoop(stoppingToken), stoppingToken);
        
        _logger.LogDebug("Cube StreamingService started");
        await Task.WhenAll(updateLoop, frameLoop);
    }

    private async Task UpdateLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (StreamingState is not StreamingState.Disconnected)
                {
                    await SendStatusRequest(token);
                }
                if (!await _updateTimer.WaitForNextTickAsync(token).ConfigureAwait(false))
                {
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in Update Loop");
            }
        }
    }
    
    private async Task SendStatusRequest(CancellationToken token)
    {
        try
        {
            var result = await _communication.SendStatusRequestAsync(Settings.StreamerVersion, token);
            if (result?.Payload is InfoResponsePayload payload)
            {
                _cubeStreamingStatus.UpdateInfo(payload.Status, payload.LastFrameTimeUs, payload.Version,
                    payload.ErrorCode);
                _cubeStreamingStatus.CommitTimings(result.SendTicks, result.ReceivedTicks, null,
                    payload.CurrentTicks);
            }
        }
        catch (Exception e)
        {
            _cubeStreamingStatus.CommitMissingResponse();
            _logger.LogError(e, "Exception in SendStatusRequest in UpdateLoopCycle");
        }
    }

    private async Task FrameLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_frameTimer is null || (_activeFrameTime != FrameTime))
                {
                    _frameTimer?.Dispose();
                    _activeFrameTime = FrameTime;
                    _frameTimer = new PeriodicTimer(_activeFrameTime);
                }

                if (StreamingState is not StreamingState.Stopped and not StreamingState.Disconnected)
                {
                    if (FrameTransmissionEnabled)
                    {
                        await SendFrame(token);
                    }
                }
                await _frameTimer.WaitForNextTickAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException _)
            {
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in Frame Loop");
            }
        }
    }
    
    private async Task SendFrame(CancellationToken token)
    {
        var cubeData = new FramePayloadData();
        _cubeRepository.GetCubeData().Serialize(cubeData);
        try
        {
            var result = await _communication.SendFrameAsync(++_frameCounter, 
                (uint)_activeFrameTime.Microseconds, cubeData, token);
            if (result?.Payload is FrameResponsePayload payload)
            {
                _cubeStreamingStatus.UpdateFrameTime(payload.Status, payload.LastFrameTimeUs, payload.FrameNumber);
                _cubeStreamingStatus.CommitTimings(result.SendTicks, result.ReceivedTicks, payload.ReceivedTicks,
                    payload.CurrentTicks);
            }
        }
        catch (Exception e)
        {
            _cubeStreamingStatus.CommitMissingResponse();
            _logger.LogError(e, "Exception in SendFrame in FrameLoopCycle");
        }
    }
    
    public async Task StartAnimationAsync(uint frameTimeUs, string animationName, CancellationToken token)
    {
        await _communication.SendStartAnimationAsync(frameTimeUs, animationName, token);
        FrameTime = TimeSpan.FromMicroseconds(frameTimeUs);
        _frameTimer?.Dispose(); //Trigger immediate frame restart
        CurrentAnimation = animationName;
        FrameTransmissionEnabled = true;
    }
    
    public async Task EndAnimationAsync(CancellationToken token)
    {
        if (!Settings.AlwaysSendFrames)
        {
            FrameTransmissionEnabled = true;
        }
        /* TODO: Current Ticks?? */
        await _communication.SendEndAnimationAsync(0, token);
        FrameTime = TimeSpan.FromMicroseconds(Settings.SendIntervalUs);
        _frameTimer?.Dispose(); //Trigger immediate frame restart
        CurrentAnimation = null;
    }

    public Task StartStreaming(CancellationToken token)
    {
        StreamingState = StreamingState.Active;
        FrameTransmissionEnabled = true;
        return Task.CompletedTask;
    }

    public Task StopStreaming(CancellationToken token)
    {
        StreamingState = StreamingState.Stopped;
        FrameTransmissionEnabled = false;
        return Task.CompletedTask;
    }

    partial void OnSettingsChanged(StreamerSettings value)
    {
        FrameTime = TimeSpan.FromMicroseconds(value.SendIntervalUs);
        //Update CubeDataConverter
        //Update LocalPort/Broadcast port? Muss das da raus?
        // _communication.RemoteHost = new HostAndPort(value.)
        
    }

    public override void Dispose()
    {
        try
        {
            _updateTimer.Dispose();
            _frameTimer?.Dispose();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in CubeStreamer Dispose()");
        }
        base.Dispose();
    }
}