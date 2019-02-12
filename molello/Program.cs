using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using mlib;
using mlib.xml;
using mlib.tools;
using mlib.db;
using molello.forms;

namespace molello {
  static class app {
    public static core _core = null;
    public static db_provider _db = null;

    public static config cfg { get { return _core.config; } }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main (string[] args) {
      try {

        // core
        _core = new core(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        Directory.EnumerateFiles(_core.app_setting("settings-folder")).ToList().ForEach(f => {
          string doc_key = strings.rel_path(_core.base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
          log.log_info("load config doc: " + doc_key + " - " + f);
          _core.load_config(new xml_doc(f), doc_key, vars_key);
        });

        // db
        _db = new db_provider(cfg.get_conn(cfg.get_var("vars.db-connection").value));

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // argomenti
        foreach (string arg in args) {
          // save-tree:<path output>
          if (arg.Length > 10 && arg.Substring(0, 10) == "save-tree:") {
            System.IO.StreamWriter sw = null;
            try {
              List<classes.node> nds = new List<classes.node>();
              foreach (classes.node n in classes.node.dal.get_nodes()) {
                n.livello = 0; nds.Add(n); add_sub_nodes(nds, n);
              }

              sw = new StreamWriter(arg.Substring(10));
              foreach (classes.node n in nds) {
                sw.WriteLine(string.Format("{0} -{3}{1} ({2})"
                  , n.link_id.ToString("D5"), n.title, n.id, " ".PadLeft((n.livello + 1) * 2, ' ')));
              }
              sw.Close(); sw = null;
              frm_popup.show_msg(_core.config.get_var("vars.app-title").value, "File '" + arg.Substring(10) + "' generato con successo!");
            } catch (Exception ex) { frm_popup.show_error(ex.Message); } finally { if (sw != null) sw.Close(); }
            return;
          }
        }

        // main
        Application.Run(new frm_nodes());
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    public static void add_sub_nodes (List<classes.node> nds, classes.node nd) {
      foreach (classes.node n in classes.node.dal.get_nodes(nd.link_id)) {
        n.livello = nd.livello + 1;
        nds.Add(n);
        add_sub_nodes(nds, n);
      }
    }
  }
}
