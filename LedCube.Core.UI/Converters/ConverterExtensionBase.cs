using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace LedCube.Core.UI.Converters;

[ValueConversion(typeof(object), typeof(object))]
public abstract class ConverterExtensionBase : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}