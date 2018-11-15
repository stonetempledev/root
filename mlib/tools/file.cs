using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mlib.tools {
  public class file {
    protected string _path;
    public file (string path) { _path = path; }

    public string path { get { return _path; } }
    public DateTime lw { get { return File.GetLastWriteTime(_path); } }
    public string ext { get { return Path.GetExtension(_path); } }
    public string file_name { get { return Path.GetFileName(_path); } }
    public string dir_name { get { return Path.GetDirectoryName(_path); } }
    public long size { get { return new System.IO.FileInfo(_path).Length; } }

    public static List<file> dir (string path, string pattern, bool order_by_lw = false, bool order_by_name = false) {
      List<file> res = new List<file>();
      foreach (string f in Directory.EnumerateFiles(path, pattern))
        res.Add(new file(f));
      if (order_by_lw) return res.OrderByDescending(f => f.lw.ToString("yyyy/MM/dd HH:mm:ss")).ToList();
      else if (order_by_name) return res.OrderBy(f => f.file_name).ToList();
      return res;
    }
  }
}
