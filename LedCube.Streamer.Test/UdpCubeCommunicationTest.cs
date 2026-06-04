using System;
using System.Threading;
using System.Threading.Tasks;
using LedCube.Streamer.Datagram;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LedCube.Streamer.Test;

public class UdpCubeCommunicationTest : TestWithLoggingBase
{
    private IUdpCubeCommunication _sut;
    
    public UdpCubeCommunicationTest(ITestOutputHelper output) : base(output)
    {
        _sut = new UdpCubeCubeCommunication(LoggerFactory);
    }
    
    [Fact]
    [Trait("Category","ManualIntegration")]
    public async Task TestSendStatusRequest()
    {
        await _sut.ReStartListeningAsync(4242, CancellationToken.None);
        await _sut.ConnectAsync("192.168.178.37", 4242, CancellationToken.None);
        var response = await _sut.SendStatusRequestAsync("Test v1.0", TimeSpan.FromSeconds(10), CancellationToken.None);
        
        
        Assert.NotNull(response);
        Logger.LogInformation(response.ToString());
        Assert.Equal(DatagramType.InfoResponse, response.Header.PayloadType);
        Assert.True(response.Header.PacketCount > 0);
        Assert.True(response.SendTicks > 0);
        Assert.True(response.ReceivedTicks > 0);
    }

    [Fact]
    [Trait("Category", "ManualIntegration")]
    public async Task TestSendStartAnimationRequest()
    {
        await _sut.ReStartListeningAsync(4242, CancellationToken.None);
        await _sut.ConnectAsync("192.168.178.37", 4242, CancellationToken.None);
        var response =
            await _sut.SendStartAnimationAsync(10000, "TestAnimation", TimeSpan.FromSeconds(10),
                CancellationToken.None);

        Assert.NotNull(response);
        Logger.LogInformation(response.ToString());
        Assert.Equal(DatagramType.AnimationStartAck, response.Header.PayloadType);
        Assert.True(response.Header.PacketCount > 0);
        Assert.True(response.SendTicks > 0);
        Assert.True(response.ReceivedTicks > 0);
    }
}