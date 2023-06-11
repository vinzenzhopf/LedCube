using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task receiver;
        private CancellationTokenSource receiverCancellationTks;

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
            receiverCancellationTks = new CancellationTokenSource();
            receiver = Task.Run(() => ReceiveMessagesAsync(receiverCancellationTks.Token));
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(token);
                    var datagram = result.Buffer;

                    var header = Header.ReadFromSpan(datagram.AsSpan().Slice(0, Header.Size));

                    var headerStr = string.Format("Header{{ Type:{0}, Count:{1} }}", header.PayloadType.ToString(),
                        header.PacketCount);

                    var payloadStr = GetPayloadString(header.PayloadType, 
                        datagram.AsSpan().Slice(Header.Size));

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
            string payloadStr;
            switch (type)
            {
                // case DatagramType.Discovery:
                //     var payload = Discovery 
                //     break;
                case DatagramType.InfoResponse:
                    var infoPayload = InfoResponsePayload.ReadFromSpan(payloadSpan);
                    payloadStr =
                        $"Payload:{{ Version:{infoPayload.Version}, RuntimeMs:{infoPayload.RuntimeMs}, " +
                        $"MaxFrameTimeUs:{infoPayload.MaxFrameTimeUs}, ErrorCode{infoPayload.ErrorCode} }}";
                    break;
                case DatagramType.ErrorResponse:
                    var errorPayload = InfoResponsePayload.ReadFromSpan(payloadSpan);
                    payloadStr =
                        $"Payload:{{ Version:{errorPayload.Version}, RuntimeMs:{errorPayload.RuntimeMs}, " +
                        $"MaxFrameTimeUs:{errorPayload.MaxFrameTimeUs}, ErrorCode{errorPayload.ErrorCode} }}";
                    break;
                case DatagramType.AnimationStartAck:
                    payloadStr = "Payload:{ <empty payload> }";
                    break;
                case DatagramType.AnimationEndAck:
                    payloadStr = "Payload:{ <empty payload> }";
                    break;
                case DatagramType.FrameDataAck:
                    payloadStr = "Payload:{ <empty payload> }";
                    break;
                default:
                    payloadStr = "Payload:{ <unknown payload> }";
                    break;
            }

            return payloadStr;
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
            SendDatagram(DatagramType.Info, InfoPayload.WriteToSpan(payload));
        }
        
        private void ButtonSendAnimationStart_OnClick(object sender, RoutedEventArgs args)
        {
            var payload = new AnimationStartPayload()
            {
                FrameTimeUs = 16667,
                AnimationName = "TestAnimation"
            };
            SendDatagram(DatagramType.AnimationStart, AnimationStartPayload.WriteToSpan(payload));
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
                    Data = new byte[512]
                };
                for (var i = 0; i < 32; i++)
                {
                    payload.Data[i] = 0xff;        
                }
                SendDatagram(DatagramType.FrameData, FramePayload.WriteToSpan(payload));
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
                    Data = new byte[512]
                };
                for (var i = 0; i < 16; i++)
                {
                    payload.Data[i+(32)] = 0xff;        
                }
                SendDatagram(DatagramType.FrameData, FramePayload.WriteToSpan(payload));
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
        {
            if (hexStr.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");
            var arr = new byte[hexStr.Length >> 1];
            for (var i = 0; i < hexStr.Length >> 1; i++)
            {
                arr[i] = (byte)((GetHexVal(hexStr[i << 1]) << 4) + (GetHexVal(hexStr[(i << 1) + 1])));
            }
            return arr;
        }

        private static int GetHexVal(char hex) {
            var val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        private static void SendDatagram(UdpClient client, ushort packetCount, DatagramType type, ReadOnlySpan<byte> dataSpan)
        {
            var header = new Header()
            {
                PacketCount = packetCount,
                PayloadType = type
            };
            var headerSpan = Header.WriteToSpan(header);

            var buffer = new byte[1024].AsSpan();
            headerSpan.CopyTo(buffer);
            dataSpan.CopyTo(buffer[headerSpan.Length..]);

            var totalLength = headerSpan.Length + dataSpan.Length;
            var messageSlice = buffer[..totalLength];
        
            client.Send(messageSlice);
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
            SendDatagram(_udpClient, GetAndUpdatePacketCount(), DatagramType.AnimationStart, AnimationStartPayload.WriteToSpan(payload));
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
                    Data = new byte[512]
                };
                var index = _activeLed / 8;
                var bit = (byte)(_activeLed % 8);
                payload.Data[index] = (byte) (1 << bit);

                _activeLed = (_activeLed + 1) % 4096;
                
                SendDatagram(_udpClient, GetAndUpdatePacketCount(), DatagramType.FrameData, FramePayload.WriteToSpan(payload));
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