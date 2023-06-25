using System;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;

namespace LedCube.Test;

public abstract class TestWithLoggingBase : IDisposable
{
    private readonly SerilogLoggerFactory _loggerFactory;

    protected ILoggerFactory LoggerFactory => _loggerFactory;

    protected TestWithLoggingBase(ITestOutputHelper output)
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.TestOutput(output)
            .MinimumLevel.Debug()
            .CreateLogger();
        Log.Logger = logger;
        _loggerFactory = new SerilogLoggerFactory(logger, true);
    }
    
    public virtual void Dispose()
    {
        _loggerFactory.Dispose();
    }
}