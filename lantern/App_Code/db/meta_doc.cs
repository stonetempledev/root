using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.db
{
  public class meta_doc {
    public enum table_type { none, data, storico, temporanea, sistema }
    public enum col_type { primary, diretta, linked, service, info }

    protected xmlDoc _doc = null;
    public xmlDoc doc { get { return _doc; } }
    public bool loaded { get { return _doc.loaded; } }
    public string path { get { return _doc.path; } }

    protected schema_doc _schema = null;

    public meta_doc(string path, schema_doc sch) { _doc = new xmlDoc(path); _schema = sch; }
    public meta_doc(XmlDocument doc, schema_doc sch) { _doc = new xmlDoc(doc); _schema = sch; }

    public void save(string path) { _doc.doc.Save(path); }

    public string ver { get { return _doc.root_value("ver"); } }

    // valori opzionali - per gestire tutti i tipi di db
    public string prefix_del() { return prefix("prefix_del"); }
    public string prefix_sys() { return prefix("prefix_sys"); }
    public string prefix_tmp() { return prefix("prefix_tmp"); }
    public string field_ins() { return prefix("fieldins"); }
    public string field_upd() { return prefix("fieldupd"); }
    public string field_del() { return prefix("fielddel"); }
    public string field_ref() { return prefix("fieldref"); }
    public bool uidx_onins() { return _doc.get_bool("/root/tables", "uidx_onins"); }
    public string test_filter() { return _doc.get_value("/root/tables", "test_filter"); }
    public bool test_without_linked() { return _doc.get_bool("/root/tables", "test_without_linked"); }
    
    protected string prefix(string attr_name) { return _doc.exist("/root/tables", attr_name) ? _doc.get_value("/root/tables", attr_name) : null; }

    public bool sysTable(string table) { return _doc.exist("/root/tables", "prefix_sys") && strings.like(table, _doc.get_value("/root/tables", "prefix_sys") + "*"); }

    public string titleCol(string col) { return _doc.get_value("/root/fields/field[@nameupper='" + col.ToUpper() + "']", "title", col); }

    public string functionIds(string table, string field) {
      return _doc.get_value_throw("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "fnc"
          , "la tabella collegata '" + table + "' di tipo list non specificato la funzione sql necessaria.");
    }

    public Dictionary<string, Dictionary<string, string>> tableCols(string table, List<col_type> types, bool or = true) {
      // fields
      List<string> ifields = _doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']"
          , "fields-info").ToLower().Split(',').ToList<string>();

      // ciclo colonne
      Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
      foreach (schema_field fld in _schema.table_fields(table)) {
        Dictionary<string, string> attrs = new Dictionary<string, string>() { { "indice", (result.Count + 1).ToString() }
                    , { "type", fld.OriginalType }, { "primary", fld.Primary.ToString().ToLower() }, { "linked", "" }
                    , { "linkedtable", "" }, { "linkedtype", "" }, { "service", "" }
                    , { "info", ifields.Contains(fld.Name.ToLower()).ToString().ToLower() }};
        attrs["service"] = (_doc.get_value("/root/tables", "fieldins").ToUpper() == fld.Name.ToUpper()
            || _doc.get_value("/root/tables", "fieldupd").ToUpper() == fld.Name.ToUpper()
            || _doc.get_value("/root/tables", "fielddel").ToUpper() == fld.Name.ToUpper()
            || _doc.get_value("/root/tables", "fieldref").ToUpper() == fld.Name.ToUpper()).ToString().ToLower();
        attrs["linked"] = _doc.exist("/root/tables/table[@nameupper='" + table.ToUpper() + "']/links/link[@fieldupper='" + fld.Name.ToUpper() + "']").ToString().ToLower();
        attrs["linkedtable"] = _doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']/links/link[@fieldupper='" + fld.Name.ToUpper() + "']", "tableupper");
        attrs["linkedtype"] = _doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']/links/link[@fieldupper='" + fld.Name.ToUpper() + "']", "type");
        attrs["diretta"] = (!bool.Parse(attrs["primary"]) && !bool.Parse(attrs["linked"]) && !bool.Parse(attrs["service"])).ToString();

        bool add = or && types != null && types.Count > 0 ? false : true;
        foreach (col_type tp in types)
          if (or && attrs[tp.ToString()] != null && bool.Parse(attrs[tp.ToString()])) { add = true; break; } 
          else if (!or && attrs[tp.ToString()] != null && !bool.Parse(attrs[tp.ToString()])) { add = false; break; }

        if (add) result.Add(fld.Name, attrs);
      }

      return result;
    }

    public table_type type_table(string table) { string table_ref = ""; return type_table(table, out table_ref); }

    public table_type type_table(string table, out string table_ref) {
      table_type res = is_table(table, "prefix_del") ? table_type.storico
          : is_table(table, "prefix_tmp") ? table_type.temporanea
          : is_table(table, "prefix_sys") ? table_type.sistema : table_type.data;

      table_ref = res == table_type.storico ? table.Substring(_doc.get_value("/root/tables", "prefix_del").Length)
        : res == table_type.temporanea ? table.Substring(_doc.get_value("/root/tables", "prefix_tmp").Length)
        : res == table_type.sistema ? table.Substring(_doc.get_value("/root/tables", "prefix_sys").Length) : "";

      return res;
    }

    protected bool is_table(string table, string attr_prefix) { return _doc.exist("/root/tables", attr_prefix) && strings.like(table, _doc.get_value("/root/tables", attr_prefix) + "*"); }

    protected meta_link link(XmlNode node) {
      return new meta_link(node.ParentNode.ParentNode.Attributes["nameupper"].Value, node.Attributes["tableupper"].Value
        , node.ParentNode.ParentNode.Attributes["title"].Value, node.Attributes["fieldupper"].Value, xmlDoc.node_val(node, "type")
        , xmlDoc.node_bool(node, "basic"), node);
    }

    public bool idlist_field_link(string table, string field) {
      return links_table(table).FirstOrDefault(y => y.field.ToLower() == field.ToLower() && y.table_link == "list") != null;
    }

    public meta_table.align_codes table_align_code(string table) {
      return meta_table.get_align_code(_doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "align_code"));
    }

    public string table_title(string table) { return _doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "title"); }

    public string table_from_code(meta_table.align_codes rec_code) { return _doc.get_value("/root/tables/table[@align_code='" + rec_code.ToString() + "']", "nameupper"); }

    public IEnumerable<meta_link> table_links(string table) { return _doc.nodes("/root/tables/table/links//link[@tableupper='" + table.ToUpper() + "']").Cast<XmlNode>().Select(x => link(x)); }

    public IEnumerable<meta_link> links_table(string table) { return _doc.nodes("/root/tables/table[@nameupper='" + table.ToUpper() + "']/links/link").Cast<XmlNode>().Select(x => link(x)); }

    public meta_table meta_tbl(string table) { return table_from_node(table, _doc.node("/root/tables/table[@nameupper='" + table.ToUpper() + "']")); }

    public bool enum_tbl(string table) { return _doc.get_bool("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "enum"); }

    protected meta_table table_from_node(string table, XmlNode node) {
      return node != null ? new meta_table(xmlDoc.node_val(node, "nameupper"), xmlDoc.node_val(node, "title")
          , xmlDoc.node_val(node, "single"), xmlDoc.node_val(node, "fields-info"), xmlDoc.node_val(node, "row_title_field")
          , xmlDoc.node_val(node, "row_notes_field"), type_table(xmlDoc.node_val(node, "nameupper")), node
          , node != null ? node.SelectNodes("rules/nochar").Cast<XmlNode>().Select(x => new meta_rule(x.Name, x.Attributes["fieldupper"].Value, x.Attributes["value"].Value)) : null
          , node != null ? node.SelectNodes("links/link").Cast<XmlNode>().Select(x => link(x)) : null, xmlDoc.node_val(node, "align_code"), xmlDoc.node_bool(node, "enum")) :
          new meta_table(table, table, table, "", table, "", type_table(table), node);
    }

    public idx_table indexUnique(string table) {
      return _schema.table_indexes(table, true).FirstOrDefault(
        x => x.Fields.Count > 1 || (x.Fields.Count == 1 && field_ins() != null && x.Fields[0].Name.ToLower() != field_ins().ToLower())
        || (x.Fields.Count == 1 && field_ins() == null));
    }

    public idx_table indexUniqueOnIns(string table) {
      return _schema.table_indexes(table, true).FirstOrDefault(
        x => x.Fields.Count == 1 && field_ins() != null && x.Fields[0].Name.ToLower() == field_ins().ToLower());
    }
  }
}