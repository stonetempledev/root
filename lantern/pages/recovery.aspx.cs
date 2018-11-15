using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using deeper.db;
using deeper.frmwrk;
using deeper.frmwrk.ctrls;
using deeper.lib;

public partial class _recovery : deeper.frmwrk.lib_page
{
  string _dbname = "";

  protected void Page_Init(object sender, EventArgs e) {
    // connessioni al database
    dbs.Items.Add(new ListItem("seleziona il database...", ""));
    cfg_nodes("/root/dbconns/dbconn").ForEach(db => {
      dbs.Items.Add(new ListItem(db.Attributes["name"].Value, db.Attributes["name"].Value));
    });
  }

  protected void Page_Load(object sender, EventArgs e) {
    // userConn
    if (!IsPostBack) {
      if (string.IsNullOrEmpty(userConn)) {
        if (base_conn() != "") dbs.SelectedValue = base_conn();
      } else {
        for (int i = 0; i < dbs.Items.Count; i++)
          if (dbs.Items[i].Value != "" && dbs.Items[i].Value != userConn) { dbs.Items.RemoveAt(i); i--; }
        if (dbs.Items.Count > 0) dbs.SelectedValue = userConn;
      }
    }
    _dbname = dbs.SelectedItem.Value;

    initCtrls();
  }

  protected void initCtrls(bool reconnect = true) {

    // inizializzazioni
    conn_des.InnerText = _dbname != "" ? cfg_value("/root/dbconns/dbconn[@name='" + _dbname + "']", "des") : "";
    view_tables.Visible = view_meta.Visible = view_schema.Visible = title_infos.Visible = import_pocket.Visible
      = gen_schema.Visible = integrita_schema.Visible = upgrade_schema.Visible = init_infos.Visible
      = exp_all.Visible = exp_db.Visible = false;
    infos.Controls.Clear();

    // utente
    uid.InnerText = userLogged;
    utype.InnerText = userTypeLogged;
    uconn.InnerText = userConn;
    try { uvis.InnerText = classPage.user_childs(userId); } catch (Exception ex) { uvis.InnerText = ex.Message; }

    // mi connetto
    db_schema db = null;
    try {
      db = _dbname != "" ? conn_db(_dbname, true, reconnect) : null;
      conn_state.InnerText = "connessione effettuata con successo";
      conn_ver.InnerText = db.ver == "" ? "versione non presente, è necessario inizializzare le infos!" : db.ver;
      last_ver.InnerText = _dbname != "" ? conn_curver(_dbname) : "nessun database selezionato";
    } catch (Exception ex) {
      _dbname = "";
      conn_state.InnerText = "non è stato possibile effettuare la connessione: " + ex.Message;
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message + ". <br><br><b>E' consigliabile un ripristino del database o selezionare una connessione raggiungibile.</b>", "Caricamento pagina"));
    }

    // check utenti
    check_utenti.InnerText = "";
    set_utenti.Visible = false;
    if (db != null && db.name != base_conn() && xmlDoc.node_val(conn_group(db.name), "name") == "web") {
      remove_cfg_style(check_utenti, "rcvry-sec-err");
      check_utenti.InnerText = "ok";
      db_schema base_db = conn_db_base();
      cfg_var("user_tables").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
        .ForEach(tbl => {
          string reason = "";
          if (!base_db.same_contents(tbl.Trim(), db, out reason)) {
            add_cfg_style(check_utenti, "rcvry-sec-err"); check_utenti.InnerText = reason; set_utenti.Visible = true; return;
          }
        });
    }

    // aggiorno i controlli
    try {
      
      // tables
      if (view_tables.Visible = db != null && db.exist_schema) view_tables.HRef = getPageRef("tables_list", "dbname=" + _dbname);

      if (_dbname != "") {
        init_infos.Visible = db.ver == "";
        gen_schema.Visible = exp_db.Visible = true;
        exp_all.Visible = integrita_schema.Visible = db.ver != "";

        // meta
        if (view_meta.Visible = db.exist_meta && File.Exists(schema_path_fromconn(_dbname, false, false, true, db.ver)))
          view_meta.HRef = schema_path_fromconn(_dbname, false, true, true, db.ver);

        // schema
        if (view_schema.Visible = db.exist_schema && File.Exists(schema_path_fromconn(_dbname, false, false, false, db.ver)))
          view_schema.HRef = schema_path_fromconn(_dbname, false, true, false, db.ver);

        // infos
        if (db.existInfos()) {
          title_infos.Visible = true;
          foreach (Dictionary<string, string> info in db.getInfos())
            ctrlsToParent(infos, new label(info["name"] + ":", "rcvry-sec-subtitle").control
                , dd_exclude("name", db.info_story(info["name"])));
        }

        // pockets
        import_pocket.Visible = true;

        // upgrade schema
        if (db.ver != "" && db.ver != db.cur_ver && db.ver_long < db.cur_ver_long) upgrade_schema.Visible = true;
      }

      // backups
      refresh_backups(db);

      // scripts
      refresh_scripts(db);
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message
          + ". <br><br><b>E' consigliabile un ripristino del database o selezionare una connessione raggiungibile.</b>"
          , "Caricamento pagina"));
    }
  }

  public void expschema_onclick(Object sender, EventArgs e) {
    try {
      string tmp_folder = tmpFolder();
      string outpath = System.IO.Path.Combine(tmp_folder, System.IO.Path.GetRandomFileName());
      conn_db(_dbname, false).genSchema(outpath
          , "export schema", DateTime.Now, "export schema");

      classPage.sendFile(outpath, "export-schema.xml");
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Esportazione schema"));
    }

    initCtrls();
  }

  public void logout_onclick(Object sender, EventArgs e) {
    try { redirect(getPageRef("login")); } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Logout"));
    }
  }

  public void exp_db_onclick(Object sender, EventArgs e) {
    try {
      // invio file 
      classPage.sendFile(conn_db(_dbname).export(tmpFolder(), "export database only data", DateTime.Now, tmpFolder()
          , "export database", Path.GetRandomFileName()), "export-data.gz");
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Logout"));
    }
  }

  public void exp_all_onclick(Object sender, EventArgs e) {
    try {
      // invio file 
      classPage.sendFile(conn_db(_dbname).export(tmpFolder(), "export database with files", DateTime.Now, tmpFolder()
          , "export database", Path.GetRandomFileName(), cfg_var("filesFolder")), "export-data.gz");
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Logout"));
    }
  }

  public void init_infos_onclick(Object sender, EventArgs e) {
    try {
      db_schema db = conn_db(_dbname);
      if (db.ver == "") {
        if (!db.exist_info)
          db.create_table(db_schema.open_schema(db.cur_ver, db.schema_path).table_node("__infos"));
        db.setInfo("ver", db.cur_ver, 0, "inizializzazione tabella infos");
      }
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Pulizia tabelle storico"));
    }

    initCtrls();
  }

  public void exec_script(Object sender, EventArgs e) {
    try {
      if (scripts.SelectedValue == "") classPage.regScript(classPage.scriptStartAlert("Devi selezionare uno script!", "Esecuzione script"));

      conn_db(_dbname, false).exec_script(scripts.SelectedValue);

      classPage.regScript(classPage.scriptStartAlert("Lo script è stato eseguito con successo!", "Esecuzione script"));
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message + "<br>"
            , "Esecuzione script di manutenzione"));
    }

    initCtrls();
  }

  public void view_script(Object sender, EventArgs e) {
    try {
      if (scripts.SelectedValue == "") classPage.regScript(classPage.scriptStartAlert("Devi selezionare uno script!", "Esecuzione script"));

      classPage.sendFile(conn_db(_dbname, false).script(scripts.SelectedValue).path
          , "script.xml");
    } catch (Exception ex) { classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Estrazione script")); }

    initCtrls();
  }

  protected string pathLog(string prefix = "") {
    return System.IO.Path.Combine(tmpFolder(), prefix + _dbname + ".log");
  }

  public void integrita_onclick(Object sender, EventArgs e) {
    redirect(getPageRef("tables_check", "dbname=" + _dbname));
  }

  public void reset_users_onclick(Object sender, EventArgs e) {
    try {
      db_schema db = conn_db(_dbname);
      db_schema base_db = conn_db_base();
      string reason = "";
      cfg_var("user_tables").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
        .ForEach(tbl => { if (!db.init_table(tbl.Trim(), base_db, out reason)) throw new Exception(reason); });

      classPage.regScript(classPage.scriptStartAlert("Tabelle utente inizializzate con successo!", "Copia impostazioni utenti"));
    } catch (Exception ex) { classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Inizializzazione utenti")); }

    initCtrls();
  }

  public void upgrade_onclick(Object sender, EventArgs e) {
    redirect(getPageRef("tables_upload", "dbname=" + _dbname + "&tover=" + conn_curver(_dbname) + "&type=sch"));
  }

  string table_url(string table, string db_name) { return getPageRef("table_view", "tbl=" + table + "&dbname=" + db_name); }

  protected void refresh_backups(db_schema db) {
    mount_row.Visible = db != null && db.ver != "";
    if (!mount_row.Visible) return;

    // elenco backups
    string group = conn_group(db.name).Attributes["name"].Value;
    if (group != bcksGroup.Value) {
      bcks.Items.Clear();
      string idx = System.IO.Path.Combine(cfg_var("backupsFolder"), cfg_var("fsIndex"));
      if (File.Exists(idx)) {
        xmlDoc doc = new xmlDoc(idx);
        foreach(System.Xml.XmlNode file in doc.nodes("/root/files/file[@type='db-backup']")
            .Cast<XmlNode>().OrderByDescending(x => x.Attributes["date"].Value)) {
            XmlNode cg = conn_group(file.SelectSingleNode("infos/info[@name='conn']").InnerText, false);
            if(cg != null && group == cg.Attributes["name"].Value)
                bcks.Items.Add(new ListItem(file.Attributes["title"].Value
                  + " - del: " + DateTime.Parse(file.Attributes["date"].Value).ToString(classPage.formatDates("dataEstesa"))
                  + (file.SelectSingleNode("infos/info[@name='conn']") != null ? " - conn.: " + file.SelectSingleNode("infos/info[@name='conn']").InnerText : "")
                  + (file.SelectSingleNode("infos/info[@name='ver']") != null ? " - ver.: " + file.SelectSingleNode("infos/info[@name='ver']").InnerText : "")
                  , file.Attributes["idfile"].Value));
        }
      }
      bcksGroup.Value = group;
    }

    btnMount.Visible = bcks.SelectedValue != "";
    btnMountData.Visible = bcks.SelectedValue != "";
  }

  protected void refresh_scripts(db_schema db) {
    scripts_row.Visible = db != null && db.ver != "";
    if (!scripts_row.Visible) return;

    // elenco scripts
    string group = conn_group(db.name).Attributes["name"].Value;
    if (group != scriptsGroup.Value) {
      scripts.Items.Clear();
      foreach (db_script scr in group_scripts(group))
        scripts.Items.Add(new ListItem((new xmlDoc(scr.path)).root_value("title"), scr.name));

      scriptsGroup.Value = group;
      scripts_onsel(null, null);
    }

    btnExecScript.Visible = scripts.SelectedValue != "";
  }

  public void scripts_onsel(Object sender, EventArgs e) {
    script_des.InnerText = scripts.SelectedValue != "" ?
      (new xmlDoc(conn_db(_dbname, false).script(scripts.SelectedValue).path)).root_value("des") : "";
  }

  public void importDb(Object sender, EventArgs e) {
    try {
      if (fileUpload.PostedFile.FileName == "") throw new Exception("devi selezionare un pacchetto o uno schema valido!");

      // schema o pacchetto
      bool xml = System.IO.Path.GetExtension(fileUpload.PostedFile.FileName).ToLower() == ".xml";
      string pathfile = System.IO.Path.Combine(tmpFolder(), System.IO.Path.GetRandomFileName() + (xml ? ".xml" : ".gz"));
      fileUpload.PostedFile.SaveAs(pathfile);

      // decompressione dello zip nel folder di destinazione
      string idx = xml ? pathfile : classPage.extract_dbpck(pathfile);
      conn_db(_dbname).upgrade_data(conn_schema(idx));

      files_from_backup(idx);

      classPage.regScript(classPage.scriptStartAlert("Il pacchetto è stato caricato con successo!", "Caricamento pacchetto"));
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Caricamento pacchetto"));
    }

    initCtrls();
  }

  public void extractPck(Object sender, EventArgs e) {
    try {
      if (fileUpload.Value == "") throw new Exception("devi selezionare un pacchetto valido!");
      if (System.IO.Path.GetExtension(fileUpload.PostedFile.FileName).ToLower() == ".xml") throw new Exception("devi selezionare un pacchetto valido!");
      if (pathFolder.Value == "") throw new Exception("devi specificare una cartella valida!");

      // upload pacchetto
      string pathfile = System.IO.Path.Combine(tmpFolder(), System.IO.Path.GetRandomFileName() + ".gz");
      fileUpload.PostedFile.SaveAs(pathfile);

      // creo l'eventuale folder di destinazione
      string outFolder = System.IO.Path.Combine(pathFolder.Value, System.IO.Path.GetFileNameWithoutExtension(fileUpload.Value));

      // decompressione dello zip nel folder di destinazione
      zip.unzip(pathfile, outFolder);

      classPage.regScript(classPage.scriptStartAlert("Il pacchetto è stato estratto con successo!", "Estrazione pacchetto"));
    } catch (Exception ex) { classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Estrazione pacchetto")); }
  }

  public void genPck(Object sender, EventArgs e) {
    try {
      if (pathFolderGen.Value == "" ||
          (pathFolderGen.Value != "" && !System.IO.Directory.Exists(pathFolderGen.Value)))
        throw new Exception("devi specificare una cartella valida!");

      // zippo il tutto
      string pathpck = System.IO.Path.Combine(tmpFolder(), System.IO.Path.GetRandomFileName() + ".gz");
      zip.zipFolder(pathFolderGen.Value, pathpck);

      // invio il pacchetto
      classPage.sendFile(pathpck, "export-data.gz");

      classPage.regScript(classPage.scriptStartAlert("Il pacchetto è stato creato con successo!", "Creazione pacchetto"));
    } catch (Exception ex) { classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Creazione pacchetto")); }
  }

  public void mountData(Object sender, EventArgs e) {
    if (bcks.SelectedValue == "") classPage.regScript(classPage.scriptStartAlert("Devi selezionare un backup!", "Ripristino backup"));

    mount_backup(bcks.SelectedValue, true);

    initCtrls();
  }

  public void mountDb(Object sender, EventArgs e) {
    if (bcks.SelectedValue == "") classPage.regScript(classPage.scriptStartAlert("Devi selezionare un backup!", "Ripristino backup"));

    mount_backup(bcks.SelectedValue, false);

    initCtrls();
  }

  protected void mount_backup(string idfile, bool onlydata = false) {
    try {
      if (idfile == "") return;

      deeper.pages.db_utilities.mountBackup(int.Parse(idfile), this, onlydata, "backup", _dbname);

      classPage.regScript(classPage.scriptStartAlert("Il backup è stato caricato con successo!", "Ripristino backup"));
    } catch (Exception ex) { classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message + "<br>", "Ripristino backup")); }

    initCtrls();
  }

  public void expBackup(Object sender, EventArgs e) {
    try {
      if (bcks.SelectedValue == "") classPage.regScript(classPage.scriptStartAlert("Devi selezionare un backup!", "Ripristino backup"));

      xmlDoc doc = new xmlDoc(System.IO.Path.Combine(cfg_var("backupsFolder"), cfg_var("fsIndex")));
      string pocket = System.IO.Path.Combine(cfg_var("backupsFolder"),
          doc.get_value("/root/files/file[@idfile=" + bcks.SelectedValue + "]", "name"));
      classPage.sendFile(System.IO.Path.Combine(cfg_var("backupsFolder")
          , doc.get_value("/root/files/file[@idfile=" + bcks.SelectedValue.ToString() + "]", "name")), "export-data.gz");
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Esportazione backup"));
    }
  }

  public void expSchema(Object sender, EventArgs e) {
    try {
      if (bcks.SelectedValue == "") classPage.regScript(classPage.scriptStartAlert("Devi selezionare un backup!", "Ripristino backup"));

      xmlDoc doc = new xmlDoc(System.IO.Path.Combine(cfg_var("backupsFolder"), cfg_var("fsIndex")));
      string pocket = System.IO.Path.Combine(cfg_var("backupsFolder"),
          doc.get_value("/root/files/file[@idfile=" + bcks.SelectedValue + "]", "name"));
      classPage.sendFile(classPage.extract_dbpck_index(pocket), "export-schema.xml");
    } catch (Exception ex) {
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore: " + ex.Message, "Esportazione schema"));
    }
  }
}
