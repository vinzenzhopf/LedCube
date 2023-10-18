using LedCube.Streamer.UdpCom;

namespace LedCube.Core.UI.Dialog.BroadcastSearchDialog;

public record BroadcastSearchDialogResult(HostAndPort? HostAndPort, bool? DialogResult);