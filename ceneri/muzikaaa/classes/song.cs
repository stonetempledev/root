using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace muzikaaa.classes
{
  public class song
  {
    protected string _path, _cestone;

    public string path { get { return _path; } }
    public string cestone { get { return _cestone; } }

    public song(string path, string cestone) { _path = path; _cestone = cestone; }
  }
}
