using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using mlib;
using mlib.xml;
using mlib.tools;
using mlib.db;

namespace molinafy {
  public static class app {
    public static core _core = null;
    public static db_provider _db = null;

    public static config cfg { get { return _core.config; } }
    public static string var (string path) { return _core.config.get_var(path).value; }    

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// 
    [STAThread]
    static void Main (string[] args) {

      try {

        log.log_info("starting molinafy...");

        // core
        _core = new core(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        Directory.EnumerateFiles(_core.app_setting("settings-folder")).ToList().ForEach(f => {
          string doc_key = strings.rel_path(_core.base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
          log.log_info("load config doc: " + doc_key + " - " + f);
          _core.load_config(new xml_doc(f), doc_key, vars_key);
        });

        // db
        _db = cfg.exists_var("vars.db-connection") ? new db_provider(cfg.get_conn(cfg.get_var("vars.db-connection").value)) : null;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new frm_molinafy());

      } catch (Exception ex) { log.log_err(ex); MessageBox.Show(ex.Message, "Errore!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
  }
}