using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.DebugUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task _receiver;
        private CancellationTokenSource _receiverCancellationTks;

        public ObservableCollection<string> Messages { get; }

        public MainWindow()
        {
            Messages = new ObservableCollection<string>();
            InitializeComponent();

            RemoteHost.Text = "192.168.178.37";
            Port.Text = "4242";
            Counter.Text = "1";
            FrameBuffer.Text = "FFFF";
            TimerCycleTime.Text = "100000";
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _udpClient = new UdpClient(4242);
            _receiverCancellationTks = new CancellationTokenSource();
            _receiver = Task.Run(() => ReceiveMessagesAsync(_receiverCancellationTks.Token));
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(token);
                    var datagram = result.Buffer;

                    CubeDatagramHeader header = default;
                    CubeDatagramHeader.ReadFrom(datagram.AsSpan()[..CubeDatagramHeader.Size], ref header);

                    var headerStr = $"Header{{ Type:{header.PayloadType.ToString()}, Count:{header.PacketCount} }}";
                    var payloadStr = GetPayloadString(header.PayloadType, 
                        datagram.AsSpan()[CubeDatagramHeader.Size..]);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add($"{headerStr} {payloadStr}");
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private string GetPayloadString(DatagramType type, ReadOnlySpan<byte> payloadSpan)
        {
            var payloadStr = "<unknown payload>";
            switch (type)
            {
                case DatagramType.Discovery:
                    var discoveryPayload = DatagramExtensions.Read<InfoResponsePayload>(payloadSpan);
                    payloadStr = discoveryPayload.ToString(); 
                    break;
                case DatagramType.InfoResponse:
                    var infoPayload = DatagramExtensions.Read<InfoResponsePayload>(payloadSpan);
                    payloadStr = infoPayload.ToString();
                    break;
                case DatagramType.ErrorResponse:
                    var errorPayload = DatagramExtensions.Read<InfoResponsePayload>(payloadSpan);
                    payloadStr = errorPayload.ToString();
                    break;
                case DatagramType.AnimationStartAck:
                    var animStartPayload = DatagramExtensions.Read<AnimationStartResponsePayload>(payloadSpan);
                    payloadStr = animStartPayload.ToString();
                    break;
                case DatagramType.AnimationEndAck:
                    var animEndPayload = DatagramExtensions.Read<AnimationEndResponsePayload>(payloadSpan);
                    payloadStr = animEndPayload.ToString();
                    break;
                case DatagramType.FrameDataAck:
                    var frameDataAck = DatagramExtensions.Read<FrameResponsePayload>(payloadSpan);
                    payloadStr = frameDataAck.ToString();
                    break;
            }

            return $"Payload:{{ {payloadStr} }}";
        }
        
        private void ButtonSendDiscovery_OnClick(object sender, RoutedEventArgs args)
        {
            var span = Array.Empty<byte>().AsSpan();
            SendDatagram(DatagramType.Discovery, span);
        }

        private void ButtonSendInfo_OnClick(object sender, RoutedEventArgs args)
        {
            var payload = new InfoPayload()
            {
                Version = StreamerInfo.DataVersion
            };
            SendDatagram(DatagramType.Info, payload);
        }
        
        private void ButtonSendAnimationStart_OnClick(object sender, RoutedEventArgs args)
        {
            var payload = new AnimationStartPayload()
            {
                FrameTimeUs = 16667,
                AnimationName = "TestAnimation"
            };
            SendDatagram(DatagramType.AnimationStart, payload);
        }
        
        private void ButtonSendAnimationEnd_OnClick(object sender, RoutedEventArgs args)
        {
            var span = Array.Empty<byte>().AsSpan();
            SendDatagram(DatagramType.AnimationEnd, span);
        }
        
        private void ButtonSendFrame1_OnClick(object sender, RoutedEventArgs args)
        {
            try
            {
                var payload = new FramePayload()
                {
                    FrameNumber = 42,
                    FrameTimeUs = 2000,
                    Data = default
                };
                for (var i = 0; i < 32; i++)
                {
                    payload.Data[i] = 0xff;        
                }
                SendDatagram(DatagramType.FrameData, payload);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        private void ButtonSendFrame2_OnClick(object sender, RoutedEventArgs args)
        {
            try
            {
                var payload = new FramePayload()
                {
                    FrameNumber = 42,
                    FrameTimeUs = 200000,
                    Data = default
                };
                for (var i = 0; i < 16; i++)
                {
                    payload.Data[i+(32)] = 0xff;        
                }
                SendDatagram(DatagramType.FrameData, payload);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ButtonSendFrame3_OnClick(object sender, RoutedEventArgs args)
        {
            
        }
        
        private void ButtonSendFrame4_OnClick(object sender, RoutedEventArgs args)
        {
            
        }
        
        private void ButtonSendFrame5_OnClick(object sender, RoutedEventArgs args)
        {
            
        }
        
        private void ButtonSendFrame6_OnClick(object sender, RoutedEventArgs args)
        {
            
        }
        
        public static byte[] HexStringToByteArray(ReadOnlySpan<char> hexStr) 
            => Convert.FromHexString(hexStr);
        
        private static void SendDatagram(UdpClient client, ushort packetCount, DatagramType type, ReadOnlySpan<byte> dataSpan)
        {
            Span<byte> buffer = new byte[CubeDatagramHeader.Size + dataSpan.Length];
            var header = new CubeDatagramHeader()
            {
                PacketCount = packetCount,
                PayloadType = type
            };
            CubeDatagramHeader.WriteTo(buffer, in header);
            dataSpan.CopyTo(buffer[CubeDatagramHeader.Size..]);
            client.Send(buffer);
        }

        private void SendDatagram<TDatagram>(DatagramType type, in TDatagram datagram) where TDatagram : IWritableDatagram<TDatagram>
        {
            try
            {
                Span<byte> buffer = new byte[TDatagram.Size];
                TDatagram.WriteTo(buffer, in datagram);
                _udpClient.Connect(RemoteHost.Text, int.Parse(Port.Text));
                // Sends a message to the host to which you have connected.
                ushort.TryParse(Counter.Text, out var packetCount);
                Counter.Text = $"{packetCount+1}";
                SendDatagram(_udpClient, packetCount, type, buffer);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        
        private static void SendDatagram<TDatagram>(UdpClient client, ushort packetCount, DatagramType type, in TDatagram datagram) where TDatagram : IWritableDatagram<TDatagram>
        {
            Span<byte> buffer = new byte[CubeDatagramHeader.Size + TDatagram.Size];
            var header = new CubeDatagramHeader()
            {
                PacketCount = packetCount,
                PayloadType = type
            };
            CubeDatagramHeader.WriteTo(buffer, in header);
            TDatagram.WriteTo(buffer[CubeDatagramHeader.Size..], in datagram);
            client.Send(buffer);
        }
        
        private void SendDatagram(DatagramType type, ReadOnlySpan<byte> dataSpan)
        {
            try
            {
                _udpClient.Connect(RemoteHost.Text, int.Parse(Port.Text));
                // Sends a message to the host to which you have connected.
                ushort.TryParse(Counter.Text, out var packetCount);
                Counter.Text = $"{packetCount+1}";
                SendDatagram(_udpClient, packetCount, type, dataSpan);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private Timer? _timer;
        private UdpClient _udpClient;
        private void StartTimer_OnClick(object sender, RoutedEventArgs e)
        {
            if (_timer is not null)
            {
                return;
            }
            
            _udpClient.Connect(RemoteHost.Text, int.Parse(Port.Text));
            
            ushort.TryParse(Counter.Text, out var packetCount);
            Counter.Text = $"{packetCount+1}";
            
            uint.TryParse(TimerCycleTime.Text, out var timerCycleTime);
            var payload = new AnimationStartPayload()
            {
                FrameTimeUs = timerCycleTime,
                AnimationName = "TestTimerAnim"
            };
            SendDatagram(_udpClient, GetAndUpdatePacketCount(), DatagramType.AnimationStart, payload);
            var ts = TimeSpan.FromMicroseconds(timerCycleTime);
            _timer = new Timer(Timer_Tick, timerCycleTime, ts, ts);
            TimerIsRunning.IsChecked = true;
        }

        private ushort _packageCount = 0;

        public ushort GetAndUpdatePacketCount()
        {
            var currentCount = ++_packageCount;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Counter.Text = $"{currentCount}";
            });
            return _packageCount;
        }

        private uint _activeLed = 0;
        private uint _frameNumber = 0;
        
        private void Timer_Tick(object? state)
        {
            if (_udpClient is null)
            {
                return;
            }
            if (state is not uint timerCycleTime)
            {
                return;
            }
            try
            {
                var payload = new FramePayload()
                {
                    FrameNumber = _frameNumber++,
                    FrameTimeUs = timerCycleTime,
                    Data = default
                };
                var index = _activeLed / 8;
                var bit = (byte)(_activeLed % 8);
                payload.Data[unchecked((int) index)] = (byte) (1 << bit);

                _activeLed = (_activeLed + 1) % 4096;
                
                SendDatagram(_udpClient, GetAndUpdatePacketCount(), DatagramType.FrameData, payload);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StopTimer_OnClick(object sender, RoutedEventArgs e)
        {
            if (_timer is not null)
            {
                _timer.Dispose();
                _timer = null;
            }
            TimerIsRunning.IsChecked = false;
        }

        private void TimerTickManually_OnClick(object sender, RoutedEventArgs e)
        {
            if (_timer is not null)
            {
                return;
            }
            int.TryParse(TimerCycleTime.Text, out var timerCycleTime);
            Timer_Tick(timerCycleTime);
        }
    }
}