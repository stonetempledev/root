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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using mlib.db;
using mlib.tools;
using mlib.xml;
using mlib.tiles;
using toyn;

public partial class _backups : tl_page {

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);
    cmd c = new cmd(qry_val("cmd"));

    if (Request.Headers["toyn-post"] != null) {
      json_result res = new json_result(json_result.type_result.ok);

      try {

        string json = String.Empty;
        Request.InputStream.Position = 0;
        using (var inputStream = new StreamReader(Request.InputStream)) {
          json = inputStream.ReadToEnd();
        }

        if (!string.IsNullOrEmpty(json)) {
          JObject jo = JObject.Parse(json);

          // del_backup
          if (jo["action"].ToString() == "del_backup") {
            string fn = jo["fn"].ToString(), tp = _core.config.get_var("vars.bck.type").value;
            if (tp == "") throw new Exception("il backup non è configurato correttamente!");
            // fs
            if (tp == "fs") {
              string net_user = _core.config.get_var("vars.bck.net-user").value
                , net_pwd = _core.config.get_var("vars.bck.net-pwd").value
                , net_folder = _core.config.get_var("vars.bck.net-folder").value;
              if (net_user != "") {
                using (unc_access unc = new unc_access()) {
                  if (unc.NetUseWithCredentials(net_folder, net_user, "", net_pwd)) {
                    string bf = Path.Combine(net_folder, fn);
                    if (File.Exists(bf)) File.Delete(bf);
                    else throw new Exception("il backup '" + fn + "' non è stato trovato!");
                  } else throw new Exception(unc.DesLastError);
                }
              } else {
                string bf = Path.Combine(net_folder, fn);
                if (File.Exists(bf)) File.Delete(bf);
                else throw new Exception("il backup '" + fn + "' non è stato trovato!");
              }
            } else throw new Exception("il backup di tipo '" + tp + "' non è gestito!");          
          }
        } else throw new Exception("nessun dato da elaborare!");
      } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

      Response.Clear();
      Response.ContentType = "application/json";
      Response.Write(JsonConvert.SerializeObject(res));
      Response.Flush();
      Response.SuppressContent = true;
      HttpContext.Current.ApplicationInstance.CompleteRequest();

      return;
    }


    // check cmd
    if (string.IsNullOrEmpty(qry_val("cmd"))) return;

    gen_backups.Visible = del_backups.Visible = res_backups.Visible = view_backups.Visible = false;

    // gen backup
    if (c.action == "gen" && c.obj == "backup") {
      try {
        gen_backups.Visible = true;
        result_bck.Visible = false;

        if (!this.IsPostBack) {
          val_type.Value = _core.config.get_var("vars.bck.type").value;
          prefix_filename.Value = _core.config.get_var("vars.bck.prefix-filename").value;
          val_file_format.Value = _core.config.get_var("vars.bck.file-format").value;
          val_net_user.Value = _core.config.get_var("vars.bck.net-user").value;
          val_net_pwd.Value = _core.config.get_var("vars.bck.net-pwd").value;
          val_net_folder.Value = _core.config.get_var("vars.bck.net-folder").value;
          sql_command.InnerText = _core.config.get_var("vars.bck.sql-command").value;
        }
      } catch (Exception ex) {
        result_bck.Visible = true;
        result_bck.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "err-label", ex.Message } });
      }
    }
      // restore backup
    else if (c.action == "restore" && c.obj == "backup") {
      try {
        res_backups.Visible = true;
        result_res.Visible = false;

        if (!this.IsPostBack) {
          res_val_type.Value = _core.config.get_var("vars.bck.type").value;
          res_val_net_user.Value = _core.config.get_var("vars.bck.net-user").value;
          res_val_net_pwd.Value = _core.config.get_var("vars.bck.net-pwd").value;
          res_val_net_folder.Value = _core.config.get_var("vars.bck.net-folder").value;
          res_sql_command.InnerText = _core.config.get_var("vars.bck.sql-command-restore").value;
        }
      } catch (Exception ex) {
        result_res.Visible = true;
        result_res.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "err-label", ex.Message } });
      }
    }
      // delete backup
    else if (c.action == "del" && c.obj == "backup") {
      try {
        del_backups.Visible = true;
        result_del.Visible = false;

        if (!this.IsPostBack) {
          del_val_type.Value = _core.config.get_var("vars.bck.type").value;
          del_val_net_user.Value = _core.config.get_var("vars.bck.net-user").value;
          del_val_net_pwd.Value = _core.config.get_var("vars.bck.net-pwd").value;
          del_val_net_folder.Value = _core.config.get_var("vars.bck.net-folder").value;
        }
      } catch (Exception ex) {
        result_del.Visible = true;
        result_del.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "err-label", ex.Message } });
      }
    }
      // view backups
    else if (c.action == "view" && c.obj == "backups") {
      try {
        view_backups.Visible = true;
        result_view.Visible = false;

        // lista files
        string tp = _core.config.get_var("vars.bck.type").value;

        if (tp == "") throw new Exception("il backup non è configurato correttamente!");

        // fs
        if (tp == "fs") {
          string net_user = _core.config.get_var("vars.bck.net-user").value
            , net_pwd = _core.config.get_var("vars.bck.net-pwd").value
            , net_folder = _core.config.get_var("vars.bck.net-folder").value;

          List<file> files = new List<file>();
          if (net_user != "") {
            using (unc_access unc = new unc_access()) {
              if (unc.NetUseWithCredentials(net_folder, net_user, "", net_pwd)) {
                foreach (file f in file.dir(net_folder, "*.bak", true))
                  files.Add(f);
              } else throw new Exception(unc.DesLastError);
            }
          } else {
            foreach (file f in file.dir(net_folder, "*.bak", true))
              files.Add(f);
          }

          // lista
          blocks blk = new blocks();
          nano_node list = blk.add("list");
          foreach (file f in files)
            list.add_xml(string.Format("<l-row-del title=\"{0}\" href=\"{2}\" on-del=\"del_backup('" + f.file_name + "');\" row-data='" + f.file_name + "'>{1}</l-row-del>"
              , f.file_name, "data: " + f.lw.ToString("yyyy/MM/dd") + ", size: " + ((int)(f.size / 1024)).ToString("N0", new System.Globalization.CultureInfo("it-IT")) + " Kb"
              , master.url_cmd("restore backup '" + f.file_name + "'")));

          res_view.InnerHtml = blk.parse_blocks(_core);
        } else throw new Exception("il backup di tipo '" + tp + "' non è gestito!");

      } catch (Exception ex) {
        result_view.Visible = true;
        result_view.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "err-label", ex.Message } });
      }
    }
  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
  }

  protected void gen_backup(object sender, EventArgs e) {
    bool del_tmp = false; string tmp_file = "";
    try {
      if (val_type.Value == "") throw new Exception("il backup automatico non è stato configurato correttamente!");

      // fs
      if (val_type.Value == "fs") {
        // genero il backup del database
        string f_name = prefix_filename.Value + DateTime.Now.ToString(val_file_format.Value) + ".bak";
        tmp_file = Path.Combine(_core.config.get_var("vars.tmp-folder").value, f_name);
        db_conn.exec(sql_command.InnerText.Replace("##TMP-FILE##", tmp_file));
        del_tmp = true;

        // sposto il file nella cartella di destinazione remota
        if (val_net_user.Value != "") {
          string net_user = val_net_user.Value, net_pwd = val_net_pwd.Value
            , net_folder = val_net_folder.Value;
            using (unc_access unc = new unc_access()) {
              if (unc.NetUseWithCredentials(net_folder, net_user, "", net_pwd)) {
                string dest_file = Path.Combine(net_folder, f_name);
                File.Copy(tmp_file, dest_file);
              } else throw new Exception(unc.DesLastError);
            }
        }
          // sposto il file nella cartella di destinazione locale
        else {
          string dest_file = Path.Combine(val_net_folder.Value, f_name);
          File.Copy(tmp_file, dest_file);
        }
      } else throw new Exception("il backup di tipo '" + val_type.Value + "' non è gestito!");

      result_bck.Visible = true;
      result_bck.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "ok-label", "BACKUP EFFETTUATO CON SUCCESSO!" } });
    } catch (Exception ex) {
      result_bck.Visible = true;
      result_bck.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "err-label", ex.Message } });
    } finally { if (del_tmp) File.Delete(tmp_file); }
  }

  protected void res_backup(object sender, EventArgs e) {
    bool del_tmp = false; string tmp_file = "";
    try {
      cmd c = new cmd(qry_val("cmd"));
      string fn = c.sub_obj();

      if (res_val_type.Value == "") throw new Exception("il backup automatico non è stato configurato correttamente!");

      // fs
      if (res_val_type.Value == "fs") {

        // sposto il file nella cartella di destinazione remota
        tmp_file = Path.Combine(_core.config.get_var("vars.tmp-folder").value, fn);
        if (res_val_net_user.Value != "") {
          using (unc_access unc = new unc_access()) {
            if (unc.NetUseWithCredentials(res_val_net_folder.Value, res_val_net_user.Value, "", res_val_net_pwd.Value)) {
              string src_file = Path.Combine(res_val_net_folder.Value, fn);
              if (File.Exists(tmp_file)) File.Delete(tmp_file);
              File.Copy(src_file, tmp_file);
              del_tmp = true;
            } else throw new Exception(unc.DesLastError);
          }
        }
          // sposto il file nella cartella di destinazione locale
        else {
          string src_file = Path.Combine(val_net_folder.Value, fn);
          if (File.Exists(tmp_file)) File.Delete(tmp_file);
          File.Copy(src_file, tmp_file);
          del_tmp = true;
        }

        // genero il backup del database
        close_conn();

        db_provider db = conn_to(_core.config.get_var("vars.bck.conn-restore").value);
        db.exec(res_sql_command.InnerText.Replace("##RESTORE-FILE##", tmp_file));
        db.close_conn();

      } else throw new Exception("il backup di tipo '" + res_val_type.Value + "' non è gestito!");

      result_res.Visible = true;
      result_res.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "ok-label", "RIPRISTINO EFFETTUATO CON SUCCESSO!" } });
    } catch (Exception ex) {
      result_res.Visible = true;
      result_res.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "err-label", ex.Message } });
    } finally { if (del_tmp) File.Delete(tmp_file); }
  }

  protected void del_backup(object sender, EventArgs e) {
    try {
      cmd c = new cmd(qry_val("cmd"));
      string fn = c.sub_obj();

      if (del_val_type.Value == "") throw new Exception("il backup automatico non è stato configurato correttamente!");

      // fs
      if (del_val_type.Value == "fs") {

        // sposto il file nella cartella di destinazione remota
        if (del_val_net_user.Value != "") {
          using (unc_access unc = new unc_access()) {
            if (unc.NetUseWithCredentials(del_val_net_folder.Value, del_val_net_user.Value, "", del_val_net_pwd.Value)) {
              string src_file = Path.Combine(del_val_net_folder.Value, fn);
              if (File.Exists(src_file)) File.Delete(src_file);
              else throw new Exception("il backup '" + fn + "' non è stato trovato!");
            } else throw new Exception(unc.DesLastError);
          }
        }
          // sposto il file nella cartella di destinazione locale
        else {
          string src_file = Path.Combine(val_net_folder.Value, fn);
          if (File.Exists(src_file)) File.Delete(src_file);
          else throw new Exception("il backup '" + fn + "' non è stato trovato!");
        }

      } else throw new Exception("il backup di tipo '" + del_val_type.Value + "' non è gestito!");

      result_del.Visible = true;
      result_del.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "ok-label", "BACKUP CANCELLATO CON SUCCESSO!" } });
    } catch (Exception ex) {
      result_del.Visible = true;
      result_del.InnerHtml = blocks.html_block(_core, new Dictionary<string, string>() { { "err-label", ex.Message } });
    } finally { }
  }
}