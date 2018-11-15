using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace deeper.frmwrk
{
  /// <summary>
  /// Summary description for fncs
  /// </summary>
  public static class ext_fncs
  {
    public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection) {
      if (collection == null) throw new ArgumentNullException("Collection is null");
      foreach (var item in collection) {
        if (!source.ContainsKey(item.Key))
          source.Add(item.Key, item.Value);
        else { }
      }
    }
  }
}