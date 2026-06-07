global using Xunit;

// The root type 'Animation' shares a simple name with the 'LedCube.Animation' namespace
// segment, so it cannot be referenced unqualified from inside this assembly. Alias it.
global using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

// The root type 'Playlist' shares a simple name with the 'LedCube.Animation.FileFormat.Playlist'
// namespace segment, so it cannot be referenced unqualified. Alias it.
global using PlaylistModel = LedCube.Animation.FileFormat.Playlist.Model.Playlist;
