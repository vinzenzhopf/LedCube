using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.UI.Util;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LedCube.Core.UI.Dialog.BroadcastSearchDialog;

public partial class BroadcastSearchDialogViewModel : ObservableObject
{
    private readonly Func<IUdpCubeCommunication> _cubeCommunicationFactory;
    private readonly ILogger _logger;
    
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private bool _broadcastSearchRunning;
    
    // private readonly ApplicationSettings _settings;
    public ObservableCollection<NetworkAdapterViewModel> NetworkAdapters { get; } = new();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentIP))]
    [NotifyPropertyChangedFor(nameof(CurrentIPString))]
    [NotifyPropertyChangedFor(nameof(BroadcastSearchEnabled))]
    private NetworkAdapterViewModel? _selectedAdapter;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BroadcastSearchEnabled))]
    private int? _remotePort;
    
    public IPAddress? CurrentIP => SelectedAdapter?.Address;
    public string CurrentIPString => CurrentIP?.ToString() ?? string.Empty;

    public ObservableCollection<HostAndPort> Destinations { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OkEnabled))]
    [NotifyCanExecuteChangedFor(nameof(OkClickedCommand))]
    private HostAndPort? _selectedDestination;

    public bool OkEnabled => SelectedDestination is not null;
    
    public bool BroadcastSearchEnabled =>
        SelectedAdapter?.Address != null &&
        RemotePort is > ushort.MinValue and < ushort.MaxValue;

    public BroadcastSearchDialogResult? DialogResult { get; set; }

    public BroadcastSearchDialogViewModel(
        ILoggerFactory loggerFactory,
        Func<IUdpCubeCommunication> cubeCommunicationFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _cubeCommunicationFactory = cubeCommunicationFactory;
        // _settings = settings;
        RemotePort = 4242; //settings.LastBroadcastPort;
    }
    
    private async Task UpdateAdaptersInternal(CancellationToken token)
    {
        try
        {
            await Task.Run((Func<Task?>) (async () =>
            {
                
                Log.Verbose("UpdateAdaptersCommand started");
                var adapterViewModels = 
                    await Task.Run(() => NetworkAdapterUtil.GetAdapters().Select(x => new NetworkAdapterViewModel(x)).ToList(), token)
                        .ConfigureAwait(true);
                Log.Verbose("Found {count} NetworkAdapters.", adapterViewModels.Count);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Log.Verbose("Start updating AdapterList in UI");
                    var selectedIp = SelectedAdapter?.Address;
                    NetworkAdapters.Clear();
                    NetworkAdapters.Add(new NetworkAdapterViewModel("0.0.0.0", "Any", IPAddress.Any));
                    foreach (var a in adapterViewModels)
                    {
                        NetworkAdapters.Add(a);
                        Dispatcher.Yield(DispatcherPriority.Background);
                    }
                    // if (selectedIp == null && _settings.LastSelectedIp != null) 
                    //     selectedIp = _settings.LastSelectedIp;
                    if (selectedIp != null)
                        SelectedAdapter = NetworkAdapters?.FirstOrDefault(a => Equals(a.Address, selectedIp));
                    else
                        // SelectedAdapterIndex = 0
                        SelectedAdapter = NetworkAdapters.First();
                    Log.Verbose("Updated AdapterList in UI");
                }, DispatcherPriority.Background, token);
            }), token);
            Log.Verbose("UpdateAdaptersCommand finished");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in UpdateNetworkAdapters");
        }
    }
    
    [RelayCommand(AllowConcurrentExecutions = false, IncludeCancelCommand = true)]
    private async Task BroadcastSearch(CancellationToken token)
    {
        try
        {
            BroadcastSearchRunning = true;
            await Task.Run(async () =>
            {
                if (SelectedAdapter is null)
                {
                    _logger.LogError("Broadcast search - No selected Adapter!");
                    return;
                }

                if (RemotePort is not (> ushort.MinValue and < ushort.MaxValue))
                {
                    _logger.LogError("Broadcast search - Invalid Target Port!");
                    return;
                }

                using var cubeCommunication = _cubeCommunicationFactory.Invoke();
                _logger.LogInformation("BroadcastSearchCommand start searching...");
                await cubeCommunication.ReStartListeningAsync(4243, SelectedAdapter.Address, token);
                await Application.Current.Dispatcher.InvokeAsync(() => Destinations.Clear());
                var results = cubeCommunication
                    .SendBroadcastSearch("Tbd", RemotePort!.Value, TimeSpan.FromSeconds(15), token)
                    .ConfigureAwait(false);
                await foreach (var result in results)
                {
                    _logger.LogInformation("BroadcastSearchCommand destination found");
                    await Application.Current.Dispatcher.BeginInvoke(() => Destinations.Add(result));
                }

                _logger.LogInformation("BroadcastSearchCommand finished");
            }, token);
        }
        catch (TaskCanceledException e)
        {
            _logger.LogInformation(e, "BroadcastSearchCommand stopped");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "BroadcastSearchCommand error");
        }
        finally
        {
            BroadcastSearchRunning = false;
        }
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task Loaded(CancellationToken token)
    {
        return UpdateAdaptersInternal(token);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task UpdateAdapters(CancellationToken token)
    {
        return UpdateAdaptersInternal(token);
    }
    
    [RelayCommand(CanExecute = nameof(OkEnabled))]
    private void OnOkClicked(object window)
    {
        if (window is not Window w) return;
        DialogResult = new BroadcastSearchDialogResult(SelectedDestination, true);
        w.DialogResult = true;
        w.Close();
    } 
    
    [RelayCommand]
    private void OnCancelClicked(object window)
    {
        if (window is not Window w) return;
        DialogResult = new BroadcastSearchDialogResult(null, false);
        w.DialogResult = false;
        w.Close();
    }
    
    [RelayCommand]
    private void OnClosed(object window)
    {
        if (BroadcastSearchCancelCommand.CanExecute(window))
        {
            BroadcastSearchCancelCommand.Execute(window);
        }
        if (window is not Window w) return;
        if (DialogResult is not null) return;
        DialogResult = new BroadcastSearchDialogResult(null, null);
        w.DialogResult = null;
    }
}