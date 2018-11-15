using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using deeper.frmwrk;
using deeper.db;
using deeper.lib;

//Here is the once-per-application setup information
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace deeper.frmwrk
{
  // history
  public class history
  {
    List<Dictionary<string, string>> _refs = null;

    public int count { get { return _refs.Count; } }
    public string page(int index) { return _refs[index]["page"]; }
    public string name(int index) { return _refs[index]["name"]; }
    public string url(int index) { return _refs[index]["url"]; }
    public string lastPage { get { return count == 0 ? null : page(count - 1); } }
    public string lastName { get { return count == 0 ? null : name(count - 1); } }
    public string lastUrl { get { return count == 0 ? null : url(count - 1); } }
    public void clear() { _refs.Clear(); }

    public history() { _refs = new List<Dictionary<string, string>>(); }

    public void upd_urls(string url, string page_file = "", string page_name = "", string page_type = "") {
      bool upd = false;
      if (count == 0) upd = true;
      else {
        if (_refs[count - 1]["name"] != "" || page_name != "") {
          if ((_refs[count - 1]["name"].ToLower() != page_name.ToLower())
            || ((_refs[count - 1]["name"].ToLower() == page_name.ToLower()) 
             && (_refs[count - 1]["type"].ToLower() != page_type.ToLower()))) upd = true;
        } else if (_refs[count - 1]["url"].ToLower() != url.ToLower()) upd = true;
      }

      if (upd) _refs.Add(new Dictionary<string, string>() { { "page", page_file }, { "name", page_name }, { "url", url }, { "type", page_type } });
    }

    public string penultimoAndRemove() { removeLast(); return url(count - 1); }

    public void removeLast() { if (count >= 0) _refs.RemoveAt(_refs.Count - 1); }
  }

  // base_page
  public class lib_page : System.Web.UI.Page
  {
    // class
    private page_cls _class = null;
    public page_cls classPage { get { return _class; } }

    // framework
    protected string _approot = "";
    public string approot { get { return _approot; } }

    protected xmlDoc _config = null;
    protected parser _parser = null;
    public parser parser { get { return _parser; } }

    // user
    string[] _bckvars = { "redirectAfterLogin", "historyWeb", "consoleBrowserType" };
    protected int _userid = -1, _userChilds = 0;
    protected string _user = "", _userType = ""
      , _userTypeDes = "", _userConn = "", _userConnDes = "";
    public int userId { get { return _userid; } }
    public string userConn { get { return _userConn; } }
    public string userConnMenu { get { return _userConn != "" ? _userConn : base_conn(); } }
    public string userConnDes { get { return _userConnDes; } }
    public string userLogged { get { return _user; } }
    public string userTypeLogged { get { return _userType; } }
    public string userTypeDesLogged { get { return _userTypeDes; } }
    public int userChilds { get { return _userChilds; } }

    // log
    static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public string logTxt(string text) { _log.Info(text); return text; }
    public string logErr(string text) { _log.Error(text); return text; }
    public void logErr(Exception ex) { logErr("error", ex); }
    protected void logErr(string txt, Exception ex) { _log.Error(txt, ex); }
    public string logSql(string text) { _log.Debug(text); return text; }
    public string logWarning(string text) { _log.Warn(text); return text; }
    public string logDebug(string text) { _log.Debug(text); return text; }

    // db schemas
    protected xmlDocs _dbSchemas = null;

    // history 
    history _history = null;
    public history history { get { return _history; } }
    //public void upd_history(string url) { _history.updUrls(Request.Url.ToString()); }

    // funcs
    lib_fncs _funcs = null;
    public lib_fncs funcs { get { return _funcs; } }

    public lib_page()
      : base() { _dbSchemas = new xmlDocs(); _funcs = new lib_fncs(this); }

    protected virtual void init() {
      try {
        logDebug("inizializzazione parser");
        _parser = new frmwrk.parser(this);

        // init lantern lib                
        _approot = clean_root_path(web_setting("approot"));

        // config
        _config = addXmlDocToSession("webconfig", web_setting("config"), "webConfigDoc", web_setting_bool("reloadConfig"));
        if (!System.IO.File.Exists(_config.path))
          throw new Exception("il percorso del config '" + _config.path + "' non è configurato correttamente!");

        logDebug("caricamento pagina '" + Request.Url + "'");

        // inizializzo la variabile di sessione
        //bool tmp = isConsoleBrowser();
      } catch (Exception ex) { logErr(ex); throw ex; }
    }

    protected string clean_root_path(string path) {
      if (path == null || path == "") throw new Exception("root path non valorizzato.");
      path = System.IO.Path.GetFullPath(path);
      if (!strings.isAbsoluteUrl(path)) throw new Exception("root path '" + path + "' impostato errato.");
      if (path.Substring(path.Length - 1, 1) != "\\") path += "\\";

      return path;
    }

    #region config

    public string cfg_var(string name, string attr = "") {
      XmlNode varnode = cfgvar_node(name);
      string result = xmlDoc.node_val(varnode, attr);
      return xmlDoc.node_bool(varnode, "toparse") ? parser.get_string(result) : result;
    }

    public bool cfg_var_bool(string name, string attr = "", bool def = false) {
      XmlNode varnode = cfgvar_node(name);
      return xmlDoc.node_bool(varnode, "toparse") ? bool.Parse(_parser.get_string(xmlDoc.node_val(varnode, attr, def.ToString())))
       : xmlDoc.node_bool(varnode, attr, def);
    }

    public int cfg_var_int(string name, string attr = "", int def = 0) {
      XmlNode varnode = cfgvar_node(name);
      return xmlDoc.node_bool(varnode, "toparse") ? int.Parse(_parser.get_string(xmlDoc.node_val(varnode, attr, def.ToString())))
       : xmlDoc.node_int(varnode, attr, def);
    }

    public int cfg_value_int(string xpath, string attr = "", int def = 0) { return xmlDoc.node_int(cfg_node(attr != "" ? xpath + "[@" + attr + "]" : xpath), attr, def); }

    public bool cfg_value_bool(string xpath, string attr = "", bool def = false) { return xmlDoc.node_bool(cfg_node(attr != "" ? xpath + "[@" + attr + "]" : xpath), attr, def); }

    public int cfg_count(string xpath) { return cfg_nodes(xpath).Count; }

    public bool cfg_exist(string xpath) { return cfg_node(xpath) != null; }

    public string cfg_value(string xpath, string attr = "", string def = "", string throwErr = "") { return xmlDoc.node_val(cfg_node(attr != "" ? xpath + "[@" + attr + "]" : xpath), attr, def, throwErr); }

    public XmlNode cfgvar_node(string name) { return cfg_node("/root/vars/var[@name='" + name + "']"); }

    public List<XmlNode> cfgvar_nodes(string name) { return cfg_nodes("/root/vars/var[@name='" + name + "']"); }

    public XmlNode cfg_node(string xpath) { return _config.node(xpath); }

    public List<XmlNode> cfg_nodes(string xpath) { return _config.toList(xpath); }

    private System.Xml.XmlNode currentPageNode() {
      if (query_param("pgnm") != "")
        foreach (XmlNode pageNode in cfg_nodes("/root/pages/page"))
          if (pageNode.Attributes["name"].Value.ToLower() == query_param("pgnm").ToLower())
            return pageNode;

      if (getCurrentUrl().ToLower() == parse(cfg_value("/root/pages", "href")).ToLower())
        return cfg_node("/root/pages/page[@name='" + cfg_var("homePageName") + "']");

      foreach (System.Xml.XmlNode page in cfg_nodes("/root/pages/page"))
        if (getCurrentUrl().ToLower() == getPageRef(page.Attributes["name"].Value).ToLower())
          return page;

      return null;
    }

    public XmlNode nodePage(string name, bool throwerr = true) {
      System.Xml.XmlNode pn = cfg_node("/root/pages/page[@name='" + name + "']");
      if (pn == null && throwerr)
        throw new Exception("la pagina '" + name + "' non è stata configurata!");

      return pn;
    }

    public string getPageRef(string page_name, string args = null) {
      System.Xml.XmlNode pageNode = nodePage(page_name);
      return parse(strings.combineurl(xmlDoc.node_val(pageNode, "href", cfg_value("/root/pages", "href"))
        , new List<string> { "pgnm=" + page_name, xmlDoc.node_val(pageNode, "params"), args != null ? args : "" }));
    }

    private xmlDoc addXmlDocToSession(string nameDoc, string fullPath, string sessionVar, bool force) {
      if (Session[sessionVar] == null || force)
        Session[sessionVar] = new xmlDoc(fullPath);
      return (xmlDoc)Session[sessionVar];
    }

    protected string web_setting(string name) {
      return parse(System.Configuration.ConfigurationManager.AppSettings[name].ToString());
    }

    protected bool web_setting_bool(string name) {
      return bool.Parse(parse(System.Configuration.ConfigurationManager.AppSettings[name].ToString()));
    }

    protected string web_conn(string name) {
      return System.Configuration.ConfigurationManager.ConnectionStrings[name].ConnectionString;
    }

    protected string web_conn_provider(string name) {
      return System.Configuration.ConfigurationManager.ConnectionStrings[name].ProviderName;
    }

    public string proc_path(string code) {
      return System.IO.Path.Combine(parse(xmlDoc.node_val(conn_group_node(name_conn_user(), "procs"), "base_path"))
        , xmlDoc.node_val(conn_group_node(name_conn_user(), "procs/proc[@code='" + code + "']"), "file"));
    }

    public string proc_type(string code) { return proc_attr(code, "type"); }
    public string proc_des(string code) { return proc_attr(code, "des"); }
    public string proc_attr(string code, string attr) { return xmlDoc.node_val(conn_group_node(name_conn_user(), "procs/proc[@code='" + code + "']"), attr); }

    #endregion

    #region db

    // connessioni
    protected Dictionary<string, db_schema> _dbconns = new Dictionary<string, db_schema>();
    public string base_conn() { return cfg_value("/root/dbconns", "default"); }
    public string conn_des(string nameConn) { return cfg_value("/root/dbconns/dbconn[@name='" + nameConn + "']", "des"); }
    
    public db_schema conn_db_base(bool open = true, bool reconnect = false) { return conn_db(base_conn(), open, reconnect); }

    public db_schema conn_db_user() { return conn_db_user(true, false, "non è stato possibile ottenere una connessione valida!"); }

    public db_schema conn_db_user(bool open = true, bool reconnect = false, string throw_msg = "") {
      return conn_db(name_conn_user(), open, reconnect, throw_msg);
    }

    public string name_conn_user() { return _userConn != "" ? _userConn : base_conn(); }

    public db_schema conn_db(string name_conn, bool open = true, bool reconnect = false, string throw_msg = "") {

      if (string.IsNullOrEmpty(name_conn)) { if (throw_msg != "") throw new Exception(throw_msg); else throw new Exception("non hai specificato la connessione da aprire!"); }

      // vedo se la connessione è già stata aperta
      if (_dbconns.ContainsKey(name_conn)) {
        if (!reconnect) return _dbconns[name_conn];
        else {
          _dbconns[name_conn].close_conn();
          _dbconns.Remove(name_conn);
        }
      }

      XmlNode dbconn = conn_node(name_conn);
      db_schema newconn = db_schema.create_provider(name_conn, web_conn(name_conn), web_conn_provider(name_conn), xmlDoc.node_val(conn_group(name_conn), "curver")
        , cfg_value_int("/root/dbconns", "timeout", -1), xmlDoc.node_val(dbconn, "group"), xmlDoc.node_val(dbconn, "language"), xmlDoc.node_val(dbconn.SelectSingleNode("formats"), "datetoquery")
        , xmlDoc.node_val(dbconn.SelectSingleNode("formats"), "datetoquery2"), xmlDoc.node_val(dbconn, "des")
        , schema_path_fromconn(name_conn, false), schema_path_fromconn(name_conn, false, false, true), group_scripts(xmlDoc.node_val(dbconn, "group")));

      _dbconns.Add(name_conn, newconn);

      if (open) newconn.open_conn();

      return newconn;
    }

    protected XmlNode conn_node(string nameConn, bool throw_err = true) {
      XmlNode dbconn = cfg_node("/root/dbconns/dbconn[@name='" + nameConn + "']");
      if(dbconn == null && throw_err)
        throw new Exception("la connessione '" + nameConn + "' non è stata configurata correttamente!");

      return dbconn;
    }

    public db_xml conn_schema(string path_schema, string des = "") {
      xmlDoc schema_doc = new xmlDoc(path_schema);

      return new db_xml(schema_doc, schema_path_fromgroup(schema_doc.root_value("group"), false, false, true), des);
    }

    public void close_dbs() { _dbconns.Values.ToList().ForEach(db => { if (db.is_opened()) db.close_conn(); }); _dbconns.Clear(); }

    public void close_db(string name_conn) {
      if (_dbconns.ContainsKey(name_conn)) {
        if (_dbconns[name_conn].is_opened()) _dbconns[name_conn].close_conn();
        _dbconns.Remove(name_conn);
      }
    }

    public XmlNode conn_group_node(string name_conn, string xpath) { return node_group(conn_node(name_conn).Attributes["group"].Value).SelectSingleNode(xpath); }

    public XmlNode conn_group(string name_conn, bool throw_err = true) {
        XmlNode cn = conn_node(name_conn, throw_err);
        return cn != null ? node_group(cn.Attributes["group"].Value) : null; 
    }

    public string conn_curver(string name_conn) { return xmlDoc.node_val(conn_group(name_conn), "curver"); }

    public List<db_script> group_scripts(string group_name) {
      return new List<db_script>(node_group(group_name).SelectNodes("scripts/script").
          Cast<XmlNode>().Select(x => new db_script(x.Attributes["name"].Value, System.IO.Path.Combine(System.IO.Path.Combine(_approot, cfg_var("scripts_folder")), x.Attributes["file"].Value))));
    }

    public XmlNode node_group(string name_group) {
      return cfg_node("/root/dbgroups/dbgroup[@name='" + name_group + "']");
    }

    public string schema_path_fromconn(string name_conn, bool throwErr = true, bool url = false, bool meta = false, string ver = "") {
      return schema_path(conn_group(name_conn), throwErr, url, meta, ver);
    }

    public string schema_path_fromgroup(string group_name, bool throwErr = true, bool url = false, bool meta = false) {
      return schema_path(node_group(group_name), throwErr, url, meta);
    }

    protected string schema_path(XmlNode group_node, bool throwErr = true, bool url = false, bool meta = false, string ver = "") {
      string attr = meta ? "meta" : "schema";
      if (group_node.Attributes[attr] == null || group_node.Attributes[attr].Value == "") {
        if (throwErr) throw new Exception("il gruppo database non ha specificato l'attributo '" + attr + "'!");
        return "";
      }

      string result = url ? getRootUrl() + cfg_var("schema_folder") + "/" + xmlDoc.node_val(group_node, attr)
          : System.IO.Path.Combine(System.IO.Path.Combine(approot, cfg_var("schema_folder")), xmlDoc.node_val(group_node, attr));

      return ver != "" ? db_schema.parse_dbexpr(result, ver) : result;
    }


    #endregion

    #region parser

    public string parse(string text) { return _parser.get_string(text, ""); }

    public string parse(string text, Dictionary<string, string> fields, db_schema db = null) { return _parser.get_string(text, "", fields, null, null, null, db); }

    public string parse(string text, XmlNode row) { return _parser.get_string(text, "", null, row, null); }

    public string parse(string text, string cname) { return _parser.get_string(text, cname); }

    public string parse(string text, string cname, Dictionary<string, string> fields) { return _parser.get_string(text, cname, fields); }

    public string parse(string text, string cname, Dictionary<string, string> fields, DataRow dr) { return _parser.get_string(text, cname, fields, null, dr); }

    public string parse(string text, string cname, XmlNode row) { return _parser.get_string(text, cname, null, row, null); }

    public string parse(string text, string cname, System.Data.DataRow row) { return _parser.get_string(text, cname, null, null, row); }

    public string parse(string text, System.Data.DataRow row) { return _parser.get_string(text, "", null, null, row); }

    public string parse(string text, string cname, System.Web.UI.WebControls.GridViewRow rowgrid) { return _parser.get_string(text, cname, null, null, null, rowgrid); }

    #endregion

    #region user

    public bool loggedUser() {
      return loggedUser(out _userid, out _user, out _userType, out _userConn, out _userConnDes, out _userTypeDes, out _userChilds);
    }

    protected bool loggedUser(out int userid, out string user, out string userType, out string userConn, out string userConnDes, out string userTypeDes, out int childs) {
      userid = -1; childs = 0;
      userTypeDes = userType = userConn = userConnDes = user = "";

      try {

        if (!cfg_value_bool("/root/login", "active"))
          return true;

        // controllo utente loggato
        string cookies = cfg_value("/root/login", "cookies");
        if (Request.Cookies[cookies] == null || Request.Cookies[cookies]["uid"] == null)
          return false;

        // cerco l'utente nel db interno
        string userName = Request.Cookies[cookies]["uid"]
          , userPwd = Request.Cookies[cookies]["pwd"] != null ? Request.Cookies[cookies]["pwd"] : "";
        if (!canLogin(userName, userPwd, out userid, out userType, out userConn, out userTypeDes, out childs))
          return false;

        user = userName;
        userConnDes = conn_des(userConn != "" ? userConn : base_conn());

        return true;
      } catch { return false; }
    }

    public bool isRcvUser(string user, string pwd) { return user == "rcvry" && pwd == "rcvry32!"; }

    public void setLoggedUser(string user, string password) {
      if (!cfg_value_bool("/root/login", "active"))
        return;

      string cookies = cfg_value("/root/login", "cookies");
      Response.Cookies[cookies]["uid"] = user;
      Response.Cookies[cookies]["pwd"] = password;

      Response.Cookies[cookies].Expires = DateTime.Now.AddDays(1);
    }

    public void resetLoggedUser() {
      if (!cfg_value_bool("/root/login", "active"))
        return;

      string cookies = cfg_value("/root/login", "cookies");
      if (Request.Cookies[cookies] != null) {
        // backup variabili da mantenere...
        Dictionary<string, object> bck = new Dictionary<string, object>();
        foreach (string var in _bckvars)
          if (Session[var] != null)
            bck.Add(var, Session[var]);

        Session.Clear();
        System.Web.HttpCookie myCookie = new System.Web.HttpCookie(cookies);
        myCookie.Expires = DateTime.Now.AddDays(-1d);
        Response.Cookies.Add(myCookie);

        // ripristino variabili
        foreach (string var in _bckvars)
          if (bck.ContainsKey(var))
            Session[var] = bck[var];
      }
    }

    public bool canLogin(string user, string pwd, out int id) {
      string tmp = ""; int n = 0;

      return canLogin(user, pwd, out id, out tmp, out tmp, out tmp, out n);
    }

    public bool canLogin(string user, string pwd, out int id, out string type, out string conn, out string type_des, out int n_users) {
      id = -1; n_users = 0;
      type_des = type = conn = "";

      db_provider db = cfg_exist("/root/login[@conn]") ? conn_db(cfg_value("/root/login", "conn")) : conn_db_base();

      if (!isRcvUser(user, pwd)) {

        Dictionary<string, string> fields = new Dictionary<string, string>();
        string[] inFields = cfg_value("/root/login/infields").Split(',');
        fields.Add(inFields[0], user);
        fields.Add(inFields[1], pwd);

        System.Data.DataTable dt = classPage.dt_from_id(cfg_value("/root/login", "selid"), "", "", fields);
        if (dt == null || dt.Rows.Count <= 0)
          return false;

        string[] outFields = cfg_value("/root/login/outfields").Split(',');
        id = int.Parse(dt.Rows[0][outFields[0]].ToString());
        type = dt.Rows[0][outFields[1]].ToString();
        type_des = dt.Rows[0][outFields[2]].ToString();
        conn = dt.Rows[0][outFields[3]].ToString();
        string users = "";
        try { users = classPage.user_ids(id, true); } catch { }
        n_users = users.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
      } else { id = -1; type = "admin"; type_des = "recovery user"; conn = ""; }

      return true;
    }

    #endregion

    #region events

    protected override void Render(HtmlTextWriter writer) {
      //StringBuilder sb = new StringBuilder();
      //StringWriter sw = new StringWriter(sb);
      //HtmlTextWriter hWriter = new HtmlTextWriter(sw);
      //base.Render(hWriter);
      //string html = sb.ToString();
      //html = System.Text.RegularExpressions.Regex.Replace(html, "<input[^>]*id=\"(__VIEWSTATE)\"[^>]*>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      //writer.Write(html);

      base.Render(writer);
    }

    protected override void OnPreInit(EventArgs e) {
      init();

      // page node
      XmlNode page_node = currentPageNode();

      // storico pagine navigazione
      _history = _history == null ? Session["historyWeb"] == null ? new history() : (history)Session["historyWeb"] : _history;
      Session["historyWeb"] = _history;
      if (query_int("clrhis") == 1) _history.clear();

      if (!IsPostBack && query_param("toprev") == "" && query_param("child") != "1"
          && (!xmlDoc.node_bool(page_node, "nohistory", false) && query_param("nohstr") != "1"))
        _history.upd_urls(Request.Url.ToString(), Request.FilePath.ToString(), query_param("pgnm"), query_param("type"));

      // class
      string className = parse(xmlDoc.node_val(page_node, "class"));
      if (className != "") {
        System.Reflection.Assembly webass = System.Reflection.Assembly.GetExecutingAssembly();
        if (webass == null)
          throw new Exception("non è stato possibile ottenere l'assembly del sito web!");

        Type type = webass.GetType(className);
        if (type == null)
          throw new Exception("non è stato possibile creare il tipo classe '" + className + "'");

        object classPage = Activator.CreateInstance(type, new object[] { this, page_node });
        if (classPage == null)
          throw new Exception("non è stato possibile creare la classe '" + className + "'");

        if (!(classPage is page_cls))
          throw new Exception("la classe '" + className + "' non è derivata da pageControl");

        _class = (page_cls)classPage;
      } else _class = new page_cls(this, page_node);
      // prev
      if (query_param("toprev") == "1") {
        redirect(urlPrev());
        return;
      }
    }

    protected override void OnInit(EventArgs e) {
      base.OnInit(e);

      // httprequest
      if (Request.Headers["panel-client-request"] != null) {
        _class.onInit(this, e, true);

        xmlDoc outxml = xmlDoc.doc_from_xml("<response result='ok'/>");
        try {
          // se l'utente non è loggato apro la pagina di login...            
          if (_class.auth && (!loggedUser() || !eval_cond_id(_class.if_cond)))
            throw new Exception("utente loggato non valido!");
          else {
            xmlDoc doc = new xmlDoc(Request.InputStream);
            if (!eval_request(doc.root_value("action"), doc, outxml))
              throw new Exception("l'azione '" + doc.root_value("action") + "' non è supportata!");
          }
        } catch (Exception ex) {
          outxml.set_root_attr("result", "error");
          outxml.add_node("err").InnerText = ex.Message;
          logErr(ex);
        }

        outxml.doc.Save(Response.OutputStream);
        Response.End();
        return;
      }

      try {

        // se l'utente non è loggato apro la pagina di login...
        if (!classPage.isDefPage || (_class.auth && !loggedUser())) {
          Session["redirectAfterLogin"] = Request.Url;
          redirect(getPageRef("login"));
          return;
        }

        // se l'utente non ha i permessi lo rimando alla home page
        if (_class.auth && !eval_cond_id(_class.if_cond)) {
          Session["redirectAfterLogin"] = null;
          redirect(getPageRef(cfg_var("homePageName")));
          return;
        }

        // esecuzione query di init
        if (_class.pageDoc != null && _class.pageDoc.root_value("init_qry") != "")
          _class.exec_updates(_class.pageDoc.root_value("init_qry"), _class.pageName);

        // aggiunta variabili client
        foreach (XmlNode node in cfg_nodes("/root/vars/var[@toclient='true']"))
          _class.xml_topage("var" + node.Attributes["name"].Value, cfg_var(node.Attributes["name"].Value));

        _class.onInit(this, e);
      } catch (Exception ex) {
        _class.onInit(this, e, false, false);

        classPage.addError(ex);
        classPage.addHtmlSection("<p><a href=\"" + getPageRef("recovery") + "\">recovery page</a></p>");
      }
    }

    public void redirect(string url) {
      Response.Redirect(url, false);
      //Response.End();
      System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
    }

    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);

      // titolo pagina
      Title = cfg_var("sitetitle");

      _class.onLoad(this, e);
    }

    protected override void OnUnload(EventArgs e) {
      try {
        //if (_log != null) _log.close();

        foreach (db_provider conn in _dbconns.Values)
          conn.close_conn();

        _dbconns.Clear();
      } catch { }

      base.OnUnload(e);
    }

    public void ctrl_Click(object sender, EventArgs e) {
      _class.ctrl_Click(sender, e);
    }

    public void ctrl_DataBound(object sender, EventArgs e) {
      foreach (page_ctrl ctrl in _class.ctrls)
        if (ctrl.ctrlDataBound(sender, e))
          break;
    }

    public void ctrl_RowCommand(object sender, CommandEventArgs e) {
      foreach (page_ctrl ctrl in _class.ctrls)
        if (ctrl.ctrlRowCommand(sender, e))
          break;
    }

    #endregion

    #region base functions

    public string pageName { get { return classPage.pageName; } }

    public long key(string key) { return _class.existKey(key) ? _class.key(key) : long.Parse(query_param(key)); }

    public string keystr(string key) { return _class.existKey(key) ? _class.key(key).ToString() : query_param(key); }

    public string tmpFolder(bool create = true) {
      string result = cfg_var("tmpFolder");
      if (!System.IO.Directory.Exists(result))
        System.IO.Directory.CreateDirectory(result);

      return result;
    }

    public virtual bool action(string actionName, string cname, string keys = "", string noConfirm = "", string refurl = "") { return _class.action(actionName, cname, keys, noConfirm, refurl); }

    public string query_param_throw(string parName) {
      if (Request.QueryString[parName] == null || Request.QueryString[parName] == "")
        throw new Exception("il parametro '" + parName + "' della pagina non c'è");
      return Request.QueryString[parName];
    }

    public string query_param(string parName, string defvalue = "") {      
      return Request.QueryString[parName] != null && Request.QueryString[parName] != "" ? Request.QueryString[parName] : defvalue; 
    }

    public long query_long(string parName) { return long.Parse(query_param(parName, "0")); }

    public int query_int(string parName) { return int.Parse(query_param(parName, "0")); }

    public string getRootUrl() { return System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/"); }

    public string getCurrentUrl(bool withargs = false) {
      string url = System.Web.HttpContext.Current.Request.Url.ToString();
      if (!withargs && url.IndexOf('?') > 0)
        url = url.Substring(0, url.IndexOf('?'));
      return url;
    }

    public string urlPrev() { return _history.count > 1 ? _history.penultimoAndRemove() : getPageRef(cfg_var("homePageName")); }

    #endregion

    #region eval for pages

    public bool eval_cond_id(string expr) { return eval_cond_id("", expr); }

    public string code_cond_id(string cname, string id) { return code_type_id(cname, id, "condition"); }

    public string code_nodes_id(string cname, string id) { return code_type_id(cname, id, "xmlnodes"); }

    public string code_table_id(string cname, string id) { return code_type_id(cname, id, "table"); }

    protected string code_type_id(string cname, string id, string nodeName) {
      if (_class.pageDoc != null && _class.pageDoc.root_node.SelectSingleNode("scripts/" + nodeName + "[@name='" + id + "']") != null)
        return _class.pageDoc.root_node.SelectSingleNode("scripts/" + nodeName + "[@name='" + id + "']").InnerText;
      else if (cname != null && cname != "" && _class.existControl(cname)
          && _class.control(cname).rootExist("scripts/" + nodeName + "[@name='" + id + "']"))
        return _class.control(cname).rootValue("scripts/" + nodeName + "[@name='" + id + "']");
      else if (cfg_exist("/root/scripts/" + nodeName + "[@name='" + id + "']"))
        return cfg_value("/root/scripts/" + nodeName + "[@name='" + id + "']");

      throw new Exception("il codice da eseguire '" + id + "' non è stato configurato!");
    }

    public bool eval_cond_id(string cname, string expr) { return eval_cond_id(cname, expr, null); }

    public bool eval_cond_id(string cname, string expr, DataRow dr) { if (string.IsNullOrEmpty(expr)) return true; return parser.eval_bool(parse(expr, cname, dr)); }

    protected virtual bool eval_request(string action, xmlDoc doc, xmlDoc outxml) {
      if (action == "ctrl_request") {
        if (_class.evalControlRequest(doc, outxml))
          return true;
      } else if (action == "save_unload_keys") {
        _class.saveUnloadKeys(doc);
        return true;
      }

      return false;
    }

    #endregion

    #region xml

    protected bool validateXml(string pathxml, out string err) {
      err = "";

      bool result = true;
      try {
        validateXml(pathxml);
      } catch (Exception ex) {
        err = ex.Message;

        result = false;
        logErr(ex);
      }

      return result;
    }

    protected void validateXml(string pathxml) {
      xmlValidator validator = new xmlValidator();

      System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
      doc.Load(pathxml);

      string pathxsd = classPage.path_xsd(doc);
      if (!validator.valid(doc, "", pathxsd))
        throw new Exception(validator.validationError);
    }

    #endregion

    #region controlli

    public DropDownList dd(string key, List<Dictionary<string, string>> list) {
      DropDownList dd = new DropDownList();
      foreach (Dictionary<string, string> row in list)
        dd.Items.Add(new ListItem(row[key]));
      return dd;
    }

    public DropDownList dd_exclude(string exclude_key, List<Dictionary<string, string>> list) {
      DropDownList dd = new DropDownList();
      foreach (Dictionary<string, string> row in list)
        dd.Items.Add(new ListItem(string.Join(", ", row.Where(x => x.Key != exclude_key).Select(x => x.Key + ": " + x.Value).ToArray())));
      return dd;
    }

    public Literal literal(string html) {
      Literal ctrl = new Literal();
      ctrl.Text = html;
      return ctrl;
    }

    public void ctrlsToParent(Control parent, Control ctrl, Control ctrl2 = null, Control ctrl3 = null) {
      parent.Controls.Add(ctrl);
      if (ctrl2 != null) parent.Controls.Add(ctrl2);
      if (ctrl3 != null) parent.Controls.Add(ctrl3);
    }

    public Control ctrlsToParent(Control parent, Control ctrl) {
      parent.Controls.Add(ctrl);

      return ctrl;
    }

    public void add_cfg_style(HtmlControl ctrl, string style) {
      cfg_nodes("/root/styles/style[@name='" + style + "']/key").ForEach(key => {
        ctrl.Style.Add(key.Attributes["name"].Value, key.InnerText);
      });
    }

    public void remove_cfg_style(HtmlControl ctrl, string style) {
      cfg_nodes("/root/styles/style[@name='" + style + "']/key").ForEach(key => {
        ctrl.Style.Remove(key.Attributes["name"].Value);
      });
    }

    #endregion

    #region ambient

    /// <summary>
    /// Copia files di backup nell'ambiente
    /// </summary>
    public void files_from_backup(string index_path) {
      // copio i files in locale
      string bck = Path.GetDirectoryName(index_path), files_doc = Path.Combine(bck, deeper.db.db_schema._filesXml)
       , local = cfg_var("filesFolder"), err_cpy = "";
      if (!Directory.Exists(local)) Directory.CreateDirectory(local);
      if (Directory.Exists(Path.Combine(bck, db.db_schema._filesFolder)) && File.Exists(files_doc)) {
        xmlDoc doc_files = new xmlDoc(files_doc);
        doc_files.nodes("/files/file").Cast<XmlNode>().ToList().ForEach(file => {
          try {
            string from = Path.Combine(bck, db.db_schema._filesFolder, xmlDoc.node_val(file, "folder"), xmlDoc.node_val(file, "file"))
                , to = Path.Combine(local, xmlDoc.node_val(file, "folder"), xmlDoc.node_val(file, "file"))
                , folder_to = Path.GetDirectoryName(to);
            logDebug(string.Format("copy file from '{0}' to '{1}'", from, to));
            if (!Directory.Exists(folder_to)) {
              logDebug(string.Format("creazione folder '{0}'", folder_to));
              Directory.CreateDirectory(folder_to);
            }
            File.Copy(from, to, true);
          } catch (Exception ex) { logErr(ex); err_cpy = err_cpy == "" ? ex.Message : err_cpy; }
        });
      }
      if (err_cpy != "") throw new Exception("errore durante la copia dei files: " + err_cpy);
    }

    #endregion
  }
}
