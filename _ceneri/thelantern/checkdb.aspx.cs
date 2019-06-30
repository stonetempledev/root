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
using mlib.db;
using mlib.tools;
using mlib.xml;
using mlib.tiles;

public partial class _checkdb : tl_page {

  protected override void OnInit (EventArgs e) {
    base.OnInit(e);
  }

  protected override void OnLoad (EventArgs e) {
    base.OnLoad(e);

    // checks
    blocks blk = new blocks();

    // preinit
    if (config.exists_var("preinit-qry"))
      db_conn.exec_qry(config.get_query(config.get_var("preinit-qry").value), core);

    // exec_script
    if (Page.IsPostBack) {
      try {
        string action = master.get_val("__action"), args = master.get_val("__args");
        if (action == "exec_script") {
          Dictionary<string, string> res = strings.get_args(args, ';', ':');
          config.table_row r = config.get_table("checks").find_row(new Dictionary<string, string>() { { "id", res["id"] } });
          db_conn.exec_qry(config.get_query(r[res["script"]]), core);
        } else if (action == "exec_scripts") {
          foreach (string id in args.Split(new char[] { ',' })) {
            config.table_row r = config.get_table("checks").find_row(new Dictionary<string, string>() { { "id", id } });
            db_conn.exec_qry(config.get_query(r["qry_init"]), core);
          }
        }
      } catch (Exception ex) {
        blk.add_xml("<row><col cols='12'><err-label>SI È VERIFICATO UN ERRORE NELL'ESECUZIONE DELLO SCRIPT: " + ex.Message + "</err-label></col></row>");
      }
    }

    try {
      cmd c = new cmd(qry_val("cmd"));

      if (c.action != "check" || c.obj != "db") throw new Exception("COMANDO NON RICONOSCIUTO!");

      // controlli
      blocks checks = new blocks(); bool first = true; int n_ok = 0, n_no = 0, n_err = 0; string scripts = "";
      foreach (config.table_row tr in config.get_table("checks").rows) {
        if (tr["group"] != "") {
          nano_node grp = blk.add("row");
          nano_node cols = grp.add_xml("<col cols='12'>"
            + "<title-blu>" + tr["group"] + "</title-blu></col>");
          first = true;
        }

        nano_node row = blk.add("row");
        try {
          nano_node cols = row.add_xml("<col cols='12'/>");
          cols.add_xml((!first ? "<hr/>" : "") + "<title-sm>" + tr["title"] + "</title-sm>");
          row.add_xml("<col cols='6'><p>" + tr["des"] + "</p></col>");

          // check
          bool ok = true;
          if (tr["qry_exists"] != "") {
            config.query q = config.get_query(tr["qry_exists"]);
            if (db_conn.get_count(q.text) == 0) {
              ok = false; scripts += (scripts != "" ? "," : "") + tr["id"];
              n_no++; row.add_xml("<col cols='3'><war-label text=\"" + (q.des != "" ? q.des : "NON ESISTE!") + "\"/></col>"
                + "<col cols='3'><btn-sm tp='primary' onclick=\"exec_script('" + tr["id"] + "', 'qry_init');\">INIZIALIZZA " + tr["title"] + "</btn-sm></col>");
            } 
          }

          // miss
          if (ok && tr["qry_miss"] != ""
             && db_conn.get_count(config.get_query(tr["qry_miss"]).text) > 0) {
            config.query q = config.get_query(tr["qry_miss"]);
            ok = false; scripts += (scripts != "" ? "," : "") + tr["id"];
            n_no++; row.add_xml("<col cols='3'><war-label text=\"" + (q.des != "" ? q.des.ToUpper() : "MANCA ALMENO UN ELEMENTO!") + "\"/></col>"
              + "<col cols='3'><btn-sm tp='primary' onclick=\"exec_script('" + tr["id"] + "', 'qry_init');\">ALLINEA " + tr["title"] + "</btn-sm></col>");
          }

          // more
          if (ok && tr["qry_more"] != ""
            && db_conn.get_count(config.get_query(tr["qry_more"]).text) > 0) {
            config.query q = config.get_query(tr["qry_more"]);
            ok = false; scripts += (scripts != "" ? "," : "") + tr["id"];
            n_no++; row.add_xml("<col cols='3'><war-label text=\"" + (q.des != "" ? q.des.ToUpper() : "CI SONO TROPPI ELEMENTI!") + "\"/></col>"
              + "<col cols='3'><btn-sm tp='primary' onclick=\"exec_script('" + tr["id"] + "', 'qry_init');\">ALLINEA " + tr["title"] + "</btn-sm></col>");
          }

          // do - while
          if (ok && tr["qry_do"] != ""
            && db_conn.get_n_rows(config.get_query(tr["qry_do"]).text_do) > 0) {
            config.query q = config.get_query(tr["qry_do"]);
            ok = false; scripts += (scripts != "" ? "," : "") + tr["qry_do"];
            n_no++; row.add_xml("<col cols='3'><war-label text=\"" + (q.des != "" ? q.des.ToUpper() : "CI SONO DEI DISALLINEAMENTI!") + "\"/></col>"
              + "<col cols='3'><btn-sm tp='primary' onclick=\"exec_script('" + tr["id"] + "', 'qry_do');\">ALLINEA " + tr["title"] + "</btn-sm></col>");
          }

          // del
          if (ok) {
            n_ok++;
            if (tr["qry_reset"] != "") {
              config.query q = config.get_query(tr["qry_reset"]);
              row.add_xml("<col cols='3'><ok-label>OK</ok-label></col>"
                + "<col cols='3'><btn-sm tp='default' onclick=\"exec_script('" + tr["id"] + "', 'qry_reset');\">" + (q.des != "" ? q.des.ToUpper() : "RICREA " + tr["title"]) + "</btn-sm></col>");
            } else row.add_xml("<col cols='6'><ok-label>OK</ok-label></col>");
          }

          first = false;
        } catch (Exception ex) { n_err++; row.add_xml("<col cols='12'><err-label>" + ex.Message + "</err-label></col>"); }
      }

      // riepilogo finale
      blk.add_xml("<row><col cols='12'><hr/><title-lblu text='riepilogo'/></col></row>");
      nano_node r = blk.add_xml("<row/>");
      if (n_ok > 0 && n_no > 0) r.add_xml("<col cols='12'><ok-label>" + n_ok.ToString() + " ELEMENTI SONO OK!</ok-label></col>");
      if (n_ok > 0 && n_no == 0) r.add_xml("<col cols='12'><ok-label>È TUTTO REGOLARE!</ok-label></col>");
      if (n_no > 0) r.add_xml("<col cols='12'><war-label>" + n_no.ToString() + " ELEMENTI NON SONO ALLINEATI!</war-label></col>");
      if (n_err > 0) r.add_xml("<col cols='12'><err-label>SI È VERIFICATO ALMENO UN ERRORE DURANTE I CONTROLLI!</err-label></col>");
      if (n_no > 0) r.add_xml("<col cols='12'><btn-lg tp='primary' text='ESEGUI TUTTI GLI SCRIPTS IN UNA VOLTA!' onclick=\"exec_scripts('" + scripts + "');\"/></col>");

    } catch (Exception ex) { blk.add_xml("<row><col cols='12'><err-label>" + ex.Message + "</err-label></col></row>"); }
    div_contents.InnerHtml = blk.parse_blocks(_core);
  }

  protected override void OnUnload (EventArgs e) {
    base.OnUnload(e);
  }

}