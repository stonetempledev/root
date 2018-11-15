using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Text.RegularExpressions;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.db
{
  public class schema_doc
  {
    protected xmlDoc _doc = null;
    public xmlDoc doc { get { return _doc; } }
    public bool loaded { get { return _doc.loaded; } }
    public string path { get { return _doc.path; } }

    public schema_doc() { _doc = new xmlDoc(); }
    public schema_doc(string path) { _doc = new xmlDoc(path); }
    public schema_doc(XmlDocument doc) { _doc = new xmlDoc(doc); }
    public schema_doc(xmlDoc doc) { _doc = new xmlDoc(doc.doc, doc.path); }

    public schema_field create_schema_field(string table, string field, fieldType fld_tp) { return new_schema_field(add_field(table, field, fld_tp)); }
    public schema_field new_schema_field(string table, string field) { return new_schema_field(field_node(table, field)); }
    public schema_field new_schema_field(XmlNode node, string attr_name = "") {
      return new schema_field(db_type(), node.Attributes["name"].Value, node.Attributes["type"].Value, xmlDoc.node_bool(node, "nullable")
          , node.Attributes["maxlength"] != null ? int.Parse(node.Attributes["maxlength"].Value) : (int?)null
          , node.Attributes["numprec"] != null ? int.Parse(node.Attributes["numprec"].Value) : (int?)null
          , node.Attributes["numscale"] != null ? int.Parse(node.Attributes["numscale"].Value) : (int?)null
          , xmlDoc.node_val(node, "default"), xmlDoc.node_bool(node, "autonumber")
          , node.SelectSingleNode("../../indexes/index[@primary='true']/fields/field[@nameupper='" + node.Attributes["name"].Value.ToUpper() + "']") != null, 0, attr_name);
    }
    public static XmlNode create_field_node(XmlDocument doc, schema_field fld) {
      return xmlDoc.set_attrs(doc.CreateElement("col"), new Dictionary<string, string>() { { "name", fld.Name }, { "nameupper", fld.Name.ToUpper() }
            , { "type", fld.OriginalType }, { "autonumber", fld.AutoNumber ? "true" : "" }
            , { "nullable", fld.Nullable ? "true" : "" }, { "default", fld.Default }
            , { "maxlength", fld.MaxLength.HasValue ? fld.MaxLength.Value.ToString() : "" }
            , { "numprec", fld.NumPrecision.HasValue ? fld.NumPrecision.Value.ToString() : "" }
            , { "numscale", fld.NumScale.HasValue ? fld.NumScale.Value.ToString() : "" }});
    }
    public XmlNode create_field_node(schema_field fld) { return create_field_node(_doc.doc, fld); }
    protected XmlNode add_field(string table, string fld, fieldType type) {
      return _doc.add_xml("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols"
        , "<col name='" + fld + "' nameupper='" + fld.ToUpper() + "' type='" + schema_field.typeToOriginal(db_type(), type) + "' />");
    }
    public XmlNode set_field(string table, schema_field new_field) {
      XmlNode col_node = field_node(table, new_field.Name);
      if (col_node == null)
        return xmlDoc.add_node(table_node(table).SelectSingleNode("cols"), create_field_node(new_field));

      XmlNode res = xmlDoc.add_node(table_node(table).SelectSingleNode("cols"), create_field_node(new_field), col_node);
      col_node.ParentNode.RemoveChild(col_node);
      return res;
    }

    public dbType db_type() { return (dbType)Enum.Parse(typeof(dbType), _doc.root_value("type")); }
    public string ver { get { return _doc.root_value("ver"); } }
    public string group { get { return _doc.root_value("group"); } }
    public string title { get { return _doc.root_value("title"); } }

    public List<schema_field> table_fields(string table) { return tableFields(_doc.nodes("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols/col")); }
    public List<schema_field> table_fields(string table, string name_field) { return tableFields(_doc.nodes("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols/col"), name_field); }
    public schema_field table_field(string table, string field) { return tableFields(_doc.nodes("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols/col"), field)[0]; }
    public catField cat_field(string table, string field) { return db_provider.cat_field(table_field(table, field).TypeField); }
    public string def_toqry(string table, string field) { return cat_field(table, field) == catField.TEXT ? "''" : "0"; }
    public List<schema_field> tableFields(XmlNode tableNode) { return tableFields(tableNode.SelectNodes("cols/col")); }
    protected List<schema_field> tableFields(XmlNodeList cols) { return new List<schema_field>(cols.Cast<XmlNode>().Select(node => new_schema_field(node))); }
    protected List<schema_field> tableFields(XmlNodeList cols, string field) {
      XmlNode col = cols.Cast<XmlNode>().FirstOrDefault(x => xmlDoc.node_val(x, "nameupper") == field.ToUpper());
      return col == null ? new List<schema_field>() : new List<schema_field>() { new_schema_field(col) };
    }

    public string table_name(string table) { return _doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "name"); }
    public XmlNode table_node(string table) { return _doc.node("/root/tables/table[@nameupper='" + table.ToUpper() + "']"); }
    public XmlNode field_node(string table, string field) {
      return _doc.node("/root/tables/table[@nameupper='" + table.ToUpper()
        + "']/cols/col[@nameupper='" + field.ToUpper() + "']");
    }

    public string[] fields_name(string table, bool without_pk = false) {
      string pk_field = pkOfTable(table);
      return _doc.nodes("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols/col")
        .Cast<XmlNode>().Where(x => without_pk ? x.Attributes["nameupper"].Value != pk_field.ToUpper() : true).Select(node => node.Attributes["name"].Value).ToArray();
    }

    public string[] tables_name(string like = "") {
      Regex reg = like != "" ? strings.reg_like(like) : null;
      return reg == null ? _doc.nodes("/root/tables/table").Cast<XmlNode>().Select(node => node.Attributes["name"].Value).ToArray()
          : _doc.nodes("/root/tables/table").Cast<XmlNode>().Where(node => reg.IsMatch(node.Attributes["name"].Value))
              .Select(node => node.Attributes["name"].Value).ToArray();
    }

    public string[] functions_name(string like = "") {
      Regex reg = like != "" ? strings.reg_like(like) : null;
      return reg == null ? _doc.nodes("/root/functions/function").Cast<XmlNode>().Select(node => node.Attributes["name"].Value).ToArray()
          : _doc.nodes("/root/functions/function").Cast<XmlNode>().Where(node => reg.IsMatch(node.Attributes["name"].Value))
              .Select(node => node.Attributes["name"].Value).ToArray();
    }

    public string function_content(string name) { return _doc.get_value("/root/functions/function[@name='" + name + "']"); }

    public string sp_content(string name) { return _doc.get_value("/root/procedures/procedure[@name='" + name + "']"); }

    public string[] sps_name(string like = "") {
      Regex reg = like != "" ? strings.reg_like(like) : null;
      return reg == null ? _doc.nodes("/root/procedures/procedure").Cast<XmlNode>().Select(node => node.Attributes["name"].Value).ToArray()
          : _doc.nodes("/root/procedures/procedure").Cast<XmlNode>().Where(node => reg.IsMatch(node.Attributes["name"].Value))
              .Select(node => node.Attributes["name"].Value).ToArray();
    }

    static protected idx_table index(string table, XmlNode node) {
      int i = 0;
      return node == null ? null : new idx_table(table, node.Attributes["name"].Value, xmlDoc.node_bool(node, "clustered")
          , xmlDoc.node_bool(node, "unique"), xmlDoc.node_bool(node, "primary")
          , node.SelectNodes("fields/field").Cast<XmlNode>().Select(x => new idx_field(x.Attributes["name"].Value, bool.Parse(x.Attributes["ascending"].Value), i++)));
    }

    static public List<idx_table> table_indexes(XmlNode tableNode) {
      return new List<idx_table>(tableNode.SelectNodes("indexes/index").Cast<XmlNode>()
        .Select(node => index(node.Attributes["name"].Value, node)));
    }

    public bool there_pk(string table) { return _doc.exist("/root/tables/table[@nameupper='" + table.ToUpper() + "']/indexes/index[@primary='true']/fields/field"); }

    public string pkOfTable(string table, bool throwErr = true) {
      return _doc.get_value_throw("/root/tables/table[@nameupper='" + table.ToUpper() + "']"
          + "/indexes/index[@primary='true']/fields/field", "nameupper", throwErr ? "la tabella '" + table + "' non ha specificato l'indice primario!" : "");
    }

    public string tableFromPk(string pkey) {
      return _doc.get_value_throw("/root/tables/table[indexes/index[@primary='true']/fields/field[@nameupper='" + pkey.ToUpper() + "']]"
          , "name", "non è possibile risalire alla tabella da gestire!");
    }

    public bool existCol(string table, string field) { return _doc.exist("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols/col[@nameupper='" + field.ToUpper() + "']"); }

    public bool existTable(string table) { return _doc.exist("/root/tables/table[@nameupper='" + table.ToUpper() + "']"); }

    public bool existFunction(string name) { return _doc.exist("/root/functions/function[@name='" + name + "']"); }

    public bool existSP(string name) { return _doc.exist("/root/procedures/procedure[@name='" + name + "']"); }

    public idx_table indexPrimary(string table) { return index(table, _doc.node("/root/tables/table[@nameupper='" + table.ToUpper() + "']/indexes/index[@primary='true']")); }

    public bool isFieldNullable(string table, string field) { return _doc.get_bool("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols/col[@nameupper='" + field.ToUpper() + "']", "nullable"); }

    public List<idx_table> table_indexes(string table, bool? uniques = null) {
      return new List<idx_table>(_doc.nodes("/root/tables/table[@nameupper='" + table.ToUpper() + "']/indexes/index")
        .Cast<XmlNode>().Select(node => index(table, node)).Where(idx => idx_table.filter_unique(uniques, idx.Unique, idx.Primary)));
    }

    public XmlNode add_idx(string table, idx_table idx) {
      return xmlDoc.add_node(xmlDoc.add_node(table_node(table), "indexes"), create_idx_node(idx));
    }

    public XmlNode create_idx_node(idx_table idx) { return create_idx_node(_doc, idx); }

    static public XmlNode create_idx_node(xmlDoc doc, idx_table idx) {
      XmlNode idx_node = doc.doc.CreateElement("index");
      idx_node.Attributes.Append(doc.doc.CreateAttribute("name")).Value = idx.Name;
      idx_node.Attributes.Append(doc.doc.CreateAttribute("clustered")).Value = idx.Clustered.ToString().ToLower();
      idx_node.Attributes.Append(doc.doc.CreateAttribute("unique")).Value = idx.Unique.ToString().ToLower();
      idx_node.Attributes.Append(doc.doc.CreateAttribute("primary")).Value = idx.Primary.ToString().ToLower();

      XmlNode fieldsNode = idx_node.AppendChild(doc.doc.CreateElement("fields"));
      foreach (idx_field field in idx.Fields) {
        XmlNode fieldNode = fieldsNode.AppendChild(doc.doc.CreateElement("field"));
        fieldNode.Attributes.Append(doc.doc.CreateAttribute("name")).Value = field.Name;
        fieldNode.Attributes.Append(doc.doc.CreateAttribute("nameupper")).Value = field.Name.ToUpper();
        fieldNode.Attributes.Append(doc.doc.CreateAttribute("ascending")).Value = field.Ascending.ToString().ToLower();
      }

      return idx_node;
    }

    public void create_history_table(string table, string h_table, string fld_del = null, string fld_ref = null) {
      // elimino la tabella se già presente
      _doc.remove("/root/tables/table[@nameupper='" + h_table.ToUpper() + "']");

      // copia dello schema senza gli indici
      XmlNode cols = _doc.add_xml("/root/tables", "<table nameupper='" + h_table.ToUpper() + "' name='" + h_table + "'/>")
          .AppendChild(_doc.node("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols").CloneNode(true));

      // inizializzo le colonne 
      foreach (XmlNode col in cols.ChildNodes) xmlDoc.set_attr(col, "nullable", "true");
      if (!string.IsNullOrEmpty(fld_del)) xmlDoc.add_xml(cols, "<col name='" + fld_del + "' nameupper='" + fld_del.ToUpper() + "' type='datetime' />");
      if (!string.IsNullOrEmpty(fld_ref)) xmlDoc.add_xml(cols, "<col name='" + fld_ref + "' nameupper='" + fld_ref.ToUpper() + "' type='datetime' />");
      xmlDoc.set_attrs(cols.SelectSingleNode("col[@autonumber='true']")
        , new Dictionary<string, string>() { { "nullable", "" }, { "autonumber", "" } });
    }

    public bool table_autonumber(string table) {
      return _doc.exist("/root/tables/table[@nameupper='" + table.ToUpper() + "']/cols/col[@autonumber='true']");
    }

    public void save() { _doc.save(); }

    public string qry_fields(string table, bool without_pk = false) {
      return string.Join(", ", fields_name(table, without_pk).Select(x => "[" + x + "]"));
    }

  }
}