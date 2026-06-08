using LedCube.Core.Common.Model;

namespace LedCube.PluginBase;

/// <summary>
/// Cube metadata handed to <see cref="FrameGeneratorInfo"/> estimators so they can compute frame
/// time / frame count without instantiating the generator. A record struct so more fields (LED
/// format, orientation, ...) can be added later without breaking estimator signatures.
/// </summary>
public readonly record struct CubeInfo(Point3D Size);
