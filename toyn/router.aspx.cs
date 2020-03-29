using System;
using System.IO;
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

public partial class _router : tl_page {

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // elab cmd
    if (!IsPostBack) {
      blocks blk = new blocks();
      try {
        string cmd = qry_val("cmd");
        if (string.IsNullOrEmpty(cmd)) return;

        cmd c = read_cmd(cmd);
        if (c == null)
          throw new Exception("Comando '" + cmd + "' non riconosciuto!");

        // filtro type
        if (c.type != "" && !c.type.Split(new char[] { ',' }).Contains(_user.type.ToString()))
          throw new Exception("Comando '" + cmd + "' non riconosciuto!");

        // page
        if (!string.IsNullOrEmpty(c.page)) { master.redirect_to(c.page); return; }

        if (c.action == "view" && c.obj == "cmds") {

          bool first_gr = true;
          foreach (config.table_row gr in _core.config.get_table("cmds.cmds-groups").rows_ordered("title")) {
            nano_node list = null;
            foreach (config.table_row tr in _core.config.get_table("cmds.base-cmds")
              .rows_ordered("action", "object", "subobjs").Where(r => r.field("group") == gr.field("name"))) {

              // filtro type
              string type = tr.field("type");
              if (type != "" && !type.Split(new char[] { ',' }).Contains(_user.type.ToString())) continue;

              // gruppo
              if (list == null) {
                blk.add_xml(string.Format(@"{2}<title-blu des=""{1}"">{0}</title-blu><m-20/>"
                  , gr.field("title"), gr.field("des"), !first_gr ? "<m-40/>" : ""));
                list = blk.add("list");
              }

              // comando
              list.add_xml(string.Format("<l-row title=\"{0}\">{1}</l-row>"
                , tr.field("action") + " " + tr.field("object") + (tr.field("subobjs") != "" ? " " + tr.field("subobjs") : ""), tr.field("des")));
            }
            first_gr = false;
          }

        } else if (c.action == "exit") {
          log_out("login.aspx");
        } else if (c.action == "view" && c.obj == "user") {
          blk.add("title-blu", "Dettagli utente loggato");
          blk.add("list").add_xml("<l-row-var title='nome'>" + user.name + "</l-row-var>"
            + "<l-row-var title='email'>" + user.email + "</l-row-var>"
            + "<l-row-var title='tipo utente'>" + user.type.ToString() + "</l-row-var>"
            + "<l-row-var title='id utente'>" + user.id + "</l-row-var>");
        } else if (c.action == "crypt") {
          if (!string.IsNullOrEmpty(c.obj) && !string.IsNullOrEmpty(c.sub_obj())) {
            blk.add("label", "parola criptata");
            blk.add("text-box", cry.encrypt(c.obj, c.sub_obj()));
          } else if (!string.IsNullOrEmpty(c.obj)) {
            blk.add("label", "parola criptata");
            blk.add("text-box", cry.encode_tobase64(c.obj));
          }
        } else if (c.action == "check" && c.obj == "conn") {
          string err = ""; bool ok = false; try { ok = db_reconn(true); } catch (Exception ex) { err = ex.Message; }
          blk.add("title-blu", "Check DB connection");
          blk.add("list").add_xml("<l-row title='Provider'>" + cfg_conn.provider + "</l-row>"
            + "<l-row title='Stringa di connessione'>" + cfg_conn.conn_string.Replace(";", "; ") + "</l-row>"
            + "<l-row title='Data format'>" + cfg_conn.date_format + "</l-row>");
          blk.add_xml(ok ? "<m-40/><title-label-ok>CONNESSIONE AVVENUTA CON SUCCESSO</title-label-ok>"
            : "<title-label-war>CONNESSIONE NON AVVENUTA!</title-label-war>");
          if (err != "") blk.add_xml("<div><title-label-err>" + err + "</title-label-err></div>");
        } else if (c.action == "view" && c.obj == "vars") {
          blk.add("title-blu", "Variabili di sistema");
          blk.add("list").add_xml("<l-row-var title='machine name'>" + mlib.core.machine_name() + "</l-row-var>"
            + "<l-row-var title='machine ip'>" + mlib.core.machine_ip() + "</l-row-var>");

          // browser capabilities
          System.Web.HttpBrowserCapabilities browser = Request.Browser;
          blk.add_xml("<m-40/><title-blu>Browser Capabilities</title-blu>");
          blk.add("list").add_xml("<l-row-var title='Type'>" + browser.Type + "</l-row-var>"
              + "<l-row-var title='Name'>" + browser.Browser + "</l-row-var>"
              + "<l-row-var title='Version'>" + browser.Version + "</l-row-var>"
              + "<l-row-var title='Major Version'>" + browser.MajorVersion + "</l-row-var>"
              + "<l-row-var title='Minor Version'>" + browser.MinorVersion + "</l-row-var>"
              + "<l-row-var title='Platform'>" + browser.Platform + "</l-row-var>"
              + "<l-row-var title='Is Beta'>" + browser.Beta + "</l-row-var>"
              + "<l-row-var title='Is Crawler'>" + browser.Crawler + "</l-row-var>"
              + "<l-row-var title='Is AOL'>" + browser.AOL + "</l-row-var>"
              + "<l-row-var title='Is Win16'>" + browser.Win16 + "</l-row-var>"
              + "<l-row-var title='Is Win32'>" + browser.Win32 + "</l-row-var>"
              + "<l-row-var title='Supports Frames'>" + browser.Frames + "</l-row-var>"
              + "<l-row-var title='Supports Tables'>" + browser.Tables + "</l-row-var>"
              + "<l-row-var title='Supports Cookies'>" + browser.Cookies + "</l-row-var>"
              + "<l-row-var title='Supports VBScript'>" + browser.VBScript + "</l-row-var>"
              + "<l-row-var title='Supports JavaScript'>" + browser.EcmaScriptVersion.ToString() + "</l-row-var>"
              + "<l-row-var title='Supports Java Applets'>" + browser.JavaApplets + "</l-row-var>"
              + "<l-row-var title='Supports ActiveX Controls'>" + browser.ActiveXControls + "</l-row-var>"
              + "<l-row-var title='Supports JavaScript Version'>" + browser["JavaScriptVersion"] + "</l-row-var>");

          // server variables
          blk.add_xml("<m-40/><title-blu>Server Variables</title-blu>");
          System.Collections.Specialized.NameValueCollection coll = Request.ServerVariables;
          String[] arr1 = coll.AllKeys; nano_node list = blk.add("list");
          for (int loop1 = 0; loop1 < arr1.Length; loop1++) {
            string val = ""; String[] arr2 = coll.GetValues(arr1[loop1]);
            for (int loop2 = 0; loop2 < arr2.Length; loop2++)
              val += (loop2 > 0 ? ", " : "") + Server.HtmlEncode(arr2[loop2]);
            list.add_xml("<l-row-var title=\"" + "Key - " + arr1[loop1] + "\">" + val + "</l-row-var>");
          }

          // db factories
          blk.add_xml("<m-40/><title-blu>Db Factory classes</title-blu>");
          nano_node ul = blk.add("ul");
          foreach (DataRow dr in System.Data.Common.DbProviderFactories.GetFactoryClasses().Rows) {
            ul.add_xml("<li><title-sm des=\"" + dr["DESCRIPTION"].ToString() + "\">" + dr["NAME"].ToString() + "</title-sm></li>");
            nano_node ul2 = ul.add("ul");
            foreach (DataColumn col in dr.Table.Columns)
              if (col.ColumnName.ToLower() != "name" && col.ColumnName.ToLower() != "description"
                && dr[col.ColumnName] != DBNull.Value) ul2.add_xml("<l-row-var title=\"" + col.ColumnName + "\">" + dr[col.ColumnName].ToString() + "</l-row-var>");
          }
        } else if (c.action == "view" && c.obj == "logs") {
          nano_node list = blk.add("list");
          string fn = log.file_name();
          if (!string.IsNullOrEmpty(fn)) {
            string dir = Path.GetDirectoryName(fn);
            if (Directory.Exists(dir)) {
              foreach (file f in file.dir(dir, "*" + Path.GetExtension(fn), true))
                list.add_xml(string.Format("<l-row title=\"{0}\" href=\"{2}\">{1}</l-row>"
                  , f.file_name, "data: " + f.lw.ToString("yyyy/MM/dd") + ", size: " + ((int)(f.size / 1024)).ToString("N0", new System.Globalization.CultureInfo("it-IT")) + " Kb"
                  , master.url_cmd("view log '" + f.file_name + "'")));
            }
          } else throw new Exception("NON È IMPOSTATO IL LOG!");
        } else if (c.action == "view" && c.obj == "log") {
          view_log(Path.Combine(log.dir_path(), c.sub_obj()), blk);
        } else if (c.action == "view" && c.obj == "log-today") {
          view_log(log.file_name(), blk);
        } else throw new Exception("COMANDO NON RICONOSCIUTO!");

      } catch (Exception ex) {
        master.err_txt(ex.Message);
      }
      div_contents.InnerHtml = blk.parse_blocks(_core);
    }
  }

  protected void view_log(string fn, blocks blk) {
    file f = new file(fn);
    blk.add_xml("<title-blu>" + fn + "</title-blu>");
    blk.add_xml("<title-sm>" + string.Format("data: {0}, size: {1}"
      , f.lw.ToString("yyyy/MM/dd"), ((int)(f.size / 1024)).ToString("N0", new System.Globalization.CultureInfo("it-IT")) + " Kb") + "</title-sm>");
    string[] lines = File.ReadAllLines(fn);
    string block = "";
    for (int i = lines.Length - 1; i >= 0; i--) {
      string ln = lines[i].Replace("<", "&lt;").Replace(">", "&gt;");

      //2020
      if (ln.Length > 4 && ln.Substring(0, 4) == DateTime.Now.Year.ToString()) {
        if (block != "") {
          string title = block.IndexOf(" - ") > 0 ? block.Substring(0, block.IndexOf(" - ")) : ""
            , txt = block.IndexOf(" - ") > 0 ? block.Substring(block.IndexOf(" - ")) : block;
          blk.add_xml(string.Format("<d-row title=\"{1}\">{0}</d-row>", txt, title)); 
        }
        block = ln;
      } else block += ln;
    }
    if (block != "") {
      string title = block.IndexOf(" - ") > 0 ? block.Substring(0, block.IndexOf(" - ")) : ""
        , txt = block.IndexOf(" - ") > 0 ? block.Substring(block.IndexOf(" - ")) : block;
      blk.add_xml(string.Format("<d-row title=\"{1}\">{0}</d-row>", txt, title));
    }
  }

  protected cmd read_cmd(string cmd) {
    cmd c = new cmd(cmd);
    config.table_row tr = config.get_table("cmds.base-cmds").find_row(
        c.obj != null ? new Dictionary<string, string>() { { "action", c.action }, { "object", c.obj } }
        : new Dictionary<string, string>() { { "action", c.action } });
    if (tr == null) return null;
    c.type = tr.field("type");
    c.page = tr.field("page");
    return c;
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
  }

  protected override void OnUnload(EventArgs e) {
    base.OnUnload(e);
  }

}