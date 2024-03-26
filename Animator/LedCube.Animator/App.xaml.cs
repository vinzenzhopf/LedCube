using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using LedCube.Animator.Controls.LogAppender;
using LedCube.Animator.Settings;
using LedCube.Core.Common;
using LedCube.Core.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LedCube.Animator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private IConfigurationRoot? _configurationRoot;

        protected override void OnStartup(StartupEventArgs e)
        {
            var basePath = Directory.GetCurrentDirectory();
#if DEBUG
            const bool debugBuild = true;
#else
            const bool debugBuild = false;
#endif
            
            //Initialize Configuration and AppSettings
            _configurationRoot = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(e.Args)
                .Build();
            
            //Initialize Logger
            var logFile = _configurationRoot.GetValue<string>("LogFile") ?? "LedCube.Animator.log";
            var logAppenderControlSink = new LogAppenderControlSink();
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.File(logFile)
                .WriteTo.Debug()
                .WriteTo.LogAppenderControlSink(logAppenderControlSink)
                .CreateLogger();
            Log.Verbose("Logger initialized. Logging to {0}", logFile);

            //Initialize UserSettings
            var settingsFile = _configurationRoot.GetValue<string>("SettingsFile") ?? "LedCube.Animator.json";
            var settingsProvider = new SettingsProvider<LedCubeAnimatorSettings>("LedCube", settingsFile);
            settingsProvider.Load(LedCubeAnimatorSettings.Default);

            var assembly = Assembly.GetExecutingAssembly();
            var appInfo = new AppInfo(
                assembly.GetName().Version!.ToString(),
                GetLinkerTime(assembly),
                debugBuild
            );

            //Build Service Collection
            var services = new ServiceCollection();
            services.AddSingleton(appInfo);
            services.AddSingleton<IConfiguration>(_configurationRoot);
            services.AddSingleton<ISettingsProvider<LedCubeAnimatorSettings>>(settingsProvider);
            services.AddSingleton<ISettings<LedCubeAnimatorSettings>>(settingsProvider);
            services.AddLogAppenderControlViewModel(logAppenderControlSink);
            services.AddSingleton<NavigationController>();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            Log.Verbose("ServiceProvider built!");
            
            
            //Start Application
            var mainWindow = _serviceProvider.GetService<Controls.MainWindow.MainWindow>();
            Log.Information("Application started!");
            mainWindow!.Show();
        }
        
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Controls.MainWindow.MainViewModel>();
            services.AddSingleton<Controls.MainWindow.MainWindow>();
            services.AddSingleton<Controls.MenuBar.MenuBarViewModel>();
            services.AddSingleton<Controls.MenuBar.MenuBar>();
            services.AddTransient<Controls.SettingsWindow.SettingsViewModel>();
            services.AddTransient<Controls.SettingsWindow.SettingsWindow>();
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
    }
}