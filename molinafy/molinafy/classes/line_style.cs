using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace molinafy {
  public class line_style {
    protected Font _font = null;
    protected Color? _fore_color, _bg_color;

    public Font font { get { return _font; } }
    public Color? fore_color { get { return _fore_color; } }
    public Color? bg_color { get { return _bg_color; } }

    public line_style (Font fnt = null, Color? fc = null, Color? bc = null) {
      _font = fnt; _fore_color = fc; _bg_color = bc;
    }
  }
}
