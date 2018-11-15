using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Data;
using deeper.lib;
using deeper.db;

namespace deeper.frmwrk
{
  /// <summary>
  /// Contenitore delle funzioni dedicate alle pagine web
  /// </summary>
  public class lib_fncs
  {
    lib_page _page = null;
    db_schema _db = null;

    public lib_fncs(lib_page page) { _page = page; }

    public void set_active_db(db_schema db) { _db = db; }
    public int is_mag(string a, string b) { return int.Parse(string.IsNullOrEmpty(a) ? "0" : a) > int.Parse(string.IsNullOrEmpty(b) ? "0" : b) ? 1 : 0; }
    public int is_equal(string a, string b) { return a == b ? 1 : 0; }
    public int is_diff(string a, string b) { return a != b ? 1 : 0; }
    public string meta_attr(string table, string field, string attr) {
      if (_db == null) throw new Exception("non è stata specificata la connessione attiva!");
      return _db.meta_doc.meta_tbl(table).find_link(db_provider.name_field(field)).attr(attr);
    }
    public string info(string name) { return _page.classPage.info(name); }

    public XmlNodeList backups_list() {
      string idxPath = System.IO.Path.Combine(_page.cfg_var("backupsFolder"), _page.cfg_var("fsIndex"));
      if (!System.IO.File.Exists(idxPath))
        return null;

      xmlDoc doc = new xmlDoc(idxPath);
      // infos
      doc.nodes("/root/files/file[@type='db-backup']").Cast<XmlNode>().ToList().ForEach(file => {
        xmlDoc.set_attr(file, "infos", string.Join(",", file.SelectNodes("infos/info").Cast<XmlNode>().Select(info =>
          xmlDoc.node_val(_page.cfg_node("/root/fs/filetypes/filetype[@name='db-backup']"
            + "/infos/info[@name='" + info.Attributes["name"].Value + "']"), "title") + ": '" + info.InnerText + "'")));
      });

      return doc.nodes("/root/files/file[@type='db-backup']");
    }

    public List<XmlNode> schemas_list() {
      return _page.cfg_nodes("/root/xmlschemas/xmlschema");
    }

    public DataTable conns_list() {
      DataTable dt = new DataTable();
      dt.Columns.AddRange(new DataColumn[] { new DataColumn("conn", typeof(string))
        , new DataColumn("conn_des", typeof(string)) });

      foreach (XmlNode conn in _page.cfg_nodes("/root/dbconns/dbconn[@group='web']"))
        dt.Rows.Add(new object[] { conn.Attributes["name"].Value, conn.Attributes["des"].Value });

      return dt;
    }

    public DataTable procs_list() {
      DataTable dt = new DataTable();
      dt.Columns.AddRange(new DataColumn[] { new DataColumn("code", typeof(string))
        , new DataColumn("des", typeof(string)) });

      foreach (XmlNode s in _page.conn_group(_page.name_conn_user()).SelectNodes("procs/proc"))
        dt.Rows.Add(new object[] { s.Attributes["code"].Value, s.Attributes["des"].Value });

      return dt;
    }
  }
}