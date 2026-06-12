using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.UI.CubeView3D;

namespace LedCube.Core.UI.Controls.Cube3DWrapper;

/// <summary>
/// ViewModel for <see cref="Cube3DWrapperView"/>. Bundles the 3D preview control with its settings band,
/// exposing the underlying <see cref="CubeView3D.Cube3DViewModel"/> the view binds against.
/// </summary>
public partial class Cube3DWrapperViewModel : ObservableObject
{
    public Cube3DViewModel Cube3DViewModel { get; }

    public Cube3DWrapperViewModel(Cube3DViewModel cube3DViewModel)
    {
        Cube3DViewModel = cube3DViewModel;
    }
}
