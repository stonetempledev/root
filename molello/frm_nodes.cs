using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using mlib;
using mlib.tools;
using mlib.db;

namespace molello {
  public partial class frm_nodes : frm_base {
    protected int _link_id = -1, _node_id = -1;

    public frm_nodes () {
      InitializeComponent();
    }

    private void lbl_title_MouseDown (object sender, MouseEventArgs e) { mouse_down(e); }
    private void lbl_title_MouseMove (object sender, MouseEventArgs e) { mouse_move(e); }
    private void lbl_title_MouseUp (object sender, MouseEventArgs e) { mouse_up(); }

    private void pb_close_MouseClick (object sender, MouseEventArgs e) { Close(); }

    private void frm_nodes_Load (object sender, EventArgs e) {
      try {
        lbl_title.Text = app._core.config.get_var("vars.title-app").value;

        wb_toolbar.ObjectForScripting = new script_manager(this);
        wb_menu.ObjectForScripting = new script_manager(this);
        wb_body.ObjectForScripting = new script_manager(this);

        reload_top();
        reload_menu();
        reload_body();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    protected void reload_top () { load_top(html_top(_node_id)); }

    protected void reload_menu () { load_menu(html_menu(_node_id)); }

    protected void reload_body () { load_body(html_body()); }

    protected string html_body () {
      StringBuilder res = new StringBuilder();
      try {
        res.Append(@"");
        return res.ToString();
      } catch (Exception ex) { return html_ex(ex); }
    }

    protected string html_top (int link_id = -1) {
      StringBuilder res = new StringBuilder();
      try {
        if (link_id < 0) {
          res.Append(@"<li><span>ROOT</span></li>");
        } else {
          res.Append(@"<li><span onclick='window.external.open_node(-1, -1)'>ROOT</span><span onclick='to_root()' class='ar'></span></li>");
          DataTable dt = app._db.dt_qry(app.cfg.get_query("q-nodes.get-link-path"), app._core, new Dictionary<string, object>() { { "link_id", link_id } });
          for (int n = 0; n < dt.Rows.Count; n++) {
            DataRow dr = dt.Rows[n];
            res.Append(string.Format(@"<li><span onclick='window.external.open_node({0}, {1})'>{2}</span>{3}</li>"
              , db_provider.str_val(dr["link_id"]), db_provider.str_val(dr["node_id"]), db_provider.str_val(dr["node_title"])
              , n < dt.Rows.Count - 1 ? "<span onclick='to_root()' class='ar'></span>" : ""));
          }
        }

        return res.ToString();
      } catch (Exception ex) { return html_ex(ex); }
    }

    protected string html_menu (int node_id = -1) {
      StringBuilder res = new StringBuilder();
      try {
        res.Append(@"<table style='width:100%;height:100%;border:0px;margin:0px;'>
          <tr><td class='bar-menu'>
            <img title='aggiungi nodo...' onclick='window.external.add_node()' src='{@var='vars.path-images'}\add-24-black.png' style='cursor:pointer;'/></td></tr>");

        DataTable dt = node_id < 0 ? app._db.dt_qry(app.cfg.get_query("q-nodes.get-root-nodes"), app._core)
          : app._db.dt_qry(app.cfg.get_query("q-nodes.get-child-nodes"), app._core, new Dictionary<string, object>() { { "node_id", node_id } });
        foreach (DataRow dr in dt.Rows) {
          int nid = db_provider.int_val(dr["node_id"]), lid = db_provider.int_val(dr["link_id"]);
          res.Append("<tr><td class='node' onclick='window.external.open_node(" + lid.ToString() + ", " + nid.ToString() + ")'>" 
            + db_provider.str_val(dr["node_title"]) + "</td></tr>");
        }

        res.Append("</table>");

        return res.ToString();
      } catch (Exception ex) { return html_ex(ex); }
    }

    protected string html_ex (Exception ex) {
      return string.Format(@"<p style='color:tomato;'>{0}<br><br>{1}</p>"
        , ex.Message, ex.StackTrace);
    }

    protected void load_top (string html) {
      reload_html(wb_toolbar, app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-top").value)
          , new Dictionary<string, object>() { { "html", html } }));
    }

    protected void load_menu (string html) {
      reload_html(wb_menu, app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-menu").value)
        , new Dictionary<string, object>() { { "html", html } }));
    }

    protected void load_body (string html) {
      reload_html(wb_body, app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-body").value)
        , new Dictionary<string, object>() { { "html", html } }));
    }

    protected void reload_html (WebBrowser wb, string html) {
      try {
        if (wb.Document == null) wb.DocumentText = html;
        else {
          wb.Navigate("about:blank");
          wb.Document.OpenNew(false);
          wb.Document.Write(html);
          wb.Refresh();
        }
      } catch { }
    }

    private void lbl_title_DoubleClick (object sender, EventArgs e) {
      if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
      else this.WindowState = FormWindowState.Normal;
    }

    public void add_node () {
      try {
        string title = frm_add_node.add_node();
        if (!string.IsNullOrEmpty(title)) {
          if (_node_id < 0)
            app._db.exec_qry(app.cfg.get_query("q-nodes.add-root-node"), app._core
              , new Dictionary<string, object>() { { "node_title", title } });
          else
            app._db.exec_qry(app.cfg.get_query("q-nodes.add-child-node"), app._core
              , new Dictionary<string, object>() { { "node_title", title }, { "link_id", _link_id }, { "node_id", _node_id } });
          reload_menu();
        }
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public void open_node (int link_id, int node_id) {
      try {
        _link_id = link_id; _node_id = node_id;
        reload_top();
        reload_menu();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }
  }

  [ComVisible(true)]
  public class script_manager {
    frm_nodes _form;

    public script_manager (frm_nodes form) { _form = form; }

    public void add_node () { _form.add_node(); }
    public void open_node (int link_id, int node_id) { _form.open_node(link_id, node_id); }
  }

}
