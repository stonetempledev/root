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

public partial class _attivita : tl_page {

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

      // tasks
      if (_cmd != null && _cmd.action == "view" && _cmd.obj == "tasks") {
        // carico la struttura folders
        List<synch_folder> sf = ob.load_folders();

        // menu
        StringBuilder sb = new StringBuilder();
        sb.Append(@"<ul class='nav flex-column'>
            <li class='primo'><a class='h5' href='#'>Primo Livello</a></li>
            <ul>
              <li class='secondo'><a href='#'>Secondo Livello</a></li>
              <li class='secondo'><a href='#'>Secondo Livello</a></li>
              <ul>
                <li class='terzo'><a href='#'>terzo livello</a></li>
                <li class='terzo'><a href='#'>terzo livello</a></li>
                <li class='terzo'><a href='#'>terzo livello</a></li>
                <li class='terzo'><a href='#'>terzo livello</a></li>
                <ul>
                  <li class='quarto'><a href='#'>quarto livello</a></li>
                  <li class='quarto'><a href='#'>quarto livello</a></li>
                </ul>
              </ul>
            </ul>
          </ul>");
        menu.InnerHtml = sb.ToString();

        // vista attivita

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
