using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using mlib;

// https://gist.github.com/aksakalli/9191056
/*
  <add key="ad-usr" value="fIdUH9Pz71AW4S1BGQDIemBGqOg="/>
    <add key="ad-pwd" value="7l9Dd3C2RH4JlQ8CkD0IN7lqho4="/>

        if (cry.encode_tobase64(user_mail.Value) == core.app_setting("ad-usr")) {
          string uname = user_mail.Value, upass = user_pass.Value, uid = "0";
          if (cry.encode_tobase64(upass) != core.app_setting("ad-pwd")) {
            err_login("PASSWORD ERRATA!"); return;
          }
          FormsAuthentication.RedirectFromLoginPage(uname + "|" + uid, true);
        }
 * */
namespace molinafy {
  public class app {
    public static Thread _listener = null;
    public static HttpListener _http_listener = null;
    public static core _core = null;
    public static config cfg { get { return _core.config; } }
    public static string var (string path) { return _core.config.get_var(path).value; }
    public static string site_path { get; set; }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main () {
      try {
        log.log_info("starting molinafy...");

        // core
        _core = new core(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        Directory.EnumerateFiles(_core.app_setting("settings-folder")).ToList().ForEach(f => {
          string doc_key = strings.rel_path(_core.base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
          log.log_info("load config doc: " + doc_key + " - " + f);
          _core.load_config(new xml_doc(f), doc_key, vars_key);
        });

        app.site_path = app.var("vars.site-path");

        _listener = new Thread(listener);
        _listener.Start();

        log.log_info("application run...");
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        app pg = new app();
        Application.Run();

      } catch (Exception ex) {
        log.log_err(ex);
        MessageBox.Show(ex.Message, "Errore!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    protected static void listener () {
      bool started = false;
      try {
        if (!HttpListener.IsSupported)
          throw new Exception("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");

        _http_listener = new HttpListener();

        // prefixes
        foreach (string s in app.var("vars.prefixes").Split(new char[] { ',' })) {
          log.log_info("add prefix: " + s);
          _http_listener.Prefixes.Add(s);
        }

        // inits
        List<string> indexes = new List<string>(app.var("vars.indexes").Split(new char[] { ',' }));
        List<string> pages_ext = new List<string>(app.var("vars.pages-ext").Split(new char[] { ',' }));
        List<string> exclude_logged = new List<string>(app.var("vars.exclude-logged").ToLower().Split(new char[] { ',' }));

        // listening
        _http_listener.Start(); started = true;
        log.log_info("listening...");
        db_provider db = null;
        while (true) {
          HttpListenerContext context = _http_listener.GetContext();
          try {

            db = cfg.exists_var("vars.db-connection") ? new db_provider(cfg.get_conn(cfg.get_var("vars.db-connection").value)) : null;

            HttpListenerRequest request = context.Request;
            string cip = request.UserHostAddress;
            log.log_info(string.Format("Recived request from {0} for {1}", cip, request.Url.ToString()));

            string contents = null;
            using (Stream receiveStream = request.InputStream) {
              using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8)) {
                contents = readStream.ReadToEnd();
              }
            }

            // richieste
            HttpListenerResponse response = context.Response;
            if (!string.IsNullOrEmpty(contents)) {
              log.log_info("contents: " + contents);

              JToken args = JToken.Parse(contents);

              //string json = "{ \"responseDateTime\": \"hello hello there!\" }";
              //string json = JsonConvert.SerializeObject(new result(result.code.ok));
              string json = JsonConvert.SerializeObject(new result(result.code.error, "utente non valido"));
              byte[] buf = Encoding.UTF8.GetBytes(json);
              response.ContentLength64 = buf.Length;
              response.ContentType = "application/json";
              response.StatusCode = (int)HttpStatusCode.OK;
              Stream output = response.OutputStream;
              output.Write(buf, 0, buf.Length);
              output.Close();
              
              continue;
            }

            // local path
            string local_path = request.Url.LocalPath.Replace("/", "\\");
            string lp = local_path.Length > 0 && (local_path[0] == '/' || local_path[0] == '\\') ? local_path.Substring(1) : local_path;
            lp = Path.Combine(site_path, lp);

            byte[] buffer = null;

            // autentication
            DataRow dr_user = null;
            if (!exclude_logged.Contains(local_path.ToLower())) {
              DataTable dt = base_dal.dt_qry(db, "queries.check-user", new Dictionary<string, object>() { { "address_ip", cip } });
              dr_user = dt.Rows.Count > 0 ? dt.Rows[0] : null;
              if (dr_user == null) {
                log.log_warning("utente non loggato!");
                response.Redirect(app.var("vars.login-page"));
                response.StatusCode = 302;
                response.Close();
                continue;
              }
            }

            if (buffer == null) {
              // file
              if (File.Exists(lp)) {
                if (pages_ext.FirstOrDefault(e => Path.GetExtension(lp).ToLower() == e.ToLower()) != null) {
                  log.log_info("send page: " + lp);
                  buffer = Encoding.UTF8.GetBytes(elab_page(lp));
                } else {
                  log.log_info("send file: " + lp);
                  buffer = File.ReadAllBytes(lp);
                }
              }
                // folder index
              else if (Directory.Exists(lp)) {
                string fi = indexes.FirstOrDefault(fn => File.Exists(Path.Combine(lp, fn)));
                if (fi != null) {
                  string ip = Path.Combine(lp, fi);
                  log.log_info("send index: " + ip);
                  buffer = Encoding.UTF8.GetBytes(elab_page(ip));
                }
              }
            }

            if (buffer == null) {
              log.log_warning("non è stato trovato nulla!");
              buffer = Encoding.UTF8.GetBytes(err_page("non è stato trovato nulla!"));
            }

            // send
            if (buffer != null) {
              response.ContentLength64 = buffer.Length;
              Stream output = response.OutputStream;
              output.Write(buffer, 0, buffer.Length);
              output.Close();
            }
          } catch (Exception ex) { log.log_err(ex); } finally { if (db != null) db.close_conn(); }
        }
      } catch (Exception ex) { log.log_err(ex); } finally {
        if (started) _http_listener.Stop();
      }
    }

    protected static string err_page (string des) {
      return app._core.parse(System.IO.File.ReadAllText(Path.Combine(app.site_path, app.var("vars.err-page")))
        , new Dictionary<string, object>() { { "description", des } });
    }

    protected static string elab_page (string file_path) {
      return File.ReadAllText(file_path);
    }

    #region tray icon

    private NotifyIcon _ni;
    private ContextMenu _cm;
    private MenuItem _mi;
    private IContainer _components;

    app () {
      create_ni();
    }

    private void create_ni () {
      this._components = new System.ComponentModel.Container();
      this._cm = new System.Windows.Forms.ContextMenu();
      this._mi = new System.Windows.Forms.MenuItem();

      // Initialize menuItem1
      this._mi.Index = 0;
      this._mi.Text = "E&xit";
      this._mi.Click += new System.EventHandler(this.mi_Click);

      // Initialize contextMenu1
      this._cm.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { this._mi });

      // Create the NotifyIcon.
      this._ni = new System.Windows.Forms.NotifyIcon(this._components);
      _ni.Icon = molinafy.Properties.Resources.spotify_16;
      _ni.ContextMenu = this._cm;
      _ni.Text = "molinafy server";
      _ni.Visible = true;
      _ni.DoubleClick += new System.EventHandler(this.ni_DoubleClick);
      _ni.Click += new System.EventHandler(this.ni_Click);

    }
    private void ni_Click (object Sender, EventArgs e) {
      //MessageBox.Show("clicked");
    }

    private void ni_DoubleClick (object Sender, EventArgs e) {
      //MessageBox.Show("Double clicked");
    }

    private void mi_Click (object Sender, EventArgs e) {
      try {
        log.log_info("exit...");
        app._http_listener.Stop();
        Application.Exit();
      } catch (Exception ex) { log.log_err(ex); }
    }

    #endregion
  }
}
