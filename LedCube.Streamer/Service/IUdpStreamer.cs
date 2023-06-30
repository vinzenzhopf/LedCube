namespace LedCube.Streamer.Service;

public interface IFrameData
{
    public byte[] GetData();
}

public record StreamerSettings(int LocalPort, bool KeepSendingFrames);

public record HostAndPort(string Hostname, int Port);

public record RemoteInfo(
    string Version,
    UInt32 LastFrameTimeUs,
    UInt32 CurrentTicks,
    int ErrorCode,
    int Status
);

public interface IUdpStreamer
{
    bool IsConnected { get; }
    
    HostAndPort RemoteHost { get; }
    
    RemoteInfo RemoteInfo { get; }
    
    string CurrentAnimation { get; }
    
    StreamerSettings Settings { get; }

    public IFrameData FrameData { get; set; }
    
    bool FrameTransmissionActive { get; }
    
    int CurrentPingTimeMs { get; }
    
    /// <summary>
    /// Connects to the specified remote host and port ensures, that the listener is listening for answers.
    /// </summary>
    /// <param name="hostname">Hostname of the remote client</param>
    /// <param name="port">Port to send packages to</param>
    /// <returns></returns>
    Task<bool> Connect(string hostname, int port);

    /// <summary>
    /// Starts listening with the provided settings.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns>True, if has successfully start or re-start the listener for the given settings.</returns>
    Task<bool> ReStartListeningAsync(StreamerSettings settings, CancellationToken stoppingToken);

    /// <summary>
    /// Sends a discovery Datagram to the local Broadcast address and the specified remotePort, and returns all received hosts.
    /// <param name="remotePort">The remote Port to send the datagrams to.</param>
    /// <param name="timeout">The maximum search Timeout, after that the search is stopped and the results are returned.</param>
    /// <returns>Return all found results</returns>
    Task<List<HostAndPort>> SendBroadcastSearch(int remotePort, TimeSpan timeout);
    
    /// <summary>
    /// Sends an StartAnimation-Datagram to the remote host and waits for the acknowledge or returns with an timeout-Exception.
    /// </summary>
    /// <param name="frameTimeUs">frameTimeUs parameter in the Datagram</param>
    /// <param name="animationName">animationName parameter in the Datagram</param>
    /// <returns></returns>
    Task<bool> StartAnimationAsync(int frameTimeUs, string animationName);
    
    /// <summary>
    /// Sends an EndAnimation-Datagram to the remote host and waits for the acknowledge or returns with an timeout-Exception.
    /// </summary>
    /// <returns></returns>
    Task<bool> EndAnimationAsync();

    /// <summary>
    /// Starts asynchronous the automatic sending of frames in the interval that is specified in updateTimeUs 
    /// </summary>
    /// <para name="updateTimeUs">The interval in which the frame data is sent.</para>
    /// <returns>When the sending started and the first acknowledge is returned, this method returns true</returns>
    Task<bool> StartSendingAsync(int updateTimeUs);

    /// <summary>
    /// Stops the automatic sending of frames.
    /// </summary>
    /// <returns>True, if the sending could be stopped successfully.</returns>
    bool StopSendingAsync();
}