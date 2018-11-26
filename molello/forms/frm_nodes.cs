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

namespace molello.forms {
  public partial class frm_nodes : frm_base {
    protected int _link_id = -1, _node_id = -1;
    protected List<classes.node> _active_nodes = null;
    protected List<classes.node> _copy_nodes = null;

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
        _copy_nodes = new List<classes.node>();

        wb_toolbar.ObjectForScripting = new script_manager(this);
        wb_menu.ObjectForScripting = new script_manager(this);
        wb_body.ObjectForScripting = new script_manager(this);

        reload_top();
        reload_menu();
        reload_body();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    protected void reload_top () { load_top(html_top(_link_id)); }

    protected void reload_menu () { load_menu(html_menu(_link_id)); }

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
          List<classes.node> nds = classes.node.dal.get_link_path(link_id);
          for (int n = 0; n < nds.Count; n++) {
            classes.node nd = nds[n];
            res.Append(string.Format(@"<li><span onclick='window.external.open_node({0}, {1})'>{2}</span>{3}</li>"
              , nd.link_id, nd.id, nd.title, n < nds.Count - 1 ? "<span onclick='to_root()' class='ar'></span>" : ""));
          }
        }

        return res.ToString();
      } catch (Exception ex) { return html_ex(ex); }
    }

    protected string html_menu (int parent_link_id = -1) {
      StringBuilder res = new StringBuilder();
      try {
        res.Append(@"<table style='width:100%;height:100%;border:0px;margin:0px;'>
          <tr><td class='bar-menu'>
            <img id='img-edit-nodes' title='modifica i nodi selezionati...' onclick='window.external.edit_nodes()' src='{@var='vars.path-images'}\edit-badge.png' style='cursor:pointer;display:none;'/>
            <img id='img-copy-nodes' title='copia i nodi selezionati...' onclick='window.external.copy_nodes()' src='{@var='vars.path-images'}\copy-24.png' style='cursor:pointer;display:none;'/>
            <img id='img-add-copy-nodes' title='aggiungi alla copia i nodi selezionati...' onclick='window.external.add_copy_nodes()' src='{@var='vars.path-images'}\add-copy-24.png' style='cursor:pointer;display:none;'/>
            <img id='img-paste-nodes' title='incolla i nodi selezionati...' onclick='window.external.paste_nodes()' src='{@var='vars.path-images'}\share-24.png' style='cursor:pointer;display:none;'/>
            <img id='img-remove-nodes' title='elimina i nodi selezionati...' onclick='window.external.remove_nodes()' src='{@var='vars.path-images'}\remove-24.png' style='cursor:pointer;display:none;'/>
            <img title='aggiungi nodo...' onclick='window.external.add_node()' src='{@var='vars.path-images'}\add-24-black.png' style='cursor:pointer;'/>
          </td></tr>");

        _active_nodes = classes.node.dal.get_nodes(parent_link_id);
        foreach (classes.node nd in _active_nodes) {
          bool copy = _copy_nodes.FirstOrDefault(n => n.link_id == nd.link_id || n.id == nd.id) != null;
          res.AppendFormat(@"<tr><td class='node{4}' onclick='check_link_id({0})'><span onclick='window.external.open_node({0}, {1})'>{2}{3}</span>
            <input chk_link_id='{0}' style='float:right;' type='checkbox' onclick='chk_checked(this);'/></td></tr>"
            , nd.link_id, nd.id, nd.title, nd.n_childs > 0 ? "<span class='puntini'>...</span>" : "", copy ? " copy" : "");
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
          classes.node.dal.add_node(title, _link_id, _node_id);
          reload_menu();
        }
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public void remove_nodes () {
      try {
        string ids = wb_menu.Document.InvokeScript("get_checked_ids").ToString();
        if (frm_popup.show_yesno("CANCELLAZIONE NODI", "Sei sicuro di voler cancellare per sempre i nodi selezionati?")) {
          foreach (int link_id in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)))
            classes.node.dal.remove_node(link_id);
          reload_menu();
        }
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public void copy_nodes (bool clear = true) {
      try {
        if (clear) _copy_nodes.Clear();
        string ids = wb_menu.Document.InvokeScript("get_checked_ids").ToString();
        foreach (int link_id in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s))) {
          classes.node n = find_active_node(link_id);
          if (n != null && _copy_nodes.FirstOrDefault(nd => nd.link_id == link_id || nd.id == n.id) == null)
            _copy_nodes.Add(n);
        }
        reload_menu();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public void paste_nodes () {
      try {
        bool refresh = false;
        foreach (classes.node nd in _copy_nodes) {
          if (_active_nodes.FirstOrDefault(n => n.link_id == nd.link_id || n.id == nd.id) == null) {
            classes.node.dal.add_link(nd.id, _link_id, _node_id);
            refresh = true;
          }
        }
        _copy_nodes.Clear();
        if(refresh) reload_menu();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    classes.node find_active_node (int link_id) { return _active_nodes.FirstOrDefault(n => n.link_id == link_id); }

    public bool can_copy_nodes (string ids) {
      foreach (int link_id in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s))) {
        classes.node n = find_active_node(link_id);
        if (n != null && _copy_nodes.FirstOrDefault(nd => nd.link_id == link_id || nd.id == n.id) == null)
          return true;
      }
      return false;
    }

    public bool can_paste_nodes () {
      foreach (classes.node n in _copy_nodes) {
        if (_active_nodes.FirstOrDefault(nd => nd.link_id == n.link_id || nd.id == n.id) == null) 
          return true;
      }
      return false;
    }

    public void edit_nodes () {
      try {
        string ids = wb_menu.Document.InvokeScript("get_checked_ids").ToString();
        int link_id = int.Parse(ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0]);
        if(frm_edit_node.edit_node(link_id))
          reload_menu();
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
    public void remove_nodes () { _form.remove_nodes(); }
    public void edit_nodes () { _form.edit_nodes(); }
    public void copy_nodes () { _form.copy_nodes(); }
    public void add_copy_nodes () { _form.copy_nodes(false); }
    public void paste_nodes () { _form.paste_nodes(); }
    public void open_node (int link_id, int node_id) { _form.open_node(link_id, node_id); }
    public bool can_copy_nodes (string ids) { return _form.can_copy_nodes(ids); }
    public bool can_paste_nodes () { return _form.can_paste_nodes(); }
  }

}
