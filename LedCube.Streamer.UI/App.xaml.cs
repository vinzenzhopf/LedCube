using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Core.Common.Settings;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Controls.SettingsDialog;
using LedCube.Core.UI.Controls.StreamingControl;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;
using LedCube.Core.UI.Dialog.SelectAnimationDialog;
using LedCube.Core.UI.Dialog.SimpleDialog;
using LedCube.Core.UI.Messages;
using LedCube.Core.UI.Services;
using LedCube.Core.UI.Services.Hotkey;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Settings;
using LedCube.PluginHost;
using LedCube.Streamer.UI.Settings;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SimpleDialogWindow = LedCube.Core.UI.Dialog.SimpleDialog.SimpleDialogWindow;

namespace LedCube.Streamer.UI
{
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

        protected override void OnStartup(StartupEventArgs e)
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
                    config.AddCommandLine(e.Args);
                    config.ConfigurePluginHost(_pluginHostContext);
                })
                .ConfigureLogging((context, logging) =>
                {
                    var logFile = context.Configuration.GetValue<string>("LogFile") ?? "LedCube.Streamer.UI.log";
                    var logger = new LoggerConfiguration()
                        .Enrich.WithThreadId()
                        .Enrich.FromLogContext()
                        .MinimumLevel.Debug()
                        .WriteTo.File(logFile)
                        .WriteTo.Debug()
                        .WriteTo.LogAppenderControlSink(logAppenderControlSink)
                        .CreateLogger();
                    Log.Logger = logger;
                    logging.AddSerilog(logger, dispose: true);
                    Log.Verbose("Logger initialized. Logging to {0}", logFile);
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

                    var hotkeySettingsProvider = new SettingsProvider<KeyboardControlConfig>("LedCube", "hotkeys.json");
                    hotkeySettingsProvider.Load();
                    services.AddSingleton<ISettingsProvider<KeyboardControlConfig>>(hotkeySettingsProvider);
                    services.AddSingleton<ISettingsFacade, StreamerSettingsFacade>();
                    services.AddLogAppenderControlViewModel(logAppenderControlSink);

                    services.SetupPluginHost(_pluginHostContext);

                    ConfigureServices(services);
                })
                .Build();

            Log.Verbose("HostBuilder built!");
            base.OnStartup(e);
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            WeakReferenceMessenger.Default.Register<OpenSimpleDialogMessage>(this);
            WeakReferenceMessenger.Default.Register<OpenBroadcastSearchDialogMessage>(this);
            WeakReferenceMessenger.Default.Register<OpenSelectAnimationDialogMessage>(this);
            WeakReferenceMessenger.Default.Register<EditAnimationInstanceDialogMessage>(this);
            WeakReferenceMessenger.Default.Register<OpenSettingsNavigationMessage>(this);
            WeakReferenceMessenger.Default.Register<ExitApplicationNavigationMessage>(this);
            WeakReferenceMessenger.Default.Register<OpenSettingsHotkeyInputDialogMessage>(this);

            await _host!.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<Controls.MainWindow.MainWindow>();
            Log.Information("Application started!");
            mainWindow.Show();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            using (_host)
            {
                await _host?.StopAsync(TimeSpan.FromSeconds(5))!;
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
            services.AddSingleton<StreamingControl>();
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

            services.AddSingleton<IPlaybackService, PlaybackService>();
            services.AddSingleton<AnimationListViewModel>();
            services.AddSingleton<AnimationList>();
            services.AddSingleton<PlaybackControlViewModel>();
            services.AddSingleton<PlaybackControl>();
            services.AddSingleton<IHotkeyService, HotkeyService>();
            services.AddTransient<SettingsDialogViewModel>();
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

        public void Receive(OpenSimpleDialogMessage message)
        {
            Log.Information("Showing SimpleDialog");
            var viewModel = new SimpleDialogViewModel()
            {
                Buttons = message.Buttons,
                Text = message.Text,
                Title = message.Title
            };
            var dialog = new SimpleDialogWindow() { DataContext = viewModel };
            Dispatcher.Invoke(() => dialog.ShowDialog());
            message.Result = viewModel.Result;
        }

        public void Receive(OpenBroadcastSearchDialogMessage message)
        {
            Log.Information("Showing BroadcastSearchDialog");
            var viewModel = _host!.Services.GetRequiredService<BroadcastSearchDialogViewModel>();
            var window = new BroadcastSearchDialogWindow() { DataContext = viewModel };
            Dispatcher.Invoke(() => window.ShowDialog());
            message.DialogResult = viewModel.DialogResult;
        }

        public void Receive(OpenSelectAnimationDialogMessage message)
        {
            Log.Information("Showing SelectAnimationDialog");
            var viewModel = _host!.Services.GetRequiredService<SelectAnimationDialogViewModel>();
            var window = new SelectAnimationDialog() { DataContext = viewModel };
            Dispatcher.Invoke(() => window.ShowDialog());
            message.Result = viewModel.DialogResult;
        }

        public void Receive(EditAnimationInstanceDialogMessage message)
        {
            Log.Information("Showing EditAnimationInstanceDialog");
            var viewModel = _host!.Services.GetRequiredService<EditAnimationInstanceDialogViewModel>();
            viewModel.Message = message;
            var window = new EditAnimationInstanceDialog() { DataContext = viewModel };
            Dispatcher.Invoke(() => window.ShowDialog());
            message.Result = viewModel.DialogResult;
        }

        public void Receive(OpenSettingsNavigationMessage message)
        {
            Log.Information("Showing SettingsDialog");
            var viewModel = _host!.Services.GetRequiredService<SettingsDialogViewModel>();
            var window = new SettingsDialog() { DataContext = viewModel };
            viewModel.CloseAction = window.Close;
            Dispatcher.Invoke(() => window.ShowDialog());
        }

        public void Receive(OpenSettingsHotkeyInputDialogMessage message)
        {
            Log.Information("Showing SettingsHotkeyInputDialog");
            var viewModel = _host!.Services.GetRequiredService<SettingsHotkeyInputDialogViewModel>();
            viewModel.Function = message.Function;
            viewModel.Description = message.Description;
            viewModel.Reset();
            var window = new SettingsHotkeyInputDialog() { DataContext = viewModel };
            Dispatcher.Invoke(() => window.ShowDialog());
            message.DialogResult = viewModel.DialogResult;
            if (viewModel.DialogResult == true)
                message.ResultBinding = viewModel.CapturedBinding;
        }
        
        public void Receive(ExitApplicationNavigationMessage message)
        {
            this.Shutdown();
        }
    }
}
