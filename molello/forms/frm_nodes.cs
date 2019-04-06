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
using HtmlAgilityPack;
using Newtonsoft.Json;
using mlib;
using mlib.tools;
using mlib.db;

namespace molello.forms {
  public partial class frm_nodes : frm_base {

    public enum tp_menu { none, nodes, items }

    protected int _link_id = -1, _node_id = -1;
    protected List<int> _linked_ids = null;
    protected List<classes.node> _active_nodes = null;
    protected List<classes.node> _copy_nodes = null;
    protected tp_menu _tp_menu = tp_menu.none;

    public frm_nodes () {
      InitializeComponent();
    }

    public void mdown () { _fp = Cursor.Position; _mdown = true; }
    public void mup () { _mdown = false; }
    public void mmove () {
      if (_mdown && _fp != Cursor.Position) { this.Location = new Point(this.Location.X - (_fp.X - Cursor.Position.X), this.Location.Y - (_fp.Y - Cursor.Position.Y)); _fp = Cursor.Position; }
    }
    public void dblclick () {
      if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
      else this.WindowState = FormWindowState.Normal;
    }

    private void frm_nodes_Load (object sender, EventArgs e) {
      try {
        _copy_nodes = new List<classes.node>();
        _linked_ids = new List<int>();

        wb_title.ObjectForScripting = new script_manager(this);
        wb_toolbar.ObjectForScripting = new script_manager(this);
        wb_menu.ObjectForScripting = new script_manager(this);
        wb_body.ObjectForScripting = new script_manager(this);

        int lid, nid;
        if (classes.node.dal.get_last_nav(out lid, out nid)) { _link_id = lid; _node_id = nid; }

        reload_title();
        reload_top();
        reload_menu();
        reload_body();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    protected void reload_title () { load_title(html_title(_link_id)); }

    protected void reload_top () { load_top(html_top(_link_id)); }

    protected void reload_menu (tp_menu tp = tp_menu.nodes, string id = "") { load_menu(html_menu(_link_id, tp, id), tp); _tp_menu = tp; }

    protected void reload_body () { load_body(html_body()); }

    protected string html_body () { try { return (new classes.items_doc(_node_id)).html(); } catch (Exception ex) { return html_ex(ex); } }

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

    protected string html_title (int link_id = -1) {
      StringBuilder res = new StringBuilder();
      try {
        res.Append(@"<span class='title-txt'>" + app._core.config.get_var("vars.app-title").value + @"</span>
          <img class='close-icon' onmousedown='md_close()' src=''/>");
        return res.ToString();
      } catch (Exception ex) { return html_ex(ex); }
    }

    protected string html_menu (int parent_link_id = -1, tp_menu tp = tp_menu.nodes, string id = "") {
      try {
        StringBuilder res = new StringBuilder();

        // header
        res.Append(@"<table style='width:100%;height:100%;border:0px;margin:0px;overflow:hidden;border-right:1px solid lightgrey;'>
          <tr><td id='sec-header' class='bar-menu-header'>");

        if (tp == tp_menu.nodes)
          res.Append(@"<img id='img-edit-nodes' class='img-header' title='modifica i nodi selezionati...' onclick='window.external.edit_nodes()' src='{@var='vars.path-images'}\edit-badge.png' />
            <img id='img-copy-nodes' class='img-header' title='copia i nodi selezionati...' onclick='window.external.copy_nodes()' src='{@var='vars.path-images'}\copy-24.png' />
            <img id='img-add-copy-nodes' class='img-header' title='aggiungi alla copia i nodi selezionati...' onclick='window.external.add_copy_nodes()' src='{@var='vars.path-images'}\add-copy-24.png' />
            <img id='img-paste-copy' class='img-header' title='duplica i nodi copiati...' onclick='window.external.paste_nodes()' src='{@var='vars.path-images'}\down-arrow-24.png' />
            <img id='img-paste-linked' class='img-header' title='collega i nodi copiati...' onclick='window.external.paste_nodes(false)' src='{@var='vars.path-images'}\share-24.png' />
            <img id='img-remove-nodes' class='img-header' title='elimina i nodi selezionati e/o copiati...' onclick='window.external.remove_nodes()' src='{@var='vars.path-images'}\remove-24.png' />
            <img id='img-add-node' class='img-header' title='aggiungi nodo...' onclick='window.external.add_node()' src='{@var='vars.path-images'}\add-24-black.png' />
            <span class='stretch'></span></td></tr>");
        if (tp == tp_menu.items) res.Append(@"<img id='img-change-menu' title='cambia menu...' onclick='window.external.menu_nodes()' src='{@var='vars.path-images'}\left-arrow-24.png' />
          <span class='stretch'></span></td></tr>");

        // center
        res.Append(@"<tr><td class='bar-menu-center' style='height:100%;'>
            <div id='sec-center' style='overflow:auto;height:100%;width:100%;'><table style='margin:0px;border:0px;width:100%;'>");

        if (tp == tp_menu.nodes) {
          _active_nodes = classes.node.dal.get_nodes(parent_link_id);
          foreach (classes.node nd in _active_nodes) {
            bool copy = _copy_nodes.FirstOrDefault(n => n.link_id == nd.link_id || n.id == nd.id) != null;
            res.AppendFormat(@"<tr><td class='node{4}' onclick='check_link_id({0})'><span onclick='window.external.open_node({0}, {1})'>{2}{3}</span>            
            <img chk_link_id='{0}' nd-checked='" + (copy ? "true" : "") + @"' src='{5}\" + (copy ? "check-mark-24.png" : "dot-24.png") + @"' 
              style='float:right;" + (copy ? "" : "opacity:0.3;") + "'/></td></tr>", nd.link_id, nd.id, nd.title, nd.n_childs > 0 ? "<span class='puntini'>...</span>" : ""
                , copy ? " copy" : "", app._core.config.get_var("vars.path-images").value);
          }
        }
        res.Append("</table></div></td></tr>");

        // footer
        if (tp == tp_menu.nodes) res.Append(@"<tr><td id='sec-footer' class='bar-menu-footer'>MENU NODI</td></tr></table>");
        else res.Append(@"<tr><td id='sec-footer' class='bar-menu-footer'>MENU DOCUMENTO</td></tr></table>");

        return res.ToString();
      } catch (Exception ex) { return html_ex(ex); }
    }

    protected string html_ex (Exception ex) {
      return string.Format(@"<p style='color:tomato;'>{0}<br><br>{1}</p>"
        , ex.Message, ex.StackTrace);
    }

    protected void load_title (string html) {
      string h = app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-title").value)
          , new Dictionary<string, object>() { { "html", html } });
      reload_html(wb_title, h);
    }

    protected void load_top (string html) {
      reload_html(wb_toolbar, app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-top").value)
          , new Dictionary<string, object>() { { "html", html } }));
    }

    protected void load_menu (string html, tp_menu tp) {
      reload_html(wb_menu, app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-menu").value)
        , new Dictionary<string, object>() { { "html", html }, { "tp_menu", tp.ToString() } }));
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

    public void add_node () {
      try {
        string title = frm_add_node.add_node();
        if (string.IsNullOrEmpty(title)) return;
        bool linked = _linked_ids.Count > 0 ? frm_popup.show_yesno("AGGIUNTA NODO", "Vuoi aggiornare anche i nodi collegati?") : false;
        int node_id = -1;
        List<int> lids = _node_id >= 0 ? (linked ? classes.node.dal.get_all_links_node(_node_id) : new List<int>() { _link_id }) : new List<int>() { -1 };
        foreach (int link_id in lids) {
          if (node_id < 0) node_id = classes.node.dal.add_node(title, link_id);
          else classes.node.dal.add_link(node_id, link_id);
        }
        reload_menu();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public void remove_nodes () {
      try {
        string ids = wb_menu.Document.InvokeScript("get_checked_ids").ToString();
        if (frm_popup.show_yesno("CANCELLAZIONE NODI", "Sei sicuro di voler eliminare i nodi selezionati?")) {
          List<classes.node> nodes = new List<classes.node>(_copy_nodes);
          if (!string.IsNullOrEmpty(ids)) nodes.AddRange(classes.node.dal.get_links_node(ids));
          bool linked = nodes.Count(x => x.n_links > 0) > 0 ?
            frm_popup.show_yesno("CANCELLAZIONE NODI", "...anche tutti i nodi collegati?") : false;

          List<classes.node> lnds = !linked ? classes.node.dal.get_sublink_path(string.Join(",", nodes.Select(n => n.link_id).ToList()))
            : classes.node.dal.get_sublink_path(string.Join(","
              , classes.node.dal.get_all_links_node(string.Join(",", nodes.Select(n => n.id).ToList()))));

          classes.node.dal.remove_links(string.Join(",", lnds.Select(n => n.link_id).ToList()));

          reload_menu();
        }
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public void copy_nodes (bool clear = true) {
      try {
        if (clear) _copy_nodes.Clear();
        string ids = wb_menu.Document.InvokeScript("get_checked_ids").ToString();
        foreach (int link_id in core.split_ints(ids)) _copy_nodes.Add(find_active_link_id(link_id));
        reload_menu();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public bool can_paste_links () {
      List<int> cnids = _copy_nodes.Select(n => n.id).ToList(), clids = _copy_nodes.Select(n => n.link_id).ToList();
      if (cnids.Count == 0 || _active_nodes.FirstOrDefault(n => cnids.Contains(n.id)) != null
        || cnids.Contains(_node_id) || classes.node.dal.get_link_path(_node_id.ToString(), string.Join(",", cnids)).Count > 0)
        return false;
      return true;
    }

    public bool has_copied () { return _copy_nodes.Count > 0; }

    public bool can_paste_copy () {
      List<int> cnids = _copy_nodes.Select(n => n.id).ToList(), clids = _copy_nodes.Select(n => n.link_id).ToList();
      if (cnids.Count == 0)
        return false;
      return true;
    }

    public void paste_nodes (bool copy = true) {
      try {

        // nodi collegati
        List<int> plids = new List<int>() { _link_id };
        if (_linked_ids.Count > 0 && frm_popup.show_yesno(copy ? "DUPLICA NODI" : "COLLEGA NODI", "Vuoi aggiornare anche i nodi collegati?"))
          plids.AddRange(_linked_ids);

        foreach (int plid in plids) {
          // nodes to copy
          List<classes.node> nodes = classes.node.dal.get_sublink_path(string.Join(",", _copy_nodes.Select(n => n.link_id).ToList()));
          while (nodes.Count > 0) {
            classes.node n = nodes[0];
            int lid = !copy ? (n.livello == 0 ? classes.node.dal.add_link(n.id, plid) : classes.node.dal.add_link(n.id, n.parent_link_id))
              : (n.livello == 0 ? classes.node.dal.copy_node(n.id, plid) : classes.node.dal.copy_node(n.id, n.parent_link_id));
            nodes.Where(x => !x.assigned).ToList().ForEach(xx => {
              if (xx.livello > 0 && xx.parent_link_id == n.link_id) { xx.parent_link_id = lid; xx.assigned = true; }
            });
            nodes.RemoveAt(0);
          }
        }
        reload_menu();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    classes.node find_active_node_id (int node_id) { return _active_nodes.FirstOrDefault(n => n.id == node_id); }
    classes.node find_active_link_id (int link_id) { return _active_nodes.FirstOrDefault(n => n.link_id == link_id); }
    classes.node find_copy_node_id (classes.node n) { return n != null ? _copy_nodes.FirstOrDefault(x => x.id == n.id) : null; }
    classes.node find_copy_node_id (int node_id) { return _copy_nodes.FirstOrDefault(n => n.id == node_id); }

    public bool can_copy_nodes (string ids) { return true; }

    public bool can_add_copy_nodes (string ids) {
      foreach (int link_id in core.split_ints(ids)) {
        if (!can_add_copy_node(find_active_link_id(link_id)))
          return false;
      }
      return true;
    }

    public bool can_add_copy_node (classes.node nd) {
      // c'è già il nodo copiato? o un qualche nodo babbo o figlio in canna?
      string node_ids = string.Join(",", _copy_nodes.Select(cn => cn.id.ToString()));
      return !(_copy_nodes.FirstOrDefault(x => x.id == nd.id) != null
        || classes.node.dal.get_link_path(nd.link_id.ToString(), node_ids).Count > 0
        || classes.node.dal.get_sublink_path(nd.link_id.ToString(), node_ids).Count > 0);
    }

    public void edit_nodes () {
      try {
        string ids = wb_menu.Document.InvokeScript("get_checked_ids").ToString();
        int link_id = int.Parse(ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0]);
        if (frm_edit_node.edit_node(link_id))
          reload_menu();
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public void open_node (int link_id, int node_id) {
      try {
        _link_id = link_id; _node_id = node_id;
        _linked_ids = _node_id > 0 ? classes.node.dal.get_all_links_node(_node_id, _link_id) : new List<int>();
        reload_top();
        reload_menu();
        reload_body();
        classes.node.dal.nav_link(_link_id);
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public string html_void_item (string tp) {
      try {
        return classes.item.html_void_item(tp);
      } catch (Exception ex) { frm_popup.show_error(ex.Message, 2000); return ""; }
    }

    public bool save_items (string h, bool show_err = true) {
      try {
        //System.IO.File.WriteAllText("c:\\tmp\\doc.html", h);
        classes.node.dal.save_items(_node_id, classes.item.parse_html(h));
        return true;
      } catch (Exception ex) { if (show_err) frm_popup.show_error(ex.Message); return false; }
    }

    public void menu_items (string id) { reload_menu(tp_menu.items); }
    public void menu_nodes () { reload_menu(tp_menu.nodes); }

    public string transform_items (string html) {
      StringBuilder res = new StringBuilder();
      foreach (classes.item i in classes.item.parse_html(html))
        res.Append(classes.item.transform(i).html_item());
      return res.ToString();
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
    public void paste_nodes (bool copy = true) { _form.paste_nodes(copy); }
    public void open_node (int link_id, int node_id) { _form.open_node(link_id, node_id); }
    public bool can_copy_nodes (string ids) { return _form.can_copy_nodes(ids); }
    public bool can_add_copy_nodes (string ids) { return _form.can_add_copy_nodes(ids); }
    public bool can_paste_links () { return _form.can_paste_links(); }
    public bool can_paste_copy () { return _form.can_paste_copy(); }
    public bool has_copied () { return _form.has_copied(); }
    public void close_window () { _form.Close(); }
    public void mouse_down () { _form.mdown(); }
    public void mouse_up () { _form.mup(); }
    public void mouse_move () { _form.mmove(); }
    public void dblclick () { _form.dblclick(); }
    public string html_void_item (string tp) { return _form.html_void_item(tp); }
    public void save_items (string h) { _form.save_items(h); }
    public void menu_items (string id) { _form.menu_items(id); }
    public void menu_nodes () { _form.menu_nodes(); }
    public string transform_items (string h) { return _form.transform_items(h); }
    public string get_todo_states () { return JsonConvert.SerializeObject(molello.classes.item.todo_stati()); }
  }

}
