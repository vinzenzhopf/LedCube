using System.Net;
using LedCube.Core.UI.Util;

namespace LedCube.Core.UI.Dialog.BroadcastSearchDialog;

public class NetworkAdapterViewModel
{
    public string Name { get; }
    public string Description { get; }
    public IPAddress Address { get; }

    public string TextSimple => $"{Address}";
    public string TextExtended => $"{Address} - {Name} \"{Description.Trim()}\"";

    public NetworkAdapterViewModel(string name, string description, IPAddress address)
    {
        Name = name;
        Description = description;
        Address = address;
    }

    public NetworkAdapterViewModel(NetworkAdapter adapter)
    {
        Name = adapter.Name;
        Description = adapter.Description;
        Address = adapter.Address;
    }
}