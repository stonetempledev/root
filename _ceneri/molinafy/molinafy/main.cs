using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutocompleteMenuNS;
using mlib;

namespace molinafy {
  public partial class main : frm_base {

    protected bool _showed_eventi = true;

    public main ()
      : base() {
      InitializeComponent();

    }

    #region main events

    protected ToolStripControlHost lbl_warning = null, lbl_user = null;
    private void main_Load (object sender, EventArgs e) {
      try {
        add_info("loading main...");

        // inits
        lbl_title.Text = "molinafy";

        lbl_user = new ToolStripControlHost(new Label { Text = "è connesso: " + app._user.user_name, ForeColor = Color.Gray, BackColor = Color.Transparent });
        ss.Items.Add(lbl_user);

        lbl_warning = new ToolStripControlHost(new Label { Text = "", Cursor = Cursors.Hand, ForeColor = Color.Tomato, BackColor = Color.Transparent });
        ss.Items.Add(lbl_warning);
        lbl_warning.Click += new EventHandler(lbl_warning_Click);

        upd_eventi_item_text();

        add_info("...main loaded...");

      } catch (Exception ex) { add_err(ex); }
    }

    void lbl_warning_Click (object sender, EventArgs e) { }

    private void pb_close_Click (object sender, EventArgs e) { this.Close(); }
    private void pb_icon_MouseDown (object sender, MouseEventArgs e) {
      if (e.Button == System.Windows.Forms.MouseButtons.Left) {
        cms_main.Show(this, new Point(e.X, e.Y));
      }
    }

    private void lbl_title_DoubleClick (object sender, EventArgs e) { if (title_doubleclick(e)) return; }
    private void lbl_title_MouseDown (object sender, MouseEventArgs e) { if (title_mousedown(e)) return; }
    private void lbl_title_MouseMove (object sender, MouseEventArgs e) { if (title_mousemove(e)) return; }
    private void lbl_title_MouseUp (object sender, MouseEventArgs e) { if (title_mouseup(e)) return; }

    private void logout_item_Click (object sender, EventArgs e) {
      this.Hide();
      if (!app.logout()) { Application.Exit(); return; }
      this.Show();
    }

    private void events_item_Click (object sender, EventArgs e) {
      try {
        split.Panel2Collapsed = !split.Panel2Collapsed;
        upd_eventi_item_text();
      } catch { }
    }

    private void tmr_warning_Tick (object sender, EventArgs e) {
      lbl_warning.Text = "";
      tmr_warning.Stop();
    }

    private void main_FormClosing (object sender, FormClosingEventArgs e) {
      try {
        // salvataggio web doc
        if (_doc != null) _doc.check_changed_doc();
      } catch (Exception ex) {
        if (MessageBox.Show(ex.Message + "\n\nVuoi chiudere ugualmente?"
          , "Si è verificato un errore!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.No)
          e.Cancel = true;
      }
    }

    #endregion

    #region interfaccia

    protected void upd_eventi_item_text () {
      events_item.Text = !split.Panel2Collapsed ? "Nascondi Eventi" : "Visualizza Eventi";
    }

    public void add_info (string txt) { rtbe.add_info(txt); }
    public void add_err (string txt) { rtbe.add_err(txt); }
    public void add_err (Exception ex) { rtbe.add_err(ex); }
    public void add_war (string txt) { rtbe.add_war(txt); }

    protected void status_avviso (string txt) { lbl_warning.ForeColor = Color.DarkGray; lbl_warning.Text = txt; tmr_warning.Interval = 2000; tmr_warning.Start(); }
    protected void status_errore (string txt) { lbl_warning.ForeColor = Color.Tomato; lbl_warning.Text = txt; tmr_warning.Interval = 4000; tmr_warning.Start(); }

    #endregion

    #region entities

    protected wb_doc _doc = null;
    protected void open_doc (int id_doc) {
      try {
        _doc = new wb_doc(id_doc);
        _doc.Top = rtb_cmd.Bottom;
        _doc.Left = 0;
        _doc.Width = split.Panel1.Width;
        _doc.Height = split.Panel1.Height - rtb_cmd.Bottom;
        _doc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
        _doc.SavedElement += new EventHandler(doc_SavedElement);
        split.Panel1.Controls.Add(_doc);
      } catch (Exception ex) { status_errore("si è verificato un errore nel caricare il documento!"); add_err(ex); }
    }

    void doc_SavedElement (object sender, EventArgs e) { status_avviso("elemento salvato"); }

    #endregion

  }
}

