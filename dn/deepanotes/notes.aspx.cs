using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using mlib.db;
using mlib.tools;
using mlib.xml;
using deepanotes;
using dn_lib;

public partial class _notes : tl_page {

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {

      notes ob = new notes();

      if (json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

          //// set_title_hp
          //if (jr.action == "set_title_hp") {
          //  string new_title = hpb.set_main_title(jr.val_str("new_title"));
          //  res.contents = !string.IsNullOrEmpty(new_title) ? new_title : "la " + user.name + " home page";
          //}

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // notes
      if (_cmd != null && _cmd.action == "view" && _cmd.obj == "notes") {
        int? fi = qry_val("id") != "" ? qry_int("id") : (int?)null
          , sfi = qry_val("sf") != "" ? qry_int("sf") : (int?)null;
        ob.load_notes(fi, sfi);
        menu.InnerHtml = parse_menu(ob.synch_folders, sfi.HasValue || fi.HasValue);
        folder_id.Value = qry_val("id");

        content.InnerHtml = parse_tasks(ob);
      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected string parse_tasks(notes n) {
    StringBuilder sb = new StringBuilder();

    sb.Append(core.parse_html_block("title-attivita"));

    List<int> orders = n.tasks.Select(x => x.order).Distinct().ToList();
    orders.Sort();
    foreach (int order in orders) {
      bool first = true;
      List<task> sub_tasks = n.tasks.Where(x => x.order == order).OrderByDescending(xx => xx.dt_upd).ToList();
      foreach (task t in sub_tasks) {
        
        // title
        if (first) {
          sb.Append(core.parse_html_block("open-title-sub-attivita", new string[,] { 
          { "title", sub_tasks.Count == 1 ? t.title_singolare : t.title_plurale }, { "cls", t.cls } }));
          first = false;
        }

        // task
        sb.Append(parse_task(t));
      }
      if(!first) sb.Append(core.parse_html_block("close-title-sub-attivita"));
    }

    return sb.ToString();
  }

  protected string parse_task(task t) {
    return string.Format(@"<div><h4><span class='badge badge-{0}'>{1}{2}{4}
        <small class='mr-2'>{3}</small></span></h4></div>"
      , t.cls, t.title, !string.IsNullOrEmpty(t.title_singolare) ? " - " + t.title_singolare : ""
      , t.dt_upd.HasValue ? "aggiornata " + t.dt_upd.Value.ToString("dddd dd MMMM yyyy") : ""
      , !string.IsNullOrEmpty(t.user) ? " - assegnata a " + t.user : "");
  }

  protected string parse_menu(List<synch_folder> sfs, bool open_home) {
    StringBuilder sb = new StringBuilder();
    foreach (synch_folder sf in sfs) {

      // tasks
      List<task> l = sf.tasks; int order = l.Count > 0 ? l.Min(x => x.order) : -1, cc = order >= 0 ? l.Count(x => x.order == order) : 0;
      string t_plurale = cc > 0 ? l.First(x => x.order == order).title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.order == order).title_singolare : ""
        , color = cc > 0 ? l.First(x => x.order == order).cls : "";

      sb.Append(core.parse_html_block(block_level(0)
        , new string[,] { { "id", sf.id.ToString() }, { "tp", "synch-folder" }
          , { "url_open_home", open_home ? master.url_cmd("notes") : "" }
          , { "url_synch_folder", master.url_cmd("notes", pars: new string[,] { {"sf",sf.id.ToString() } }) }
          , { "title", sf.title }, { "childs", parse_folders(sf.folders, 1) }
          , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita-synch"
            , new string[,] { { "class_spin", color }, { "synch_folder_id", sf.id.ToString() }
              , { "title", cc == 1 ? "una attività " + t_singolare :  cc.ToString() + " attività " + t_plurale }
              , { "c_attivita", cc.ToString() }} ) : "" }}));
    }
    return string.Format("<ul class='nav flex-column'>{0}</ul>", sb.ToString());
  }

  protected string parse_folders(List<folder> fs, int lvl) {
    StringBuilder sb = new StringBuilder();
    foreach (folder f in fs.Where(x => x.task == null)) {
      string url_open_folder = lvl >= 3 && f.folders.Where(x => x.task == null).Count() > 0
        ? master.url_cmd("notes", pars: new string[,] { { "id", f.parent_id.ToString() } }) : "";

      // tasks
      List<task> l = f.tasks; int order = l.Count > 0 ? l.Min(x => x.order) : -1, cc = order >= 0 ? l.Count(x => x.order == order) : 0; 
      string t_plurale = cc > 0 ? l.First(x => x.order == order).title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.order == order).title_singolare : ""
        , color = cc > 0 ? l.First(x => x.order == order).cls : "";

      sb.Append(core.parse_html_block(block_level(lvl), new string[,] { { "id", f.folder_id.ToString() }, { "tp", "folder" }
        , { "title", f.folder_name }, { "childs", parse_folders(f.folders, lvl + 1) }, { "url_open_folder", url_open_folder }
        , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita"
          , new string[,] { { "class_spin", color }, { "folder_id", f.folder_id.ToString() }
            , { "title", cc == 1 ? "una attività " + t_singolare :  cc.ToString() + " attività " + t_plurale }
            , { "c_attivita", cc.ToString() }} ) : "" }}));
    }
    return sb.Length > 0 ? string.Format("<ul>{0}</ul>", sb.ToString()) : "";
  }

  protected string block_level(int lvl) {
    return "item-" + (lvl == 0 ? "synch-folder" : (lvl == 1 ? "secondo" : (lvl == 2 ? "terzo" : "quarto")));
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    //if (_cmd.action == "xml") this.master.set_status_txt("caricamento dati...");

  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

}
