﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;
using System.IO;
using System.Data;
using mlib;
using mlib.tools;
using mlib.db;
using mlib.xml;
using mlib.tiles;

public class tl_page : System.Web.UI.Page {

  protected core _core = null;
  protected db_provider _db = null;
  protected user _user = null;
  protected bool _db_connected = false;
  protected bool _is_mobile = false;

  protected string _base_path = null;
  public string base_path { get { if (_base_path == null) _base_path = System.Web.HttpContext.Current.Server.MapPath("~"); return _base_path; } }
  public string base_url {
    get {
      return Request.Url.Scheme + "://" + Request.Url.Authority +
        Request.ApplicationPath.TrimEnd('/') + "/";
    }
  }
  public string abs_path { get { return HttpContext.Current.Request.Url.AbsolutePath; } }
  public string page_name { get { return (new FileInfo(abs_path)).Name; } }

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // base
    _is_mobile = this.master.is_mobile();
  }

  protected override void OnPreInit(EventArgs e) {

    try {

      // core
      bool reload_cfg = false;
      if (Cache["core_obj"] == null) {
        log.log_info("reload core");
        core cr = new core(base_path, base_url);
        reload_cfg = true;
        Cache["core_obj"] = cr;
      } 

      // configs
      _core = (core)Cache["core_obj"];
      _core.base_url = this.base_url;
      foreach (string key in _core.config_keys) if (Cache[key] == null) { reload_cfg = true; break; }
      reload_cfg = true;
      if (reload_cfg) {
        log.log_info("reload config docs");
        _core.reset_configs();
        Directory.EnumerateFiles(_core.app_setting("settings-folder")).ToList().ForEach(f => {
          string doc_key = strings.rel_path(base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
          log.log_info("load config doc: " + doc_key + " - " + f);
          xml_doc doc = Path.GetExtension(f) != _core.app_setting("enc-ext-xml") ? new xml_doc(f)
            : new xml_doc(cry.xml_decrypt(f, _core.app_setting("pwdcr-xml")));
          _core.load_config(doc, doc_key, vars_key);
          if (Cache[doc_key] == null) Cache.Insert(doc_key, true, new System.Web.Caching.CacheDependency(f));
        });

        Cache["core_obj"] = _core;
      }
    } catch (Exception ex) { throw ex; }

    // carico il config page
    string ap = abs_path, base_dir = Path.GetDirectoryName(HttpContext.Current.Server.MapPath(ap))
      , pn = Path.GetFileNameWithoutExtension(ap), xml = Path.Combine(base_dir, pn + ".xml")
      , xml_enc = Path.Combine(base_dir, pn + "." + _core.app_setting("enc-ext-xml")), dck = strings.rel_path(base_path, xml);

    xml_doc dp = null;
    if (Cache[dck] != null) dp = (xml_doc)Cache[dck];
    else {
      dp = File.Exists(xml) ? new xml_doc(xml) : (File.Exists(xml_enc) ?
        new xml_doc(cry.xml_decrypt(xml_enc, _core.app_setting("pwdcr-xml"))) : null);
      if (dp != null) Cache.Insert(dck, dp, new System.Web.Caching.CacheDependency(xml));
    }

    if (dp != null) { _core.reset_page_config(); _core.load_page_config(dp, dck); }

    // conn to db
    db_reconn();
  }

  public bool db_reconn(bool throw_err = false) {
    try {
      db_provider db = db_conn;
      if (db.is_opened()) db.close_conn();
      _db_connected = db.open_conn();
    } catch (Exception ex) { log.log_err(ex); _db_connected = false; if (throw_err) throw ex; }
    return _db_connected;
  }

  public bool close_conn() {
    bool res = false;
    try {
      db_provider db = db_conn;
      if (db.is_opened()) { db.close_conn(); res = true; }
      _db_connected = false;
    } catch (Exception ex) { log.log_err(ex); _db_connected = false; }
    return res;
  }

  public config.conn cfg_conn { get { return _core.config.get_conn(_core.config.get_var("vars.db-connection").value); } }
  public db_provider conn_to(string conn_name) { return db_provider.create_provider(_core.config.get_conn(conn_name)); }
  public db_provider db_conn { get { return _db != null ? _db : _db = db_provider.create_provider(cfg_conn); } }
  public bool db_connected { get { return _db_connected; } }
  public core core { get { return _core; } }
  public config config { get { return _core.config; } }
  public bool is_user { get { return _user != null; } }
  public bool is_admin { get { return _user != null && _user.type == mlib.tiles.user.type_user.admin; } }
  public user user { get { return _user; } }
  public void set_user(int id, string user, string email, user.type_user tp) {
    _user = new user(id, user, email, tp);
  }
  public tl_master master { get { return (tl_master)Master; } }

  protected void write_response(json_result res) {
    Response.Clear();
    Response.ContentType = "application/json";
    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(res));
    Response.Flush();
    Response.SuppressContent = true;
    HttpContext.Current.ApplicationInstance.CompleteRequest();
  }

  #region data

  public string val_str(object val, string def = "") { return val != null && val != DBNull.Value ? val.ToString() : def; }
  public int val_int(object val) { return int.Parse(val_str(val, "0")); }
  public string qry_val(string name, string def = "") { return Request.QueryString[name] != null ? Request.QueryString[name] : def; }
  public int qry_int(string name, int def = 0) { return Request.QueryString[name] != null ? int.Parse(Request.QueryString[name]) : def; }

  #endregion

  #region controls

  public HtmlForm main_form(bool throw_err = true) {
    HtmlForm result = ctrl_by_attr<HtmlForm>("mainform", "true");
    if (result == null && throw_err)
      throw new Exception("devi aggiungere al documento il form principale con l'attributo \"mainForm='true'\"");

    return result;
  }

  public T ctrl_by_attr<T>(string attr, string attr_val) where T : HtmlControl { return ctrl_by_attr<T>(this, attr, attr_val); }

  protected T ctrl_by_attr<T>(Control ctl, string attr, string attr_val) where T : HtmlControl {
    foreach (Control c in ctl.Controls) {
      if (c.GetType() == typeof(T) && ((T)c).Attributes[attr] != null && ((T)c).Attributes[attr] == attr_val)
        return (T)c;

      Control cb = ctrl_by_attr<T>(c, attr, attr_val);
      if (cb != null) return (T)cb;
    }
    return null;
  }

  protected void add_class(HtmlControl ctrl, string class_name) {
    if (ctrl.Attributes["class"] == null) ctrl.Attributes.Add("class", class_name);
    else if (!ctrl.Attributes["class"].Contains(" " + class_name) && ctrl.Attributes["class"] != class_name)
      ctrl.Attributes["class"] += " " + class_name;
  }

  protected void remove_class(HtmlControl ctrl, string class_name) {
    if (ctrl.Attributes["class"] != null) {
      if (ctrl.Attributes["class"] == class_name) ctrl.Attributes["class"] = "";
      else if (ctrl.Attributes["class"].Contains(" " + class_name))
        ctrl.Attributes["class"] = ctrl.Attributes["class"].Replace(" " + class_name, "");
    }
  }

  #endregion

  #region functionalities

  protected void send_mail(string to, string obj, string body) {
    SmtpSection cfg = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
    using (MailMessage mm = new MailMessage() {
      From = new MailAddress(cfg.From), Subject = obj, IsBodyHtml = true, Body = body
    }) {
      mm.To.Add(to);
      using (SmtpClient smtp = new SmtpClient(cfg.Network.Host, cfg.Network.Port) {
        EnableSsl = cfg.Network.EnableSsl, UseDefaultCredentials = cfg.Network.DefaultCredentials,
        Credentials = new NetworkCredential(cry.decrypt(cfg.Network.UserName, "lillo32!"), cry.decrypt(cfg.Network.Password, "lillo32!"))
      }) smtp.Send(mm);
    }
  }

  public void log_out(string to_page) {
    FormsAuthentication.SignOut();
    Response.Redirect(to_page);
  }

  protected string get_url_cmd(string ref_url) { return _core.config.var_value_par("vars.router-cmd", System.Web.HttpUtility.UrlEncode(ref_url)); }

  #endregion

  #region queries

  protected bool set_cache_var(string var_name, string var_value) {
    return string.IsNullOrEmpty(var_value) ? reset_cache_var(var_name)
      : set_cache_var2(var_name, var_value);
  }

  private bool set_cache_var2(string var_name, string var_value) { 
    if(!is_user) return false;
    int id_utente = _user.id;
    db_conn.exec(core.parse(config.get_query("base.set-cache-var").text
      , new Dictionary<string, object>() { { "id_utente", id_utente }, { "var_name", var_name }, { "var_value", var_value } }));
    return true;
  }

  protected bool reset_cache_var(string var_name) { 
    if(!is_user) return false;
    int id_utente = _user.id;
    db_conn.exec(core.parse(config.get_query("base.reset-cache-var").text
      , new Dictionary<string, object>() { { "id_utente", id_utente }, { "var_name", var_name } }));
    return true;
  }

  protected string get_cache_var(string var_name, string def = "") {
    if (!is_user) return def;
    int id_utente = _user.id;
    DataRow dr = db_conn.first_row(core.parse(config.get_query("base.get-cache-var").text
      , new Dictionary<string, object>() { { "id_utente", id_utente }, { "list_vars", "'" + var_name + "'" } }));
    return dr != null ? db_provider.str_val(dr["var_value"], def) : def;
  }

  protected Dictionary<string, string> get_cache_vars(string var_names) {
    if (!is_user) return null;
    int id_utente = _user.id;
    Dictionary<string, string> res = new Dictionary<string, string>();
    foreach (DataRow dr in db_conn.dt_table(core.parse(config.get_query("base.get-cache-var").text
      , new Dictionary<string, object>() { { "id_utente", id_utente }
        , { "list_vars", string.Join(", ", var_names.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(s => "'" + s + "'")) } })).Rows) {
          res.Add(db_provider.str_val(dr["var_name"]), db_provider.str_val(dr["var_value"]));
    }
    return res;
  }

  #endregion
}