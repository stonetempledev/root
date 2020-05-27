using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using dn_lib;
using mlib;
using mlib.tools;
using mlib.db;
using mlib.xml;

namespace fsynch {
  public partial class frm_synch : Form {

    protected core _c = null;
    protected db_provider _conn = null;

    // synch folders
    synch _s = null;
    List<synch_folder> _folders = null;

    // mouse move
    private bool _md; private Point _ll;

    // lista messaggi
    protected Font _fbold;

    public frm_synch(core c, db_provider conn) {
      _c = c; _conn = conn; InitializeComponent();
    }

    private void btn_ok_Click(object sender, EventArgs e) {
      Close();
    }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e) { _md = true; _ll = e.Location; }

    private void lbl_title_MouseMove(object sender, MouseEventArgs e) {
      if (_md) { this.Location = new Point((this.Location.X - _ll.X) + e.X, (this.Location.Y - _ll.Y) + e.Y); this.Update(); }
    }

    private void lbl_title_MouseUp(object sender, MouseEventArgs e) { _md = false; }

    private void frm_synch_Shown(object sender, EventArgs e) {
      try {

        log_debug("inizializzazione");
        _s = main.create_synch();
        reload_folders(true);

      } catch (Exception ex) { log_err(ex.Message); }
    }

    protected bool _synch = false;

    protected void reload_folders(bool first = false) {
      if (_synch) return;
      try {
        _synch = true;

        // folders to synch
        _folders = _s.list_synch_folders();
        if (first) {
          foreach (synch_folder f in _folders)
            log_debug(string.Format("cartella di sincronizzazione '{0}' - {1}, path: {2}"
              , f.title, f.des, f.synch_path));
        }

        // leggo le cartelle
        _s.clean_readed();
        foreach (synch_folder f in _folders) {
          if(first) log_debug(string.Format("leggo la cartella {0}", f.synch_path));
          init_synch_folder(f.id, f.synch_path);
        }
        int deleted = _s.del_unreaded();
        if (deleted > 0) log_debug("cancellati " + deleted.ToString() + " files/folders");
      } catch (Exception ex) { log_err(ex.Message); } finally { _synch = false; }
    }

    protected void log_debug(string txt) { try { log.log_info(txt); add_msg(txt); } catch { } }
    protected void log_err(string txt) { try { log.log_err(txt); add_msg_err(txt); } catch { } }

    protected void add_msg(string txt) {
      try { lw_msg.Items.Add(new ListViewItem() { Text = txt }); Application.DoEvents(); } catch { }
    }

    protected void add_msg_err(string txt) {
      try {
        lw_msg.Items.Add(new ListViewItem() { Text = txt, ForeColor = Color.Tomato, Font = _fbold }); Application.DoEvents();
      } catch { }
    }

    protected void init_synch_folder(int synch_folder_id, string path, long? parent_id = null) {
      try {
        // folders
        foreach (string fp in Directory.EnumerateDirectories(path)) {
          DirectoryInfo di = new DirectoryInfo(fp);
          string tp;
          long folder_id = _s.ins_folder(synch_folder_id, parent_id, di.Name, out tp);
          if (tp == "insert") log_debug("added folder: " + fp);
          init_synch_folder(synch_folder_id, Path.Combine(path, di.Name), folder_id);
        }

        // files
        if (parent_id.HasValue) {
          foreach (string fn in Directory.EnumerateFiles(path)) {
            FileInfo fi = new FileInfo(fn);
            string tp;
            _s.ins_file(parent_id.Value, fi.Name, out tp);
            if (tp == "insert") log_debug("added file: " + fn);
          }
        }
      } catch (Exception ex) { log_err(ex.Message); }
    }

    private void frm_synch_Load(object sender, EventArgs e) {
      try {
        _fbold = new Font(lw_msg.Font.FontFamily, lw_msg.Font.Size, FontStyle.Bold);
        resize_col();
      } catch (Exception ex) { MessageBox.Show(ex.Message, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void lw_msg_Resize(object sender, EventArgs e) { try { resize_col(); } catch { } }

    protected void resize_col() { try { lw_msg.Columns[0].Width = lw_msg.ClientSize.Width - 5; } catch { } }

    private void tmr_synch_Tick(object sender, EventArgs e) {
      try {
        reload_folders();
      } catch (Exception ex) { log_err(ex.Message); }
    }

  }
}

/*

var _watcher = new FileSystemWatcher();
_watcher.Path = @"\\10.31.2.221\shared\";
_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
_watcher.Filter = "*.txt";
_watcher.Created += new FileSystemEventHandler((x, y) =>Console.WriteLine("Created"));
_watcher.Error += new ErrorEventHandler( (x, y) =>Console.WriteLine("Error"));
_watcher.EnableRaisingEvents = true;
Console.ReadKey();
 
 NET USE Z: \\server2\Secondary\temp\watch_folder /user:Domain\UserName Password
 
 */