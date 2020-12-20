using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading.Tasks;
using dlib;
using dlib.db;
using dlib.tools;
using dlib.xml;

namespace dn_client
{
  public partial class frm_main : Form
  {

    public static core _c = null;
    protected Point _dp = Point.Empty;
    protected settings _settings = null;

    public frm_main(core c)
    {
      _c = c; InitializeComponent();
    }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e)
    {
      if(e.Button != MouseButtons.Left) return; _dp = new Point(e.X, e.Y);
    }

    private void lbl_title_MouseMove(object sender, MouseEventArgs e)
    {
      if(_dp == Point.Empty) return;
      Point location = new Point(this.Left + e.X - _dp.X, this.Top + e.Y - _dp.Y);
      this.Location = location;
    }

    private void lbl_title_MouseUp(object sender, MouseEventArgs e)
    {
      if(e.Button != MouseButtons.Left) return;
      _dp = Point.Empty;
    }

    private void btn_close_Click(object sender, EventArgs e)
    {
      if(!_c.config.var_bool("client.tray-icon")) {
        this.Close(); return;
      }

      this.WindowState = FormWindowState.Minimized;
    }

    private void frm_main_Resize(object sender, EventArgs e)
    {
      if(this.WindowState == FormWindowState.Minimized) {
        Hide(); ntf_main.Visible = true;
      }
    }

    private void ntf_main_DoubleClick(object sender, EventArgs e)
    {
      Show();
      this.WindowState = FormWindowState.Normal;
      ntf_main.Visible = false;
    }

    private void frm_main_Load(object sender, EventArgs e)
    {
      try {
        tmr_state.Interval = Program._interval_ss * 1000;
        db_provider conn = Program.open_conn();
        _settings = settings.read_settings(_c, conn);

        // tray
        ntf_main.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        ntf_main.ContextMenuStrip.Items.Add("Apri...", null, this.MenuApri_Click);
        ntf_main.ContextMenuStrip.Items.Add("Esci", null, this.MenuExit_Click);
        ntf_main.ContextMenuStrip.BackColor = Color.Gray;
        ntf_main.ContextMenuStrip.ForeColor = Color.White;
        ntf_main.ContextMenuStrip.Font = new Font("segoe ui light", 9, FontStyle.Regular);

        wb_main.ObjectForScripting = new client_external(this);
        wb_main.Navigate(_settings.get_value("url"));
      } catch { }
    }

    void MenuApri_Click(object sender, EventArgs e)
    {
      ntf_main_DoubleClick(null, null);
    }

    void MenuExit_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void lbl_title_DoubleClick(object sender, EventArgs e)
    {
      if(this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
      else if(this.WindowState == FormWindowState.Maximized) this.WindowState = FormWindowState.Normal;
    }

    protected void msg_error(string txt)
    {
      MessageBox.Show(txt, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void mi_exit_Click(object sender, EventArgs e)
    {
      MenuExit_Click(sender, e);
    }

    private void tmr_state_Tick(object sender, EventArgs e) { Program.client_opened(); }

    public void open_att(int file_id)
    {
      Task.Factory.StartNew(() => {
        try {
          string folder = Program._c.config.get_var("client.client-tmp-path").value;
          if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
          fi i = fi.load_fi(file_id);
          string fp = Path.Combine(folder, file_id.ToString() + "_" + i.file_name + i.extension);
          using(WebClient webClient = new WebClient()) {
            webClient.DownloadFile(i.http_path, fp);
          }
          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer", "\"" + fp + "\"") {
            RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
          });
        } catch(Exception ex) { MessageBox.Show(ex.Message, "Errore!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
      });
    }

    protected bool _tmr_att = false;
    private void tmr_att_Tick(object sender, EventArgs e)
    {
      try {
        if(_tmr_att) return;
        _tmr_att = true;

        string folder = Program._c.config.get_var("client.client-tmp-path").value
          , iname = Program._c.config.get_var("client.index-att").value;
        if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        xml_att idoc = xml_att.open();

        // enum files & check to upload
        bool savei = false;
        foreach(FileInfo i in Directory.EnumerateFiles(folder).Select(x => new FileInfo(x))) {
          if(i.Name.ToLower() == iname) continue;
          try {
            string fn = i.Name.ToLower(); int id_file = int.Parse(fn.Split(new char[] { '_' })[0]);

            // add file
            if(!idoc.exists_file(id_file, out xml_node n)) {
              n = idoc.add_file(fi.load_fi(id_file), i.LastWriteTime);
              savei = true;
              continue;
            }

            // check upload
            DateTime lwt = DateTime.Parse(n.get_attr("lwt"));
            if(lwt.ToString("yyyy-MM-dd HH:mm:ss") != i.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")) {
              savei = true;
              upload_file(i.FullName, n.get_attr("http_path"), n);
              n.set_attr("lwt", i.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
              continue;
            }

            // check delete
            if((DateTime.Now - lwt).TotalMinutes > 5) {
              n.remove();
              i.Delete();
              savei = true;
              continue;
            }

          } catch(Exception ex) { log.log_err(ex); }
        }
        if(savei) idoc.save();
      } catch(Exception ex) { log.log_err(ex); }
      _tmr_att = false;
    }

    protected void upload_file(string path, string http_path, xml_node n)
    {
      try {
        WebClient client = new WebClient();
        client.UseDefaultCredentials = true;
        client.Credentials = CredentialCache.DefaultCredentials;
        client.UploadFile(http_path, path);
        client.Dispose();
      } catch(Exception ex) { n.set_attr("upload_err", $"error at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {ex.Message}"); throw ex; }
    }
  }


  [ComVisible(true)]
  [ClassInterface(ClassInterfaceType.None)]
  public class client_external
  {
    protected frm_main _form = null;
    public client_external(frm_main form) { _form = form; }
    public void open_att(int file_id) { _form.open_att(file_id); }
  }
}

