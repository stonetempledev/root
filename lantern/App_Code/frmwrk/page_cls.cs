using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using deeper.db;
using deeper.frmwrk.ctrls;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.frmwrk {
  // serverClass
  //
  // Classe che contiene le basi per il funzionamento dei controlli definiti nella pagina web
  //
  // virtual functions
  //
  //  public virtual string info(string name)
  //  public virtual void onInit(object sender, EventArgs e, bool request = false)
  //  public virtual void onLoad(object sender, EventArgs e)
  //  public virtual bool action(string actionName, string formName, string keys, string noConfirm, string refurl)
  public class page_cls {
    protected lib_page _page = null;
    public lib_page page { get { return _page; } }

    protected bool _auth = true;
    public bool auth { get { return _auth; } }

    // page node
    private XmlNode _pageNode = null;
    public bool isDefPage { get { return _pageNode != null; } }
    public string pageName { get { return xmlDoc.node_val(_pageNode, "name"); } }
    public string if_cond { get { return xmlDoc.node_val(_pageNode, "if"); } }

    // page docs
    private Dictionary<string, xmlDoc> _pageDocs = null;
    public xmlDoc pageDoc { get { return _pageDocs.ContainsKey(_page.pageName) ? _pageDocs[_page.pageName] : null; } }
    //public bool existDoc { get { return _pageDocs.ContainsKey(_page.pageName); } }
    public xmlDoc setPageDoc(xmlDoc doc) {
      if (_pageDocs.ContainsKey(pageName)) _pageDocs[pageName] = doc;
      else _pageDocs.Add(pageName, doc); return doc;
    }
    public xmlDoc setPageDoc(string page_name, xmlDoc doc) {
      if (_pageDocs.ContainsKey(page_name)) _pageDocs[page_name] = doc;
      else _pageDocs.Add(page_name, doc); return doc;
    }

    // controlli
    private Dictionary<string, page_ctrl> _ctrls = null;
    private Dictionary<string, xmlDoc> _docs = null;
    public IEnumerable<page_ctrl> ctrls { get { return _ctrls.Values; } }
    public IEnumerable<page_ctrl> ctrlsToRender {
      get {
        return new List<page_ctrl>(from ctrl in _ctrls.Values
                                   where ctrl.render == true
                                   select ctrl);
      }
    }
    protected TextBox _page_flags = null;
    public string page_flags {
      get { return _page_flags != null ? _page_flags.Text : ""; }
      set { if (_page_flags != null) _page_flags.Text = value; }
    }
    public bool check_page_flag(string text) { return page_flags.IndexOf("[" + text + "]") >= 0; }
    public void set_page_flag(string text) { if (_page_flags != null && !check_page_flag(text)) page_flags += "[" + text + "]"; }
    public enum submitResponse { ok, ok_alert, notvalued }

    // keys & reload data
    private Dictionary<string, long> _keys = null;
    public bool existKey(string key) { return _keys.ContainsKey(key); }
    public long key(string key) { return _keys[key]; }
    public long setKey(string key, long value) { if (!_keys.ContainsKey(key)) _keys.Add(key, value); else _keys[key] = value; return value; }

    // gestione controlli
    protected int _iscript = 0;
    protected int _icontrol = 0;

    public page_cls(lib_page page, XmlNode page_node) {
      _page = page;
      _pageNode = page_node;
      _ctrls = new Dictionary<string, page_ctrl>();
      _docs = new Dictionary<string, xmlDoc>();
      _keys = new Dictionary<string, long>();
      _pageDocs = new Dictionary<string, xmlDoc>();
      if (_pageNode != null && _pageNode.Attributes["xml"] != null)
        setPageDoc(new xmlDoc(_page.parse(_pageNode.Attributes["xml"].Value)));
    }

    #region infos

    public bool boolXmlProp(string property, bool def = false) { return xmlDoc.node_bool(_pageNode, property, def); }

    protected string cfg_title_page(XmlNode page_node, string name_node, out string title_page, out string page_name) {
      title_page = page_name = "";
      foreach (System.Xml.XmlNode t_node in page_node.SelectNodes(name_node)) {
        // if, par
        if ((t_node.Attributes["if"] != null && !_page.eval_cond_id(t_node.Attributes["if"].Value)) || (t_node.Attributes["par"] != null
            && page.query_param(t_node.Attributes["par"].Value.Split('=')[0]) != t_node.Attributes["par"].Value.Split('=')[1]))
          continue;

        // select
        title_page = t_node.InnerText;
        page_name = xmlDoc.node_val(t_node, "page", "");
        return xmlDoc.node_val(t_node, "select", "");
      }

      return "";
    }

    public virtual string titlePage(XmlNode page_node, Dictionary<string, string> fields = null
        , System.Xml.XmlNode row = null, System.Data.DataRow dr = null) {

      string title_page = "", page_name = "", idselect = cfg_title_page(page_node, "title", out title_page, out page_name);
      return idselect != "" ? _page.classPage.select_from_ctrl(page_name, idselect, (title_page == "" ? xmlDoc.node_val(page_node, "title") : title_page))
         : page.parser.get_string((title_page == "" ? xmlDoc.node_val(page_node, "title") : title_page), "", fields, row, dr);
    }

    public virtual string desPage(XmlNode page_node, Dictionary<string, string> fields = null
        , System.Xml.XmlNode row = null, System.Data.DataRow dr = null) {

      string title_page = "", page_name = "", idselect = cfg_title_page(page_node, "des", out title_page, out page_name);
      return idselect != "" ? _page.classPage.select_from_ctrl(page_name, idselect, (title_page == "" ? xmlDoc.node_val(page_node, "des") : title_page))
         : page.parser.get_string((title_page == "" ? xmlDoc.node_val(page_node, "des") : title_page), "", fields, row, dr);
    }

    public virtual string info(string name) {
      if (name == "userVisibility") { string ids = user_ids(_page.userId); return ids == "" ? "-1" : ids; }

      return null;
    }

    public string user_ids(int id_user, bool not_anonimo = false) {
      return dt_from_id("user_child_ids", new Dictionary<string, string>() { { "idUser", id_user.ToString() }, { "not_anonimo", not_anonimo ? "1" : "0" } }).Rows[0][0].ToString();
    }

    public string user_childs(int id_user) {
      return dt_from_id("user_childs", new Dictionary<string, string>() { { "idUser", id_user.ToString() } }).Rows[0][0].ToString();
    }

    #endregion

    #region events

    public virtual void onInit(object sender, EventArgs e, bool request = false, bool addControls = true) {
      // eseguo i comandi speciali di postback
      //if (page.IsPostBack)
      //{
      //    string target = page.Request.Form["__EVENTTARGET"];
      //    // target: {reload-setkey}:<key id>, argument: value
      //    if (target.IndexOf("{reload-setkey}:") == 0)
      //    {
      //        page.EnableViewState = false;
      //        page.ViewStateMode = ViewStateMode.Disabled;
      //        setreload();
      //        setKey(target.Substring(16), long.Parse(page.Request.Form["__EVENTARGUMENT"]));
      //    }
      //}

      // carico la pagina
      int scriptPos = 0;
      if (!request) {
        // metto gli scripts dopo gli eventuali <meta>
        int i = 0;
        foreach (Control ctrl in _page.Header.Controls) {
          if (ctrl is HtmlMeta)
            scriptPos = i + 1;
          i++;
        }

        scriptPos = add_scripts(scriptPos);

        // header - menu
        if (isDefPage && addControls) {
          addSection(headerName(), 0);
          if (boolXmlProp("menu", true)) addCtrlFile(menuFile());
        }
      }

      // def
      _page_flags = new TextBox();
      _page_flags.Style.Add("display", "none");
      _page_flags.Attributes.Add("ctrl-id", "_page_flags");
      mainForm().Controls.Add(_page_flags);

      if (pageDoc != null) {
        // import
        foreach (XmlNode import in pageDoc.nodes("/page//import")) {
          if (import.Attributes["if"] != null && !page.eval_cond_id(import.Attributes["name"].Value, import.Attributes["if"].Value))
            continue;

          includeCtrl(import.Attributes["name"].Value + ".xml", import);
        }

        // controls
        foreach (XmlNode ctrl in pageDoc.nodes("/page/contents/*")) addCtrl(ctrl);
      }

      // add client modules - controls to page
      if (isDefPage && !request && addControls) {
        foreach (page_ctrl srv in ctrlsToRender)
          srv.moduleNodes().ForEach(module => { scriptPos = add_module(module, scriptPos); });
        foreach (page_ctrl srv in ctrlsToRender) srv.add();
      }
    }

    protected void includeCtrl(string file, XmlNode includeNode = null) {
      xmlDoc doc = new xmlDoc(System.IO.Path.Combine(_page.cfg_var("ctrlsFolder"), file));

      if (includeNode != null) {
        XmlNode node = includeNode.ParentNode.InsertAfter(includeNode.OwnerDocument.ImportNode(doc.root_node, true), includeNode);
        foreach (XmlAttribute attr in includeNode.Attributes)
          if (node.Attributes[attr.Name] == null)
            node.Attributes.Append(node.OwnerDocument.CreateAttribute(attr.Name)).Value = attr.Value;
        includeNode.ParentNode.RemoveChild(includeNode);
      }
    }

    protected void addCtrlFile(string file, bool render = true) {
      xmlDoc doc = new xmlDoc(System.IO.Path.Combine(_page.cfg_var("ctrlsFolder"), file));

      if (doc.root_value("name") == "")
        doc.set_root_attr("name", System.IO.Path.GetFileNameWithoutExtension(file));

      if (_ctrls.ContainsKey(doc.root_value("name")))
        return;

      _docs.Add(doc.root_value("name"), doc);

      addCtrl(doc.root_node, render);
    }

    protected void addCtrl(XmlNode defnode, bool render = true) {
      // elemento che definisce il controllo nella pagina
      string name = defnode.Attributes["name"].Value;
      if (_ctrls.ContainsKey(name))
        return;

      if (defnode != null && (defnode.Attributes["if"] != null && !page.eval_cond_id(name, defnode.Attributes["if"].Value)))
        return;

      // il nome del controllo non può essere quello di una pagina definita
      if (_page.nodePage(defnode.Attributes["name"].Value, false) != null)
        throw new Exception("il controllo '" + defnode.Attributes["name"].Value + "' ha lo stesso nome di una pagina definita");

      if (defnode.Name == "form") addControl(new form_ctrl(this, defnode, render));
      else if (defnode.Name == "form-attach" && lastControl("form") != null) ((form_ctrl)lastControl("form")).insertAttach(defnode);
      else if (defnode.Name == "listview") addControl(new lw_ctrl(this, defnode, render));
      else if (defnode.Name == "grid") addControl(new grid_ctrl(this, defnode, render));
      else if (defnode.Name == "tiles") addControl(new tiles_ctrl(this, defnode, render));
      else if (defnode.Name == "tabs") addControl(new tabs_ctrl(this, defnode, render));
      else if (defnode.Name == "menu") addControl(new menu_ctrl(this, defnode, render));
      else if (defnode.Name == "section") addControl(new section_ctrl(this, defnode, render));
      else if (defnode.Name == "graph") addControl(new graph_ctrl(this, defnode, render));
      else throw new Exception("il controllo '" + name + "' non è supportato!");
    }

    public virtual void onLoad(object sender, EventArgs e) {
      //if (_page.IsPostBack) { if(_page.Request.Params["__EVENTTARGET"] == "refresh") set_page_flag("refreshed"); }

      foreach (page_ctrl ctrl in ctrlsToRender) ctrl.onLoad();
    }

    public virtual bool action(string actionName, string formName, string keys = "", string noConfirm = "", string refurl = "") { return false; }

    public virtual string demandBeforeSubmit(string name) { return ""; }

    public virtual submitResponse submitFormBeforeData(string name) { return submitResponse.notvalued; }

    public virtual submitResponse submitFormAfterData(string name) { return submitResponse.notvalued; }

    public virtual bool ctrl_Click(object sender, EventArgs e) {
      // gestione particolare dei controlli
      foreach (page_ctrl ctrl in ctrls)
        if (ctrl.ctrlClick(sender, e))
          return true;

      if (sender is Button && ((Button)sender).Attributes["btn_type"] != null) {
        string type = ((Button)sender).Attributes["btn_type"];
        if ((type == "submit" && submit_Click(sender, e)) || (type == "action" && action_Click(sender, e))
         || (type == "exit" && exit_Click(sender, e))) return true;
      }

      return false;
    }


    #endregion

    #region controls

    public string newIdControl { get { _icontrol++; return "ctrl_" + _icontrol.ToString(); } }

    protected bool exit_Click(object sender, EventArgs e) {
      page.redirect(page.urlPrev());

      return true;
    }

    protected bool action_Click(object sender, EventArgs e) {
      try {
        Button submit = (Button)sender;

        // convalida form
        string type = submit.Attributes["btn_type"], form_name = submit.Attributes["form_name"]
          , checks = submit.Attributes["checks"];
        if (checks == "true") {
          foreach (page_ctrl ctrl in ctrls)
            if (ctrl is form_ctrl && ((form_ctrl)ctrl).render && ((form_ctrl)ctrl).name == form_name) {
              string err = "";
              if (!((form_ctrl)ctrl).convalidaForm(out err))
                throw new Exception(err);
            }
        }

        return action(submit.Attributes["actionname"], form_name, ""
            , submit.Attributes["noconfirm"], submit.Attributes["ref"]) ? true : false;
      } catch (Exception ex) {
        regScript(scriptStartAlert(ex.Message, "Attenzione!"));
      }

      return false;
    }

    protected bool submit_Click(object sender, EventArgs e) {
      bool result = false, alert = false, noexit = ((Button)sender).Attributes["noexit"] != null && ((Button)sender).Attributes["noexit"] == "true";
      string bck_modalita = page_flags, form_name = ((Button)sender).Attributes["form_name"];
      try {
        // convalida dei dati
        if (!check_page_flag("confirmed")) {
          foreach (page_ctrl ctrl in ctrls)
            if (ctrl is form_ctrl && ((form_ctrl)ctrl).render && ((form_ctrl)ctrl).name == form_name) {
              result = true;

              string err = "";
              if (!((form_ctrl)ctrl).convalidaForm(out err))
                throw new Exception(err);
            }
        }

        // la domandina 
        if (!check_page_flag("confirmed")) {
          foreach (page_ctrl ctrl in ctrls) {
            if (ctrl.name == form_name) {
              string msg = demandBeforeSubmit(ctrl.name);
              if (msg != "") {
                regScript(scriptStartAlert(msg, "Attenzione!", "set_page_flag('confirmed');$('input[form_name="
                    + ((WebControl)sender).Attributes["form_name"] + "][btn_type=" + ((WebControl)sender).Attributes["btntype"] + "]').click()", null, true));
                return true;
              }
            }
          }
        }
        page_flags = "";

        // submit data
        foreach (page_ctrl ctrl in ctrls)
          if (ctrl is form_ctrl && ((form_ctrl)ctrl).render && ((form_ctrl)ctrl).name == form_name) {

            if (submitFormBeforeData(ctrl.name) == submitResponse.ok_alert) alert = true;

            ((form_ctrl)ctrl).submitDataForm();

            if (submitFormAfterData(ctrl.name) == submitResponse.ok_alert) alert = true;

            result = true;
          }

      } catch (Exception ex) {
        if (bck_modalita != "") page_flags = bck_modalita;
        regScript(scriptStartAlert("Ci sono stati dei problemi durante l'aggiornamento: '" + ex.Message + "'", "Attenzione!"));

        return result;
      }

      if (page.query_param("child") == "1" && !noexit)
        regScript(scriptStart((page.query_param("field") != "" ? "window.opener.request_combo('" + page.query_param("cname") + "', '" + page.query_param("field") + "');"
          : (page.query_param("refresh") == "1" ? "window.opener.refresh_page();" : "")) + " window.close();"));
      else {
        string ref_page = ((Button)sender).Attributes["ref"] != null ? page.parse(((Button)sender).Attributes["ref"]) : ""
          , url = ref_page != "" ? ref_page : page.urlPrev();
        bool show_msg = ((Button)sender).Attributes["noconfirm"] != null ? !bool.Parse(((Button)sender).Attributes["noconfirm"])
          : (((Button)sender).Attributes["confirm"] != null ? bool.Parse(((Button)sender).Attributes["confirm"]) : _page.cfg_var_bool("forms_def_confirm", "", true));
        if (!alert) {
          if (!show_msg) {
            //if (ref_page != "") page.upd_history(url);
            page.redirect(url);
          } else regScript(scriptStartAlert("I dati sono stati aggiornati correttamente", "Messaggio", null, url));
        }
      }

      return result;
    }

    protected page_ctrl lastControl(string name) {
      for (int i = _ctrls.Count - 1; i >= 0; i--)
        if (_ctrls.ElementAt(i).Value.rootNode.Name == name)
          return _ctrls.ElementAt(i).Value;

      return null;
    }

    protected page_ctrl addControl(page_ctrl control, bool oninit = true) {
      if (_ctrls.ContainsKey(control.name)) {
        if (control.render && !_ctrls[control.name].render)
          _ctrls[control.name].render = true;

        return _ctrls[control.name];
      }

      _ctrls.Add(control.name, control);
      if (oninit && control.render)
        control.onInit();

      return control;
    }

    public HtmlForm mainForm(bool throwIfNotExists = true) {
      HtmlForm result = findControlByAttribute<HtmlForm>(_page, "mainform", "true");
      if (result == null && throwIfNotExists)
        throw new Exception("devi aggiungere al documento il form principale con l'attributo \"mainForm='true'\"");

      return result;
    }

    public String menuFile() {
      if (_pageNode == null)
        return "";

      if (_pageNode.Attributes["menufile"] != null)
        return _pageNode.Attributes["menufile"].Value;

      if (_page.cfg_node("/root/pages").Attributes["menufile"] != null)
        return _page.cfg_node("/root/pages").Attributes["menufile"].Value;

      return "";
    }

    protected string headerName() {
      if (boolXmlProp("noheader")) return "";
      if (_page.history.count > 1)
        return _pageNode != null && _pageNode.Attributes["headerprev"] != null && !boolXmlProp("noheaderprev")
          ? _pageNode.Attributes["headerprev"].Value
           : (_page.cfg_node("/root/pages").Attributes["headerprev"] != null && !boolXmlProp("noheaderprev")
            ? _page.cfg_node("/root/pages").Attributes["headerprev"].Value
            : (_pageNode != null && _pageNode.Attributes["header"] != null ? _pageNode.Attributes["header"].Value : _page.cfg_value("/root/pages", "header")));
      else return _pageNode != null && _pageNode.Attributes["header"] != null ? _pageNode.Attributes["header"].Value
         : _page.cfg_value("/root/pages", "header");
    }

    public T findControlByAttribute<T>(string attributeName, string attributeValue) where T : HtmlControl {
      return findControlByAttribute<T>(_page, attributeName, attributeValue);
    }

    protected T findControlByAttribute<T>(Control ctl, string attributeName, string attributeValue) where T : HtmlControl {
      foreach (Control c in ctl.Controls) {
        if (c.GetType() == typeof(T) && ((T)c).Attributes[attributeName] != null && ((T)c).Attributes[attributeName] == attributeValue) {
          return (T)c;
        }
        Control cb = findControlByAttribute<T>(c, attributeName, attributeValue);
        if (cb != null)
          return (T)cb;
      }
      return null;
    }

    public T findControlByAttributes<T>(string attrName1, string attrName2) where T : WebControl {
      return findControlByAttributes<T>(_page, attrName1, null, attrName2, null);
    }

    protected T findControlByAttributes<T>(Control ctl, string attrName1, string attrName2) where T : WebControl {
      return findControlByAttributes<T>(ctl, attrName1, null, attrName2, null);
    }

    public T findControlByAttributes<T>(string attrName1, string attrValue1
        , string attrName2, string attrValue2) where T : WebControl {
      return findControlByAttributes<T>(_page, attrName1, attrValue1, attrName2, attrValue2);
    }

    public T findHtmlControlByAttributes<T>(string attrName1, string attrValue1
        , string attrName2, string attrValue2) where T : HtmlControl {
      return findHtmlControlByAttributes<T>(_page, attrName1, attrValue1, attrName2, attrValue2);
    }

    protected T findControlByAttributes<T>(Control ctl, string attrName1, string attrValue1
        , string attrName2, string attrValue2) where T : WebControl {
      foreach (Control c in ctl.Controls) {
        // controllo sui valori
        if (attrValue1 != null || attrValue2 != null) {
          if (c.GetType() == typeof(T)
              && (((T)c).Attributes[attrName1] != null && ((T)c).Attributes[attrName1] == attrValue1)
              && (((T)c).Attributes[attrName2] != null && ((T)c).Attributes[attrName2] == attrValue2)) {
            return (T)c;
          }
        }
          // controllo sull'esistenza degli attributi
        else if (attrValue1 == null || attrValue2 == null) {
          if (c.GetType() == typeof(T)
              && ((T)c).Attributes[attrName1] != null && ((T)c).Attributes[attrName2] != null) {
            return (T)c;
          }
        }

        Control cb = findControlByAttributes<T>(c, attrName1, attrValue1, attrName2, attrValue2);
        if (cb != null)
          return (T)cb;
      }

      return null;
    }

    protected T findHtmlControlByAttributes<T>(Control ctl, string attrName1, string attrValue1
        , string attrName2, string attrValue2) where T : HtmlControl {
      foreach (Control c in ctl.Controls) {
        // controllo sui valori
        if (attrValue1 != null || attrValue2 != null) {
          if (c.GetType() == typeof(T)
              && (((T)c).Attributes[attrName1] != null && ((T)c).Attributes[attrName1] == attrValue1)
              && (((T)c).Attributes[attrName2] != null && ((T)c).Attributes[attrName2] == attrValue2)) {
            return (T)c;
          }
        }
          // controllo sull'esistenza degli attributi
        else if (attrValue1 == null || attrValue2 == null) {
          if (c.GetType() == typeof(T)
              && ((T)c).Attributes[attrName1] != null && ((T)c).Attributes[attrName2] != null) {
            return (T)c;
          }
        }

        HtmlControl cb = findHtmlControlByAttributes<T>(c, attrName1, attrValue1, attrName2, attrValue2);
        if (cb != null)
          return (T)cb;
      }

      return null;
    }

    public TextBox input_ctrl(string formName, string fieldName) {
      return findControlByAttributes<TextBox>("field_name", fieldName, "form_name", formName);
    }

    public page_ctrl control(string name) { return control(name, false, true); }
    public page_ctrl control(string name, bool throw_err) { return control(name, false, throw_err); }
    public form_ctrl form_control(string name) { return (form_ctrl)control(name, false, true); }

    protected page_ctrl control(string name, bool addnorender = false, bool throwerr = true) {
      if (addnorender && !_ctrls.ContainsKey(name)) addCtrlFile(name + ".xml", false);

      if (throwerr && !_ctrls.ContainsKey(name)) throw new Exception("il controllo specificato '" + name + "' non esiste!");

      return _ctrls.ContainsKey(name) ? _ctrls[name] : null;
    }

    public bool existControl(string name) { return _ctrls.ContainsKey(name); }

    public bool evalControlRequest(xmlDoc doc, xmlDoc outxml) {
      return _ctrls[doc.get_value("/request/pars/par[@name='ctrlname']")]
          .evalControlRequest(doc, outxml);
    }

    public void saveUnloadKeys(xmlDoc doc) {
      foreach (XmlNode nd in doc.nodes("/request/pars"))
        save_key(xmlDoc.node_val_x(nd, "par[@name='keys']"), xmlDoc.node_val_x(nd, "par[@name='var']")
          , xmlDoc.node_val_x(nd, "par[@name='val']"), xmlDoc.node_bool_x(nd, "par[@name='onsession']"));
    }

    protected void reset_session_keys(int id_user) { exec_updates("reset_session_keys", "", new Dictionary<string, string>() { { "idutente", id_user.ToString() } }); }

    protected void save_key(string keys, string var, string val, bool on_session = true) {
      exec_updates("save_page_key", "", new Dictionary<string, string>() { { "idutente", _page.userId.ToString() }, { "keys", keys }, { "var", var }
        , { "val", val } , { "on_session", on_session ? "1" : "0" } });
    }

    public string get_key(string keys, string var, string def_val = "") {
      if (keys == "") return def_val;
      DataTable dt = dt_from_id("get_page_key", new Dictionary<string, string>() { { "idutente", _page.userId.ToString() }, { "keys", keys }, { "var", var } });
      return dt != null && dt.Rows.Count > 0 ? dt.Rows[0]["val"].ToString() : def_val;
    }

    #endregion

    #region scripts

    /// <summary>
    /// Aggiunta moduli css e js 
    /// </summary>
    protected int add_scripts(int pos) {

      List<XmlNode> grps = new List<XmlNode>(_page.cfg_nodes("/root/include/grp_module"));
      if (pageDoc != null) grps.AddRange(pageDoc.nodes("/page/include/grp_module").Cast<XmlNode>());

      List<XmlNode> mods = new List<XmlNode>(modules_from_groups(grps));
      mods.AddRange(_page.cfg_nodes("/root/include/module"));
      if (pageDoc != null) mods.AddRange(pageDoc.nodes("/page/include/module|/page/include/script").Cast<XmlNode>());

      mods.ForEach(mod => { pos = add_module(mod, pos); });

      return pos;
    }

    public List<XmlNode> modules_from_groups(List<XmlNode> groups) {
      List<XmlNode> modules = new List<XmlNode>();
      groups.ForEach(grp => {
        modules.AddRange(_page.cfg_nodes("/root/grp_modules/grp_module[@name='"
          + grp.Attributes["name"].Value + "']/module"));
      });

      return modules;
    }

    protected int add_module(XmlNode module, int pos) {
      if (module.Name == "module") {
        string type = module.Attributes["type"].Value, href = _page.parse(module.Attributes["href"].Value);
        if (type == "css") {
          System.Web.UI.HtmlControls.HtmlGenericControl script = new System.Web.UI.HtmlControls.HtmlGenericControl("link");
          script.Attributes.Add("type", "text/css");
          script.Attributes.Add("rel", "stylesheet");
          script.Attributes.Add("href", href);
          _page.Header.Controls.AddAt(pos, script);
        } else if (type == "script") {
          System.Web.UI.HtmlControls.HtmlGenericControl script = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
          script.Attributes.Add("type", "text/javascript");
          script.Attributes.Add("src", href);
          _page.Header.Controls.AddAt(pos, script);
        } else throw new Exception("elemento lato client '" + type + "' non riconosciuto");
      } else if (module.Name == "script") {
        _page.Header.Controls.Add(
            new LiteralControl(
                "<script language='javascript'>" + _page.parse(module.InnerText) + "</script>"
            )
        );

      } else throw new Exception("elemento lato client '" + module.Name + "' non riconosciuto");
      pos++;

      return pos;
    }

    public string scriptStartAlert(string msg, string title, string evalonclose = null, string refonclose = null, bool? yesno = null) {
      msg = msg.Replace("\"", "'").Replace("\r\n", ".").Replace("\n", ".").Replace("\\", "/");
      return "$(window).load(function () { metro_alert(\"" + msg + "\", \"" + title + "\""
          + (refonclose != null ? ", \"" + refonclose + "\"" : ", null")
          + (evalonclose != null ? ", function() { " + evalonclose + " }" : ", null")
          + (((yesno.HasValue && yesno.Value) ? ", true" : ", false")) + "); });";
    }

    public string scriptStart(string script) {
      return "$(window).load(function () {" + script + " });";
    }

    public string clientScript(string id) {
      return _page.cfg_value("/*/clientscripts/clientscript[@name='" + id + "']");
    }

    public void regScript(string script) {
      _page.ClientScript.RegisterClientScriptBlock(GetType(), "tmpStart_" + _iscript.ToString(), script, true);

      _iscript++;
    }

    #endregion

    #region sections

    public void addError(Exception ex, string msg) { addError(ex, null, msg); }

    public void addError(Exception ex, int? atIndex = null, string msg = "") {
      Dictionary<string, string> fields = new Dictionary<string, string>();
      fields.Add("exception", ex.Message);

      addSection("error", atIndex, fields);
      _page.logErr(ex);
    }

    public void addSection(string name) { addSection(name, null); }

    public void addSection(string name, int? atIndex, Dictionary<string, string> fields = null) {
      if (!string.IsNullOrEmpty(name)) {
        // section
        if (_page.cfg_count("/root/sections/section[@name='" + name + "']") == 0)
          throw new Exception("la sezione '" + name + "' non è stata configurata!");

        // parse
        addHtmlSection(_page.parse(_page.cfg_value("/root/sections/section[@name='" + name + "']"), fields)
          , atIndex);
      }
    }

    public string htmlSection(string cname, string name, out string ifCond) {
      string tmp;

      return htmlSection(cname, name, out ifCond, out tmp);
    }

    public string htmlSection(string cname, string name, out string ifCond, out string keys) {
      ifCond = "";
      keys = "";

      // dal controllo
      if (control(cname, true, false) != null && control(cname).rootCount("sections/section[@name='" + name + "']") > 0) {
        ifCond = control(cname).rootValue("sections/section[@name='" + name + "']", "if");
        keys = control(cname).rootValue("sections/section[@name='" + name + "']", "keys");

        return control(cname).rootValue("sections/section[@name='" + name + "']");
      }

      // dal config
      if (_page.cfg_count("/root/sections/section[@name='" + name + "']") > 0) {
        ifCond = _page.cfg_value("/root/sections/section[@name='" + name + "']", "if");
        keys = _page.cfg_value("/root/sections/section[@name='" + name + "']", "keys");

        return _page.cfg_value("/root/sections/section[@name='" + name + "']");
      }

      throw new Exception("la sezione '" + name + "' non è stata configurata!");
    }

    // aggiunta html alla sezione specificata della pagina
    public void addHtmlSection(string html, int? atIndex) { addHtmlSection(html, mainForm(), atIndex); }

    public void addHtmlSection(string html) { addHtmlSection(html, mainForm()); }

    public void addHtmlSection(string html, HtmlControl form) { addHtmlSection(html, form, null); }

    public void addHtmlSection(string html, HtmlControl form, int? atIndex) {
      Literal litHtml = new Literal();
      litHtml.Text = html;

      if (atIndex.HasValue) form.Controls.AddAt(atIndex.Value, litHtml);
      else form.Controls.Add(litHtml);
    }

    public string xml_topage(string xml) { return xml_topage(newIdControl, xml); }

    public string xml_topage(string id, string xml) {
      HtmlTextArea xmlArea = new HtmlTextArea();
      xmlArea.ID = id;
      xmlArea.Value = xml;
      xmlArea.Style.Add("visibility", "hidden");
      xmlArea.Style.Add("display", "none");
      mainForm().Controls.AddAt(0, xmlArea);
      return id;
    }

    #endregion

    #region data access

    // nodo xml del controllo o della pagina specificata
    public XmlNode ctrl_node(string cname) {
      if (string.IsNullOrEmpty(cname)) return null;

      // si tratta di una pagina?
      XmlNode result = _page.nodePage(cname, false);

      // carico la pagina
      if (result != null) result = setPageDoc(result.Attributes["name"].Value, new xmlDoc(_page.parse(result.Attributes["xml"].Value))).root_node;

      if (result == null) { page_ctrl ctrl = control(cname, true, false); if (ctrl != null) result = ctrl.rootNode; }

      return result;
    }

    public string select_from_ctrl(string cname, string id_sel, string expr) {
      System.Data.DataTable dt = dt_from_id(id_sel, cname);
      return dt == null || dt.Rows.Count == 0 ? "" :
        _page.parse(expr, cname, dt.Rows[0]);
    }

    protected XmlNode sqlsel_node(string name, int index = 0, string cname = "", XmlNode cnode = null) {
      cnode = cnode == null ? ctrl_node(cname) : cnode;
      string xpath = "select[@name='" + name + "'][" + (index + 1).ToString() + "]";
      return cnode != null && cnode.SelectSingleNode("queries/" + xpath) != null ? cnode.SelectSingleNode("queries/" + xpath)
        : (pageDoc != null && pageDoc.exist("/page/queries/" + xpath) ? pageDoc.node("/page/queries/" + xpath)
        : (_page.cfg_exist("/root/queries/" + xpath) ? _page.cfg_node("/root/queries/" + xpath) : (XmlNode)null));
    }

    public bool there_sql(string name, XmlNode cnode = null, string cname = "") { return sqlsel_node(name, 0, cname, cnode) != null; }

    protected sql_select sqlsel(string name, int index = 0, string cname = "", XmlNode cnode = null) {

      XmlNode nd = sqlsel_node(name, index, cname, cnode);

      string if_cond = xmlDoc.node_val(nd, "if");
      bool func = nd.Attributes["script"] != null && nd.Attributes["script"].Value != ""
        , join = nd.Attributes["from"] != null && nd.Attributes["from"].Value != "";
      return func ? sql_select.create_script(name, nd.Attributes["script"].Value, if_cond)
        : (join ? sql_select.create_join(name, nd.InnerText, nd.Attributes["from"].Value, nd.Attributes["join"].Value, if_cond)
        : sql_select.create_select(name, nd.InnerText, if_cond, xmlDoc.node_val(nd, "data-fields")));
    }

    public string qry_text(string id, string cname = "", XmlNode cnode = null) {

      cnode = cnode == null ? ctrl_node(cname) : cnode;
      string xpath = "select[@id='" + id + "']";
      XmlNode nd = cnode != null && cnode.SelectSingleNode("queries//" + xpath) != null ? cnode.SelectSingleNode("queries//" + xpath)
        : (pageDoc != null && pageDoc.exist("/page/queries//" + xpath) ? pageDoc.node("/page/queries//" + xpath)
        : (_page.cfg_exist("/root/queries//" + xpath) ? _page.cfg_node("/root/queries//" + xpath) : (XmlNode)null));

      if (nd == null) {
        xpath = "update[@id='" + id + "']";
        nd = cnode != null && cnode.SelectSingleNode("queries//" + xpath) != null ? cnode.SelectSingleNode("queries//" + xpath)
          : (pageDoc != null && pageDoc.exist("/page/queries//" + xpath) ? pageDoc.node("/page/queries//" + xpath)
          : (_page.cfg_exist("/root/queries//" + xpath) ? _page.cfg_node("/root/queries//" + xpath) : (XmlNode)null));
      }

      return nd != null ? nd.InnerText : "";
    }

    int select_count(string cname, string id) {
      XmlNode cnode = ctrl_node(cname);
      return cnode != null && cnode.SelectSingleNode("queries/select[@name='" + id + "']") != null ?
        cnode.SelectNodes("queries/select[@name='" + id + "']").Count
        : (_page.cfg_exist("/root/queries/select[@name='" + id + "']") ?
         _page.cfg_nodes("/root/queries/select[@name='" + id + "']").Count : 0);
    }

    public DataTable dt_from_ids(string ids, string cname = "", string table_name = "") {
      return dt_from_sql(sql_sel(cname, ids), cname, null, null, table_name);
    }

    public DataTable dt_from_id(string id, Dictionary<string, string> fields) {
      return dt_from_id(id, "", "", fields);
    }

    public DataTable dt_from_id(string idSelect, string cname = ""
      , string table_name = "", Dictionary<string, string> fields = null, DataRow dr = null, bool throwerr = true, XmlNode cnode = null) {
      return dt_from_sql(sqlsel(idSelect, 0, cname, cnode), cname, fields, dr, table_name, throwerr);
    }

    public string sql_from_id(string idSelect, string cname = ""
      , Dictionary<string, string> fields = null, DataRow dr = null, XmlNode cnode = null) {
      return sql_text(sqlsel(idSelect, 0, cname, cnode), cname, fields, dr);
    }

    public DataTable dt_from_sql(string sql, string cname = "", Dictionary<string, string> fields = null, DataRow dr = null
      , string table_name = "", bool throwerr = true) {
      if (string.IsNullOrEmpty(sql) == null) return null;

      return (cname != "" ? _page.conn_db_user() : _page.conn_db_user()).dt_table(page.parse(sql, cname, fields, dr), table_name, throwerr);
    }

    public DataTable dt_from_sql(sql_select sql, string cname = "", Dictionary<string, string> fields = null, DataRow dr = null
      , string table_name = "", bool throwerr = true) {
      if (sql == null) return null;

      // join - script
      if (sql.is_join || sql.is_script) {
        DataTable dt = sql.is_join ? join_tables(sql.from, sql.join, sql.fields, cname)
          : dt_from_script(cname, sql.command);

        if (table_name != "") dt.TableName = table_name;

        return dt;
      }
      // select
      return (cname != "" ? _page.conn_db_user() : _page.conn_db_user()).dt_table(page.parse(sql.command, cname, fields, dr), table_name, throwerr);
    }

    protected string sql_text(sql_select sql, string cname, Dictionary<string, string> fields = null, DataRow dr = null) {
      if (sql == null || (sql != null && (sql.is_join || sql.is_script))) return null;
      return page.parse(sql.command, cname, fields, dr);
    }

    public sql_select sql_sel(string cname, string ids) {
      foreach (string id_sel in ids.Split(',')) {
        for (int i = 0; i < select_count(cname, id_sel); i++) {
          sql_select sql = sqlsel(id_sel, i, cname);
          if (sql != null && sql.if_cond != "") {
            if (sql.if_cond.Split(',').FirstOrDefault(x => _page.eval_cond_id(cname, x)) != null)
              return sql;
            continue;
          }
          return sql;
        }
      }
      return null;
    }

    public List<sql_select> sql_list(string cname, string ids) {
      List<sql_select> res = new List<sql_select>();
      foreach (string id_sel in ids.Split(',')) {
        for (int i = 0; i < select_count(cname, id_sel); i++) {
          sql_select sql = sqlsel(id_sel, i, cname);
          if (sql != null && sql.if_cond != "") {
            if (sql.if_cond.Split(',').FirstOrDefault(x => _page.eval_cond_id(cname, x)) != null)
              res.Add(sql);
            continue;
          }
          res.Add(sql);
        }
      }
      return res;
    }

    public List<XmlNode> sql_updates(string cname, string ids, XmlNode cnode = null) {
      List<XmlNode> result = new List<XmlNode>();

      cnode = cnode == null ? ctrl_node(cname) : cnode;

      foreach (string id in ids.Split(',')) {
        // individuo i nodi xml che contengono le queries
        List<System.Xml.XmlNode> updates = new List<System.Xml.XmlNode>();

        if (cnode != null && cnode.SelectSingleNode("queries/updates[@name='" + id + "']") != null)
          updates.AddRange(cnode.SelectNodes("queries/updates[@name='" + id + "']").Cast<XmlNode>().ToList());
        else if (_page.cfg_exist("/root/queries/updates[@name='" + id + "']"))
          updates.AddRange(_page.cfg_nodes("/root/queries/updates[@name='" + id + "']"));

        if (cnode != null && cnode.SelectSingleNode("queries/update[@name='" + id + "']") != null)
          updates.AddRange(cnode.SelectNodes("queries/update[@name='" + id + "']").Cast<XmlNode>().ToList());
        else if (_page.cfg_exist("/root/queries/update[@name='" + id + "']"))
          updates.AddRange(_page.cfg_nodes("/root/queries/update[@name='" + id + "']"));

        // torno le queries
        foreach (System.Xml.XmlNode updateNode in updates) {
          if (updateNode.Name == "updates") {
            if (!(updateNode.Attributes["if"] != null && !_page.eval_cond_id(cname, updateNode.Attributes["if"].Value))) {
              foreach (System.Xml.XmlNode node in updateNode.SelectNodes("update"))
                if (!(node.Attributes["if"] != null && !_page.eval_cond_id(cname, node.Attributes["if"].Value)))
                  result.Add(node);
            }
          } else {
            if (!(updateNode.Attributes["if"] != null && !_page.eval_cond_id(cname, updateNode.Attributes["if"].Value)))
              result.Add(updateNode);
          }
        }
      }

      return result;
    }

    protected DataTable dt_from_script(string cname, string script_id) {

      if (string.IsNullOrEmpty(cname)) throw new Exception("non è stato specificato il controllo d'origine");

      object set = _page.parser.invoke(_page.code_table_id(cname, script_id), cname);
      if (set == null) return null;

      if (!(set is DataTable)) throw new Exception("lo script '" + script_id + "' deve tornare un DataTable");

      return (DataTable)set;
    }

    /// <summary>
    /// Join fra più tabelle tramite DataRelation
    /// </summary>
    /// <param name="ds">Dataset contenente tutte le tabelle da unire</param>
    /// <param name="from">nome tabella principale seguito da eventuale alias, es.: 'spese s' oppure 'spese'</param>
    /// <param name="joins">definizione delle relazioni fra le tabelle secondarie comprese di alias e campo
    /// di relazione, es.: 'eventi on idevento, tipispesa t on idtipospesa'
    /// nota bene: il campo di relazione dev'essere solo uno appartenente alla chiave primaria della tabella secondaria</param>
    /// <param name="fields">elenco dei campi da includere nella table finale specificando anche gli alias
    /// , es.: 'spese.*, eventi.evento, t.tipospesa'</param>
    /// <returns></returns>
    protected DataTable join_tables(string from, string joins, string fields, string cname) {

      // funzioni interne
      Func<string, string> field_join = (string txt) => { return txt.Substring(txt.ToLower().IndexOf(" on ") + 4).Trim(); };
      Func<string, string> table_join = (string txt) => { return txt.Substring(0, txt.ToLower().IndexOf(" on ")).Trim(); };
      Func<string, string> get_table = (string txt) => { return txt.IndexOf(' ') >= 0 ? txt.Substring(0, txt.IndexOf(' ')) : txt; };
      Func<string, string> get_alias = (string txt) => { return txt.IndexOf(' ') >= 0 ? txt.Substring(txt.IndexOf(' ') + 1) : txt; };
      Func<string, string, bool> same_table = (string tbl_on, string t) => {
        return get_table(tbl_on).ToLower() == t.ToLower() || get_alias(tbl_on).ToLower() == t.ToLower();
      };
      Func<string, string> find_table = (string tbl) => {
        return same_table(from, tbl) ? from
          : joins.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).First(x => same_table(table_join(x), tbl)).Trim();
      };

      // data tables
      DataSet ds = new DataSet();
      ds.Tables.Add(dt_from_id(get_table(from), cname, get_table(from)));
      foreach (string rel in joins.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        ds.Tables.Add(dt_from_id(get_table(table_join(rel)), cname, get_table(table_join(rel))));

      // relazioni
      foreach (string rel in joins.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
        string fld = field_join(rel), tbl = table_join(rel);
        ds.Relations.Add(new DataRelation("rel_" + get_table(tbl), ds.Tables[get_table(tbl)].Columns[fld]
          , ds.Tables[get_table(from)].Columns[fld]));
      }

      // asterischi
      while (fields.IndexOf("*") >= 0) {
        int virgola = fields.IndexOf(",", 0, fields.IndexOf("*"));
        string tbl = (virgola >= 0 ? fields.Substring(virgola + 1, fields.IndexOf("*") - virgola)
          : fields.Substring(0, fields.IndexOf("*") - 1)).Trim();
        fields = fields.Replace(tbl + ".*", string.Join(", ", ds.Tables[get_table(find_table(tbl))].Columns.Cast<DataColumn>()
          .Select(x => string.Format("{0}.{1}", tbl, x.ColumnName))));
      }

      // campi selezione
      Dictionary<string, string> flds = fields.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
        .ToDictionary(field => (field.IndexOf('.') >= 0 ? field.Substring(field.IndexOf('.') + 1) : field).Trim()
          , field => (field.IndexOf('.') >= 0 ? field.Substring(0, field.IndexOf('.')) : get_table(from)).Trim());

      // tabella joined
      DataTable dt_joined = new DataTable("joined");
      foreach (KeyValuePair<string, string> fld in flds)
        dt_joined.Columns.Add(new DataColumn(fld.Key, ds.Tables[get_table(find_table(fld.Value))].Columns[fld.Key].DataType));

      // copia dei valori
      Dictionary<string, DataRow> drs = joins.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .ToDictionary(x => get_table(table_join(x)), x => (DataRow)null);
      foreach (DataRow dr_src in ds.Tables[get_table(from)].Rows) {
        joins.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(t => {
          string tbl = get_table(table_join(t)); drs[tbl] = dr_src.GetParentRow("rel_" + tbl);
        });

        List<object> vals = new List<object>(flds.Select(fld => same_table(from, fld.Value) ? dr_src[fld.Key]
          : (drs[get_table(find_table(fld.Value))] != null ? drs[get_table(find_table(fld.Value))][fld.Key] : DBNull.Value)));
        dt_joined.Rows.Add(vals.ToArray());
      }

      return dt_joined;
    }

    public string path_xsd(XmlDocument doc, bool throwerr = true) {
      if (doc.DocumentElement.Attributes["schema"] == null) {
        if (throwerr)
          throw new Exception("il documento non ha specificato lo schema xml!");

        return "";
      }

      string schema = doc.DocumentElement.Attributes["schema"].Value;
      string pathxsd = _page.parser.get_string(_page.cfg_value("/root/xmlschemas/xmlschema[@name='" + schema + "']", "file"));
      if (pathxsd == "" && throwerr)
        throw new Exception("lo schema '" + schema + "' non è configurato correttamente!");

      return pathxsd;
    }

    /// <summary>
    /// Ottiene il valore della chiave specificata, es.: 'idfile=34&idfolder=43'
    /// </summary>
    protected string keyValue(string keys, string key) {
      int start = keys.ToLower().IndexOf(key.ToLower() + "=");
      if (start >= 0) {
        int end = key.IndexOf("&", start);
        if (end < 0)
          end = keys.Length;

        return keys.Substring(start + key.Length + 1, end - (start + key.Length + 1));
      }

      return "";
    }

    public long exec_updates(string ids, string cname = "", Dictionary<string, string> values = null, DataRow dr = null, XmlNode cnode = null) {
      bool trans = false;
      deeper.db.db_provider db = _page.conn_db_user();
      try {
        long rows = 0;
        foreach (XmlNode qry in sql_updates(cname, ids, cnode)) {
          if (!trans) { db.begin_trans(); trans = true; }
          if (qry.Attributes["setkey"] != null) {
            string sql = page.parse(qry.InnerText, cname, values, dr);
            long id = db.exec(sql, true);
            setKey(qry.Attributes["setkey"].Value, id);
            rows++;
          } else {
            long tmp = db.exec(page.parse(qry.InnerText, cname, values, dr));
            rows = tmp >= 0 ? rows + tmp : rows;
          }
        }

        if (trans) db.commit();

        return rows;
      } catch (Exception ex) { if (trans) db.rollback(); throw ex; }
    }

    #endregion

    #region system

    public string extract_dbpck(string path_pck) {
      if (!System.IO.File.Exists(path_pck))
        throw new Exception("non esiste il pacchetto da importare '" + path_pck + "'!");

      string tmp_folder = System.IO.Path.Combine(page.tmpFolder(), System.IO.Path.GetFileNameWithoutExtension(path_pck));
      if (!System.IO.Directory.Exists(tmp_folder)) zip.unzip(path_pck, tmp_folder);

      return System.IO.Path.Combine(tmp_folder, db_schema._indexXml);
    }

    public string extract_dbpck_index(string path_pck) {
      string tmp_folder = page.tmpFolder();
      string outpath = System.IO.Path.Combine(tmp_folder, System.IO.Path.GetRandomFileName());
      zip.unzip(path_pck, tmp_folder, db_schema._indexXml);
      System.IO.File.Move(System.IO.Path.Combine(tmp_folder, db_schema._indexXml), outpath);

      return outpath;
    }


    public void sendFile(string path, string clientname = "") {
      System.IO.Stream iStream = null;
      byte[] buffer = new Byte[10000];

      bool err = false;

      try {
        if (clientname == "") clientname = System.IO.Path.GetFileName(path);

        iStream = new System.IO.FileStream(path, System.IO.FileMode.Open,
                    System.IO.FileAccess.Read, System.IO.FileShare.Read);

        _page.Response.Clear();
        _page.Response.ContentType = "application/octet-stream";
        _page.Response.AddHeader("Content-Disposition", "attachment; filename=" + clientname);
        _page.Response.AddHeader("Content-Length", iStream.Length.ToString());

        long dataToRead = iStream.Length;
        while (dataToRead > 0) {
          if (_page.Response.IsClientConnected) {
            int length = iStream.Read(buffer, 0, 10000);

            _page.Response.OutputStream.Write(buffer, 0, length);
            _page.Response.Flush();

            buffer = new Byte[10000];
            dataToRead = dataToRead - length;
          } else dataToRead = -1;
        }
      } catch (Exception ex) { string msg = ex.Message; err = true; } finally { if (iStream != null) iStream.Close(); if (!err) _page.Response.Close(); }
    }

    #endregion

    #region config

    public string formatDates(string name, bool defaultIfEmpty = true) {
      string clientfmt = "";
      string serverfmt = "";

      if (!formatDates(name, out serverfmt, out clientfmt, defaultIfEmpty))
        return "";

      return serverfmt;
    }

    public bool formatDates(string name, out string serverfmt, bool defaultIfEmpty = true) {
      serverfmt = "";
      string clientfmt = "";

      return formatDates(name, out serverfmt, out clientfmt, defaultIfEmpty);
    }

    public bool formatDates(string name, out string serverfmt, out string clientfmt, bool defaultIfEmpty = true) {
      serverfmt = "";
      clientfmt = "";

      if (defaultIfEmpty && (name == "" || name == null))
        return defaultFormatDates(out serverfmt, out clientfmt);

      if (!_page.cfg_exist("/root/dateformats/dateformat[@name='" + name + "']"))
        return false;

      serverfmt = _page.cfg_value("/root/dateformats/dateformat[@name='" + name + "']", "serverfmt");
      clientfmt = _page.cfg_value("/root/dateformats/dateformat[@name='" + name + "']", "clientfmt");

      return true;
    }

    public bool defaultFormatDates(out string serverfmt, out string clientfmt) {
      serverfmt = "";
      clientfmt = "";

      return formatDates(_page.cfg_value("/root/dateformats", "default"), out serverfmt, out clientfmt);
    }

    public string formatFromGridType(string col_type) {
      return page.cfg_value("/root/gridsettings/types/type[@name='" + col_type + "']", "format");
    }

    protected string gridTypeCol(deeper.db.db_provider db, string schema_type) {
      return page.cfg_value("/root/gridsettings/schematypes/col[@schematype='" + schema_type.ToLower() + "']", "type", ""
          , "corrispondenza non trovata con il tipo colonna: " + schema_type);
    }

    #endregion
  }
}

// 1161