using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Web.UI.WebControls;
using System.Globalization;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.frmwrk
{
  public class page_ctrl
  {
    protected string _id = "";

    protected string _name;
    public string name { get { return _name; } }

    protected page_cls _cls = null;
    public page_cls page { get { return _cls; } }

    //protected xmlDoc _doc = null;
    protected XmlNode _defNode = null;
    public XmlNode rootNode { get { return _defNode; } }

    protected bool _render;
    public bool render { get { return _render; } set { _render = value; } }

    public bool rootExist(string xpath) { return rootNode.SelectSingleNode(xpath) != null; }
    public bool rootExistAttr(string attr) { return _defNode.Attributes[attr] != null && _defNode.Attributes[attr].Value != ""; }
    public string rootAttr(string attr, string defValue = "") { return xmlDoc.node_val(rootNode, attr, defValue); }
    public int rootAttrInt(string attr, int defValue = 0) { return xmlDoc.node_int(rootNode, attr, defValue); }
    public bool rootAttrBool(string attr, bool defValue = false) { return xmlDoc.node_bool(rootNode, attr, defValue); }
    public string rootValue(string xpath, string attr = "", string defValue = "") { return xmlDoc.node_val(rootNode.SelectSingleNode(xpath), attr, defValue); }
    public int rootInt(string xpath, string attr = "", int defValue = 0) { return xmlDoc.node_int(rootNode.SelectSingleNode(xpath), attr, defValue); }
    public bool rootBool(string xpath, string attr = "", bool defValue = false) { return xmlDoc.node_bool(rootNode.SelectSingleNode(xpath), attr, defValue); }
    public XmlNode rootSelNode(string xpath) { return rootNode.SelectSingleNode(xpath); }
    public int rootCount(string xpath) { return rootNode.SelectNodes(xpath).Count; }
    public XmlNodeList rootSelNodes(string xpath) { return rootNode.SelectNodes(xpath); }
    public List<XmlNode> rootList(string xpath) { return xmlDoc.toList(rootSelNodes(xpath)); }

    public Dictionary<string, string> _addstyles = null;

    public page_ctrl(page_cls page, XmlNode defNode, bool render = true) {
      _cls = page;
      _defNode = defNode;
      _name = defNode.Attributes["name"].Value;
      _render = render;

      // styles
      _addstyles = new Dictionary<string, string>();

      // hcenter
      if (xmlDoc.node_bool(_defNode, "hcenter")) {
        _addstyles.Add("margin-left", "auto");
        _addstyles.Add("margin-right", "auto");
      }

      // topmargin
      int topmargin = xmlDoc.node_int(_defNode, "topmargin");
      if (topmargin >= 0)
        _addstyles.Add("margin-top", topmargin.ToString() + "px");
    }

    #region init, events, interface

    public virtual void onInit() {
      _id = _cls.newIdControl;
    }

    public virtual void onLoad() { }

    public virtual void add() { }

    public virtual bool ctrlClick(object sender, EventArgs e) { return false; }

    public virtual bool ctrlDataBound(object sender, EventArgs e) { return false; }

    public virtual bool ctrlRowCommand(object sender, CommandEventArgs e) { return false; }

    public virtual bool fieldValue(string field, out string value, bool force_input = false) {
      value = "";

      return false;
    }

    public virtual string fieldValue(string field, bool throw_exc = true) {
      string value = "";
      if (fieldValue(field, out value)) return value;

      if(throw_exc) throw new Exception("non è stato trovato il campo '" + field + "'");

      return "";
    }

    public virtual bool evalControlRequest(xmlDoc doc, xmlDoc outxml) { return false; }

    #endregion

    #region base function

    protected string applySectionKeys(string name, string html, string keys) {
      foreach (string key in keys.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        html = html.Replace(key, rootValue("sectionskeys/sectionkeys[@name='" + name + "']/key[@name='" + key + "']"));
      return html;
    }

    protected string htmlSection(string name) {
      string tmp = "";
      return _cls.htmlSection(_name, name, out tmp);
    }

    protected string htmlSection(string name, out string keys) {
      keys = "";
      string tmp = "";

      return _cls.htmlSection(_name, name, out tmp, out keys);
    }

    protected string htmlSection(string name, out string ifCond, out string keys) {
      keys = "";
      ifCond = "";

      return _cls.htmlSection(_name, name, out ifCond, out keys);
    }

    static public Dictionary<string, string> key_fields(string keyValues) {
      Dictionary<string, string> result = new Dictionary<string, string>();

      foreach (string key in keyValues.Split(',')) {
        string field = key.Trim(), qryFld = key.Trim();
        if (field.IndexOf('(') > 0) {
          string clean = field;
          field = clean.Substring(0, clean.IndexOf('('));
          qryFld = clean.Substring(clean.IndexOf('(') + 1, clean.IndexOf(')') - clean.IndexOf('(') - 1);
        }

        result.Add(field, qryFld);
      }

      return result;
    }

    public virtual Dictionary<string, string> key_fields() {
      return null;
    }

    public List<XmlNode> moduleNodes() {
      List<XmlNode> mods = new List<XmlNode>(rootNode.SelectNodes("include/module").Cast<XmlNode>());
      mods.AddRange(_cls.modules_from_groups(rootNode.SelectNodes("include/grp_module").Cast<XmlNode>().ToList()));
      return mods;
    }

    protected string attr(XmlNode node, string name, string composite = "{{value}}", bool parse = false) {
      return node.Attributes[name] == null ? ""
        : (parse ? composite.Replace("{{value}}", _cls.page.parse(node.Attributes[name].Value, _name))
          : composite.Replace("{{value}}", node.Attributes[name].Value));
    }

    protected string getTab() { return xmlDoc.node_val(_defNode, "tab"); }

    protected bool tabVisible(string tab) {
      return tab == "" ? true : evalIfs(page.pageDoc.node("page/contents/tabs[@name='"
        + tab.Substring(0, tab.IndexOf(".")) + "']/tab[@name='" + tab.Substring(tab.IndexOf(".") + 1) + "']"));
    }

    protected System.Web.UI.HtmlControls.HtmlControl parentOnAdd() {
      string tab = getTab();
      return !tabVisible(tab) ? null : tab != "" ? ((deeper.frmwrk.ctrls.tabs_ctrl)_cls.control(
          tab.Substring(0, tab.IndexOf(".")))).findTab(tab.Substring(tab.IndexOf(".") + 1)) : _cls.mainForm();
    }

    protected bool evalIfs(XmlNode node) {
      return !(node.Attributes["if"] != null && !_cls.page.eval_cond_id(_name, node.Attributes["if"].Value));
    }

    #endregion

    #region formats

    static public string dbValueToText(object dataValue) {
      string result = "";
      if (dataValue == DBNull.Value)
        return result;

      result = dataValue.ToString();

      return result;
    }

    static public bool dbValueToCheck(object dataValue) {
      bool result = true;
      if (dataValue != DBNull.Value) {
        if (dataValue.ToString().Trim() == "" || dataValue.ToString() == "0" || dataValue.ToString().ToLower() == "false"
            || dataValue.ToString().ToLower() == "falso" || dataValue.ToString().ToLower() == "no") {
          result = false;
        }
      }
      else
        result = false;

      return result;
    }

    static public string dbValueToDate(object dataValue, string formatString = "") {
      string result = "";
      if (dataValue == DBNull.Value)
        return result;

      try {
        string value = dataValue.ToString();
        DateTime dateValue = DateTime.Parse(value);

        if (formatString != "")
          result = dateValue.ToString(formatString);
        else
          result = dateValue.ToString("yyyy/MM/dd");
      }
      catch { }

      return result;
    }

    static public string dbValueToInt(object dataValue, string format = "") {
      string result = "";
      if (dataValue == DBNull.Value) return result;

      try {
        int value = Convert.ToInt32(dataValue.ToString());
        result = format != "" ? value.ToString(format) : value.ToString();
      }
      catch { result = "0"; }

      return result;
    }

    static public string dbValueToEuro(object value) {
      return value == DBNull.Value ? "" : Convert.ToDecimal(value).ToString("c", new CultureInfo("it-IT"));
    }

    static public string dbValueToMigliaia(object value) {
      return value == DBNull.Value ? "" : ((int)Convert.ToDecimal(value)).ToString("#,###");
    }

    static public double euroToDouble(string value) {
      double result = 0;
      if (value == "")
        return result;

      return Double.Parse(value, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
    }

    static public bool isEuro(string value) {
      double tmp = 0;

      return Double.TryParse(value, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.GetCultureInfo("it-IT"), out tmp);
    }

    static public bool isMigliaia(string value) {
      double tmp = 0;

      return Double.TryParse(value, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.GetCultureInfo("it-IT"), out tmp);
    }

    static public bool isDouble(string value) {
      double tmp = 0;

      return Double.TryParse(value, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.GetCultureInfo("it-IT"), out tmp);
    }

    #endregion

    #region data access

    protected string valueFromDataRow(DataRow dr, string field) { try { return dr[field].ToString(); } catch { return ""; } }

    protected object listFromXmlScript(string loadScript, out int count) {
      count = -1;

      object listNodes = _cls.page.parser.invoke(_cls.page.code_nodes_id(_name, loadScript));
      if (listNodes == null) return null;

      if (listNodes.GetType().ToString() == "System.Xml.XPathNodeList")
        count = ((System.Xml.XmlNodeList)listNodes).Count;
      else if (listNodes.GetType().ToString() == "System.Collections.Generic.List`1[System.Xml.XmlNode]")
        count = ((List<XmlNode>)listNodes).Count;
      else
        throw new Exception("il tipo di oggetto ottenuto dallo script C# '" + loadScript + "' non è corretto!");

      return listNodes;
    }

    protected XmlNode indexNode(object nodes, int i) {
      return nodes.GetType().ToString() == "System.Xml.XPathNodeList" ? ((System.Xml.XmlNodeList)nodes)[i]
        : (nodes.GetType().ToString() == "System.Collections.Generic.List`1[System.Xml.XmlNode]" ? ((List<XmlNode>)nodes)[i] : null);
    }

    #endregion
  }
}

