using System;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Plugin.Base;

public abstract class FrameGeneratorBase : IDisposable
{
	public TimeSpan FrameTime { get; set; }

	public abstract void Initialize(GeneratorCubeConfiguration config);

	public abstract void AnimationStart();

	public abstract void DrawFrame(FrameContext frameContext);

	public abstract void AnimationEnd();

	public abstract void Dispose();
}

public class FrameGenerator : FrameGeneratorBase
{
	private GeneratorCubeConfiguration Config { get; set; }
    
	public override void Initialize(GeneratorCubeConfiguration config)
	{
		Config = config;
	}

	public override void AnimationStart()
	{
		throw new NotImplementedException();
	}

	public override void DrawFrame(FrameContext frameContext)
	{
		throw new NotImplementedException();
	}

	public override void AnimationEnd()
	{
		throw new NotImplementedException();
	}

	public override void Dispose()
	{
		throw new NotImplementedException();
	}
}

public record GeneratorCubeConfiguration(
	CubeDimensions Dimensions	
);

public record FrameContext(
	TimeSpan TargetFrameTime,
	TimeSpan LastFrameTime,
	ICubeDataBuffer Buffer
);