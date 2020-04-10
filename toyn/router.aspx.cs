﻿using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
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
      StringBuilder sb = new StringBuilder();
      try {
        string cmd = qry_val("cmd");
        if (string.IsNullOrEmpty(cmd)) return;

        cmd c = check_cmd(cmd);
        if (c == null)
          throw new Exception("Comando '" + cmd + "' non riconosciuto!");

        // filtro type
        if (c.type != "" && !c.type.Split(new char[] { ',' }).Contains(_user.type.ToString()))
          throw new Exception("Comando '" + cmd + "' non riconosciuto!");

        // page
        if (!string.IsNullOrEmpty(c.page)) { master.redirect_to(c.page); return; }

        if (c.action == "view" && c.obj == "cmds") {

          foreach (config.table_row gr in _core.config.get_table("cmds.cmds-groups").rows_ordered("title")) {
            StringBuilder sb2 = null;
            foreach (config.table_row tr in _core.config.get_table("cmds.base-cmds")
              .rows_ordered("action", "object", "subobj").Where(r => r.field("group") == gr.field("name"))) {

              // filtro type
              string type = tr.field("type");
              if (type != "" && !type.Split(new char[] { ',' }).Contains(_user.type.ToString())) continue;

              // gruppo
              if (sb2 == null) {
                sb.AppendFormat("<h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>{0}&nbsp;<small>{1}</small></h3>"
                  , gr.field("title"), gr.field("des"));
                sb2 = new StringBuilder();
                sb2.AppendFormat("<div class='list-group'>");
              }

              // comando
              string cc = tr.field("action") + " " + tr.field("object") + (tr.field("subobj") != "" ? " " + tr.field("subobj") : ""), href = "";
              if (tr.fld_bool("call")) href = get_url_cmd(cc);
              else if (tr.fld_bool("compile")) href = "javascript:compile('" + cc.Replace("'", "") + "')";

              sb2.AppendFormat(@"<a class='list-group-item list-group-item-action flex-column align-items-start' href=""{2}"">
                <div class='d-flex w-90 justify-content-between'>
                  <h5 class='mb-1'>{0}</h5></div><p class='mb-1'>{1}</p></a>"
                , cc, tr.field("des"), href != "" ? href : "#");
            }
            if (sb2 != null) { sb2.AppendFormat("</div><div style='height:40px;'>&nbsp;</div>"); sb.Append(sb2); }
          }

        } else if (c.action == "exit") {
          log_out("login.aspx");
        } else if (c.action == "view" && c.obj == "user") {
          sb.AppendFormat(@"<h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>Dettagli utente loggato</h3>");
          sb.Append("<div class='list-group'>");
          string row_var = "<li class='list-group-item'><b style='text-transform: uppercase;'>{0}</b>: {1}</li>";
          sb.AppendFormat(row_var, "nome", user.name);
          sb.AppendFormat(row_var, "email", user.email);
          sb.AppendFormat(row_var, "tipo utente", user.type.ToString());
          sb.AppendFormat(row_var, "id utente", user.id);
          sb.Append("</div>");
        } else if (c.action == "crypt") {
          if (!string.IsNullOrEmpty(c.obj) && !string.IsNullOrEmpty(c.sub_obj())) {
            sb.AppendFormat(@"<span class='h3'><span class='badge badge-primary d-block' style='white-space:normal;font-weight:normal;'>parola criptata</span></span>
              <input type='text' style='width:100%' value=""{0}"">", cry.encrypt(c.obj, c.sub_obj()));
          } else if (!string.IsNullOrEmpty(c.obj)) {
            sb.AppendFormat(@"<span class='h3'><span class='badge badge-primary d-block' style='white-space:normal;font-weight:normal;'>parola criptata</span></span>
              <input type='text' style='width:100%' value=""{0}"">", cry.encode_tobase64(c.obj));
          }
        } else if (c.action == "decrypt") {
          if (!string.IsNullOrEmpty(c.obj) && !string.IsNullOrEmpty(c.sub_obj())) {
            sb.AppendFormat(@"<span class='h3'><span class='badge badge-primary d-block' style='white-space:normal;font-weight:normal;'>parola de-criptata</span></span>
              <input type='text' style='width:100%' value=""{0}"">", cry.decrypt(c.obj, c.sub_obj()));
          } 
        } else if (c.action == "check" && c.obj == "conn") {
          string err = ""; bool ok = false; try { ok = db_reconn(true); } catch (Exception ex) { err = ex.Message; }
          sb.AppendFormat("<h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>Check DB connection</h3>");
          sb.Append("<div class='list-group'>");
          string row = @"<a class='list-group-item list-group-item-action flex-column align-items-start'>
            <div class='d-flex w-90 justify-content-between'>
              <h5 class='mb-1'>{0}</h5></div><p class='mb-1'>{1}</p></a>";
          sb.AppendFormat(row, "Provider", cfg_conn.provider);
          sb.AppendFormat(row, "Stringa di connessione", cfg_conn.conn_string.Replace(";", "; "));
          sb.AppendFormat(row, "Data format", cfg_conn.date_format);
          sb.Append("</div><div style='height:40px;'>&nbsp;</div>");
          sb.AppendFormat(@"<h3 style='text-transform: uppercase;'>
            <span class='badge badge-{1} d-block' style='white-space:normal;'>{0}</span></h3>"
            , ok ? "CONNESSIONE AVVENUTA CON SUCCESSO" : "CONNESSIONE NON AVVENUTA!"
            , ok ? "success" : "warning");

          if (err != "") sb.AppendFormat(@"<h3 style='text-transform: uppercase;'>
            <span class='badge badge-danger d-block' style='white-space:normal;'>{0}</span></h3>", err);

        } else if (c.action == "view" && c.obj == "vars") {
          sb.AppendFormat("<h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>Variabili di sistema</h3>");
          sb.Append("<div class='list-group'>");
          string row_var = "<li class='list-group-item'><b style='text-transform: uppercase;'>{0}</b>: {1}</li>";
          sb.AppendFormat(row_var, "machine name", mlib.core.machine_name());
          sb.AppendFormat(row_var, "machine ip", mlib.core.machine_ip());
          sb.Append("</div>");

          // browser capabilities
          System.Web.HttpBrowserCapabilities browser = Request.Browser;
          sb.AppendFormat(@"<div style='height:40px;'>&nbsp;</div>
            <h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>Browser Capabilities</h3>");
          sb.Append("<div class='list-group'>");
          sb.AppendFormat(row_var, "Type", browser.Type);
          sb.AppendFormat(row_var, "Name", browser.Browser);
          sb.AppendFormat(row_var, "Version", browser.Version);
          sb.AppendFormat(row_var, "Major Version", browser.MajorVersion);
          sb.AppendFormat(row_var, "Minor Version", browser.MinorVersion);
          sb.AppendFormat(row_var, "Platform", browser.Platform);
          sb.AppendFormat(row_var, "Is Beta", browser.Beta);
          sb.AppendFormat(row_var, "Is Crawler", browser.Crawler);
          sb.AppendFormat(row_var, "Is AOL", browser.AOL);
          sb.AppendFormat(row_var, "Is Win16", browser.Win16);
          sb.AppendFormat(row_var, "Is Win32", browser.Win32);
          sb.AppendFormat(row_var, "Supports Frames", browser.Frames);
          sb.AppendFormat(row_var, "Supports Tables", browser.Tables);
          sb.AppendFormat(row_var, "Supports Cookies", browser.Cookies);
          sb.AppendFormat(row_var, "Supports VBScript", browser.VBScript);
          sb.AppendFormat(row_var, "Supports JavaScript", browser.EcmaScriptVersion.ToString());
          sb.AppendFormat(row_var, "Supports Java Applets", browser.JavaApplets);
          sb.AppendFormat(row_var, "Supports ActiveX Controls", browser.ActiveXControls);
          sb.AppendFormat(row_var, "Supports JavaScript Version", browser["JavaScriptVersion"]);
          sb.Append("</div>");

          // server variables
          sb.AppendFormat(@"<div style='height:40px;'>&nbsp;</div>
            <h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>Server Variables</h3>");
          sb.Append("<div class='list-group'>");
          System.Collections.Specialized.NameValueCollection coll = Request.ServerVariables;
          String[] arr1 = coll.AllKeys; 
          for (int loop1 = 0; loop1 < arr1.Length; loop1++) {
            string val = ""; String[] arr2 = coll.GetValues(arr1[loop1]);
            for (int loop2 = 0; loop2 < arr2.Length; loop2++)
              val += (loop2 > 0 ? ", " : "") + Server.HtmlEncode(arr2[loop2]);
            sb.AppendFormat(row_var, "Key - " + arr1[loop1], val);
          }
          sb.Append("</div>");

          // db factories
          sb.AppendFormat(@"<div style='height:40px;'>&nbsp;</div>
            <h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>Db Factory classes</h3>");

          sb.Append("<ul class='list-group'>");
          foreach (DataRow dr in System.Data.Common.DbProviderFactories.GetFactoryClasses().Rows) {
            sb.AppendFormat(@"<li class='list-group-item' style='padding-left:5px;border-right:0px;padding-right:0px;'>
              <h5 style='text-transform: uppercase;'>{0}&nbsp;<small>{1}</small></h5></li>"
              , dr["NAME"].ToString(), dr["DESCRIPTION"].ToString());
            sb.Append("<ul class='list-group'>");
            foreach (DataColumn col in dr.Table.Columns)
              if (col.ColumnName.ToLower() != "name" && col.ColumnName.ToLower() != "description"
                && dr[col.ColumnName] != DBNull.Value)
                sb.AppendFormat(row_var, col.ColumnName, dr[col.ColumnName].ToString());
            sb.Append("</ul>");
          }
          sb.Append("</ul>");
        } else if (c.action == "view" && c.obj == "logs") {
          sb.AppendFormat("<div class='list-group'>");
          string fn = log.file_name();
          if (!string.IsNullOrEmpty(fn)) {
            string dir = Path.GetDirectoryName(fn);
            if (Directory.Exists(dir)) {
              foreach (file f in file.dir(dir, "*" + Path.GetExtension(fn), true))
                sb.AppendFormat(@"<a class='list-group-item list-group-item-action flex-column align-items-start' href=""{2}"">
                  <div class='d-flex w-90 justify-content-between'>
                    <h5 class='mb-1'>{0}</h5></div>
                  <p class='mb-1'>{1}</p></a>", f.file_name
                    , "data: " + f.lw.ToString("yyyy/MM/dd") + ", size: " + ((int)(f.size / 1024)).ToString("N0", new System.Globalization.CultureInfo("it-IT")) + " Kb"
                    , master.url_cmd("view log '" + f.file_name + "'"));
            }
            sb.AppendFormat("</div>");
          } else throw new Exception("NON È IMPOSTATO IL LOG!");
        } else if (c.action == "view" && c.obj == "log") {
          view_log(Path.Combine(log.dir_path(), c.sub_obj()), sb);
        } else if (c.action == "view" && c.obj == "log-today") {
          view_log(log.file_name(), sb);
        } else throw new Exception("COMANDO NON RICONOSCIUTO!");

      } catch (Exception ex) {
        master.err_txt(ex.Message);
      }
      div_contents.InnerHtml = sb.ToString();
    }
  }

  protected void view_log(string fn, StringBuilder sb) {
    file f = new file(fn);
    sb.AppendFormat("<h3 style='color:white;text-transform:uppercase;background-color:royalblue;'>{0}&nbsp;</h3>", fn);
    sb.AppendFormat("<h5 style='text-transform: uppercase;'>{0}&nbsp;</h5>", string.Format("data: {0}, size: {1}"
        , f.lw.ToString("yyyy/MM/dd"), ((int)(f.size / 1024)).ToString("N0", new System.Globalization.CultureInfo("it-IT")) + " Kb"));
    
    string[] lines = File.ReadAllLines(fn);
    string block = "";
    for (int i = lines.Length - 1; i >= 0; i--) {
      string ln = lines[i].Replace("<", "&lt;").Replace(">", "&gt;");

      //2020
      if (ln.Length > 4 && ln.Substring(0, 4) == DateTime.Now.Year.ToString()) {
        if (block != "") {
          string title = block.IndexOf(" - ") > 0 ? block.Substring(0, block.IndexOf(" - ")) : ""
            , txt = block.IndexOf(" - ") > 0 ? block.Substring(block.IndexOf(" - ")) : block;
          sb.AppendFormat("<div style='border-bottom:1pt solid lightgrey;'><b>{1}</b>{0}</div>", txt, title);
        }
        block = ln;
      } else block += ln;
    }
    if (block != "") {
      string title = block.IndexOf(" - ") > 0 ? block.Substring(0, block.IndexOf(" - ")) : ""
        , txt = block.IndexOf(" - ") > 0 ? block.Substring(block.IndexOf(" - ")) : block;
      sb.AppendFormat("<div style='border-bottom:1pt solid lightgrey;'><b>{1}</b>{0}</div>", txt, title);
    }
  }

  protected cmd check_cmd(string cmd) {
    cmd c = new cmd(cmd);
    config.table_row tr = null;
    foreach (config.table_row r in config.get_table("cmds.base-cmds").rows) {
      string action = r.field("action"), obj = r.field("object"), subobj = r.field("subobj");
      if (is_like_cmd(action, c.action) && is_like_cmd(obj, c.obj) && is_like_cmd(subobj, c.sub_obj())) 
      { tr = r; break; }
    }
    if (tr == null) return null;
    c.type = tr.field("type");
    c.page = tr.field("page");
    return c;
  }

  // SINTASSI DICHIARAZIONE object, subobj
  //  keyword - parola chiave che rappresenta un oggetto o un'azione particolare
  //  {par_value} - valore variabile dell'object o del subobj
  //  name_parameter:{par_value} - valore variabile accompagnato al nome del parametro associato all'object o al subobj
  protected bool is_like_cmd(string syntax, string txt) {
    bool res = false;
    if (string.IsNullOrEmpty(syntax) && string.IsNullOrEmpty(txt)) res = true;
    else if(!string.IsNullOrEmpty(syntax) && !string.IsNullOrEmpty(txt)) {
      if (syntax.Contains('{')) {
        int pos = syntax.IndexOf('{');
        if (pos == 0) res = true;
        else {
          string name_par = syntax.Substring(0, pos);
          if (txt.Length >= name_par.Length && txt.Substring(0, pos) == name_par) res = true;
        }
      } else {
        if (txt == syntax) res = true;
      }
    }
    return res;
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
  }

  protected override void OnUnload(EventArgs e) {
    base.OnUnload(e);
  }

}