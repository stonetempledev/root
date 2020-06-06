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
        List<synch_folder> sf = ob.load_folders(fi, sfi);
        menu.InnerHtml = parse_menu(sf, sfi.HasValue);
        folder_id.Value = qry_val("id");
      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected string parse_menu(List<synch_folder> sfs, bool open_home) {
    StringBuilder sb = new StringBuilder();
    foreach (synch_folder sf in sfs)
      sb.Append(core.parse_html_block(block_level(0)
        , new string[,] { { "id", sf.id.ToString() }, { "tp", "synch-folder" }
          , { "url_open_home", open_home ? master.url_cmd("notes") : "" }
          , { "url_synch_folder", master.url_cmd("notes", pars: new string[,] { {"sf",sf.id.ToString() } }) }
          , { "title", sf.title }, { "childs", parse_folders(sf.folders, 1) } }));
    return string.Format("<ul class='nav flex-column'>{0}</ul>", sb.ToString());
  }

  protected string parse_folders(List<folder> fs, int lvl) {
    StringBuilder sb = new StringBuilder();
    foreach (folder f in fs.Where(x => x.task == null)) {
      string url_open_folder = lvl >= 3 && f.folders.Where(x => x.task == null).Count() > 0
        ? master.url_cmd("notes", pars: new string[,] { { "id", f.parent_id.ToString() } }) : "";
      sb.Append(core.parse_html_block(block_level(lvl), new string[,] { { "id", f.folder_id.ToString() }, { "tp", "folder" }
       , { "title", f.folder_name }, { "childs", parse_folders(f.folders, lvl + 1) }
       , { "url_open_folder", url_open_folder }}));
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
