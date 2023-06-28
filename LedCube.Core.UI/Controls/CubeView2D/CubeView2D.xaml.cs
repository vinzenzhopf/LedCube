using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.UI.Controls.CubeView2D;

public partial class CubeView2D : UserControl
{
    public CubeView2D()
    {
        InitializeComponent();
    }
}