using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Data;
using mlib;

namespace molinafy {
  static class app {
    public static core _core = null;
    public static db_provider _conn = null;
    public static config cfg { get { return _core.config; } }
    public static string var (string path) { return _core.config.get_var(path).value; }
    public static user _user = null;
    public static main _main = null;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main () {
      try {
        log.log_info("starting molinafy...");

        // core
        log.log_info("open config files...");
        _core = new core(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        Directory.EnumerateFiles(_core.app_setting("settings-folder")).ToList().ForEach(f => {
          string doc_key = strings.rel_path(_core.base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
          log.log_info("load config doc: " + doc_key + " - " + f);
          _core.load_config(new xml_doc(f), doc_key, vars_key);
        });
        log.log_info("opened config files");

        // db conn
        log.log_info("connection to db...");
        _conn = cfg.exists_var("vars.db-connection") ? new db_provider(cfg.get_conn(cfg.get_var("vars.db-connection").value)) : null;
        log.log_info("db connected");

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // check user
        if (!check_user()) 
          return;

        // main
        log.log_info("application run...");
        _main = new main();
        Application.Run(_main);
      } catch (Exception ex) {
        log.log_err(ex);
        MessageBox.Show(ex.Message, "Errore!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public static bool logout () {
      _user = null;
      user.logout();
      return check_user();
    }

    public static bool check_user () {
      _user = user.active_user();
      if (_user != null) return true;
      log.log_warning("utente non loggato!");
      _user = login.enter_login();
      return _user != null;
    }
  }
}
