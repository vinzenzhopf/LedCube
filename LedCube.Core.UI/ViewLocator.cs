using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI;

/// <summary>
/// Convention-based view locator used as an application-level <see cref="IDataTemplate"/>.
/// A <c>ContentControl</c> bound to a view-model resolves and hosts the matching view, with the
/// presenter setting the view's DataContext directly — avoiding the inherited-DataContext cast
/// errors that occur when a typed child control's DataContext is injected from a differently-typed parent.
/// Resolution: strip the "ViewModel" suffix, then probe the VM's own assembly for a type named
/// &lt;Base&gt;, &lt;Base&gt;View, &lt;Base&gt;Control or &lt;Base&gt;Window.
/// </summary>
public class ViewLocator : IDataTemplate
{
    private static readonly string[] Suffixes = ["", "View", "Control", "Window"];
    private readonly Dictionary<Type, Type?> _cache = new();

    public Control Build(object? data)
    {
        if (data is null)
            return new TextBlock { Text = "(null)" };

        var viewType = ResolveViewType(data.GetType());
        return viewType is null
            ? new TextBlock { Text = $"View not found for {data.GetType().FullName}" }
            : (Control)Activator.CreateInstance(viewType)!;
    }

    public bool Match(object? data) =>
        data is ObservableObject && data.GetType().Name.EndsWith("ViewModel", StringComparison.Ordinal);

    private Type? ResolveViewType(Type vmType)
    {
        if (_cache.TryGetValue(vmType, out var cached))
            return cached;

        Type? found = null;
        if (vmType.FullName is { } fullName && fullName.EndsWith("ViewModel", StringComparison.Ordinal))
        {
            var baseName = fullName[..^"ViewModel".Length];
            foreach (var suffix in Suffixes)
            {
                if (vmType.Assembly.GetType(baseName + suffix) is { } candidate
                    && typeof(Control).IsAssignableFrom(candidate))
                {
                    found = candidate;
                    break;
                }
            }
        }

        _cache[vmType] = found;
        return found;
    }
}
