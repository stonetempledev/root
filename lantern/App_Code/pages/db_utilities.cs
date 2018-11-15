using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using deeper.frmwrk.ctrls;
using System.IO;
using System.Data;
using deeper.frmwrk;
using deeper.lib;
using deeper.db;

namespace deeper.pages
{
  public class db_utilities : deeper.frmwrk.page_cls
  {
    public db_utilities(deeper.frmwrk.lib_page page, XmlNode pageNode)
      : base(page, pageNode) {
    }

    public override void onInit(object sender, EventArgs e, bool request = false, bool addControls = true) {
      Exception exc = null;
      bool baseInit = true;
      try {
        if (_page.pageName == "export") {

          // invio file o backup database
          if (_page.query_param("idfl") != "") sendFile(file_path(_page, _page.query_int("idfl")));
          else sendFile(page.conn_db_user().export(_page.tmpFolder(), "export database", DateTime.Now, _page.tmpFolder()
            , "export database", Path.GetRandomFileName(), page.cfg_var("filesFolder")), "export-data.gz");

          baseInit = false;
        } else if (_page.query_param("type") == "sch") {
          xmlDoc doc = new xmlDoc(Path.Combine(page.cfg_var("backupsFolder"), page.cfg_var("fsIndex")));
          string pocket = Path.Combine(page.cfg_var("backupsFolder"),
              doc.get_value("/root/files/file[@idfile=" + _page.query_param("idfl") + "]", "name"));
          sendFile(extract_dbpck_index(pocket), "export-schema.xml");

          baseInit = false;
        } else if (_page.query_param("type") == "dl-bck") {
          xmlDoc doc = new xmlDoc(Path.Combine(page.cfg_var("backupsFolder"), page.cfg_var("fsIndex")));
          sendFile(Path.Combine(page.cfg_var("backupsFolder")
              , doc.get_value("/root/files/file[@idfile=" + _page.query_param("idfl") + "]", "name")), "export-data.gz");

          baseInit = false;
        }
      } catch (Exception ex) { exc = ex; }

      if (baseInit) base.onInit(sender, e, request);

      if (exc != null) addError(exc);
    }

    public override string demandBeforeSubmit(string name) {
      string msg = base.demandBeforeSubmit(name);
      if (msg != "") return msg;

      // pacchetto gz
      if (name == "form-import" && _page.query_param("type") != "csv") {
        // upload 
        string pathtmp = Path.Combine(_page.tmpFolder(), Path.GetRandomFileName() + ".gz");
        form_control(name).upload_save("fileXml", pathtmp);

        // check schema
        deeper.db.db_schema db = page.conn_db_user();
        xmlDoc doc = new xmlDoc(extract_dbpck_index(pathtmp));
        if (db.ver != doc.root_value("ver")) {
          form_control(name).upload_set_path("fileXml", pathtmp);
          return "Il database che si desidera importare ha una versione inferiore di quella installata.<br><br>"
              + "Vuoi continuare ugualmente?";
        }
      }

      return "";
    }

    public override submitResponse submitFormAfterData(string name) {
      // pacchetto gz
      if (name == "form-import" && _page.query_param("type") != "csv") {
        // upload 
        string pathfile = check_page_flag("confirmed") ? form_control(name).upload_path("fileXml")
          : Path.Combine(_page.tmpFolder(), Path.GetRandomFileName() + ".gz");
        if (!check_page_flag("confirmed")) form_control(name).upload_save("fileXml", pathfile);

        // backup database attuale
        backupDb(this, "importazione completa base dati esterna", "backup interno");

        // import sul db
        string idx = page.classPage.extract_dbpck(pathfile);
        page.conn_db_user().upgrade_data(page.conn_schema(idx));

        // files
        page.files_from_backup(idx);

        File.Delete(pathfile);

        return submitResponse.ok;
      }
        // file csv
      else if (name == "form-import" && _page.query_param("type") == "csv") {
        string path_csv = form_control(name).upload_save("fileXml"
          , Path.Combine(_page.tmpFolder(), Path.GetRandomFileName()));
        try { exec_proc(this, path_csv, _page.query_param("code")); } finally { if (File.Exists(path_csv)) File.Delete(path_csv); }
      }
        // import schema
      else if (name == "form-sys") {
        long elab = 0, inserted = 0, updated = 0, delrows = 0, err = 0;
        deeper.pages.db_utilities.exec_proc(this, "", form_control(name).fieldValue("schema_code"), out elab, out inserted, out updated, out delrows, out err);
        regScript(scriptStartAlert(string.Format((err == 0 ? "Procedura eseguita con sucesso.<br/><br/>" : "Ci sono stati dei problemi durante l'elaborazione dei dati.<br/><br/>")
          + "righe elaborate:{0}<br/><br/>righe aggiunte:{1}<br/><br/>righe aggiornate:{2}<br/><br/>righe cancellate:{3}<br/><br/>righe errate:{4}." + (err > 0 ? "<br/><br/><br/>Consultare il log per il dettaglio degli errori." : ""), elab, inserted, updated, delrows, err)
          , page.proc_des(form_control(name).fieldValue("schema_code"))));

        return submitResponse.ok_alert;
      }

      return submitResponse.notvalued;
    }

    public static void exec_proc(page_cls pg, string data_source, string code) {
      long tmp = 0;
      exec_proc(pg, data_source, code, out tmp, out tmp, out tmp, out tmp, out tmp);
    }

    public static void exec_proc(page_cls pg, string data_source, string code, out long elab
      , out long inserted, out long updated, out long delrows, out long err) {
      elab = inserted = updated = delrows = err = 0;
      try {
        string tp = pg.page.proc_type(code);
        if (tp == "csv") import_csv(pg, data_source, new xmlDoc(pg.page.proc_path(code)));
        else if (tp == "db") import_db(pg, new xmlDoc(pg.page.proc_path(code)), out elab, out inserted, out updated, out err);
        else if (tp == "func") exec_func(pg, code, out elab, out inserted, out updated, out delrows, out err);
      } catch (Exception ex) { throw ex; } finally { }
    }

    protected static void exec_func(page_cls pg, string code, out long elab, out long inserted, out long updated, out long delrows, out long err) {
      //if (code == "group-anagrafiche" || code == "group-anagrafiche-log") group_anagrafiche(pg, code, out elab, out inserted, out updated, out delrows, out err);
      //else 
      throw new Exception("funzione '" + code + "' non supportata");
    }

    protected static void import_db(page_cls pg, xmlDoc sch, out long elab, out long inserted, out long updated, out long err) {

      inserted = 0; updated = 0; err = 0; elab = 0;
      deeper.db.db_schema src = null, db = null;
      try {
        bool is_count = pg.there_sql("count", sch.root_node), is_check = pg.there_sql("check", sch.root_node);
        src = pg.page.conn_db(sch.root_value("conn")); db = pg.page.conn_db_base();
        pg.exec_updates("before", "", null, null, sch.root_node);
        foreach (DataRow dr in src.dt_table(pg.sql_from_id("load", "", src.conn_keys, null, sch.root_node)).Rows) {
          try {
            // check - count
            int? count = is_count ? int.Parse(pg.dt_from_id("count", "", "", null, dr, true, sch.root_node).Rows[0][0].ToString()): (int?)null;
            Dictionary<string, string> values = null;
            if (is_check) {
              DataTable dt = pg.dt_from_id("check", "", "", null, dr, true, sch.root_node);
              if (dt != null && dt.Rows.Count > 0) {
                values = new Dictionary<string, string>();
                foreach (DataColumn dc in dt.Columns)
                  values.Add(dc.ColumnName, dt.Rows[0][dc.ColumnName].ToString());
              }
            }

            // ins - upd rows
            if ((is_count && count.Value == 0) || (is_check && values == null)) {
              inserted += pg.exec_updates("ins", "", values, dr, sch.root_node);
            } else if ((is_count && count.Value > 0) || (is_check && values != null)) 
              updated += pg.exec_updates("upd", "", values, dr, sch.root_node);
          } catch { err++; }
          elab++;
        }
      } catch (Exception ex) { throw ex; } finally { if (db != null) pg.exec_updates("after", "", null, null, sch.root_node); }
    }

    protected static void import_csv(page_cls pg, string path_file, xmlDoc sch) {

      System.IO.StreamReader rdr = null;
      int i_line = 0;
      try {
        // schema
        Dictionary<string, string> flds = sch.nodes("/root/fields/field").Cast<XmlNode>()
            .ToDictionary(x => x.Attributes["name"].Value, x => (string)null);

        // lines
        db.db_schema db = pg.page.conn_db_user();
        rdr = new System.IO.StreamReader(path_file);
        int from_line = var_import_int(sch, "from_line");
        string line = rdr.ReadLine();
        while (!string.IsNullOrEmpty(line)) {
          if (i_line >= from_line) {
            parse_line_csv(line.Trim(), var_import(sch, "separator"), flds);
            if (int.Parse(pg.dt_from_id("check", "", "", flds, null, true, sch.root_node)
              .Rows[0][0].ToString()) == 0) { pg.exec_updates("ins", "", flds, null, sch.root_node); System.Threading.Thread.Sleep(50); } else {
              pg.exec_updates("upd", "", flds, null, sch.root_node); System.Threading.Thread.Sleep(50);
            }
          }
          line = rdr.ReadLine(); i_line++;
        }
      } catch (Exception ex) { throw new Exception(ex.Message + " (line: " + i_line.ToString() + ")"); } finally { if (rdr != null) rdr.Close(); }
    }

    protected static string var_import(xmlDoc doc, string name) { return doc.get_value("/root/vars/var[@name='" + name + "']"); }

    protected static bool var_import_bool(xmlDoc doc, string name) { return doc.get_bool("/root/vars/var[@name='" + name + "']"); }

    protected static int var_import_int(xmlDoc doc, string name, int def_value = 0) { return doc.get_int("/root/vars/var[@name='" + name + "']", "", def_value); }

    protected static void parse_line_csv(string line, string separator, Dictionary<string, string> flds) {
      int from = 0, i_field = 0;
      while (from < line.Length) {
        int to = -1; bool quote = false;
        if (line.Substring(from, 1) == "\"") { to = line.IndexOf("\"", from + 1); quote = true; } else to = line.IndexOf(separator, from);

        string val = !quote ? (to == -1 ? line.Substring(from) : line.Substring(from, to - from))
          : (to == -1 ? line.Substring(from + 1) : line.Substring(from + 1, to - (from + 1)));
        flds[flds.Keys.ElementAt(i_field)] = string.IsNullOrEmpty(val) ? null : val;

        if (to == -1) break;
        from = !quote ? to + 1 : to + 2; i_field++;
      }
    }

    public override bool action(string actionName, string formName, string keys = "", string noConfirm = "", string refurl = "") {
      if (base.action(actionName, formName, keys, noConfirm, refurl))
        return true;

      if (actionName == "remove-db-backups") {
        Directory.Delete(_page.cfg_var("backupsFolder"), true);

        return true;
      } else if (actionName == "remove-db-backup") {
        // aggiorno l'indice ed elimino il backup
        xmlDoc doc = new xmlDoc(Path.Combine(page.cfg_var("backupsFolder"), page.cfg_var("fsIndex")));
        string xp = "/root/files/file[@idfile=" + keyValue(keys, "idfile") + "]";
        File.Delete(Path.Combine(page.cfg_var("backupsFolder"), doc.get_value(xp, "name")));
        doc.remove(xp);
        doc.save();

        return true;
      } else if (actionName == "mount-db-backup") {
        mountBackup(int.Parse(keyValue(keys, "idfile")), page);

        return true;
      } else if (actionName == "mount-data-db-backup") {
        mountBackup(int.Parse(keyValue(keys, "idfile")), page, true);

        return true;
      }

      return false;
    }

    // /cartellona/catellina/cartellozza
    public static int ins_folder(string path_folder, deeper.frmwrk.lib_page page) {

      db.db_schema db = null;
      bool identity_folders = false;
      try {
        db = page.conn_db_user(true);

        path_folder = path_folder.Replace("\\", "/");
        string folders_tbl = db.meta_doc.table_from_code(deeper.db.meta_table.align_codes.folders);

        DataTable dt = db.dt_table("select tbl.* from "
          + " (select idfolder, dbo.getPathOfFolder(idfolder) as path_folder from folders) tbl "
          + "  where tbl.path_folder = " + db.val_toqry(path_folder, deeper.db.fieldType.VARCHAR));
        if (dt.Rows.Count >= 1) return int.Parse(dt.Rows[0]["idfolder"].ToString());

        int id_parent = -1;
        string tmp_folder = "";
        string[] sub_folders = path_folder.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = -1; i < sub_folders.Count(); i++) {
          // folder vuoto
          if (i < 0) {
            if (db.get_count("select count(*) from " + folders_tbl + " where idfolder = 0") == 0) {
              db.set_identity("folders", true);
              identity_folders = true;
              db.exec("insert into " + folders_tbl + " (idfolder, idfolderpadre, foldername, note, dtins) values (0, 0, '', null, getdate())", true);
              db.set_identity("folders", false);
              identity_folders = false;
            }
            id_parent = 0;
            continue;
          }

          tmp_folder += "/" + sub_folders[i];
          dt = db.dt_table("select tbl.* from "
            + " (select idfolder, dbo.getPathOfFolder(idfolder) as path_folder from folders) tbl "
            + "  where tbl.path_folder = " + db.val_toqry(tmp_folder, deeper.db.fieldType.VARCHAR));
          if (dt.Rows.Count >= 1) id_parent = int.Parse(dt.Rows[0]["idfolder"].ToString());
          else id_parent = (int)db.exec("insert into " + folders_tbl + " (idfolderpadre, foldername, note, dtins) values (" + id_parent.ToString() + ", "
              + db.val_toqry(sub_folders[i], deeper.db.fieldType.VARCHAR) + ", null, getdate())", true);

          continue;
        }
        return id_parent;
      } catch (Exception ex) { throw ex; } finally {
        if (db != null && identity_folders) db.set_identity("folders", false);
      }
    }

    public static string file_path(deeper.frmwrk.lib_page page, long id_file) {
      db.db_schema db = page.conn_db_user(true);
      string files_tbl = db.meta_doc.table_from_code(deeper.db.meta_table.align_codes.files);
      string fld_path = db.dt_table(string.Format("select dbo.getPathOfFolder(idfolder)  + '/' + filename + '.' + ext as path_file from " + files_tbl + " where idfile = {0}", id_file)).Rows[0]["path_file"].ToString();
      if (fld_path[0] == '/' || fld_path[0] == '\\') fld_path = fld_path.Substring(1);
      return System.IO.Path.Combine(page.cfg_var("filesFolder"), fld_path).Replace('/', '\\');
    }

    public static string folder_path(db.db_schema conn, deeper.frmwrk.lib_page page, long id_folder, string base_path = "") {
      string folders_tbl = conn.meta_doc.table_from_code(deeper.db.meta_table.align_codes.folders);
      string fld_path = conn.dt_table(string.Format("select dbo.getPathOfFolder({0}) as fld from {1} where {2} = {3}", conn.schema.pkOfTable(folders_tbl), folders_tbl,
        conn.schema.pkOfTable(folders_tbl), id_folder)).Rows[0]["fld"].ToString();
      if (fld_path[0] == '/' || fld_path[0] == '\\') fld_path = fld_path.Substring(1);
      return System.IO.Path.Combine(string.IsNullOrEmpty(base_path) ? page.cfg_var("filesFolder") : base_path, fld_path).Replace('/', '\\');
    }

    public static int ins_file(db.db_schema db, string path_file, deeper.frmwrk.lib_page page) {

      string rel_path = strings.rel_path(page.cfg_var("filesFolder"), path_file).Replace("\\", "/");
      string files_tbl = db.meta_doc.table_from_code(deeper.db.meta_table.align_codes.files);

      // folders
      int id_folder = ins_folder(rel_path.Substring(0, rel_path.LastIndexOf('/')), page);

      // files
      DataTable dt = db.dt_table("select tbl.* from "
       + " (select idfile, dbo.getPathOfFolder(idfolder)  + '/' + filename + '.' + ext as path_file from files) tbl "
       + " where tbl.path_file = " + db.val_toqry(rel_path, deeper.db.fieldType.VARCHAR));
      if (dt.Rows.Count >= 1) return int.Parse(dt.Rows[0]["idfile"].ToString());

      return (int)db.exec(string.Format("insert into " + files_tbl + " (idfolder, ext, filename, title, note, dtins) values ({0}, {1}, {2}, null, null, getdate()) ", id_folder
        , db.val_toqry(Path.GetExtension(path_file).Substring(1), deeper.db.fieldType.VARCHAR), db.val_toqry(Path.GetFileNameWithoutExtension(path_file), deeper.db.fieldType.VARCHAR)), true);
    }

    public static void mountBackup(int id, deeper.frmwrk.lib_page page, bool onlyData = false, string logid = "", string name_db = "") {
      xmlDoc doc = new xmlDoc(Path.Combine(page.cfg_var("backupsFolder"), page.cfg_var("fsIndex")));
      string index_path = page.classPage.extract_dbpck(Path.Combine(page.cfg_var("backupsFolder")
          , doc.get_value("/root/files/file[@idfile=" + id.ToString() + "]", "name")));

      // aggiorno db
      db.db_schema db = name_db != "" ? page.conn_db(name_db, true) : page.conn_db_user(true);
      db.upgrade_data(page.conn_schema(index_path), onlyData, false
          , doc.get_value("/root/files/file[@idfile=" + id.ToString() + "]", "title"));

      // copio i files in locale
      page.files_from_backup(index_path);
    }

    public static void backupDb(page_cls pageClass, string title, string des = "", string folder_files = "") {
      // export sql db
      deeper.db.db_schema db = pageClass.page.conn_db_user();
      DateTime date = DateTime.Now;
      string pocket = db.export(pageClass.page.tmpFolder(), title, date
          , pageClass.page.tmpFolder(), des, Path.GetRandomFileName(), folder_files);

      backupPck(pageClass, pocket, db.name, title, date, db.ver, des);
    }

    public static void backupPck(page_cls pageClass, string pocket, string dbname, string title, DateTime date, string ver = "", string des = "") {
      if (title == "") throw new Exception("Non è specificato il titolo.");
      if (dbname == "") throw new Exception("Non è specificato il nome del database.");

      // salvo il backup nella cartella dei backups
      string bckFolder = pageClass.page.cfg_var("backupsFolder");
      if (!Directory.Exists(bckFolder))
        Directory.CreateDirectory(bckFolder);
      string pathBackup = Path.Combine(pageClass.page.cfg_var("backupsFolder"), "db-backup_" + date.ToString("yyyyMMdd-HHmmss") + ".gz");
      File.Move(pocket, pathBackup);

      // aggiorno l'indice xml
      XmlNode files = null;
      string fsidx = Path.Combine(pageClass.page.cfg_var("backupsFolder"), pageClass.page.cfg_var("fsIndex"));
      {
        xmlDoc doc = new xmlDoc(fsidx);
        if (!File.Exists(fsidx))
          doc.load_xml("<root schema='xmlschema.ifs'/>");

        // id file
        int idfile = doc.get_int("/root/files", "lastid") + 1;
        files = doc.node("/root/files", true);
        doc.set_attr("/root/files", "lastid", idfile.ToString());

        // file
        XmlNode file = files.AppendChild(doc.doc.CreateElement("file"));
        file.Attributes.Append(doc.doc.CreateAttribute("idfile")).Value = idfile.ToString();
        file.Attributes.Append(doc.doc.CreateAttribute("name")).Value = Path.GetFileName(pathBackup);
        file.Attributes.Append(doc.doc.CreateAttribute("title")).Value = title;
        if (des != "")
          file.Attributes.Append(doc.doc.CreateAttribute("des")).Value = des;
        file.Attributes.Append(doc.doc.CreateAttribute("date")).Value = date.ToString("yyyy-MM-ddTHH:mm:ss");
        file.Attributes.Append(doc.doc.CreateAttribute("type")).Value = "db-backup";

        // infos
        XmlNode infos = file.AppendChild(doc.doc.CreateElement("infos"));

        // info: conn
        XmlNode iconn = infos.AppendChild(doc.doc.CreateElement("info"));
        iconn.InnerText = dbname;
        iconn.Attributes.Append(doc.doc.CreateAttribute("name")).Value = "conn";

        // info: ver
        if (ver != "") {
          XmlNode iver = infos.AppendChild(doc.doc.CreateElement("info"));
          iver.InnerText = ver;
          iver.Attributes.Append(doc.doc.CreateAttribute("name")).Value = "ver";
        }

        doc.save();
      }
    }
  }
}


