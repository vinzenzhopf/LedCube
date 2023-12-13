using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core;
using LedCube.Core.Config;
using LedCube.Core.CubeData.Repository;
using LedCube.Core.Settings;
using LedCube.Core.UI.Controls.CubeView2D;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Dialog;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Core.UI.Dialog.SimpleDialog;
using LedCube.PluginBase;
using LedCube.PluginHost;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.SmallUI.Controls.StreamingControl;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using SimpleDialogWindow = LedCube.Core.UI.Dialog.SimpleDialog.SimpleDialogWindow;

namespace LedCube.Streamer.SmallUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IRecipient<OpenSimpleDialogMessage>, IRecipient<OpenBroadcastSearchDialogMessage>
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            #if DEBUG
                const bool debugBuild = true;
            #else
                const bool debugBuild = false;
            #endif
            
            Log.Verbose("Starting HostBuilder built...");
            _host = new HostBuilder()
                .ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(context.HostingEnvironment.ContentRootPath);
                    configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    configurationBuilder.AddEnvironmentVariables();
                    configurationBuilder.AddCommandLine(e.Args);
                })
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    var logFile = context.Configuration.GetValue<string>("LogFile") ?? "LedCube.Steamer.SmallUI.log";
                    var logAppenderControlSink = new LogAppenderControlSink();
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
                    
                    loggingBuilder.AddSerilog(logger, true);
                    loggingBuilder.Services.AddSingleton<Serilog.ILogger>(logger);
                    loggingBuilder.Services.AddSingleton<ILoggerFactory>(loggerFactory);
                    loggingBuilder.Services.AddSingleton(logAppenderControlSink);
                })
                .ConfigureServices((context, services) =>
                    {
                        //Initialize UserSettings
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
                        
                        // services.AddSingleton<IConfiguration>(_configurationRoot);
                        services.AddSingleton<ISettingsProvider<LedCubeStreamerSettings>>(settingsProvider);
                        services.AddSingleton<ISettings<LedCubeStreamerSettings>>(settingsProvider);
                        services.AddSingleton<ICubeConfigRepository>(settingsProvider.Settings);
                        services.AddSingleton<LogAppenderViewModel>();
                        // services.AddSingleton<NavigationController>();
                        
                        
                        // services.SetupPluginHost(context.Configuration);
                        SetupPluginHost(services, context.Configuration);
                        
                        ConfigureServices(services);
                    })
                .Build();
            Log.Verbose("HostBuilder built!");
            
            base.OnStartup(e);
        }

        public static void SetupPluginHost(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PluginOptions>(
                configuration.GetSection(PluginOptions.Key));

            var assemblies = Directory
                .GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories)
                .Select(Assembly.LoadFrom)
                .ToList();

            // var frameGeneratorTypes = assemblies
            //     .SelectMany(a => a.DefinedTypes
            //         .Where(x =>
            //             typeof(IFrameGenerator).IsAssignableFrom(x) &&
            //             !x.IsInterface &&
            //             !x.IsAbstract))
            //     .ToList();
            //
            // foreach (var type in frameGeneratorTypes)
            // {
            //     services.AddTransient(typeof(IFrameGenerator), type);
            // }
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            WeakReferenceMessenger.Default.Register<OpenSimpleDialogMessage>(this);
            WeakReferenceMessenger.Default.Register<OpenBroadcastSearchDialogMessage>(this);
            
            await _host!.StartAsync();
            
            //Start Application
            var mainWindow = _host.Services.GetService<Controls.MainWindow.MainWindow>();
            
            Log.Information("Application started!");
            mainWindow!.Show();
        }
        
        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            using (_host)
            {
                await _host?.StopAsync(TimeSpan.FromSeconds(5))!;
            }
        }
        
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICubeRepository, CubeRepository>();
            services.AddSingleton<Controls.MainWindow.MainViewModel>();
            services.AddSingleton<Controls.MainWindow.MainWindow>();
            services.AddSingleton<Controls.MenuBar.MenuBarViewModel>();
            services.AddSingleton<Controls.MenuBar.MenuBar>();
            services.AddSingleton<CubeView2DViewModel>();
            services.AddSingleton<CubeView2D>();
            services.AddSingleton<StreamingControlViewModel>();
            services.AddSingleton<StreamingControl>();
            services.AddSingleton<BroadcastSearchDialogViewModel>();

            services.AddTransient<IUdpCubeCommunication, UdpCubeCubeCommunication>();
            services.AddSingleton<ICubeStreamingStatusMutable, CubeStreamingStatus>();
            services.AddSingleton<ICubeStreamingStatus>(p => p.GetService<ICubeStreamingStatusMutable>()!);
            services.AddSingleton<CubeStreamingStatusViewModel>();
            services.AddSingleton<CubeStreamerService>();
            services.AddSingleton<ICubeStreamer>(p => p.GetService<CubeStreamerService>()!);
            services.AddHostedService(p => p.GetService<CubeStreamerService>()!);
            services.AddTransient<Func<IUdpCubeCommunication>>(
                x => () => x.GetService<IUdpCubeCommunication>()!);

            // services.AddSingleton<AnimationListViewModel>();
            // services.AddSingleton<AnimationList>();
            // services.AddSingleton<PlaybackControlViewModel>();
            // services.AddSingleton<PlaybackControl>();

            // services.AddTransient<Controls.SettingsWindow.SettingsViewModel>();
            // services.AddTransient<Controls.SettingsWindow.SettingsWindow>();
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
                    value = value.Substring((index + buildVersionMetadataPrefix.Length));
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
            var dialog = new SimpleDialogWindow()
            {
                DataContext = viewModel
            };
            this.Dispatcher.Invoke(() => dialog.ShowDialog());
            message.Result = viewModel.Result;
        }

        public void Receive(OpenBroadcastSearchDialogMessage message)
        {
            Log.Information("Showing BroadcastSearchDialog");
            var viewModel = _host!.Services.GetService<BroadcastSearchDialogViewModel>();
            var window = new BroadcastSearchDialogWindow()
            {
                DataContext = viewModel
            };
            this.Dispatcher.Invoke(() => window.ShowDialog());
            message.DialogResult = viewModel!.DialogResult;
        }
    }
}