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

public partial class _spese : tl_page
{

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e)
  {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if(this.IsPostBack) return;

    try {

      if(json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

          // save_row
          if(jr.action == "save_row") {
            throw new Exception("work in progress.....");
          }

        } catch(Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // check cmd
      if(_cmd == null) return;

      // view
      if(_cmd.action == "view" && _cmd.obj == "spese") {

        int top = 100;
        List<spesa> l = load_spese(top);
        sub_title.InnerText = sub_title_sm.InnerText = l.Count == top ? "le tue ultime " + top.ToString() + " spese quotidiane ordinate per data descresente"
          : "le tue spese quotidiane ordinate per data descresente";
        StringBuilder sb = new StringBuilder(); bool alt_row = false;
        foreach(spesa s in l) {
          sb.Append(core.parse_html_block("row-spesa", new Dictionary<string, object>() { { "spesa", s }, { "alt-class", alt_row ? "bck-ws" : "" } }));
          alt_row = !alt_row;
        }
        grid_spese.InnerHtml = sb.ToString();

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch(Exception ex) { log.log_err(ex); if(!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected override void OnLoad(EventArgs e)
  {
    base.OnLoad(e);

  }

  protected override void OnLoadComplete(EventArgs e)
  {
    base.OnLoadComplete(e);
  }

  protected List<spesa> load_spese(int? top = null)
  {
    List<spesa> res = new List<spesa>();
    foreach(DataRow dr in db_conn.dt_table(core.parse_query("spese", new string[,] { { "user_id", this.user.id_str }, { "top", top.HasValue ? "top " + top.Value.ToString() : "" } })).Rows)
      res.Add(load_spesa(dr));
    return res;
  }

  protected spesa load_spesa(DataRow dr)
  {
    return new spesa(db_provider.int_val(dr["spesa_id"]), db_provider.str_val(dr["cosa"]), db_provider.int_val_null(dr["qta"])
      , db_provider.dt_val(dr["dt_spesa"]).Value, db_provider.dec_val(dr["prezzo"]), db_provider.dec_val(dr["totale"])
      , new tipo_spesa(db_provider.int_val(dr["tipo_spesa_id"]), db_provider.str_val(dr["tipo_spesa"]), db_provider.str_val(dr["note_tipo_spesa"]), db_provider.int_val(dr["entrata"]) == 1)
      , dr["evento_id"] != DBNull.Value ? new evento(db_provider.int_val(dr["evento_id"]), db_provider.str_val(dr["evento"]), db_provider.str_val(dr["note_evento"])
        , db_provider.dt_val(dr["dt_da"]).Value, db_provider.dt_val(dr["dt_a"]).Value
        , new tipo_evento(db_provider.int_val(dr["tipo_evento_id"]), db_provider.str_val(dr["tipo_evento"]), db_provider.str_val(dr["note_tipo_evento"]))) : null);
  }

}
