using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Renci.SshNet;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using MySql.Data;
using MySql.Data.MySqlClient;
using dlib;
using dlib.db;
using dlib.tools;
using dlib.xml;

namespace dn_client {
  static class Program {

    public static core _c = null;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {

      db_provider conn = null;
      try {

        // init
        _c = new core(AppDomain.CurrentDomain.BaseDirectory);

        // configs

        // docs
        Dictionary<string, xml_doc> docs = new Dictionary<string, xml_doc>();
        Directory.EnumerateFiles(_c.app_setting("settings-folder")).ToList().ForEach(f => {
          string doc_key = strings.rel_path(_c.base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
          log.log_info("load xml config doc: " + doc_key + " - " + f);
          docs.Add(string.Format("{0};{1};{2}", doc_key, vars_key, f), new xml_doc(f));
        });

        // vars
        foreach (KeyValuePair<string, xml_doc> d in docs) {
          string[] keys = d.Key.Split(new char[] { ';' });
          string doc_key = keys[0], vars_key = keys[1], f = keys[2];
          log.log_info("load vars doc: " + doc_key + " - " + f);
          _c.load_base_config(d.Value, doc_key, vars_key);
        }

        // conn
        conn = open_conn();

        // docs
        foreach (KeyValuePair<string, xml_doc> d in docs) {
          string[] keys = d.Key.Split(new char[] { ';' });
          string doc_key = keys[0], vars_key = keys[1], f = keys[2];
          log.log_info("load config doc: " + doc_key + " - " + f);
          _c.load_config(d.Value, doc_key, conn, vars_key: vars_key);
        }

        // opened client
        conn.exec(_c.parse_query("client.opened", new string[,] { { "ip_machine", dlib.core.machine_ip() } }));
        conn.close_conn(); conn = null;

        // data providers
        //string l = "";
        //foreach (DataRow dr in DbProviderFactories.GetFactoryClasses().Rows)
        //  l += "\n - " + string.Join(", ", dr.Table.Columns.Cast<DataColumn>().Select(
        //    col => dr[col.ColumnName] != DBNull.Value ? col.ColumnName + ": " + dr[col.ColumnName].ToString() : ""));

        // test
        //try {
        //  List<string> files = new List<string>() { "c:\\tmp\\deepanotes.sql", "c:\\tmp\\toyn.docs.sql", "c:\\tmp\\toyn.sql" };
        //  System.Diagnostics.Process process = new System.Diagnostics.Process();
        //  System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        //  startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        //  startInfo.FileName = "cmd.exe";
        //  startInfo.Arguments = "/c \"" + files[0] + "\"";
        //  process.StartInfo = startInfo;
        //  process.Start(); 
        //  System.Diagnostics.Process.Start("cmd.exe", "\"c:\\tmp\\toyn.sql\""); 
        //  return;
        //} catch (Exception ex) { MessageBox.Show(ex.Message); }

        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new frm_main(_c));

        // close client
        conn = open_conn();
        conn.exec(_c.parse_query("client.closed", new string[,] { { "ip_machine", dlib.core.machine_ip() } }));
        conn.close_conn(); conn = null;

      } catch (Exception ex) {
        MessageBox.Show(ex.Message, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        if (conn != null) { conn.close_conn(); }
      }
    }

    public static db_provider open_conn() {
      return db_provider.create_provider(_c.config.get_conn(_c.config.get_var("lib-vars.db-connection").value));
    }
  }
}
