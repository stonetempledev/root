using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace deeper.frmwrk.ctrls
{
  // base ctrl
  public class ctrl
  {
    protected Control _ctrl = null;
    public Control control { get { return _ctrl; } }
    public WebControl w_ctrl {
      get {
        if (!(_ctrl is WebControl)) throw new Exception("controllo non gestito '" + _ctrl.GetType().ToString() + "'");
        return (WebControl)_ctrl;
      }
    }
    public HtmlControl h_ctrl {
      get {
        if (!(_ctrl is HtmlControl)) throw new Exception("controllo non gestito '" + _ctrl.GetType().ToString() + "'");
        return (HtmlControl)_ctrl;
      }
    }

    public ctrl(Control c) { assign(c); }

    public void assign(Control c) {
      if (c == null || !(c is Control)) throw new Exception(c == null ? "controllo non inizializzato"
        : "controllo non gestito '" + c.GetType().ToString() + "'");
      _ctrl = c;
    }

    public ctrl add(ctrl child) { _ctrl.Controls.Add(child.control); return child; }
    public Control add(Control child) { _ctrl.Controls.Add(child); return child; }

    public ctrl add_at(int i, ctrl child) { _ctrl.Controls.AddAt(i, child.control); return child; }
    public Control add_at(int i, Control child) { _ctrl.Controls.AddAt(i, child); return child; }

    public void add_style(object key, object value) { add_styles(new object[,] { { key, value } }); }
    public void add_styles(Dictionary<string, string> styles) {
      foreach (KeyValuePair<string, string> s in styles) { if (_ctrl is WebControl) w_ctrl.Style.Add(s.Key, s.Value); else h_ctrl.Style.Add(s.Key, s.Value); }
    }
    public void add_styles(object[,] values) { if (_ctrl is WebControl) new styles(values).add_toctrl(w_ctrl); else new styles(values).add_toctrl(h_ctrl); }

    public void add_attrs(string[,] values) { if (_ctrl is WebControl) new attrs(values).add_toctrl(w_ctrl); else new attrs(values).add_toctrl(h_ctrl); }
    public void add_attr(string key, string value) { add_attrs(new string[,] { { key, value } }); }
  }

  // Styles
  public class styles
  {
    Dictionary<object, string> _list = null;
    public styles(object[,] values = null, string expr = null) {
      _list = new Dictionary<object, string>();
      if (values != null) for (int i = 0; i < values.Length / 2; i++) _list.Add(values[i, 0], (string)values[i, 1]);
      if (expr != null) add_styles(expr);
    }

    public styles(string expr) : this(null, expr) { }

    /// <summary>
    /// Aggiunta stili contenuti nella stringa tipo: 'display:none;width:10;height:20'
    /// </summary>
    public void add_styles(string style) {
      foreach (string sstyle in style.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
        string[] value = sstyle.Split(':');
        _list.Add(value[0], value[1]);
      }
    }

    public void add_style(string key, string value) { _list.Add(key, value); }

    public void add_toctrl(WebControl ctrl) {
      foreach (KeyValuePair<object, string> k in _list.Where(x => !string.IsNullOrEmpty(x.Value)))
        if (k.Key is HtmlTextWriterStyle) ctrl.Style.Add((HtmlTextWriterStyle)k.Key, k.Value);
        else ctrl.Style.Add((string)k.Key, k.Value);
    }

    public void add_toctrl(HtmlControl ctrl) {
      foreach (KeyValuePair<object, string> k in _list.Where(x => !string.IsNullOrEmpty(x.Value)))
        if (k.Key is HtmlTextWriterStyle) ctrl.Style.Add((HtmlTextWriterStyle)k.Key, k.Value);
        else ctrl.Style.Add((string)k.Key, k.Value);
    }
  }

  // Attributes
  public class attrs
  {
    Dictionary<string, string> _list = null;
    public attrs(string[,] values) {
      _list = new Dictionary<string, string>();
      for (int i = 0; i < values.Length / 2; i++) _list.Add((string)values[i, 0], (string)values[i, 1]);
    }

    public void add_toctrl(WebControl ctrl) {
      foreach (KeyValuePair<string, string> k in _list.Where(x => !string.IsNullOrEmpty(x.Value)))
        ctrl.Attributes.Add(k.Key, k.Value);
    }
    public void add_toctrl(HtmlControl ctrl) {
      foreach (KeyValuePair<string, string> k in _list.Where(x => !string.IsNullOrEmpty(x.Value)))
        ctrl.Attributes.Add(k.Key, k.Value);
    }
  }

  // Table
  public class tbl : ctrl
  {
    public Table table { get { return (Table)_ctrl; } }

    public tbl(string id = null, string css_class = null, List<tbl_row> rows = null, attrs at = null , styles st = null, string caption = null)
      : base(new Table()) {

      if (!string.IsNullOrEmpty(id)) table.ID = id;
      if (!string.IsNullOrEmpty(css_class)) table.CssClass = css_class;
      if (rows != null) rows.ForEach(rw => { add(rw); });
      if (at != null) at.add_toctrl(table);
      if (st != null) st.add_toctrl(table);
      if (!string.IsNullOrEmpty(caption)) table.Caption = caption;
    }
    public tbl_row add_row(tbl_row row) { ((Table)_ctrl).Rows.Add(row.row); return row; }
  }

  // TableRow
  public class tbl_row : ctrl
  {
    public TableRow row { get { return (TableRow)_ctrl; } }

    public tbl_row(List<tbl_cell> cells = null, string tooltip = null, styles st = null)
      : base(new TableRow()) {
      if (cells != null) cells.ForEach(cl => { add(cl); });
      if (tooltip != null) row.ToolTip = tooltip;
      if (st != null) st.add_toctrl(row);
    }
    public tbl_cell add_cell(tbl_cell cl) { row.Cells.Add(cl.cell); return cl; }
  }

  // TableCell
  public class tbl_cell : ctrl
  {
    public TableCell cell { get { return (TableCell)_ctrl; } }

    public tbl_cell(string id = null, HorizontalAlign? align = null, List<ctrl> ctrls = null, attrs a = null
      , styles st = null, string css = "", int col_span = -1, string tooltip = "", HorizontalAlign hal = HorizontalAlign.NotSet
      , VerticalAlign val = VerticalAlign.NotSet)
      : base(new TableCell()) {
      if (!string.IsNullOrEmpty(id)) _ctrl.ID = id;
      if (align.HasValue) cell.HorizontalAlign = align.Value;
      if (ctrls != null) ctrls.ForEach(ctrl => { add(ctrl.control); });
      if (a != null) a.add_toctrl(cell);
      if (st != null) st.add_toctrl(cell);
      if (!string.IsNullOrEmpty(css)) cell.CssClass = css;
      if (col_span >= 0) cell.ColumnSpan = col_span;
      if (!string.IsNullOrEmpty(tooltip)) cell.ToolTip = tooltip;
      if (hal != HorizontalAlign.NotSet) cell.HorizontalAlign = hal;
      if (val != VerticalAlign.NotSet) cell.VerticalAlign = val;
    }

    public Control add(Control ctrl) { _ctrl.Controls.Add(ctrl); return ctrl; }
  }

  // Literal
  public class div : ctrl
  {
    public Literal literal { get { return (Literal)_ctrl; } }

    public div(string id = null, string txt = null, string style = null)
      : base(new Literal()) {
      if (!string.IsNullOrEmpty(id)) _ctrl.ID = id;
      if (!string.IsNullOrEmpty(txt)) ((Literal)_ctrl).Text = txt;
    }
  }

  // HyperLink
  public class link : ctrl
  {
    public HyperLink hlink { get { return (HyperLink)_ctrl; } }

    public link(string txt = null, string tooltip = null, string css_class = null, string url = null, attrs a = null
      , styles st = null, string id = null, string target = null)
      : base(new HyperLink()) {
      if (!string.IsNullOrEmpty(css_class)) hlink.CssClass = css_class;
      if (!string.IsNullOrEmpty(txt)) hlink.Text = txt;
      if (!string.IsNullOrEmpty(tooltip)) hlink.ToolTip = tooltip;
      if (a != null) a.add_toctrl(hlink);
      if (st != null) st.add_toctrl(hlink);
      if (!string.IsNullOrEmpty(url)) hlink.NavigateUrl = url;
      if (id != null) hlink.ID = id;
      if (target != null) hlink.Target = target;
    }

    public link(EventHandler eh, string css_class = null)
      : base(new HyperLink()) {
      if (!string.IsNullOrEmpty(css_class)) hlink.CssClass = css_class;
      hlink.DataBinding += eh;
    }
  }

  // Label
  public class label : ctrl
  {
    public Label lbl { get { return (Label)_ctrl; } }
    public label(EventHandler eh)
      : base(new Label()) {
      lbl.DataBinding += eh;
    }

    public label(string text, string css = "", string tooltip = "", attrs a = null, styles st = null, string id = null)
      : base(new Label()) {
      lbl.Text = text;
      if (!string.IsNullOrEmpty(css)) lbl.CssClass = css;
      if (!string.IsNullOrEmpty(tooltip)) lbl.ToolTip = tooltip;
      if (a != null) a.add_toctrl(lbl);
      if (st != null) st.add_toctrl(lbl);
      if (id != null) lbl.ID = id;
    }
  }

  // TextBox
  public class text : ctrl
  {
    public TextBox txt { get { return (TextBox)_ctrl; } }

    public text(string text, string css = "", string tooltip = "", attrs a = null, styles st = null, string id = null)
      : base(new TextBox()) {
      txt.Text = text;
      if (!string.IsNullOrEmpty(css)) txt.CssClass = css;
      if (!string.IsNullOrEmpty(tooltip)) txt.ToolTip = tooltip;
      if (a != null) a.add_toctrl(txt);
      if (st != null) st.add_toctrl(txt);
      if (id != null) txt.ID = id;
    }

    public text(EventHandler eh, bool ro = false)
      : base(new TextBox()) {
      txt.DataBinding += eh;
      txt.ReadOnly = ro;
    }
  }

  // CheckBox
  public class check : ctrl
  {
    public CheckBox chk { get { return (CheckBox)_ctrl; } }

    public check(string id, EventHandler eh, bool en = true)
      : base(new CheckBox()) {
      chk.ID = id;
      chk.Enabled = en;
      chk.DataBinding += eh;
    }
  }

  // Button
  public class button : ctrl
  {
    public Button btn { get { return (Button)_ctrl; } }

    public button(string txt, string css = "", string tooltip = "", attrs a = null, styles s = null, EventHandler eclick = null)
      : base(new Button()) {
      btn.Text = txt;
      if (css != "") btn.CssClass = css;
      if (tooltip != "") btn.ToolTip = tooltip;
      if (a != null) a.add_toctrl(btn);
      if (s != null) s.add_toctrl(btn);
      if (eclick != null) btn.Click += eclick;
    }
  }

  // HtmlTextArea
  public class txt_area : ctrl
  {
    public HtmlTextArea harea { get { return (HtmlTextArea)_ctrl; } }

    public txt_area(string id, string txt, attrs a = null, styles s = null)
      : base(new HtmlTextArea()) {
      harea.ID = id;
      harea.InnerText = txt;
      if (s != null) s.add_toctrl(h_ctrl);
      if (a != null) a.add_toctrl(h_ctrl);
    }
  }

  // HtmlGenericControl
  public class html_ctrl : ctrl
  {
    public HtmlGenericControl hctrl { get { return (HtmlGenericControl)_ctrl; } }

    public html_ctrl(string tag, attrs a = null, styles s = null, string id = null, string html = null)
      : base(new HtmlGenericControl(tag)) {
      if (id != null) hctrl.ID = id;
      if (html != null) hctrl.InnerHtml = html;
      if (s != null) s.add_toctrl(h_ctrl);
      if (a != null) a.add_toctrl(h_ctrl);
    }
  }
}