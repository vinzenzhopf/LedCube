using System;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Animator.Controls.SettingsWindow;
using LedCube.Animator.Messages;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LedCube.Animator;

public class NavigationController : 
    IRecipient<ExitApplicationNavigationMessage>, 
    IRecipient<OpenSettingsNavigationMessage>
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(ExitApplicationNavigationMessage message)
    {
        Log.Debug("Received ExitApplicationNavigationMessage {0}, Sender: {1}", message.Target, message.Sender);
        Application.Current.Shutdown();
    }

    public void Receive(OpenSettingsNavigationMessage message)
    {
        Log.Debug("Received OpenSettingsNavigationMessage {0}, Sender: {1}", message.Target, message.Sender);
        var settingsWindow = _serviceProvider.GetService<SettingsWindow>();

        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
        {
            settingsWindow!.ShowDialog();
        });
    }
}