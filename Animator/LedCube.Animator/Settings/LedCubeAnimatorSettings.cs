﻿using LedCube.Core.Config;

namespace LedCube.Animator.Settings;

public class LedCubeAnimatorSettings : ICubeConfigRepository
{
    public LedCubeAnimatorSettings()
    {
    }
    
    public LedCubeAnimatorSettings(LedCubeAnimatorSettings other)
    {
    }

    public CubeConfig CubeConfig { get; }
}