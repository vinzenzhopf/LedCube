namespace LedCube.Core.UI.Services.Library.Model;

public sealed record LibraryChangeEventArgs<T>(LibraryChangeKind Kind, T? NewEntry, T? OldEntry, string Key);