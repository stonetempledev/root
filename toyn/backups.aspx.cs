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
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using mlib.db;
using mlib.tools;
using mlib.xml;
using Ionic.Zip;
using toyn;

public partial class _backups : tl_page {

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);
    cmd c = master.check_cmd(qry_val("cmd"));

    if (json_request.there_request(this)) {
      json_result res = new json_result(json_result.type_result.ok);

      try {
        json_request jr = new json_request(this);

        // del_backup
        if (jr.action == "del_backup") {
          string fn = jr.val_str("fn"), tp = _core.config.get_var("vars.bck.type").value;
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
                  if (File.Exists(bf)) {
                    File.Delete(bf); unc.NetUseDelete();
                  } else { unc.NetUseDelete(); throw new Exception("il backup '" + fn + "' non è stato trovato!"); }
                } else throw new Exception(unc.DesLastError);
              }
            } else {
              string bf = Path.Combine(net_folder, fn);
              if (File.Exists(bf)) File.Delete(bf);
              else throw new Exception("il backup '" + fn + "' non è stato trovato!");
            }
          } else throw new Exception("il backup di tipo '" + tp + "' non è gestito!");
        }
          // down_backup
        else if (jr.action == "down_backup") {
          string fn = jr.val_str("fn"), tp = _core.config.get_var("vars.bck.type").value;
          if (tp == "") throw new Exception("il backup non è configurato correttamente!");
          // fs
          if (tp == "fs") {
            string net_user = _core.config.get_var("vars.bck.net-user").value
              , net_pwd = _core.config.get_var("vars.bck.net-pwd").value
              , net_folder = _core.config.get_var("vars.bck.net-folder").value
              , tmp_folder = _core.config.get_var("vars.tmp-folder").value
              , tmp_fn = Path.Combine(tmp_folder, fn);
            if (net_user != "") {
              using (unc_access unc = new unc_access()) {
                if (unc.NetUseWithCredentials(net_folder, net_user, "", net_pwd)) {
                  string bf = Path.Combine(net_folder, fn);
                  if (File.Exists(tmp_fn)) File.Delete(tmp_fn);
                  if (File.Exists(bf)) {
                    File.Copy(bf, tmp_fn); unc.NetUseDelete();
                  } else { unc.NetUseDelete(); throw new Exception("il backup '" + fn + "' non è stato trovato!"); }
                } else throw new Exception(unc.DesLastError);
              }
            } else {
              string bf = Path.Combine(net_folder, fn);
              if (File.Exists(tmp_fn)) File.Delete(tmp_fn);
              if (File.Exists(bf)) {
                File.Copy(bf, tmp_fn);
              } else throw new Exception("il backup '" + fn + "' non è stato trovato!");
            }

            res.url_file = _core.config.get_var("vars.tmp-url").value + "/" + fn;
            res.url_name = fn;
          } else throw new Exception("il backup di tipo '" + tp + "' non è gestito!");
        }
      } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

      write_response(res);
      return;
    }


    // check cmd
    if (string.IsNullOrEmpty(qry_val("cmd"))) return;

    gen_backups.Visible = res_backups.Visible = view_backups.Visible = false;

    // gen backup
    if (c.action == "gen" && c.obj == "backup") {
      try {
        gen_backups.Visible = true;

        if (!this.IsPostBack) {
          val_type.Value = _core.config.get_var("vars.bck.type").value;
          prefix_filename.Value = _core.config.get_var("vars.bck.prefix-filename").value;
          val_file_format.Value = _core.config.get_var("vars.bck.file-format").value;
          val_libs_folders.Value = _core.config.get_var("vars.bck.libs-folders").value;
          val_net_user.Value = _core.config.get_var("vars.bck.net-user").value;
          val_net_pwd.Value = _core.config.get_var("vars.bck.net-pwd").value;
          val_net_folder.Value = _core.config.get_var("vars.bck.net-folder").value;
          sql_command.InnerText = _core.config.get_var("vars.bck.sql-command").value;
        }
      } catch (Exception ex) {
        master.err_txt(ex.Message);
      }
    }
      // restore backup
    else if (c.action == "restore" && c.obj == "backup") {
      try {
        res_backups.Visible = true;

        if (!this.IsPostBack) {
          res_val_type.Value = _core.config.get_var("vars.bck.type").value;
          res_val_net_user.Value = _core.config.get_var("vars.bck.net-user").value;
          res_val_net_pwd.Value = _core.config.get_var("vars.bck.net-pwd").value;
          res_val_net_folder.Value = _core.config.get_var("vars.bck.net-folder").value;
          res_sql_command.InnerText = _core.config.get_var("vars.bck.sql-command-restore").value;
        }
      } catch (Exception ex) {
        master.err_txt(ex.Message);
      }
    }
      // view backups
    else if (c.action == "view" && c.obj == "backups") {
      try {
        view_backups.Visible = true;

        // lista files
        string tp = _core.config.get_var("vars.bck.type").value;

        if (tp == "") throw new Exception("il backup non è configurato correttamente!");

        // fs
        if (tp == "fs") {

          // lista notes
          DataTable dt = db_conn.dt_table(config.get_query("get-backup-notes").text);

          string net_user = _core.config.get_var("vars.bck.net-user").value
            , net_pwd = _core.config.get_var("vars.bck.net-pwd").value
            , net_folder = _core.config.get_var("vars.bck.net-folder").value;

          List<file> files = new List<file>();
          if (net_user != "") {
            using (unc_access unc = new unc_access()) {
              if (unc.NetUseWithCredentials(net_folder, net_user, "", net_pwd)) {
                foreach (file f in file.dir(net_folder, "*.zip", true))
                  files.Add(f);
                unc.NetUseDelete();
              } else throw new Exception(unc.DesLastError);
            }
          } else {
            foreach (file f in file.dir(net_folder, "*.zip", true))
              files.Add(f);
          }

          // lista
          StringBuilder sb = new StringBuilder();
          sb.Append("<div class='list-group'>");
          foreach (file f in files) {
            DataRow[] rn = dt.Select("name_file = '" + f.file_name + "'");
            string notes = rn.Count() > 0 ? db_provider.str_val(rn[0]["notes"]) : "";
            sb.AppendFormat(@"<a class='list-group-item list-group-item-action flex-column align-items-start' href=""{3}"" row-data=""{4}"">
              <div class='d-flex w-90 justify-content-between'>
                <span class='h4 mb-1'>{0}</span></div>
                <h4><small class='mb-1'>{2}</small></h4>
                <p class='mb-1' style='margin-top:20px;'>{1}</p>
              <span class='float-right' style='margin-left:5px;'>
                <button type='button' class='btn btn-danger' onclick=""del_backup('{4}'); return false;"">Cancella</button>
              </span><span class='float-right'>
                <button type='button' class='btn btn-primary' onclick=""down_backup('{4}');  return false;"">Scarica</button>
              </span></a>", "Backup effettuato " + f.lw.ToString("dddd dd MMMM yyyy")
              , "file: " + f.file_name + ", size: " + ((int)(f.size / 1024)).ToString("N0", new System.Globalization.CultureInfo("it-IT")) + " Kb"
                , (notes != "" ? "dettagli: " + notes : ""), master.url_cmd("restore backup '" + f.file_name + "'"), f.file_name);
          }
          sb.Append("</div>");

          res_view.InnerHtml = sb.ToString();
        } else throw new Exception("il backup di tipo '" + tp + "' non è gestito!");

      } catch (Exception ex) {
        master.err_txt(ex.Message);
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
    bool del_bck_tmp = false, del_zip = false; string tmp_bck_file = "", tmp_zip_file = "";
    try {
      if (val_type.Value == "") throw new Exception("il backup automatico non è stato configurato correttamente!");

      // fs
      if (val_type.Value == "fs") {

        string tmp_folder = _core.config.get_var("vars.tmp-folder").value
          , libs_folders = val_libs_folders.Value;
        string f_bck_name = prefix_filename.Value + DateTime.Now.ToString(val_file_format.Value) + ".bak"
          , f_zip_name = prefix_filename.Value + DateTime.Now.ToString(val_file_format.Value) + ".zip";

        // memorizzo la nota
        db_conn.exec(core.parse(config.get_query("save-backup-note").text
          , new Dictionary<string, object>() { { "name_file", f_zip_name }, { "notes", notes_txt.InnerText } }));

        // genero il backup del database
        tmp_bck_file = Path.Combine(tmp_folder, f_bck_name);
        db_conn.exec(sql_command.InnerText.Replace("##TMP-FILE##", tmp_bck_file));
        del_bck_tmp = true;

        // genero lo zip
        tmp_zip_file = Path.Combine(tmp_folder, f_zip_name);

        using (ZipFile zf = new ZipFile(tmp_zip_file)) {
          zf.AddFile(tmp_bck_file, "__db");
          zf.AddDirectory(_base_path, "__site");
          if (libs_folders != "") {
            List<string> fnames = new List<string>();
            foreach (string lf in libs_folders.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
              string name = Path.GetFileName(lf);
              int i = 2; string name2 = name;
              while (fnames.Contains(name2)) {
                name2 = name + "_" + i.ToString();
                i++;
              }
              zf.AddDirectory(lf, "__libs\\" + name2);
              fnames.Add(name2);
            }
          }
          zf.RemoveSelectedEntries("__site/tmp/*");
          zf.RemoveSelectedEntries("__site/_log/*");
          zf.Save();
        }
        del_zip = true;

        // sposto il file nella cartella di destinazione remota
        if (val_net_user.Value != "") {
          string net_user = val_net_user.Value, net_pwd = val_net_pwd.Value
            , net_folder = val_net_folder.Value;
          using (unc_access unc = new unc_access()) {
            if (unc.NetUseWithCredentials(net_folder, net_user, "", net_pwd)) {
              string dest_file = Path.Combine(net_folder, f_zip_name);
              File.Copy(tmp_zip_file, dest_file);
              unc.NetUseDelete();
            } else throw new Exception(unc.DesLastError);
          }
        }
          // sposto il file nella cartella di destinazione locale
        else {
          string dest_file = Path.Combine(val_net_folder.Value, f_zip_name);
          File.Copy(tmp_zip_file, dest_file);
        }
      } else throw new Exception("il backup di tipo '" + val_type.Value + "' non è gestito!");

      master.status_txt("Backup effettuato con successo!");
    } catch (Exception ex) {
      master.err_txt(ex.Message);
    } finally {
      if (del_bck_tmp) File.Delete(tmp_bck_file);
      if (del_zip) File.Delete(tmp_zip_file);
    }
  }

  protected void res_backup(object sender, EventArgs e) {
    bool del_tmp = false, del_folder = false; string tmp_file = "", tmp_folder = "";
    try {
      cmd c = master.check_cmd(qry_val("cmd"));
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
              unc.NetUseDelete();
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

        // restore del database
        tmp_folder = Path.Combine(_core.config.get_var("vars.tmp-folder").value, Path.GetRandomFileName());
        using (ZipFile zf = new ZipFile(tmp_file)) {
          zf.ExtractSelectedEntries("*.*", "__backup", tmp_folder);
          del_folder = true;
        }

        string bak = Directory.EnumerateFiles(Path.Combine(tmp_folder, "__backup")).ElementAt(0);
        close_conn();
        db_provider db = conn_to(_core.config.get_var("vars.bck.conn-restore").value);
        db.exec(res_sql_command.InnerText.Replace("##RESTORE-FILE##", bak));
        db.close_conn();

      } else throw new Exception("il backup di tipo '" + res_val_type.Value + "' non è gestito!");

      master.status_txt("Ripristino effettuato con successo!");
    } catch (Exception ex) {
      master.err_txt(ex.Message);
    } finally {
      try {
        if (del_tmp) File.Delete(tmp_file);
        if (del_folder) (new DirectoryInfo(tmp_folder)).Delete(true);
      } catch { }
    }
  }
}