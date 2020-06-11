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
        int? fi = qry_val("idt") != "" ? qry_int("idt") : (qry_val("id") != "" ? qry_int("id") : (int?)null)
          , sfi = qry_val("sft") != "" ? qry_int("sft") : (qry_val("sf") != "" ? qry_int("sf") : (int?)null);
        ob.load_notes(fi, sfi);
        menu.InnerHtml = parse_menu(ob.synch_folders, sfi.HasValue || fi.HasValue);
        folder_id.Value = qry_val("id");

        folder f = fi.HasValue ? ob.find_folder(fi.Value) : null;
        synch_folder sf = sfi.HasValue ? ob.find_synch_folder(sfi.Value) : null;
        string title = f != null ? f.folder_name : (sf != null ? sf.title : "");
        content.InnerHtml = parse_tasks(ob, title, f != null ? ob.find_synch_folder(f.synch_folder_id).title + f.path : "");
      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected string parse_tasks(notes n, string title_folder = "", string path_folder = "") {
    StringBuilder sb = new StringBuilder();

    sb.Append(core.parse_html_block(title_folder == "" ? "title-attivita" : "title-attivita-folder"
      , new string[,] { { "title-folder", title_folder }, { "path-folder", path_folder } }));

    List<int> orders = n.tasks.Select(x => x.order).Distinct().ToList();
    orders.Sort();
    foreach (int order in orders) {
      bool first = true;
      List<task> sub_tasks = n.tasks.Where(x => x.order == order).OrderByDescending(xx => xx.dt_upd).ToList();
      foreach (task t in sub_tasks) {
        
        // title
        if (first) {
          string sub_title = sub_tasks.Count == 1 ? t.title_singolare : t.title_plurale;
          sb.Append(core.parse_html_block("open-title-sub-attivita", new string[,] { 
          { "title", sub_title != "" ? sub_title : "GENERICHE" }, { "cls", t.cls } }));
          first = false;
        }

        // task
        string folder_path = "";
        if (t.file_id.HasValue) {
          folder f = t.folder_id.HasValue ? n.synch_folders.FirstOrDefault(x => x.id == t.synch_folder_id).get_folder(t.folder_id.Value) : null;
          folder_path = f != null ? f.path : "";
        } else {
          synch_folder sf = n.synch_folders.FirstOrDefault(x => x.id == t.synch_folder_id);
          folder f = sf.get_folder(t.folder_id.Value)
            , fp = sf.get_folder(f.parent_id.Value);
          folder_path = fp != null ? fp.path : "";
        }
        sb.Append(parse_task(t, n.find_synch_folder(t.synch_folder_id).title + folder_path));
      }
      if(!first) sb.Append(core.parse_html_block("close-title-sub-attivita"));
    }

    return sb.ToString();
  }

  protected string parse_task(task t, string folder_path) {
    return string.Format(@"<div class='d-block badge badge-light'>{5}</div>
        <div class='badge badge-{0} mb-3 d-block text-left'>
        <h3>{1}<small class='float-right'>{2}</small></h3><hr/>
        <h5>{4}<small class='float-right'>{3}</small></h5></div>"
      , t.cls, t.title, !string.IsNullOrEmpty(t.title_singolare) ? t.title_singolare : "generica"
      , t.dt_upd.HasValue ? "aggiornata " + t.dt_upd.Value.ToString("dddd dd MMMM yyyy") : ""
      , !string.IsNullOrEmpty(t.user) ? "assegnata a " + t.user : "&nbsp;", folder_path);
  }

  protected string parse_menu(List<synch_folder> sfs, bool open_home) {
    StringBuilder sb = new StringBuilder();
    foreach (synch_folder sf in sfs) {

      // tasks
      List<task> l = sf.tasks; int order = l.Count > 0 ? l.Min(x => x.order) : -1, cc = order >= 0 ? l.Count(x => x.order == order) : 0;
      string t_plurale = cc > 0 ? l.First(x => x.order == order).title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.order == order).title_singolare : ""
        , color = cc > 0 ? l.First(x => x.order == order).cls : "";

      t_plurale = t_plurale == "" ? "generiche" : t_plurale;
      t_singolare = t_singolare == "" ? "generica" : t_singolare;

      sb.Append(core.parse_html_block(block_level(0)
        , new string[,] { { "id", sf.id.ToString() }, { "tp", "synch-folder" }
          , { "url_open_home", open_home ? master.url_cmd("notes") : "" }
          , { "url_synch_folder", master.url_cmd("notes", pars: new string[,] { {"sf", sf.id.ToString() } }) }
          , { "title", sf.title }, { "childs", parse_folders_menu(sf.folders, 1) }
          , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita-synch"
            , new string[,] { { "class_spin", color }, { "synch_folder_id", sf.id.ToString() }
              , { "url_open_tasks", master.url_cmd("notes", pars: new string[,] { { "sft", sf.id.ToString() } }) }
              , { "title", cc == 1 ? "una attività " + t_singolare : cc.ToString() + " attività " + t_plurale }
              , { "c_attivita", cc.ToString() }} ) : "" }}));
    }
    return string.Format("<ul class='nav flex-column'>{0}</ul>", sb.ToString());
  }

  protected string parse_folders_menu(List<folder> fs, int lvl) {
    StringBuilder sb = new StringBuilder();
    foreach (folder f in fs.Where(x => x.task == null)) {
      string url_open_folder = lvl >= 3 && f.folders.Where(x => x.task == null).Count() > 0
        ? master.url_cmd("notes", pars: new string[,] { { "id", f.parent_id.ToString() } }) : "";

      // tasks
      List<task> l = f.tasks; int order = l.Count > 0 ? l.Min(x => x.order) : -1, cc = order >= 0 ? l.Count(x => x.order == order) : 0; 
      string t_plurale = cc > 0 ? l.First(x => x.order == order).title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.order == order).title_singolare : ""
        , color = cc > 0 ? l.First(x => x.order == order).cls : "";

      t_plurale = t_plurale == "" ? "generiche" : t_plurale;
      t_singolare = t_singolare == "" ? "generica" : t_singolare;

      sb.Append(core.parse_html_block(block_level(lvl), new string[,] { { "id", f.folder_id.ToString() }, { "tp", "folder" }
        , { "title", f.folder_name }, { "childs", parse_folders_menu(f.folders, lvl + 1) }, { "url_open_folder", url_open_folder }
        , { "url_folder", master.url_cmd("notes", pars: new string[,] { { "id", f.folder_id.ToString() } }) }
        , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita"
          , new string[,] { { "class_spin", color }, { "folder_id", f.folder_id.ToString() }
            , { "url_open_tasks", master.url_cmd("notes", pars: new string[,] { { "idt", f.folder_id.ToString() } }) }
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
