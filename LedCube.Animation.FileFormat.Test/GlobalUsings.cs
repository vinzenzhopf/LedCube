global using Xunit;

// The root type 'Animation' shares a simple name with the 'LedCube.Animation' namespace
// segment, so it cannot be referenced unqualified from inside this assembly. Alias it.
global using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;
