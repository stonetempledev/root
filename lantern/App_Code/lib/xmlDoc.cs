using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Linq;

namespace deeper.lib
{
  // xmlDocs
  public class xmlDocs
  {
    protected Dictionary<string, xmlDoc> _docs = null;

    public xmlDocs() {
      _docs = new Dictionary<string, xmlDoc>();
    }

    public xmlDoc add(string key, xmlDoc doc) {
      _docs.Add(key, doc);

      return doc;
    }

    public bool contains(string key) { return _docs.ContainsKey(key); }
    public xmlDoc this[string key] {
      get { return _docs[key]; }
      set { _docs[key] = value; }
    }
  }

  // xmlDoc
  public class xmlDoc
  {
    protected XmlDocument _doc = null;
    public XmlDocument doc { get { load(); return _doc; } }

    protected string _path = "";
    public string path { get { return _path; } }

    public bool loaded { get { return _doc != null; } }

    public xmlDoc() { }

    public xmlDoc(string path) { _path = path; }

    public xmlDoc(XmlDocument doc) { _doc = doc; }

    public xmlDoc(XmlDocument doc, string path) { _doc = doc; _path = path; }

    public xmlDoc(System.IO.Stream str) { _doc = new XmlDocument(); _doc.Load(str); }

    public static xmlDoc create_fromxml(string xml) {
      xmlDoc res = new xmlDoc();

      res.load_xml(xml);

      return res;
    }

    public void load_xml(string xml) {
      if (_doc == null)
        _doc = new XmlDocument();

      _doc.LoadXml(xml);
    }

    public void save() {
      if (_doc == null)
        return;

      _doc.Save(_path);
    }

    protected void load() {
      if (_doc != null)
        return;

      _doc = new XmlDocument();
      _doc.Load(_path);
    }

    static public xmlDoc doc_from_xml(string xml) {
      xmlDoc doc = new xmlDoc();
      doc.load_xml(xml);
      return doc;
    }

    #region nodes

    public XmlNode root_node {
      get {
        load();

        return _doc.DocumentElement;
      }
    }

    public bool exist(string xpath, string attr = "") {
      load();

      if (_doc.SelectSingleNode(xpath) == null)
        return false;

      if (attr != "" && _doc.SelectSingleNode(xpath).Attributes[attr] == null)
        return false;

      return true;
    }

    public void set_attr(string xpath, string attr, string value, bool optional = true) {
      load();

      set_attr(_doc, xpath, attr, value, optional);
    }

    public void set_root_attr(string attr, string value, bool optional = true) {
      load();

      set_attr(_doc, "/*", attr, value, optional);
    }

    public static XmlNode set_attrs(XmlNode node, Dictionary<string, string> attrs) { foreach (KeyValuePair<string, string> attr in attrs) set_attr(node, attr.Key, attr.Value); return node; }

    public static XmlNode set_attr(XmlDocument doc, string xpath, string attr, string value, bool optional = true) { return set_attr(doc.SelectSingleNode(xpath), attr, value, optional); }

    public static XmlNode set_attr(XmlNode node, string attr, string value, bool optional = true) {
      if (node == null || (node != null && string.IsNullOrEmpty(value) && optional && node.Attributes[attr] == null)) return node;
      else if (string.IsNullOrEmpty(value) && optional && node.Attributes[attr] != null) {
        node.Attributes.RemoveNamedItem(attr);
        return node;
      }

      if (node.Attributes[attr] == null) node.Attributes.Append(node.OwnerDocument.CreateAttribute(attr));

      node.Attributes[attr].Value = value;

      return node;
    }

    public string root_value(string attr, string defValue = "") {
      load();

      return node_val(root_node, attr, defValue);
    }

    public bool root_bool(string attr, bool defValue = false) {
      load();

      return node_bool(root_node, attr, defValue);
    }

    public int root_int(string attr, int defValue = -1) {
      load();

      return node_int(root_node, attr, defValue);
    }

    public string get_value(string xpath, string attr = "", string defValue = "") { return get_value2(xpath, attr, defValue); }

    public string get_value_throw(string xpath, string attr = "", string throwErr = "") { return get_value2(xpath, attr, "", throwErr); }

    protected string get_value2(string xpath, string attr = "", string defValue = "", string throwErr = "") {
      load();

      return node_val(_doc.SelectSingleNode(attr == "" ? xpath : xpath + "[@" + attr + "]"), attr, defValue, throwErr);
    }

    public int get_int(string xpath, string attr = "", int defValue = 0) {
      load();

      return node_int(node(attr == "" ? xpath : xpath + "[@" + attr + "]"), attr, defValue);
    }

    public bool get_bool(string xpath, string attr = "", bool defValue = false) {
      load();

      return node_bool(node(attr == "" ? xpath : xpath + "[@" + attr + "]"), attr, defValue);
    }

    public bool remove(string xpath) {
      XmlNode nd = node(xpath);
      if (nd == null) return false;

      nd.ParentNode.RemoveChild(nd);

      return true;
    }

    public XmlNode add_node(string xpath, string name) {
      XmlNode nd = node(xpath);
      return nd == null ? null : add_node(nd, name);
    }

    public XmlNode add_after(string xpath, XmlNode node_add, string xpath_after) {
      XmlNode nd = node(xpath);
      return nd == null ? null : add_node(nd, node_add, node(xpath_after));
    }

    public XmlNode add_before(string xpath, XmlNode node_add, string xpath_before) {
      XmlNode nd = node(xpath);
      return nd == null ? null : add_node(nd, node_add, null, node(xpath_before));
    }

    public XmlNode add_node(string xpath, XmlNode node_add) {
      XmlNode nd = node(xpath);
      return nd == null ? null : add_node(nd, node_add);
    }

    public XmlNode add_node(string name) { return add_node(root_node, name); }

    public XmlNode add_xml(string xpath, string xml) {
      XmlNode nd = node(xpath);
      return nd == null ? null : add_xml(nd, xml);
    }

    static public XmlNode add_node(XmlNode node, string name) {
      return node.AppendChild(node.OwnerDocument.CreateElement(name));
    }

    static public XmlNode add_node(XmlNode node, string name, Dictionary<string, string> attrs) {
      return set_attrs(node.AppendChild(node.OwnerDocument.CreateElement(name)), attrs);
    }

    static public XmlNode add_node(XmlNode node, XmlNode node_add, XmlNode node_after = null, XmlNode node_before = null) {
      return node_after != null ? node.InsertAfter(node_add, node_after) :
        node_before != null ? node.InsertBefore(node_add, node_before) : node.AppendChild(node_add);
    }

    static public XmlNode add_xml(XmlNode node, string xml) {
      return node.AppendChild(node.OwnerDocument.ImportNode(
          xmlDoc.doc_from_xml(xml).root_node, true));
    }

    public XmlNode root_add(string name) {
      XmlNode nd = root_node;
      return nd == null ? null :
        nd.SelectSingleNode(name) != null ? nd.SelectSingleNode(name) 
        : nd.AppendChild(nd.OwnerDocument.CreateElement(name));
    }

    public XmlNode node(string xpath, bool create = false) {
      load();

      XmlNode result = _doc.SelectSingleNode(xpath);
      if (result == null && create) {
        string[] xparts = xmlDoc.xpathParts(xpath);
        XmlNode parent = null;
        string tmpPath = "";
        foreach (string xpart in xparts) {
          string namePart = xmlDoc.nameElement(xpart);
          tmpPath += "/" + namePart;
          result = _doc.SelectSingleNode(tmpPath);
          if (result == null) {
            if (parent == null) {
              _doc.LoadXml("<" + namePart + "/>");
              result = _doc.DocumentElement;
            }
            else result = parent.AppendChild(_doc.CreateElement(namePart));
          }

          parent = result;
        }
      }

      return result;
    }

    public int count(string xpath) {
      return nodes(xpath).Count;
    }

    public XmlNodeList nodes(string xpath) {
      load();

      return _doc.SelectNodes(xpath);
    }

    public List<XmlNode> toList(string xpath) { return xmlDoc.toList(nodes(xpath)); }

    static public string node_val_x(XmlNode node, string xpath, string attr = "", string defValue = "", string throwErr = "") {
      return node_val(node.SelectSingleNode(xpath), attr, defValue, throwErr);
    }

    static public bool node_bool_x(XmlNode node, string xpath, string attr = "", bool defValue = false) {
      return node_bool(node.SelectSingleNode(xpath), attr, defValue);
    }

    static public string node_val(XmlNode node, string attr = "", string defValue = "", string throwErr = "") {
      if (throwErr != "" && (node == null || (attr != "" && node.Attributes[attr] == null)))
        throw new Exception(throwErr);

      return (node != null && node.Attributes[attr] != null) ? node.Attributes[attr].Value
          : (node != null && attr == "") ? node.InnerText : defValue;
    }

    static public bool node_bool(XmlNode node, string attr = "", bool defValue = false/*, parser toparse = null*/) { return bool.Parse(node_val(node, attr, defValue.ToString())); }

    static public int node_int(XmlNode node, string attr = "", int defValue = -1/*, parser toparse = null*/) { return int.Parse(node_val(node, attr, defValue.ToString())); }

    static public List<XmlNode> toList(XmlNodeList nodes) { return nodes.Cast<XmlNode>().ToList(); }

    #endregion

    #region xpath

    static public string[] xpathParts(string xpath) {
      if (xpath.Substring(0, 1) != "/")
        throw new Exception("il percorso '" + xpath + "' non può essere valutato");

      return xpath.Substring(1).Split('/');
    }

    static public string nameElement(string xpathPart) {
      string name = xpathPart;
      if (name.IndexOf('[') > 0)
        name = name.Substring(0, name.IndexOf('['));

      return name;
    }

    #endregion
  }
}
