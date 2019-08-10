using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mlib;

namespace molinafy {
  public partial class main : frm_base {

    protected bool _showed_eventi = true;

    public main ()
      : base() {
      InitializeComponent();

    }

    #region eventi

    protected ToolStripControlHost lbl_changed = null, lbl_user = null;
    private void main_Load (object sender, EventArgs e) {
      try {
        add_info("loading main...");
        
        // inits
        lbl_title.Text = "molinafy";

        lbl_user = new ToolStripControlHost(new Label { Text = "è connesso: " + app._user.user_name, ForeColor = Color.Gray, BackColor = Color.Transparent });
        ss.Items.Add(lbl_user);

        lbl_changed = new ToolStripControlHost(new Label { Text = "", Cursor = Cursors.Hand, ForeColor = Color.Tomato, BackColor = Color.Transparent });
        ss.Items.Add(lbl_changed);
        lbl_changed.Click += new EventHandler(lbl_changed_Click);

        upd_eventi_item_text();
        
        add_info("...main loaded...");

        // test open documento
        open_doc(1);

      } catch (Exception ex) { add_err(ex); }
    }

    void lbl_changed_Click (object sender, EventArgs e) { _doc.save_doc(); }

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

    #endregion

    #region interfaccia

    protected void upd_eventi_item_text () {
      events_item.Text = !split.Panel2Collapsed ? "Nascondi Eventi" : "Visualizza Eventi";
    }

    public void add_info (string txt) { rtbe.add_info(txt); }
    public void add_err (string txt) { rtbe.add_err(txt); }
    public void add_err (Exception ex) { rtbe.add_err(ex); }
    public void add_war (string txt) { rtbe.add_war(txt); }

    #endregion

    #region documents

    protected wb_doc _doc = null;
    protected void open_doc (int id_doc) {
      try {
        _doc = new wb_doc(id_doc);
        _doc.Top = rtb_cmd.Bottom;
        _doc.Left = 0;
        _doc.Width = split.Panel1.Width;
        _doc.Height = split.Panel1.Height - rtb_cmd.Bottom;
        _doc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
        _doc.ChangedDoc += new EventHandler(doc_ChangedDoc);
        _doc.SavedDoc += new EventHandler(doc_SavedDoc);
        split.Panel1.Controls.Add(_doc);
      } catch (Exception ex) { add_err(ex); }
    }

    void doc_SavedDoc (object sender, EventArgs e) {
      lbl_changed.Text = "";
    }

    void doc_ChangedDoc (object sender, EventArgs e) {
      lbl_changed.Text = "salva documento modificato";
    }

    #endregion

  }
}

/*

    private void rtb_KeyPress (object sender, KeyPressEventArgs e) {
      if (e.KeyChar == '/') {
        e.Handled = true;
        rtb.AppendText("\\");
      }
    }

    private void rtb_KeyUp (object sender, KeyEventArgs e) {
      if (am.Visible || e.KeyCode == Keys.Escape) return;
      add_event(string.Format("rtb_KeyUp - keycode: {0} - keydata: {1} - keyvalue {2}"
        , e.KeyCode.ToString(), e.KeyData.ToString(), e.KeyValue));

      try {
        if (Char.IsLetterOrDigit((char)e.KeyValue)) {
          string last_3 = rtb.TextLength >= 3 ? rtb.Text.Substring(rtb.SelectionStart - 3, 3) : "";
          if (last_3.Length > 0) {
            if (last_3 == "pop") {
              add_event("pop");
              int st = rtb.SelectionStart;
              rtb.Select(rtb.SelectionStart - 3, 3);
              rtb.SelectionColor = Color.Blue;
              rtb.SelectionStart = st;
              rtb.SelectionLength = 0;
              rtb.SelectionColor = Color.White;
            } else if (last_3 == "pip") {
              add_event("pip");
              if (!am.Visible) {
                add_event("show autocomplete");
                am.SetAutocompleteItems(new List<AutocompleteItem>() { new ACItem("azz", "Osti:", "la mannaia\ndomar\nasdo\nmar")
                , new ACItem("zzi")
                , new ACItem("zzzzo")
                , new ACItem("sti")});
                am.Show(rtb, true);
              }
            }
          }
        }
      } catch (Exception ex) { add_event(ex.Message, Color.Red); }
    }

    protected void am_Selecting (object sender, SelectingEventArgs e) {
      add_event("am_Selecting");
      e.Cancel = true;
      
      rtb.SelectionStart += rtb.SelectionLength;
      rtb.SelectionLength = 0;
      rtb.SelectedText = e.Item.Text;

      //rtb.AppendText(e.Item.Text);
      am.Close();
    }

    protected void add_event (string line, Color? clr = null) {
      try {
        if (clr.HasValue) {
          rt_events.SelectionColor = clr.Value;
        } else rt_events.SelectionColor = Color.Yellow;
        rt_events.AppendText(line + Environment.NewLine);
        rt_events.ScrollToCaret();
      } catch { }
    }

    private void Form1_Load (object sender, EventArgs e) {
      am.Selecting += new EventHandler<SelectingEventArgs>(am_Selecting);
    }

    private void rtb_MouseClick (object sender, MouseEventArgs e) {
      try {
        int n = rtb.SelectionStart;
        add_event(string.Format("sel {0}", n), Color.LightBlue);
      } catch (Exception ex) { add_event(ex.Message, Color.Red); }
    }
  }

  internal class ACItem : AutocompleteItem {
    public ACItem (string text, string tt_title = "", string tt_text = "")
      : base(text) {
      //ImageIndex = 0;
      if (tt_title != "" && tt_text != "") {
        ToolTipTitle = tt_title;
        ToolTipText = tt_text;
      }
    }

    public override CompareResult Compare (string fragmentText) {
      return CompareResult.Visible;
    }

 */