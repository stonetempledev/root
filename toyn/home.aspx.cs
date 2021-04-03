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
using toyn;

public partial class _home : tl_page {

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e)
  {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {

      //sections hpb = new sections();

      if (json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // check cmd
      if (_cmd == null) return;

      // view
      if (_cmd.action == "view" && _cmd.obj == "home-page") {

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected override void OnLoad(EventArgs e)
  {
    base.OnLoad(e);

  }

  protected override void OnLoadComplete(EventArgs e)
  {
    base.OnLoadComplete(e);
  }

}
