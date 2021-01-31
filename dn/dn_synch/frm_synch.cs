using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using dn_lib;
using dn_lib.tools;
using dn_lib.db;
using dn_lib.xml;

namespace fsynch
{
  public partial class frm_synch : Form
  {

    protected core _c = null;
    protected db_provider _conn = null;
    protected synch_machine _sm = null;

    // synch folders
    synch _s = null;
    protected bool _synch = false;
    protected int _max_lines = 1000, _min_lines = 700;

    // mouse move
    protected bool _md; private Point _ll;

    public frm_synch(core c, db_provider conn)
    {
      _c = c; _conn = conn; InitializeComponent();
    }

    #region double buffered

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);
    private const int SetExtendedStyle = 0x1036;
    private const int GetExtendedStyle = 0x1037;
    private const int BorderSelect = 0x00008000;
    private const int DoubleBuffer = 0x00010000;

    public static void enable_double_buffering(ListView listView)
    {
      if(listView == null) throw new ArgumentNullException("listView", "listView should not be null.");
      IntPtr handle = listView.Handle;
      int styles = SendMessage(handle, GetExtendedStyle, IntPtr.Zero, IntPtr.Zero).ToInt32();
      styles |= DoubleBuffer | BorderSelect;
      SendMessage(handle, SetExtendedStyle, IntPtr.Zero, (IntPtr)styles);
    }

    #endregion

    #region form

    protected void status_txt(string txt = "") { try { tlt_status.Text = txt; Application.DoEvents(); } catch { } }

    private void btn_ok_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e) { _md = true; _ll = e.Location; }

    private void lbl_title_MouseMove(object sender, MouseEventArgs e)
    {
      if(_md) { this.Location = new Point((this.Location.X - _ll.X) + e.X, (this.Location.Y - _ll.Y) + e.Y); this.Update(); }
    }

    private void lbl_title_MouseUp(object sender, MouseEventArgs e) { _md = false; }

    private void frm_synch_Shown(object sender, EventArgs e)
    {
      try {
        _s = main.create_synch();
        _s.synch_event += s_synch_event;

        status_txt();
        log_debug("pc name: " + Environment.MachineName);
        _sm = _s.get_synch_machine(Environment.MachineName);
        if(_sm == null || !_sm.active) {
          log_war("il pc '" + Environment.MachineName + "' non è configurato, è necessario inserirlo e attivarlo nella tabella synch_machines!");
          status_txt("in attesa di aggiornamento...");
          _sm = null;
          tmr_synch.Interval = 25 * 1000;
          tmr_synch.Start();
          return;
        }
        if(_sm.state != synch_machine.states.none) {
          log_war("c'è già un deepa synch attivo sul pc '" + Environment.MachineName + "', controllare la tabella synch_machines o i processi attivi su quella macchina!");
          status_txt("in attesa di chiusura synch...");
          _sm = null;
          tmr_synch.Interval = 25 * 1000;
          tmr_synch.Start();
          return;
        }

        _s.start_machine(_sm.synch_machine_id, main.local_ip());
        log_debug("secondi rilettura cartelle: " + _sm.seconds.ToString());
        log_debug("inizializzazione");
        try {
          _synch = true;
          status_txt("lettura cartelle...");
          reload_folders(true);
          tmr_synch.Interval = _sm.seconds * 1000;
          tmr_synch.Start();
        } finally { _synch = false; status_txt(); }
      } catch(Exception ex) { log_err(ex.Message); } 
    }

    private void s_synch_event(object sender, synch_event_args e) { if(e.init && !_first_synch) return; log_debug(e.message); }

    protected void add_msg_err(string txt) { append_line(txt, Color.Tomato); }

    protected void append_line(string txt, Color? clr = null)
    {
      try {
        rt_main.SelectionStart = rt_main.TextLength;
        rt_main.SelectionLength = 0;
        if(clr.HasValue) rt_main.SelectionColor = clr.Value;
        rt_main.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + txt + Environment.NewLine);
        rt_main.SelectionColor = rt_main.ForeColor;
        if(rt_main.Lines.Length > _max_lines) {
          int start_index = rt_main.GetFirstCharIndexFromLine(0);
          int count = rt_main.GetFirstCharIndexFromLine((rt_main.Lines.Count() - _min_lines) + 1) - start_index;
          rt_main.Text = rt_main.Text.Remove(start_index, count);
        }
        rt_main.ScrollToCaret();

        Application.DoEvents();
      } catch { }
    }

    protected void add_msg_warning(string txt) { append_line(txt, Color.Yellow); }

    protected void log_debug(string txt) { try { log.log_info(txt); add_msg(txt); } catch { } }
    protected void log_err(string txt) { try { log.log_err(txt); add_msg_err(txt); } catch { } }
    protected void log_war(string txt) { try { log.log_warning(txt); add_msg_warning(txt); } catch { } }

    protected void add_msg(string txt) { append_line(txt, Color.WhiteSmoke); }

    private void frm_synch_Load(object sender, EventArgs e)
    {
      try {
      } catch(Exception ex) { MessageBox.Show(ex.Message, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void tmr_synch_Tick(object sender, EventArgs e)
    {
      try {
        if(_synch) return;

        _synch = true;
        status_txt("lettura cartelle...");

        if(_sm == null) {
          _sm = _s.get_synch_machine(Environment.MachineName);
          if(_sm == null || !_sm.active || _sm.state != synch_machine.states.none) { _sm = null; return; }
          log_debug("secondi rilettura cartelle: " + _sm.seconds.ToString());
          log_debug("inizializzazione");
          _s.start_machine(_sm.synch_machine_id, main.local_ip());
          reload_folders();
          tmr_synch.Interval = _sm.seconds * 1000;
          return;
        }

        synch_machine sm = _s.get_synch_machine(Environment.MachineName);
        if(!sm.active) {
          log_war("deepa synch disattivato per il pc '" + Environment.MachineName + "'");
          _s.stop_machine(_sm.synch_machine_id);
          _sm = null;
          return;
        }

        if(sm.seconds != _sm.seconds) { _sm.seconds = sm.seconds; tmr_synch.Interval = _sm.seconds * 1000; }
        reload_folders();
      } catch(Exception ex) { log_err(ex.Message); } finally {
        _synch = false;
        status_txt();
      }
    }

    private void btn_min_Click_1(object sender, EventArgs e)
    {
      this.WindowState = FormWindowState.Minimized;
    }

    private void frm_synch_FormClosing(object sender, FormClosingEventArgs e)
    {
      try { if(_sm != null) _s.stop_machine(_sm.synch_machine_id); } catch(Exception ex) { log.log_err(ex); }
    }

    #endregion

    #region load folders

    protected bool _first_synch = false;
    protected void reload_folders(bool first = false)
    {
      _first_synch = first;

      synch_results res = _s.reload_folders();
      if(_sm != null && res.scan) _s.last_synch_machine(_sm.synch_machine_id, res.folders, res.files, res.deleted, res.seconds);
      if(!string.IsNullOrEmpty(res.err)) throw new Exception(res.err);
    }

    #endregion

  }
}
