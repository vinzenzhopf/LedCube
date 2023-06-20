using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;

namespace LedCube.Animator.Controls.LogAppender;

public static class LogAppenderControlExtensions
{
    public static LoggerConfiguration LogAppenderControlSink(
        this LoggerSinkConfiguration loggerConfiguration,
        LogAppenderControlSink? logAppenderControlSink = null,
        IFormatProvider? formatProvider = null)
    {
        logAppenderControlSink ??= new LogAppenderControlSink()
        {
            FormatProvider = formatProvider
        };
        return loggerConfiguration.Sink(logAppenderControlSink);
    }

    public static IServiceCollection AddLogAppenderControlViewModel(
        this IServiceCollection services,
        LogAppenderControlSink logAppenderControlSink)
    {
        return services
            .AddSingleton(logAppenderControlSink)
            .AddSingleton<LogAppenderViewModel>();
    }
}