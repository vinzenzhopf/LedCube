using System;
using System.Globalization;
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
using LedCube.Core.UI.Controls.AnimationTest;
using LedCube.Core.UI.Controls.CubeView2D;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Controls.PlaylistControl;
using LedCube.Core.UI.Controls.StreamingControl;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;
using LedCube.Core.UI.Dialog.SelectAnimationDialog;
using LedCube.Core.UI.Dialog.SimpleDialog;
using LedCube.Core.UI.Messages;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginHost;
using LedCube.Streamer.AnimationTestUI.Controls.MainWindow;
using LedCube.Streamer.AnimationTestUI.Controls.MenuBar;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Logging;
using CubeStreamingStatus = LedCube.Streamer.CubeStreamer.CubeStreamingStatus;

namespace LedCube.Streamer.AnimationTestUI;

public partial class App : Application,
    IRecipient<OpenSimpleDialogMessage>,
    IRecipient<OpenBroadcastSearchDialogMessage>,
    IRecipient<OpenSelectAnimationDialogMessage>,
    IRecipient<EditAnimationInstanceDialogMessage>
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
                var logFile = context.Configuration.GetValue<string>("LogFile") ?? "LedCube.Steamer.AnimationTestUI.log";
                var logger = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Debug()
                    .WriteTo.File(logFile)
                    .WriteTo.Debug()
                    .WriteTo.LogAppenderControlSink(logAppenderControlSink)
                    .CreateLogger();
                Log.Logger = logger;
                var loggerFactory = new SerilogLoggerFactory(logger, false);
                Log.Verbose("Logger initialized. Logging to {0}", logFile);
                logging.AddSerilog(logger, dispose: true);
                logging.Services.AddSingleton<Serilog.ILogger>(logger);
                logging.Services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(loggerFactory);
                logging.Services.AddSingleton(logAppenderControlSink);
            })
            .ConfigureServices((context, services) =>
            {
                var settingsFile = context.Configuration.GetValue<string>("SettingsFile") ?? "LedCube.Streamer.json";
                var settingsProvider = new SettingsProvider<LedCubeStreamerSettings>("LedCube", settingsFile);
                settingsProvider.Load(LedCubeStreamerSettings.Default);

                var assembly = Assembly.GetExecutingAssembly();
                var appInfo = new AppInfo(
                    assembly.GetName().Version!.ToString(),
                    GetLinkerTime(assembly),
                    debugBuild
                );
                services.AddSingleton(appInfo);
                services.AddSingleton<ISettingsProvider<LedCubeStreamerSettings>>(settingsProvider);
                services.AddSingleton<ISettings<LedCubeStreamerSettings>>(settingsProvider);
                services.AddSingleton<ISettingsProvider<LastConnectionSettings>>(
                    new SectionSettingsProvider<LedCubeStreamerSettings, LastConnectionSettings>(
                        settingsProvider, s => s.LastConnection, (s, v) => s with { LastConnection = v }));
                services.AddLogAppenderControlViewModel(logAppenderControlSink);
                services.SetupPluginHost(_pluginHostContext);
                ConfigureServices(services);
            })
            .Build();

        WeakReferenceMessenger.Default.Register<OpenSimpleDialogMessage>(this);
        WeakReferenceMessenger.Default.Register<OpenBroadcastSearchDialogMessage>(this);
        WeakReferenceMessenger.Default.Register<OpenSelectAnimationDialogMessage>(this);
        WeakReferenceMessenger.Default.Register<EditAnimationInstanceDialogMessage>(this);

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
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
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MenuBarViewModel>();
        services.AddSingleton<MenuBar>();
        services.AddSingleton<CubeView2DViewModel>();
        services.AddSingleton<CubeView2D>();
        services.AddSingleton<StreamingControlViewModel>();
        services.AddSingleton<StreamingControlView>();
        services.AddSingleton<AnimationTestViewModel>();
        services.AddSingleton<AnimationTest>();

        services.AddSingleton<BroadcastSearchDialogViewModel>();
        services.AddSingleton<SelectAnimationDialogViewModel>();
        services.AddSingleton<EditAnimationInstanceDialogViewModel>();

        services.AddTransient<IUdpCubeCommunication, UdpCubeCubeCommunication>();
        services.AddSingleton<ICubeStreamingStatusMutable, CubeStreamingStatus>();
        services.AddSingleton<ICubeStreamingStatus>(p => p.GetRequiredService<ICubeStreamingStatusMutable>());
        services.AddSingleton<CubeStreamingStatusViewModel>();
        services.AddSingleton<CubeStreamerService>();
        services.AddSingleton<ICubeStreamer>(p => p.GetRequiredService<CubeStreamerService>());
        services.AddHostedService(p => p.GetRequiredService<CubeStreamerService>());
        services.AddTransient<Func<IUdpCubeCommunication>>(x => () => x.GetRequiredService<IUdpCubeCommunication>());

        services.AddSingleton<PlaybackService>();
        services.AddSingleton<IPlaybackService>(p => p.GetRequiredService<PlaybackService>());
        services.AddHostedService(p => p.GetRequiredService<PlaybackService>());
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<PlaylistControlViewModel>();
        services.AddSingleton<PlaylistControl>();
        services.AddSingleton<PlaybackControlViewModel>();
        services.AddSingleton<PlaybackControl>();
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
}
