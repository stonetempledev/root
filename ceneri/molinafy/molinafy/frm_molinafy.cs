using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Net;
using mlib.tools;

namespace molinafy {
  public partial class frm_molinafy : frm_base {
    protected string _auth_state = "";
    protected sptfy_token _token = null;
    protected string _last_url = null;

    public frm_molinafy () {
      InitializeComponent();
    }

    private void lbl_title_MouseDown (object sender, MouseEventArgs e) { mouse_down(e); }
    private void lbl_title_MouseMove (object sender, MouseEventArgs e) { mouse_move(e); }
    private void lbl_title_MouseUp (object sender, MouseEventArgs e) { mouse_up(); }

    private void lbl_title_MouseDoubleClick (object sender, MouseEventArgs e) {
      if (e.Button == System.Windows.Forms.MouseButtons.Left) {
        if (this.WindowState == FormWindowState.Maximized) this.WindowState = FormWindowState.Normal;
        else if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
      } else if (e.Button == System.Windows.Forms.MouseButtons.Right) this.Close();
      else if (e.Button == System.Windows.Forms.MouseButtons.Middle) this.WindowState = FormWindowState.Minimized;
    }

    private void f_main_FormClosed (object sender, FormClosedEventArgs e) {
      try { } catch { }
    }

    private void f_main_Load (object sender, EventArgs e) {
      try {

        // ask token
        _auth_state = sptfy.gen_state();
        string scopes = "playlist-read-private user-read-private user-read-email";
        navigate(wb_main, "https://accounts.spotify.com/authorize?response_type=token&client_id=" + app.var("sptfy.client-id") +
          (scopes != "" ? "&scope=" + System.Web.HttpUtility.UrlEncode(scopes) : "") +
          "&redirect_uri=" + System.Web.HttpUtility.UrlEncode(app.var("sptfy.url-redirect")) + "&state=" + _auth_state);

      } catch (Exception ex) {
        MessageBox.Show(ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void wb_main_Navigated (object sender, WebBrowserNavigatedEventArgs e) {
      try {
        string url = wb_main.Url != null ? wb_main.Url.ToString() : null;
        if (_last_url != null && url != null && _last_url.ToLower() == url.ToLower()) return;
        _last_url = url;

        // get token
        if (url.ToLower().Contains(app.var("sptfy.url-redirect".ToLower()))) {
          Uri uri = new Uri(url.Replace("/#access_token=", "?access_token="));
          if (HttpUtility.ParseQueryString(uri.Query).Get("state") == _auth_state) {
            _token = new sptfy_token() {
              expires_in = long.Parse(HttpUtility.ParseQueryString(uri.Query).Get("expires_in")),
              access_token = HttpUtility.ParseQueryString(uri.Query).Get("access_token"),
              token_type = HttpUtility.ParseQueryString(uri.Query).Get("token_type")
            };

            // elenco playlist
            dynamic pls = sptfy.get_playlists(_token, app.var("sptfy.user-id"));
            List<string> plts = new List<string>();
            foreach (dynamic pl in pls.items)
              plts.Add((string)pl.name);
            load_html(wb_main, app._core.parse(System.IO.File.ReadAllText(
              app.var("pages.sptfy-home")), new Dictionary<string, object>() { { "playlists", string.Join(", ", plts) } }));
          }
        }

      } catch (Exception ex) {
        load_html(wb_main, app._core.parse(System.IO.File.ReadAllText(app.var("pages.err"))
          , new Dictionary<string, object>() { { "description", ex.Message } }));
      }
    }

    private void wb_main_Navigating (object sender, WebBrowserNavigatingEventArgs e) {
      string url = e.Url != null ? e.Url.ToString() : "";
      if (url.ToLower().Contains(app.var("sptfy.url-redirect".ToLower()))) e.Cancel = true;
    }

    private void wb_main_Validating (object sender, CancelEventArgs e) {
      int n = 0;
    }

  }
}

