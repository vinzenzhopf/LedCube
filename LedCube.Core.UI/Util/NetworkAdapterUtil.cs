using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace LedCube.Core.UI.Util;

public static class NetworkAdapterUtil
{
    public static List<NetworkAdapter> GetAdapters()
    {
        var list = new List<NetworkAdapter>();
        
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                ni.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) 
                continue;
            var ipProperties = ni.GetIPProperties();
            if(ni.OperationalStatus != OperationalStatus.Up)
                continue;
            foreach (var ip in ipProperties.UnicastAddresses
                         .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
            {
                list.Add(new NetworkAdapter()
                {
                    Name = ni.Name,
                    Description = ni.Description,
                    Type = ni.NetworkInterfaceType,
                    Address = ip.Address
                });
            }
        }
        return list;
    }

    public static string AsString(this NetworkAdapter adapter)
    {
        return $"{adapter.Address} - {adapter.Name} \"{adapter.Description}\"";
    }
}

[Serializable]
public class NetworkAdapter
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public IPAddress Address { get; set; } = IPAddress.Any;
    public NetworkInterfaceType Type { get; set; }

    public override string ToString()
    {
        return $"{Address} - {Name} \"{Description}\"";
    }
}