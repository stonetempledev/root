using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Data;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.db
{
  public class db_schema : db_provider
  {
    static public string _indexXml = "db.xml", _metaXml = "meta.xml", _filesXml = "files.xml", _filesFolder = "files";
    protected static string _nullxml = "@null;";
    protected string _schema_path = null, _meta_path = null, _ver = "", _cur_ver = "";
    protected bool _req_meta = false, _exist_info = false, _req_schema = false;
    protected schema_doc _schema = null;
    protected meta_doc _meta = null;
    protected List<db_script> _scripts;

    public static string null_value { get { return _nullxml; } }
    public bool exist_schema { get { open_schema(); return _schema != null; } }
    public schema_doc schema { get { open_schema(); return _schema; } }
    public string schema_path { get { return _schema_path; } }
    public bool exist_info { get { return _exist_info; } }
    public string ver { get { return _ver; } }
    public string cur_ver { get { return _cur_ver; } }
    public long ver_long { get { return ver_to_long(_ver); } }
    public long cur_ver_long { get { return ver_to_long(_cur_ver); } }
    protected long ver_to_long(string ver) { return ver == "" ? 0 : (long.Parse(ver.Split('.')[0]) * 10000) + (long.Parse(ver.Split('.')[1]) * 100) + long.Parse(ver.Split('.')[2]); }
    protected void open_schema() {
      if (!_req_schema) {
        _schema = open_schema(_ver, _schema_path);
        if (_schema != null && !_schema.loaded && !System.IO.File.Exists(_schema.path))
          _schema = null;
        _req_schema = true;
      }
    }

    public static schema_doc open_schema(string ver, string schema_path) { return schema_path != "" ? new schema_doc(parse_dbexpr(schema_path, ver)) : null; }

    public bool exist_meta { get { open_meta(); return _meta != null; } }
    public string meta_path { get { return _meta_path; } }
    public meta_doc meta_doc { get { open_meta(); return _meta; } }
    protected void open_meta() {
      open_schema();

      if (!_req_meta) {
        string path = parse_dbexpression(_meta_path);
        _meta = path != "" ? new meta_doc(path, _schema) : null;
        if (path != "" && _meta != null && !_meta.loaded && !System.IO.File.Exists(_meta.path))
          _meta = null;
        //    throw new Exception("il meta xml '" + _meta.path + "' non è stato generato");
        _req_meta = true;
      }
    }

    public db_script script(string name) {
      db_script dbs = _scripts != null ? _scripts.FirstOrDefault(x => x.name == name) : null;
      if (dbs == null) throw new Exception("lo script '" + name + "' non esiste!"); return dbs;
    }

    public string parse_dbexpression(string expr, string ver = "") {
      return expr.Replace("{@ver}", ver != "" ? ver : _ver).Replace("{@cur_ver}", _cur_ver);
    }

    public static string parse_dbexpr(string expr, string ver, string cur_ver = "") {
      return expr.Replace("{@ver}", ver).Replace("{@cur_ver}", cur_ver);
    }

    public db_schema(string name, string group, int timeout, string provName, string connString, string language, string des
      , string dateFormatToQuery, string dateFormatToQuery2, string cur_ver, string name_schema = "", string name_meta = "", List<db_script> scripts = null)
      : base(name, connString, provName, timeout, group, language, des, dateFormatToQuery, dateFormatToQuery2) {
      _schema_path = name_schema;
      _meta_path = name_meta;
      _exist_info = exist_table("__INFOS");
      _cur_ver = cur_ver;
      _ver = info_value("ver");
      _scripts = scripts;
    }

    public db_schema(xmlDoc schema_doc, string meta_path = "", string des = "")
      : base("", "", "deeper.xml", -1, "", "", "", des) {
      _schema = new db.schema_doc(schema_doc);
      _req_schema = true;
      _meta_path = meta_path;
      _exist_info = exist_table("__INFOS");
      _ver = _schema.ver;
    }

    public static db_schema create_provider(string name, string connString, string provName, string cur_ver, int timeout = -1, string group = "", string language = ""
        , string dateFormatToQuery = "", string dateFormatToQuery2 = "", string des = "", string name_schema = "", string name_meta = "", List<db_script> scripts = null) {
      dbType type = db_provider.type_from_provider(connString, provName);
      return type == dbType.access ? new db_access(name, connString, provName, group, timeout, language, cur_ver, des, dateFormatToQuery, dateFormatToQuery2, name_schema, name_meta, scripts)
          : type == dbType.odbc ? new db_odbc(name, connString, provName, group, timeout, language, cur_ver, des, dateFormatToQuery, dateFormatToQuery2, name_schema, name_meta, scripts)
          : type == dbType.sqlserver ? new db_sqlserver(name, connString, provName, group, timeout, language, cur_ver, des, dateFormatToQuery, dateFormatToQuery2, name_schema, name_meta, scripts)
          : type == dbType.mysql ? new db_mysql(name, connString, provName, group, timeout, language, cur_ver, des, dateFormatToQuery, dateFormatToQuery2, name_schema, name_meta, scripts)
          : new db_schema(name, group, timeout, provName, connString, language, des, dateFormatToQuery, dateFormatToQuery2, cur_ver, name_schema, name_meta, scripts);
    }

    #region qrybuilder

    public string buildDelQry(string table, long id) {
      string pk = schema.pkOfTable(table);
      DateTime now = DateTime.Now;

      // links table
      string tmp = "";
      if (meta_doc.prefix_del() != null && meta_doc.links_table(table).Count() > 0) {
        DataRow dr = dt_table("select * from " + table + " where " + pk + " = " + id.ToString()).Rows[0];
        meta_doc.links_table(table).Where(x => dr[x.field] != DBNull.Value && dr[x.field].ToString() != "").ToList()
          .ForEach(lnk => { tmp = buildCpyRefQry(lnk.table, lnk.field, long.Parse(dr[lnk.field].ToString()), now, tmp); });
      }

      // main table
      return tmp + (meta_doc.prefix_del() != null ? "insert into " + meta_doc.prefix_del() + table + " (" + schema.qry_fields(table)
          + (meta_doc.field_del() != null ? ", " + meta_doc.field_del() : "") + ")"
          + " select " + schema.qry_fields(table) + (meta_doc.field_del() != null ? ", " + val_toqry(field_value(now), fieldType.DATETIME) : "")
          + "  from [" + table + "] where [" + pk + "] = " + id.ToString() + ";\n" : "")
          + "delete from [" + table + "] where [" + pk + "] = " + id.ToString() + ";";
    }

    protected string buildCpyRefQry(string lnk_table, string lnk_field, long id, DateTime now, string sql) {

      string pk = schema.pkOfTable(lnk_table);

      DataRow dr = dt_table("select * from " + lnk_table + " where " + pk + " = " + id.ToString()).Rows[0];
      foreach (meta_link lnk in meta_doc.links_table(lnk_table))
        sql = buildCpyRefQry(lnk.table, lnk.field, long.Parse(dr[lnk.field].ToString()), now, sql);

      string tmp2 = "delete from " + meta_doc.prefix_del() + lnk_table + " where " + string.Join(" AND ", meta_doc.indexUnique(lnk_table).Fields
       .Select(x => x.Name + " = " + val_toqry(field_value(dr[x.Name]), schema.table_field(lnk_table, x.Name).TypeField)))
       + " AND " + meta_doc.field_ref() + " <= " + val_toqry(field_value(now), fieldType.DATETIME) + "\n";

      return sql.ToLower().IndexOf(tmp2.ToLower()) < 0 ? sql + tmp2 + "insert into " + meta_doc.prefix_del() + lnk_table
        + " (" + schema.qry_fields(lnk_table) + (meta_doc.field_ref() != null ? ", " + meta_doc.field_ref() : "") + ")"
        + " select " + schema.qry_fields(lnk_table) + (meta_doc.field_ref() != null ? ", " + val_toqry(field_value(now), fieldType.DATETIME) : "")
        + "  from [" + lnk_table + "] where [" + pk + "] = " + id.ToString() + " \n" : sql;
    }

    #endregion

    #region public

    public bool sysTable(string table) { return exist_meta && _meta.sysTable(table); }

    protected XmlDocument schemaDb(string title, DateTime date, string notes = "", string objects = "") {
      // /root
      xmlDoc doc = xmlDoc.doc_from_xml("<root schema='xmlschema.db'/>");
      xmlDoc.set_attrs(doc.root_node, new Dictionary<string, string>() { { "name", name }, { "title", title }, { "date", date.ToString("yyyy/MM/dd HH:mm:ss") }
      , { "group", group }, { "des", des }, { "conn", conn }, { "type", type.ToString() }, { "ver", _ver }, { "notes", notes }});

      // /root/tables
      XmlNode tbls_node = doc.root_add("tables");
      foreach (string table in tables()) {
        logInfo("struttura tabella '" + table + "'");

        if (doc.node("/root/tables/table[@nameupper='" + table.ToUpper() + "']") != null
            || (objects != "" && !objects.ToLower().Split(',').Contains(table.ToLower())))
          continue;

        // <table>
        XmlNode tbl_node = xmlDoc.add_node(tbls_node, "table"
          , new Dictionary<string, string>() { { "nameupper", table.ToUpper() }, { "name", table } });

        // <cols>
        XmlNode cols_node = xmlDoc.add_node(tbl_node, "cols");
        foreach (schema_field col in table_fields(table))
          xmlDoc.add_node(cols_node, schema_doc.create_field_node(cols_node.OwnerDocument, col));

        // <indexes>
        XmlNode idxs_node = xmlDoc.add_node(tbl_node, "indexes");
        foreach (idx_table idx in table_idxs(table))
          idxs_node.AppendChild(idxs_node.OwnerDocument.ImportNode(schema_doc.create_idx_node(doc, idx), true));
      }

      // /root/procedures/procedure
      foreach (string procName in store_procedures()) {
        if (objects != "" && !objects.ToLower().Split(',').Contains(procName.ToLower()))
          continue;

        logInfo("s.p. '" + procName + "'");

        XmlNode procNode = xmlDoc.add_node(doc.root_add("procedures"), "procedure"
          , new Dictionary<string, string>() { { "name", procName } });
        procNode.AppendChild(doc.doc.CreateCDataSection(module_text(procName)));
      }

      // /root/functions/function
      foreach (string funcName in functions()) {
        if (objects != "" && !objects.ToLower().Split(',').Contains(funcName.ToLower()))
          continue;

        logInfo("functions '" + funcName + "'");

        XmlNode funcNode = xmlDoc.add_node(doc.root_add("functions"), "function", new Dictionary<string, string>() { { "name", funcName } });
        funcNode.AppendChild(doc.doc.CreateCDataSection(module_text(funcName)));
      }

      return doc.doc;
    }

    public string genSchema(string xmlpath, string title, DateTime date, string notes = "", string objects = "") {
      bool close = false;

      try {
        logInfo(" --- ESPORTAZIONE SCHEMA '" + conn + "'");

        if (open_conn())
          close = true;

        // paths
        string pathFolder = System.IO.Path.GetDirectoryName(xmlpath);
        if (pathFolder != "" && !System.IO.Directory.Exists(pathFolder))
          System.IO.Directory.CreateDirectory(pathFolder);

        // schema
        schemaDb(title, date, notes, objects).Save(xmlpath);
        logInfo("ok");

        return xmlpath;
      } catch (Exception ex) { logErr(ex); throw ex; } finally {
        if (close) close_conn();
      }
    }

    public void reset() {
      bool close = false;

      try {
        logInfo(" --- INIZIALIZZAZIONE TABELLE '" + conn + "'");

        if (open_conn())
          close = true;

        // ciclo tabelle
        foreach (string table in tables())
          truncate_table(table);

        logInfo("ok");
      } catch (Exception ex) { logErr(ex); throw ex; } finally {
        if (close) close_conn();
      }
    }

    public void exec_script(string name_script) {
      bool close = false;

      try {
        logInfo(" --- ESECUZIONE SCRIPT '" + name_script + "' SUL DB '" + conn + "'");

        if (open_conn())
          close = true;

        db_script scr = script(name_script);

        logInfo("titolo: " + scr.title + ", des.: " + scr.des);

        // ciclo comandi
        scr.exec(this);

        logInfo("ok");
      } catch (Exception ex) { logErr(ex); throw ex; } finally {
        if (close) close_conn();
      }
    }

    public string export(string path, string title, DateTime date, string tmpFolder, string notes, string namepocket, string folder_files = "") {
      bool close = false;

      try {
        logInfo(" --- ESPORTAZIONE DATI '" + conn + "'");

        if (open_conn()) close = true;

        // paths
        string pathFolder = System.IO.Path.Combine(tmpFolder, System.IO.Path.GetRandomFileName());
        if (System.IO.Directory.Exists(pathFolder))
          System.IO.Directory.Delete(pathFolder, true);
        System.IO.Directory.CreateDirectory(pathFolder);

        // schema
        XmlDocument doc = schemaDb(title, date, notes);
        doc.Save(System.IO.Path.Combine(pathFolder, _indexXml));

        // meta
        if (exist_meta && meta_doc.ver != "")
          meta_doc.save(System.IO.Path.Combine(pathFolder, _metaXml));

        // dati
        if (!System.IO.Directory.Exists(System.IO.Path.Combine(pathFolder, "data")))
          System.IO.Directory.CreateDirectory(System.IO.Path.Combine(pathFolder, "data"));

        long totalRows = 0;
        doc.SelectNodes("/root/tables/table").Cast<XmlNode>().ToList().ForEach(table => {
          string name = table.Attributes["name"].Value;
          long conteggio = 0;
          string rel_path = "data\\data" + name.ToUpper() + ".xml";
          if (export_data_xml(name, System.IO.Path.Combine(pathFolder, rel_path), table_fields(name), out conteggio)) {
            totalRows += conteggio;
            xmlDoc.set_attr(doc.SelectSingleNode("/root/tables"), "totalrows", totalRows.ToString());
            xmlDoc.set_attrs(table, new Dictionary<string, string>() { { "rows", conteggio.ToString() }, { "data", rel_path } });
          }
        });

        doc.Save(System.IO.Path.Combine(pathFolder, _indexXml));

        // files
        if (!string.IsNullOrEmpty(folder_files)) {
          logInfo(" --- ESPORTAZIONE FILES");

          xmlDoc doc_files = xmlDoc.create_fromxml("<files/>");

          cpy_contents_folder(doc_files.doc.DocumentElement, folder_files, System.IO.Path.Combine(pathFolder, _filesFolder));

          doc_files.doc.Save(System.IO.Path.Combine(pathFolder, _filesXml));
        }

        logInfo("ok");

        // zippo il tutto
        string pathPocket = System.IO.Path.Combine(path, namepocket + ".gz");
        zip.zipFolder(pathFolder, pathPocket);

        // ed elimino il contenuto della cartella
        if (System.IO.Directory.Exists(pathFolder))
          System.IO.Directory.Delete(pathFolder, true);

        return pathPocket;
      } catch (Exception ex) { logErr(ex); throw ex; } finally { if (close) close_conn(); }
    }

    /// <summary>
    /// Copia l'intero contenuto della cartella in un altra e annota tutti i files copiati in un documento xml
    /// </summary>
    protected void cpy_contents_folder(XmlNode files_node, string from_folder, string to_folder, string base_folder = null) {
      if (base_folder == null) base_folder = from_folder;
      if (!System.IO.Directory.Exists(to_folder)) System.IO.Directory.CreateDirectory(to_folder);
      foreach (string fl in System.IO.Directory.EnumerateFiles(from_folder)) {
        if ((System.IO.File.GetAttributes(fl) & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden) {
        xmlDoc.add_node(files_node, "file", new Dictionary<string, string>() { { "folder", from_folder.Substring(base_folder.Length).ToLower().Replace("/", "\\").Trim(new char[] { '\\' }) }
          , { "file", System.IO.Path.GetFileName(fl).ToLower() }, { "write_date", System.IO.File.GetLastWriteTime(fl).ToString("yyyy-MM-dd HH:mm:ss") }
          , { "access_date", System.IO.File.GetLastAccessTime(fl).ToString("yyyy-MM-dd HH:mm:ss") }
          , { "create_date", System.IO.File.GetCreationTime(fl).ToString("yyyy-MM-dd HH:mm:ss") }});
        System.IO.File.Copy(fl, System.IO.Path.Combine(to_folder, System.IO.Path.GetFileName(fl)));
      }
      }

      foreach (string fld in System.IO.Directory.EnumerateDirectories(from_folder))
        cpy_contents_folder(files_node, fld, System.IO.Path.Combine(to_folder, System.IO.Path.GetFileName(fld)), base_folder);
    }

    public bool export_data_xml(string table, string path_xml, List<schema_field> cols, out long conteggio, string where = "", bool request_rows = true
      , string table_alias = "TBL", XmlNode table_node = null, string order_by = "") {

      // conteggio 
      conteggio = -1;
      using (System.Data.Common.DbDataReader dr = dt_reader("select count(*) as conteggio from " + table + " " + table_alias + (where != "" ? " where " + where : ""))) {
        if (dr.Read())
          conteggio = long.Parse(dr["CONTEGGIO"].ToString());
      }

      if (request_rows && conteggio <= 0) return false;

      logInfo("esportazione dati tabella '" + table + "', righe: " + conteggio.ToString() + " in '" + path_xml + "'");

      if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path_xml)))
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path_xml));
      if (System.IO.File.Exists(path_xml)) System.IO.File.Delete(path_xml);

      using (XmlTextWriter writer = new XmlTextWriter(new System.IO.StreamWriter(path_xml))) {
        writer.WriteStartDocument();
        writer.WriteStartElement("root");
        writer.WriteAttributeString("schema", "xmlschema.tabledata");
        writer.WriteAttributeString("name", name);
        writer.WriteAttributeString("rows", conteggio.ToString());

        // <table>
        if (table_node != null) table_node.WriteContentTo(writer);

        // <data>
        writer.WriteWhitespace("\n");
        writer.WriteStartElement("data");
        cols.ForEach(col => { writer.WriteAttributeString("f" + col.Index.ToString("00"), col.Name); });
        writer.WriteEndElement(); writer.WriteWhitespace("\n");

        // <rows>
        writer.WriteStartElement("rows"); writer.WriteWhitespace("\n");

        long iRow = 0, iStart = 0, iRange = 100000;
        while (true) {

          System.Data.Common.DbDataReader rows = _dbType == dbType.sqlserver ? dt_reader(string.Format("SELECT tbl.* FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY [{0}]) AS RowNumber "
            + "  FROM (select * from {1} {5} {4}) tbl2) tbl WHERE tbl.ROWNUMBER > {2} AND tbl.ROWNUMBER <= {3} {6} "
            , cols[0].Name, table, iStart, iStart + iRange > conteggio ? conteggio : iStart + iRange, where != "" ? " where " + where : "", table_alias
            , !string.IsNullOrEmpty(order_by) ? "ORDER BY tbl." + order_by : ""))
            : dt_reader(string.Format("SELECT * FROM {0}", table));

          // <row>
          while (rows.Read()) {
            writer.WriteStartElement("row");

            // @field
            cols.ForEach(col => {
              if (field_value(rows[col.Name]) == _nullxml)
                throw new Exception("attenzione il record '" + iRow.ToString() + "' possiede il campo '" + col.Name + "' con il valore identico alla parola chiave interna '" + _nullxml + "' usata per rappresentare i DBNull");
              writer.WriteAttributeString("f" + col.Index.ToString("00"), field_value(rows[col.Name], _nullxml));
            });

            writer.WriteEndElement(); writer.WriteWhitespace("\n");
          }

          writer.Flush();
          rows.Close();

          // riciclo
          iStart += iRange;
          if (iStart >= conteggio) break;
        }

        writer.WriteEndElement(); // rows
        writer.WriteEndElement(); // root
        writer.WriteEndDocument();
      }

      return true;
    }

    public void upgrade_data(db_xml xmldb, bool onlyData = false, bool onlySchema = false, string des = "") {
      bool close = false;

      try {
        logInfo(" --- IMPORTAZIONE " + (onlyData ? "DATI" : "DATABASE") + " SU '" + conn + "'");

        if (open_conn()) close = true;

        if (xmldb.schema.doc.root_value("schema") == "") throw new Exception("l'indice xml non è corretto!");

        // controllo sul tipo di db 
        if (group != xmldb.group) throw new Exception("lo schema xml non è compatibile con il database da aggiornare!");

        // inizializzazione struttura
        if (!onlyData) initSchema(xmldb.schema.path, true, des != "" ? des : xmldb.schema.title);

        // importazione dati 
        if (!onlySchema) {
          foreach (string table in xmldb.tables()) {
            if (sysTable(table)) continue;

            // dati
            if (xmldb.exist_data(table)) {
              if (onlyData) { logInfo("cancellazione della tabella '" + table + "'"); truncate_table(table); }

              logInfo("importazione dati tabella '" + table + "'");

              if (onlyData && !exist_table(table)) {
                logWarning("il database di destinazione non contiene la tabella '" + table + "'");
                continue;
              }

              xmldb.xmldata_to_table(this, table);
            } else
              logInfo("non ci sono dati per la tabella '" + table + "'");
          }
        }

        // cancello l'eventuale folder di destinazione
        //if (System.IO.Directory.Exists(tmp_folder)) System.IO.Directory.Delete(tmp_folder, true);

        logInfo("ok");
      } finally {
        if (close) close_conn();
      }
    }

    public void dropTables() {
      bool close = false;
      try {
        if (open_conn()) close = true;

        logInfo(" --- CANCELLAZIONE DELLE TABELLE DEL DATABASE");

        tables().ForEach(tbl => { drop_table(tbl); });

        logInfo("ok");
      } finally { if (close) close_conn(); }
    }

    public void dropFunctions() {
      bool close = false;
      try {
        if (open_conn()) close = true;

        logInfo(" --- CANCELLAZIONE DELLE FUNZIONI DEL DATABASE");

        functions().ForEach(func => { drop_function(func); });

        logInfo("ok");
      } finally { if (close) close_conn(); }
    }

    public void dropProcedures() {
      bool close = false;
      try {
        if (open_conn()) close = true;

        logInfo(" --- CANCELLAZIONE DELLE STORED PROCEDURES DEL DATABASE");

        store_procedures().ForEach(sp => { drop_procedure(sp); });

        logInfo("ok");
      } finally { if (close) close_conn(); }
    }

    public void initSchema(string pathSchema, bool set_ver = true, string des = "") {
      bool close = false;

      try {
        if (!System.IO.File.Exists(pathSchema))
          throw new Exception("non esiste lo schema da importare '" + pathSchema + "'!");

        if (des == "") throw new Exception("non è specificata la descrizione dei contenuti del db che si sta importando '" + pathSchema + "'.");

        if (open_conn()) close = true;

        logInfo(" --- INIZIALIZZAZIONE SCHEMA DATABASE SU '" + conn + "' DA '" + pathSchema + "'");

        xmlDoc doc = new xmlDoc(pathSchema);

        if (doc.root_value("schema") == "")
          throw new Exception("l'indice xml non è corretto!");

        // controllo sul tipo di db 
        if (group != doc.root_value("group"))
          throw new Exception("lo schema xml non è compatibile con il database da aggiornare!");

        // ciclo tabelle 
        logInfo("ciclo sulle tabelle contenute nello schema xml");
        foreach (XmlNode table in doc.nodes("/root/tables/table")) {
          string tableName = table.Attributes["name"].Value;
          if (sysTable(tableName) && exist_table(tableName))
            continue;

          // struttura
          if (exist_table(tableName)) {
            logInfo("eliminazione della tabella '" + tableName + "'");
            drop_table(tableName);
          }

          logInfo("creazione tabella '" + tableName + "'");
          create_table(table);
        }

        // ciclo functions
        int tentativi = 1;
        while (tentativi <= 2) {
          bool err = false;

          foreach (XmlNode function in doc.nodes("/root/functions/function")) {
            string funcName = function.Attributes["name"].Value;
            string funcData = function.InnerText;

            // controllo schema
            if (funcName.IndexOf('.') >= 0) {
              string schemaName = funcName.Substring(0, funcName.IndexOf('.'));
              if (!there_schema(schemaName)) create_schema(schemaName);
            }

            // esiste già la function?
            if (exist_function(funcName)) {
              logInfo("aggiornamento function '" + funcName + "'");

              int length = 15;
              int find = funcData.IndexOf("CREATE FUNCTION", StringComparison.InvariantCultureIgnoreCase);
              if (find < 0) {
                find = funcData.IndexOf("CREATE  FUNCTION", StringComparison.InvariantCultureIgnoreCase);
                length = 16;
              }
              if (find < 0) {
                find = funcData.IndexOf("CREATE   FUNCTION", StringComparison.InvariantCultureIgnoreCase);
                length = 17;
              }

              if (find >= 0) funcData = funcData.Replace(funcData.Substring(find, length), "ALTER FUNCTION");
            } else {
              logInfo("creazione function '" + funcName + "'");

              int length = 14;
              int find = funcData.IndexOf("ALTER FUNCTION", StringComparison.InvariantCultureIgnoreCase);
              if (find < 0) {
                find = funcData.IndexOf("ALTER  FUNCTION", StringComparison.InvariantCultureIgnoreCase);
                length = 15;
              }
              if (find < 0) {
                find = funcData.IndexOf("ALTER   FUNCTION", StringComparison.InvariantCultureIgnoreCase);
                length = 16;
              }

              if (find >= 0) funcData = funcData.Replace(funcData.Substring(find, length), "CREATE FUNCTION");
            }

            try {
              exec(funcData);
            } catch { err = true; }
          }

          if (!err) break;

          tentativi++;
        }

        // ciclo store procedures
        foreach (XmlNode proc in doc.nodes("/root/procedures/procedure")) {
          string procName = proc.Attributes["name"].Value;
          string procData = proc.InnerText;

          // controllo schema
          if (procName.IndexOf('.') >= 0) {
            string schemaName = procName.Substring(0, procName.IndexOf('.'));
            if (!there_schema(schemaName)) {
              logInfo("creazione schema '" + schemaName + "'");
              create_schema(schemaName);
            }
          }

          // esiste già la procedure?
          if (exist_procedure(procName)) {
            logInfo("aggiornamento s.p. '" + procName + "'");

            int length = 16;
            int find = procData.IndexOf("CREATE PROCEDURE", StringComparison.InvariantCultureIgnoreCase);
            if (find < 0) {
              find = procData.IndexOf("CREATE  PROCEDURE", StringComparison.InvariantCultureIgnoreCase);
              length = 17;
            }
            if (find < 0) {
              find = procData.IndexOf("CREATE   PROCEDURE", StringComparison.InvariantCultureIgnoreCase);
              length = 18;
            }

            if (find >= 0)
              procData = procData.Replace(procData.Substring(find, length), "ALTER PROCEDURE");
          } else {
            logInfo("creazione s.p. '" + procName + "'");

            int length = 15;
            int find = procData.IndexOf("ALTER PROCEDURE", StringComparison.InvariantCultureIgnoreCase);
            if (find < 0) {
              find = procData.IndexOf("ALTER  PROCEDURE", StringComparison.InvariantCultureIgnoreCase);
              length = 16;
            }
            if (find < 0) {
              find = procData.IndexOf("ALTER   PROCEDURE", StringComparison.InvariantCultureIgnoreCase);
              length = 17;
            }

            if (find >= 0)
              procData = procData.Replace(procData.Substring(find, length), "CREATE PROCEDURE");
          }

          exec(procData);
        }

        if (set_ver && doc.root_value("ver") != "") setInfo("ver", doc.root_value("ver"), 0, des);

        logInfo("ok");
      } finally {
        if (close) close_conn();
      }
    }

    public bool existInfos() { return exist_table("__INFOS"); }

    public Dictionary<string, int> listInfos() {
      System.Data.DataSet ds = dt_set("SELECT DISTINCT I.NAME "
          + " , (SELECT MAX([INDEX]) FROM __INFOS WHERE NAME = I.NAME) AS MAX_INDEX "
          + " FROM __INFOS I WHERE I.[INDEX] = 0");

      Dictionary<string, int> result = new Dictionary<string, int>();
      foreach (System.Data.DataRow row in ds.Tables[0].Rows)
        result.Add(row["NAME"].ToString(), int.Parse(row["MAX_INDEX"].ToString()));

      return result;
    }

    public List<Dictionary<string, string>> getInfos() {
      List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
      foreach (string name in listInfos().Keys)
        result.Add(info(name));
      return result;
    }

    public void setInfo(string name, string value, int index = 0, string notes = "") {
      exec("INSERT INTO __INFOS ([NAME], [INDEX], [VALUE], [NOTES], DTINS) VALUES ('" + name + "', "
          + index + ", " + val_toqry(value, fieldType.VARCHAR, type) + ", "
          + val_toqry(notes, fieldType.VARCHAR, type) + ", getdate())");
    }

    public string info_value(string name, string def_val = "") { return _exist_info ? info(name, 0, def_val)["value"] : def_val; }

    public Dictionary<string, string> info(string name, int index = 0, string nullvalue = "") {
      System.Data.DataSet ds = dt_set("SELECT I.VALUE, I.NOTES FROM __INFOS I "
          + " WHERE I.[INDEX] = " + index.ToString()
          + "  AND I.NAME = " + val_toqry(name, fieldType.VARCHAR, type)
          + "  AND I.DTINS = (SELECT MAX(DTINS) FROM __INFOS WHERE NAME = I.NAME)");

      return (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) ?
          new Dictionary<string, string>() { { "name", name }, { "value", ds.Tables[0].Rows[0]["VALUE"].ToString() }
                    , { "notes", ds.Tables[0].Rows[0]["NOTES"].ToString() } }
          : new Dictionary<string, string>() { { "name", name }, { "value", nullvalue }, { "notes", "" } };
    }

    public List<Dictionary<string, string>> info_story(string name, int index = 0, string nullvalue = "") {
      System.Data.DataSet ds = dt_set("SELECT I.VALUE, I.NOTES, I.DTINS FROM __INFOS I "
          + " WHERE I.[INDEX] = " + index.ToString() + "  AND I.NAME = " + val_toqry(name, fieldType.VARCHAR, type)
          + " ORDER BY I.DTINS DESC");

      List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
      foreach (System.Data.DataRow row in ds.Tables[0].Rows)
        result.Add(new Dictionary<string, string>() { { "name", name }, { "value", row["VALUE"].ToString() }
                    , { "notes", row["NOTES"].ToString() }, { "date", row["DTINS"].ToString() } });

      return result;
    }

    // conteggi records tabelle collegati
    public Dictionary<string, long> rel_tables_count(string table, string id_val) {
      Dictionary<string, long> tables = new Dictionary<string, long>();
      foreach (meta_link linked in meta_doc.table_links(table)) {
        long cnt = count_links(linked, id_val);
        if (cnt > 0) { if (!tables.ContainsKey(linked.table_link)) tables.Add(linked.table_link, cnt); else tables[linked.table_link] += cnt; }
      }
      return tables;
    }

    public long count_links(meta_link lnk, string id_val) {
      return lnk.type == meta_link.types_link.normal ? get_count("select count(*) from [" + lnk.table_link + "] " + " where [" + lnk.field + "] = " + id_val) 
        : get_count("select count(*) from [" + lnk.table_link + "] where charindex('[" + id_val + "]', [" + lnk.field + "]) > 0");
    }

    public long count_remove_links(string table, long id) {
      return remove_links2(table, id, "", true, true);
    }

    public long count_notnull_remove_links(string table, long id) {
      return remove_links2(table, id, "", true, false, true);
    }

    /// <summary>
    /// eliminazione degli elementi che non hanno dipendenze
    /// </summary>
    public long remove_links(string table, long id, string idreplace = "", bool onlyreplace = false, bool throw_err = true, bool only_count = false) {
        return remove_links2(table, id, idreplace, onlyreplace, only_count, false, throw_err);
    }

    protected long remove_links2(string table, long id, string idreplace = ""
      , bool onlyreplace = false, bool onlycount = false, bool onlycount_notnullable = false, bool throw_err = true) {
      bool trans = !is_trans() && !(onlycount || onlycount_notnullable);
      try {
        if (onlycount && onlycount_notnullable) throw new Exception("entrambi i parametri onlycount non possono essere impostati!");

        if (trans) begin_trans();

        long records = 0;
        foreach (meta_link lnk in meta_doc.table_links(table)) {
          if (lnk.basic && !onlyreplace && !onlycount_notnullable) {
            string fldpry = schema.pkOfTable(lnk.table_link), flddip = "";
            foreach (meta_link lnk2 in meta_doc.table_links(lnk.table_link)) {
              DataTable childs = dt_table(string.Format("select {0} from {1} where {2} = {3}", fldpry, lnk.table_link, lnk.field, id));
              foreach(DataRow r_child in childs.Rows)
                records += remove_links2(lnk.table_link, long.Parse(r_child[fldpry].ToString()), "", false, onlycount, false, throw_err);
              // preservo le tabelle collegate indirette
              //flddip += (flddip != "" ? " union " : "") + "select [" + fldpry + "] from [" + lnk2.table_link + "] "
              //  + (lnk.type == meta_link.types_link.list ? " where charindex('[' + cast(tbl.[" + fldpry + "] as varchar) + ']', [" + lnk2.field + "]) > 0"
              //    : " where [" + lnk2.field + "] = tbl.[" + fldpry + "]");              
            }

            string wh = " where " + (lnk.type == meta_link.types_link.normal ? " tbl.[" + lnk.field + "] = " + id.ToString() 
              : " charindex('[" + id.ToString() + "]', tbl.[" + lnk.field + "]) > 0") + (flddip != "" ? " and not exists (" + flddip + ")" : "");
            if (onlycount) records += get_count("select count(*) from [" + lnk.table_link + "] tbl " + wh);
            else records += exec("delete tbl from [" + lnk.table_link + "] tbl " + wh);             
          } else {
            if (idreplace == "" && !_schema.isFieldNullable(lnk.table_link, lnk.field)) {
              long c = count_links(lnk, id.ToString()); 
              if (onlycount_notnullable) records += c;
              else if (c > 0) {
                if (throw_err) throw new Exception("il campo '" + lnk.field + "' della tabella '" + lnk.table_link
                   + "' è obbligatorio, è necessario specificare un valore sostitutivo!");
                else continue;
              }
            }

            if (!onlycount_notnullable) {
              if (onlycount) records += count_links(lnk, id.ToString());
              else {
                if(lnk.type == meta_link.types_link.normal) records += exec("update [" + lnk.table_link + "] set [" + lnk.field + "] = " 
                  + ((idreplace != "") ? idreplace : "NULL") + " where [" + lnk.field + "] = " + id.ToString());
                else records += exec("update [" + lnk.table_link + "] set [" + lnk.field + "] = replace([" + lnk.field + "], '[" + id.ToString() + "]', " 
                  + ((idreplace != "") ? "'[" + idreplace + "]'" : "''") + ") " + " where charindex('[" + id.ToString() + "]', [" + lnk.field + "]) > 0");
              }
            }
          }
        }

        if (trans) commit();

        return records;
      } catch (Exception ex) { if (trans) rollback(); throw ex; }
    }

    // vedo se è necessario il valore sostitutivo
    public bool susbstitute(string table, long id) {
      foreach (meta_link lnk in meta_doc.table_links(table))
        if (!lnk.basic && !_schema.isFieldNullable(lnk.table_link, lnk.field)
            && get_count("select count(*) from [" + lnk.table_link + "] where [" + lnk.field + "] = " + id.ToString()) > 0)
          return true;

      return false;
    }

    public void init_list_tags(string table, string field, string list_table, string values) {
      try {
        if (type != dbType.sqlserver) throw new Exception("funzione disponibile per il solo sql server");

        set_identity(list_table, true);

        meta_table mt = meta_doc.meta_tbl(list_table);

        exec("insert into " + list_table + " (" + _schema.pkOfTable(list_table) + (mt.row_title_field != "" ? ", " + mt.row_title_field : "")
          + (mt.row_notes_field != "" ? ", " + mt.row_notes_field : "") + (meta_doc.field_ins() != null ? ", " + meta_doc.field_ins() : "") + ")"
            + " SELECT item " + (mt.row_title_field != "" ? ", '#Codice_' + item + '#'" : "")
            + (mt.row_notes_field != "" ? ", 'Codice mancante inserito dall''applicativo'" : "") + (meta_doc.field_ins() != null ? ", getdate() " : "")
            + " FROM dbo.split('" + values + "', ',') lst");
      } catch (Exception ex) { logErr(ex); throw ex; } finally { set_identity(list_table, false); }
    }

    public virtual idx_table tableUniqueIndex(string table) {
      return table_idxs(table, true).FirstOrDefault(
        x => x.Fields.Count > 1 || (x.Fields.Count == 1 && meta_doc.field_ins() != null && x.Fields[0].Name.ToLower() != meta_doc.field_ins().ToLower())
        || (x.Fields.Count == 1 && meta_doc.field_ins() == null));
    }

    public idx_table indexUniqueOnIns(string table) {
      return table_idxs(table, true).FirstOrDefault(
        x => x.Fields.Count == 1 && meta_doc.field_ins() != null && x.Fields[0].Name.ToLower() == meta_doc.field_ins().ToLower());
    }

    public bool same_struct(string table, db_schema db2, out string reason) {
      reason = "";
      try {
        // table
        if (!db2.exist_table(table)) throw new Exception("non esiste la tabella " + table);

        // cols
        List<string> cols = new List<string>(table_fields(table).Select(fld => fld.Name.ToLower()))
          , cols2 = new List<string>(db2.table_fields(table).Select(fld => fld.Name.ToLower()));
        if (cols.Count != cols2.Count)
          throw new Exception("per la tabella " + table + " non corrisponde il numero delle colonne");
        cols.ForEach(fld => {
          if (!cols2.Contains(fld))
            throw new Exception("per la tabella " + table + " non è stata trovata la colonna '" + fld + "'");
        });

        return true;
      } catch (Exception ex) { reason = ex.Message; return false; }
    }

    public bool same_idxs(idx_table i1, idx_table i2) { string reason; return same_idxs(i1, i2, out reason); }

    public bool same_idxs(idx_table i1, idx_table i2, out string reason) {
      reason = "";
      try {
        // table
        if (i1 == null || i2 == null) throw new Exception("uno degli indici non esiste");

        // cols
        List<string> cols = new List<string>(i1.Fields.Select(fld => fld.Name.ToLower()))
          , cols2 = new List<string>(i2.Fields.Select(fld => fld.Name.ToLower()));
        if (cols.Count != cols2.Count) throw new Exception("per l'indice " + i1.Name + " non corrisponde il numero delle colonne");
        cols.ForEach(fld => {
          if (!cols2.Contains(fld))
            throw new Exception("per l'indice " + i1.Name + " non è stata trovata la colonna '" + fld + "'");
        });

        return true;
      } catch (Exception ex) { reason = ex.Message; return false; }
    }

    public bool same_contents(string table, db_schema db2, out string reason) {
      reason = "";
      try {
        if (!same_struct(table, db2, out reason)) return false;

        List<string> cols = new List<string>(table_fields(table).Select(fld => fld.Name.ToLower()));

        // data
        string pk = schema.pkOfTable(table);
        string sql = string.Format("select {0} from {1} order by {0}", string.Join(", ", cols), table);
        DataTable dt2 = (DataTable)db2.open_set(sql, true, false);
        foreach (DataRow row in ((DataTable)open_set(sql, true, false)).Rows) {
          DataRow[] row2 = dt2.Select(string.Format("{0} = {1}", pk, row[pk].ToString()));
          if (row2 == null || row2.Length == 0) throw new Exception("per la tabella " + table + " riga con '" + pk
            + "' = " + row[pk].ToString() + " non trovata");
          cols.ForEach(fld => {
            if (row[fld].ToString() != row2[0][fld].ToString())
              throw new Exception("per la tabella " + table + " la colonna " + fld + " alla riga con pk " + row[pk].ToString() + " non corrisponde");
          });
        }

        return true;
      } catch (Exception ex) { reason = ex.Message; return false; }
    }

    public bool init_table(string table, db_schema db_src, out string reason) {
      reason = "";
      bool identity = false;
      try {
        if (!db_src.same_struct(table, this, out reason)) return false;

        List<schema_field> cols = table_fields(table);
        if (schema.table_autonumber(table)) { set_identity(table, true); identity = true; }
        begin_trans();
        exec(string.Format("delete from {0}", table));
        foreach (DataRow row in db_src.dt_table(string.Format("select {0} from {1} order by {0}", string.Join(", ", cols.Select(c => c.Name)), table)).Rows) {
          exec(string.Format("insert into {0} ({1}) values ({2})"
            , table, string.Join(", ", cols.Select(c => c.Name)), string.Join(", ", cols.Select(c => val_toqry(field_value(row[c.Name]), c.TypeField)))));
        }
        commit();

        return true;
      } catch (Exception ex) { if (is_trans()) rollback(); reason = ex.Message; return false; } finally { if (identity) set_identity(table, false); }
    }

    #endregion

    #region internals struct

    protected bool upgradeTable(XmlNode tableNode, bool tryAddField = false, bool tryDelField = false, bool tryUpdField = false
        , bool tryAddIndexes = false, bool tryDelIndexes = false, bool tryUpdIndexes = false, bool noDefaults = false) {
      string tableName = tableNode.Attributes["name"].Value;

      // esistenza
      if (!exist_table(tableName))
        throw new Exception("la tabella '" + tableName + "' non esiste nel database e non può essere confrontata!");

      bool diff = false;

      // controllo struttura campi tabella
      {
        List<schema_field> dbFields = table_fields(tableName);
        List<schema_field> xmlFields = schema.tableFields(tableNode);

        // ciclo campi contenuti nell'xml che non ci sono nel db
        foreach (schema_field field in xmlFields) {
          if (findField(dbFields, field.Name) == null) {
            diff = true;

            // provo ad aggiungere il campo
            if (tryAddField)
              add_field(field, tableName);
          }
        }

        // ciclo campi contenuti nel database che non ci sono nell'XML
        foreach (schema_field field in dbFields) {
          if (findField(xmlFields, field.Name) == null) {
            diff = true;

            // provo a togliere il campo
            if (tryAddField)
              drop_field(field, tableName);
          }
        }

        // confronto campi comuni
        foreach (schema_field dbField in dbFields) {
          schema_field xmlField = findField(xmlFields, dbField.Name);
          if (xmlField == null)
            continue;

          bool diffField = false;
          if (dbField.TypeField != xmlField.TypeField) {
            diff = true;
            diffField = true;
          }

          // default
          if (!diffField && !noDefaults) {
            if (dbField.Default != xmlField.Default) {
              diff = true;
              diffField = true;
            }
          }

          // numeric, decimal
          if (!diffField) {
            if (dbField.TypeField == fieldType.DECIMAL) {
              if (dbField.NumPrecision != xmlField.NumPrecision ||
                  dbField.NumScale != xmlField.NumScale) {
                diff = true;
                diffField = true;
              }
            } else if (dbField.TypeField == fieldType.VARCHAR) {
              if (dbField.MaxLength != xmlField.MaxLength) {
                diff = true;
                diffField = true;
              }
            }
          }

          // provo ad aggiornarlo
          if (diffField && tryUpdField)
            upd_field(dbField, tableName);
        }
      }

      // confronto indici
      {
        List<idx_table> dbIndexes = table_idxs(tableName);
        List<idx_table> xmlIndexes = schema_doc.table_indexes(tableNode);

        // ciclo indici contenuti nell'xml che non ci sono nel db
        foreach (idx_table index in xmlIndexes)
          if (find_index(dbIndexes, index.Name, index.TableName) == null) {
            diff = true;
            if (tryAddIndexes) create_index(index);
          }

        // ciclo indici contenuti nel database che non ci sono nell'XML
        foreach (idx_table index in dbIndexes)
          if (find_index(xmlIndexes, index.Name, index.TableName) == null) {
            diff = true;
            if (tryDelIndexes)
              drop_index(index.Name, index.TableName);
          }

        // ciclo indici comuni
        foreach (idx_table index in dbIndexes) {
          idx_table xmlIdx = find_index(xmlIndexes, index.Name, index.TableName);
          if (xmlIdx != null && !idx_table.sameIndex(index, xmlIdx)) {
            diff = true;
            if (tryDelIndexes) {
              drop_index(index.Name, index.TableName);
              create_index(index);
            }
          }
        }
      }

      return diff;
    }

    #endregion
  }
}

// 1064