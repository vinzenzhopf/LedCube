using System;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Core.Common.Settings;

public static class SettingsServiceCollectionExtensions
{
    public static SettingsProviderBuilder<T> AddSettingsProvider<T>(
        this IServiceCollection services,
        string applicationName,
        string filename,
        T? defaultSettings = null)
        where T : class, new()
    {
        var provider = new SettingsProvider<T>(applicationName, filename);
        provider.Load(defaultSettings);
        return new SettingsProviderBuilder<T>(services, provider);
    }
}

public class SettingsProviderBuilder<TRoot> where TRoot : class, new()
{
    private readonly IServiceCollection _services;
    private readonly ISettingsProvider<TRoot> _provider;

    internal SettingsProviderBuilder(IServiceCollection services, ISettingsProvider<TRoot> provider)
    {
        _services = services;
        _provider = provider;
    }

    public SettingsProviderBuilder<TRoot> AddSection<TSection>(
        Func<TRoot, TSection> getter,
        Func<TRoot, TSection, TRoot> setter)
        where TSection : class, new()
    {
        _services.AddSingleton<ISettingsProvider<TSection>>(
            new SectionSettingsProvider<TRoot, TSection>(_provider, getter, setter));
        return this;
    }
}
