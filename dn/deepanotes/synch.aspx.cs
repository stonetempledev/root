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
using dn_lib;
using deepanotes;

public partial class _synch : tl_page
{

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {

      synch ob = bo.create_synch();
      html_blocks hb = new html_blocks();

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

      // view
      if (_cmd != null && _cmd.action == "view" && _cmd.obj == "synch" && _cmd.sub_obj() == "settings") {
        page_title.InnerText = "Synch Settings";
        page_des.InnerText = "impostazioni di sincronizzazione delle cartelle di rete con il deepa-notes";

        // folders
        StringBuilder sb = new StringBuilder(hb.section_title("Synch Folders", "cartelle di riferimento con i contenuti")
          + hb.open_list());
        foreach (synch_folder sf in ob.list_synch_folders()) {
          sb.Append(hb.list_item(sf.title, sf.des, sub_items: new string[] { "code: " + sf.code, "synch path: <b>" + sf.synch_path + "</b>, client path: <b>" + sf.client_path + "</b>"}));
        }
        sb.Append(hb.close_list());
        content.InnerHtml = sb.ToString();

        // settings
        sb.Append(hb.section_title("Synch Settings", "impostazioni servizio di sincronizzazione contenuti")
          + hb.open_list());
        foreach (synch_setting s in ob.list_settings()) {
          sb.Append(hb.list_item(s.name, s.des, sub_items: new string[] { "valore: " + s.value }));
        }
        sb.Append(hb.close_list());
        content.InnerHtml = sb.ToString();

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    //if (_cmd.action == "xml") this.master.set_status_txt("caricamento dati...");

  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

}
