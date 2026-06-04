using System.Linq;
using LedCube.Core.UI.Controls.AnimationList;
using LedCube.PluginBase;
using LedCube.PluginHost;

namespace LedCube.Core.UI.Services.Playlist;

/// <summary>Builds <see cref="PlaylistEntry"/> instances from library/plugin animation list entries.</summary>
public interface IPlaylistEntryFactory
{
    /// <summary>Creates a playlist entry for the given list entry, or null if it cannot be mapped.</summary>
    PlaylistEntry? FromAnimationListEntry(AnimationListEntryViewModel entry);
}

public class PlaylistEntryFactory(IPluginManager pluginManager) : IPlaylistEntryFactory
{
    public PlaylistEntry? FromAnimationListEntry(AnimationListEntryViewModel entry)
    {
        if (entry.AnimationType == AnimationListEntryType.PluginAnimation)
        {
            if (entry.GeneratorEntry is null)
                return null;
            return new PlaylistEntry(entry.GeneratorEntry.Info, entry.GeneratorEntry.TypeInfo)
            {
                InstanceName = entry.Name,
            };
        }

        // File animation: route it through the FileAnimation generator with its FilePath config set.
        var fileGen = FindFileAnimationGenerator();
        if (fileGen is null)
            return null;

        var playlistEntry = new PlaylistEntry(fileGen.Info, fileGen.TypeInfo)
        {
            InstanceName = string.IsNullOrWhiteSpace(entry.Name) ? entry.FileName : entry.Name,
        };

        var filePathKey = FindFilePathKey(fileGen.Info);
        if (filePathKey is not null)
            playlistEntry.Config[filePathKey] = entry.FilePath;

        return playlistEntry;
    }

    // The FileAnimation plugin is identified structurally — the only generator that exposes a
    // FilePath config descriptor — so Core.UI need not reference the plugin assembly directly.
    private FrameGeneratorEntry? FindFileAnimationGenerator() =>
        pluginManager.AllFrameGeneratorInfos()
            .FirstOrDefault(g => g.Info.ConfigDescriptors?.Any(d => d.Type == AnimationConfigType.FilePath) == true);

    private static string? FindFilePathKey(FrameGeneratorInfo info) =>
        info.ConfigDescriptors?.FirstOrDefault(d => d.Type == AnimationConfigType.FilePath)?.Key;
}
