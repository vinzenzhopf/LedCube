using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LedCube.Streamer.Test;

public abstract partial class TestWithLoggingBase : IDisposable
{
    private readonly SerilogLoggerFactory _loggerFactory;

    protected ILogger Logger { get; }

    protected ILoggerFactory LoggerFactory => _loggerFactory;
    
    protected TestWithLoggingBase(ITestOutputHelper output)
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.TestOutput(output, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .MinimumLevel.Debug()
            .CreateLogger();
        _loggerFactory = new SerilogLoggerFactory(logger, true);
        Logger = _loggerFactory.CreateLogger(GetType());
    }
    
    public virtual void Dispose()
    {
        _loggerFactory.Dispose();
    }
}
