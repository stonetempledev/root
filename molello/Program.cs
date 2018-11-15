using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using mlib;
using mlib.xml;
using mlib.tools;

namespace molello {
  static class app {
    public static core _core = null;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main () {

      // core
      _core = new core(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
      Directory.EnumerateFiles(_core.app_setting("settings-folder")).ToList().ForEach(f => {
        string doc_key = strings.rel_path(_core.base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
        log.log_info("load config doc: " + doc_key + " - " + f);
        _core.load_config(new xml_doc(f), doc_key, vars_key);
      });

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new frm_nodes());
    }
  }
}
