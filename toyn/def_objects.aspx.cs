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
using mlib.tiles;
using toyn;

public partial class _def_objects : tl_page {

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {

      def_objects ob = new def_objects();

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

      // check cmd
      if (_cmd == null) return;

      // view
      if (_cmd.action == "rebuild" && _cmd.obj == "objects") {

        page_title.InnerText = "Ridefinizione risorse oggetti utenti";
        page_des.InnerText = "ricompilazione viste, contenuti interni usati per la gestione degli oggetti definiti da/per gli utenti";

        // rebuild-views
        StringBuilder rv = new StringBuilder();
        foreach (DataRow dr in db_conn.dt_table(core.parse_query("objects-types")).Rows) {
          rv.Append(core.parse_html_block("rebuild-todo", new string[,] { { "action", "rebuild-view" }
          , { "object-type", db_provider.str_val(dr["object_type"]) }
          , { "title", "Definizione vista database oggetto '" + db_provider.str_val(dr["object_des"]) + "'" } }));
        }

        content.InnerHtml = core.parse_html_block("rebuild-objects", new string[,] { { "rebuild-views", rv.ToString() } });

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
