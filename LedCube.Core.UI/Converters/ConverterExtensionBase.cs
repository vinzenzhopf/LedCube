using System;
using Avalonia.Markup.Xaml;

namespace LedCube.Core.UI.Converters;

public abstract class ConverterExtensionBase : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
