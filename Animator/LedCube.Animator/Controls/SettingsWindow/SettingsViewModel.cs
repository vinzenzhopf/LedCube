using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Animator.Settings;
using LedCube.Core.Common.Settings;

namespace LedCube.Animator.Controls.SettingsWindow;

[ObservableObject]
public partial class SettingsViewModel
{
    private readonly ISettingsProvider<LedCubeAnimatorSettings> _settingsProvider;

    [ObservableProperty]
    private LedCubeAnimatorSettings _currentSettings = new();

    [ObservableProperty]
    private bool _saveEnabled = true;
    
    public Action? CloseAction { get; set; }

    public SettingsViewModel(ISettingsProvider<LedCubeAnimatorSettings> settingsProvider)
    {
        _settingsProvider = settingsProvider;
        _currentSettings = new LedCubeAnimatorSettings(_settingsProvider.Settings);
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseInternal();
    }
    
    [RelayCommand(CanExecute = nameof(SaveEnabled))]
    private void OnClosing()
    {
    }
    
    [RelayCommand(CanExecute = nameof(SaveEnabled))]
    private void Save()
    {
        SaveInternal();
        CloseInternal();
    }
    
    [RelayCommand(CanExecute = nameof(SaveEnabled))]
    private void Apply()
    {
        SaveInternal();
    }

    private void SaveInternal()
    {
        //TODO: Validate new Settings
        _settingsProvider.SaveAndUpdate(_currentSettings);
    }
    
    private void CloseInternal()
    {
        CloseAction?.Invoke();
    }
}