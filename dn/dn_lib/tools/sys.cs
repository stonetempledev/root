using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dn_lib.tools
{
  public class sys
  {
    public static string machine_name(bool lower = true)
    {
      try {
        return lower ? System.Environment.MachineName.ToLower() : System.Environment.MachineName;
      } catch { return ""; }
    }

    public static string machine_ip(string def = "")
    {
      try {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach(var ip in host.AddressList) {
          if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            return ip.ToString();
        }
        return def;
      } catch { return def; }
    }

    public static DateTime without_ms(DateTime from)
    {
      return new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second);
    }
  }
}
