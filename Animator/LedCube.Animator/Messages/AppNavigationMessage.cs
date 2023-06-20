using System;

namespace LedCube.Animator.Messages;

public class AppNavigationMessage
{
    public string Target { get; }
    public object? Sender { get; }

    public AppNavigationMessage(string target, object? sender = null)
    {
        Target = target;
        Sender = sender;
    }
}