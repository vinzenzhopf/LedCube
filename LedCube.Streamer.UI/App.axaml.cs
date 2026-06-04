using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Core.Common.Settings;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Controls.PlaylistControl;
using LedCube.Core.UI.Controls.SettingsDialog;
using LedCube.Core.UI.Controls.PluginConfigControl;
using LedCube.Core.UI.Controls.StreamingControl;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;
using LedCube.Core.UI.Dialog.SelectAnimationDialog;
using LedCube.Core.UI.Dialog.SimpleDialog;
using LedCube.Core.UI.Messages;
using LedCube.Core.UI.Services;
using LedCube.Core.UI.Services.Hotkey;
using LedCube.Core.UI.Services.Library;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginHost;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using AnimationListViewModel = LedCube.Core.UI.Controls.AnimationList.AnimationListViewModel;
using CubeStreamingStatus = LedCube.Streamer.CubeStreamer.CubeStreamingStatus;

namespace LedCube.Streamer.UI;

public partial class App : Application,
    IRecipient<OpenSimpleDialogMessage>,
    IRecipient<OpenBroadcastSearchDialogMessage>,
    IRecipient<OpenSelectAnimationDialogMessage>,
    IRecipient<EditAnimationInstanceDialogMessage>,
    IRecipient<OpenSettingsNavigationMessage>,
    IRecipient<ExitApplicationNavigationMessage>,
    IRecipient<OpenSettingsHotkeyInputDialogMessage>
{
    private IHost? _host;
    private readonly PluginHostContext _pluginHostContext = new();
    private Window? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += OnShutdownRequested;
            StartupAsync(desktop);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private async void StartupAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
#if DEBUG
        const bool debugBuild = true;
#else
        const bool debugBuild = false;
#endif

        _pluginHostContext.Initialize();

        var logAppenderControlSink = new LogAppenderControlSink();

        _host = new HostBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(context.HostingEnvironment.ContentRootPath);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.ConfigurePluginHost(_pluginHostContext);
            })
            .ConfigureLogging((context, logging) =>
            {
                var logFile = context.Configuration.GetValue<string>("LogFile") ?? "LedCube.Streamer.UI.log";
                var logFileFullPath = Path.GetFullPath(logFile);
                logging.Services.AddSingleton(new AppLogFileInfo(logFileFullPath));
                var logger = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose()
                    .WriteTo.File(logFile)
                    .WriteTo.Debug()
                    .WriteTo.LogAppenderControlSink(logAppenderControlSink)
                    .CreateLogger();
                Log.Logger = logger;
                logging.AddSerilog(logger, dispose: true);
                Log.Verbose("Logger initialized. Logging to {0}", logFileFullPath);
            })
            .ConfigureServices((context, services) =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var appInfo = new AppInfo(
                    assembly.GetName().Version!.ToString(),
                    GetLinkerTime(assembly),
                    debugBuild
                );
                services.AddSingleton(appInfo);

                var settingsFile = context.Configuration.GetValue<string>("SettingsFile") ?? "LedCube.Streamer.json";
                services.AddSettingsProvider<LedCubeStreamerSettings>("LedCube", settingsFile, LedCubeStreamerSettings.Default)
                    .AddSection(s => s.Cube, (s, v) => s with { Cube = v })
                    .AddSection(s => s.Connection, (s, v) => s with { Connection = v })
                    .AddSection(s => s.LastConnection, (s, v) => s with { LastConnection = v })
                    .AddSection(s => s.KeyboardControl, (s, v) => s with { KeyboardControl = v })
                    .AddSection(s => s.Library, (s, v) => s with { Library = v });
                services.AddLogAppenderControlViewModel(logAppenderControlSink);

                services.SetupPluginHost(_pluginHostContext);

                ConfigureServices(services);
            })
            .Build();

        WeakReferenceMessenger.Default.Register<OpenSimpleDialogMessage>(this);
        WeakReferenceMessenger.Default.Register<OpenBroadcastSearchDialogMessage>(this);
        WeakReferenceMessenger.Default.Register<OpenSelectAnimationDialogMessage>(this);
        WeakReferenceMessenger.Default.Register<EditAnimationInstanceDialogMessage>(this);
        WeakReferenceMessenger.Default.Register<OpenSettingsNavigationMessage>(this);
        WeakReferenceMessenger.Default.Register<ExitApplicationNavigationMessage>(this);
        WeakReferenceMessenger.Default.Register<OpenSettingsHotkeyInputDialogMessage>(this);

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<Controls.MainWindow.MainWindow>();
        _mainWindow = mainWindow;
        desktop.MainWindow = mainWindow;
        Log.Information("Application started!");
        mainWindow.Show();
    }

    private async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        using (_host)
        {
            if (_host is not null)
                await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICubeRepository, CubeRepository>();
        services.AddSingleton<Controls.MainWindow.MainViewModel>();
        services.AddSingleton<Controls.MainWindow.MainWindow>();
        services.AddSingleton<Controls.MenuBar.MenuBarViewModel>();
        services.AddSingleton<Controls.MenuBar.MenuBar>();
        services.AddSingleton<StreamingControlViewModel>();
        services.AddSingleton<StreamingControlView>();
        services.AddSingleton<CubeStreamingStatusViewModel>();
        services.AddSingleton<AnimationListViewModel>();
        services.AddSingleton<BroadcastSearchDialogViewModel>();
        services.AddSingleton<SelectAnimationDialogViewModel>();
        services.AddSingleton<EditAnimationInstanceDialogViewModel>();

        services.AddTransient<IUdpCubeCommunication, UdpCubeCubeCommunication>();
        services.AddSingleton<ICubeStreamingStatusMutable, CubeStreamingStatus>();
        services.AddSingleton<ICubeStreamingStatus>(p => p.GetRequiredService<ICubeStreamingStatusMutable>());
        services.AddSingleton<AnimationListViewModel>();
        services.AddSingleton<CubeStreamerService>();
        services.AddSingleton<ICubeStreamer>(p => p.GetRequiredService<CubeStreamerService>());
        services.AddHostedService(p => p.GetRequiredService<CubeStreamerService>());
        services.AddTransient<Func<IUdpCubeCommunication>>(x => () => x.GetRequiredService<IUdpCubeCommunication>());

        services.AddSingleton<PlaybackService>();
        services.AddSingleton<IPlaybackService>(p => p.GetRequiredService<PlaybackService>());
        services.AddHostedService(p => p.GetRequiredService<PlaybackService>());
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IPlaylistEntryFactory, PlaylistEntryFactory>();
        services.AddSingleton<LibraryService>();
        services.AddSingleton<ILibraryService>(p => p.GetRequiredService<LibraryService>());
        services.AddHostedService(p => p.GetRequiredService<LibraryService>());
        services.AddSingleton<PlaylistControlViewModel>();
        services.AddSingleton<PlaylistControl>();
        services.AddSingleton<PluginConfigControlViewModel>();
        services.AddSingleton<PluginConfigControl>();
        services.AddSingleton<PlaybackControlViewModel>();
        services.AddSingleton<PlaybackControl>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddTransient<SettingsDialogViewModel>(sp => new SettingsDialogViewModel(
            sp.GetRequiredService<ISettingsProvider<CubeSettings>>(),
            sp.GetService<ISettingsProvider<CubeStreamerSettings>>(),
            sp.GetService<ISettingsProvider<Cube3DDrawingConfig>>(),
            sp.GetRequiredService<IHotkeyService>()));
        services.AddSingleton<SettingsHotkeyInputDialogViewModel>();
        services.AddSingleton<SettingsHotkeyInputDialog>();
    }

    public static DateTime GetLinkerTime(Assembly? targetAssembly = null)
    {
        const string buildVersionMetadataPrefix = "+build";
        var assembly = targetAssembly ?? typeof(App).Assembly;
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(buildVersionMetadataPrefix, StringComparison.CurrentCulture);
            if (index > 0)
            {
                value = value[(index + buildVersionMetadataPrefix.Length)..];
                return DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss:fffZ", CultureInfo.InvariantCulture);
            }
        }
        return DateTime.MinValue;
    }

    // NOTE: Dialog results are communicated back via message.Result.
    // Since Avalonia ShowDialog is async, these handlers are async void.
    // Callers using synchronous WeakReferenceMessenger.Send + immediate message.Result
    // reads will need to be refactored to an async dialog service pattern.

    public async void Receive(OpenSimpleDialogMessage message)
    {
        try
        {
            Log.Information("Showing SimpleDialog");
            var viewModel = new SimpleDialogViewModel()
            {
                Buttons = message.Buttons,
                Text = message.Text,
                Title = message.Title
            };
            var dialog = new SimpleDialogWindow() { DataContext = viewModel };
            await dialog.ShowDialog(_mainWindow!);
            message.Result = viewModel.Result;
        }
        finally
        {
            message.Completion.TrySetResult();
        }
    }

    public async void Receive(OpenBroadcastSearchDialogMessage message)
    {
        try
        {
            Log.Information("Showing BroadcastSearchDialog");
            var viewModel = _host!.Services.GetRequiredService<BroadcastSearchDialogViewModel>();
            var window = new BroadcastSearchDialogWindow() { DataContext = viewModel };
            await window.ShowDialog(_mainWindow!);
            message.DialogResult = viewModel.DialogResult;
        }
        finally
        {
            message.Completion.TrySetResult();
        }
    }

    public async void Receive(OpenSelectAnimationDialogMessage message)
    {
        try
        {
            Log.Information("Showing SelectAnimationDialog");
            var viewModel = _host!.Services.GetRequiredService<SelectAnimationDialogViewModel>();
            var window = new SelectAnimationDialog() { DataContext = viewModel };
            await window.ShowDialog(_mainWindow!);
            message.Result = viewModel.DialogResult;
        }
        finally
        {
            message.Completion.TrySetResult();
        }
    }

    public async void Receive(EditAnimationInstanceDialogMessage message)
    {
        try
        {
            Log.Information("Showing EditAnimationInstanceDialog");
            var viewModel = _host!.Services.GetRequiredService<EditAnimationInstanceDialogViewModel>();
            viewModel.Message = message;
            var window = new EditAnimationInstanceDialog() { DataContext = viewModel };
            await window.ShowDialog(_mainWindow!);
            message.Result = viewModel.DialogResult;
        }
        finally
        {
            message.Completion.TrySetResult();
        }
    }

    public async void Receive(OpenSettingsNavigationMessage message)
    {
        Log.Information("Showing SettingsDialog");
        var viewModel = _host!.Services.GetRequiredService<SettingsDialogViewModel>();
        var window = new SettingsDialog() { DataContext = viewModel };
        viewModel.CloseAction = window.Close;
        await window.ShowDialog(_mainWindow!);
    }

    public async void Receive(OpenSettingsHotkeyInputDialogMessage message)
    {
        try
        {
            Log.Information("Showing SettingsHotkeyInputDialog");
            var viewModel = _host!.Services.GetRequiredService<SettingsHotkeyInputDialogViewModel>();
            viewModel.Function = message.Function;
            viewModel.Description = message.Description;
            viewModel.Reset();
            var window = new SettingsHotkeyInputDialog() { DataContext = viewModel };
            await window.ShowDialog(_mainWindow!);
            message.DialogResult = viewModel.DialogResult;
            if (viewModel.DialogResult == true)
                message.ResultBinding = viewModel.CapturedBinding;
        }
        finally
        {
            message.Completion.TrySetResult();
        }
    }

    public void Receive(ExitApplicationNavigationMessage message)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}
