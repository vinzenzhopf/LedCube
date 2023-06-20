using System;

namespace LedCube.Animator;

public interface ICloseAction
{
    bool CanClose { get; }
    Action CloseAction { get; set; }
}