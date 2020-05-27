﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using mlib;
using mlib.db;
using mlib.tools;
using mlib.xml;
using dn_lib;

namespace fsynch {
  static class main {

    public static core _c = null;
    public static db_provider _conn = null;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
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
        _conn = db_provider.create_provider(_c.config.get_conn(_c.config.get_var("vars.db-connection").value));

        // docs
        foreach (KeyValuePair<string, xml_doc> d in docs) {
          string[] keys = d.Key.Split(new char[] { ';' });
          string doc_key = keys[0], vars_key = keys[1], f = keys[2];
          log.log_info("load config doc: " + doc_key + " - " + f);
          _c.load_config(d.Value, doc_key, _conn, vars_key: vars_key);
        }

        Application.Run(new frm_synch(_c, _conn));
      } catch (Exception ex) {
        MessageBox.Show(ex.Message, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      } finally {
        if (_conn != null) _conn.close_conn();
      }
    }

    public static synch create_synch() { return new synch(_conn, _c, _c.config); }
  }
}
