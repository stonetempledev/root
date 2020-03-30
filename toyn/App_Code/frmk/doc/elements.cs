﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib;
using mlib.db;
using mlib.tools;

namespace toyn {
  public class elements {
    public db_provider db_conn { get; protected set; }
    public core core { get; protected set; }
    public config config { get; protected set; }
    public int user_id { get; protected set; }

    public elements(db_provider db, core c, config cfg, int user_id) {
      this.db_conn = db; this.core = c; this.config = cfg; this.user_id = user_id;
    }

    public List<attribute> load_attributes() {
      List<attribute> attrs = new List<attribute>();
      foreach (DataRow dr in db_conn.dt_table(core.parse(config.get_query("elements.elements-attributes").text)).Rows)
        attrs.Add(create_attribute(dr));
      return attrs;
    }

    public attribute create_attribute(DataRow dr) {
      string etype = db_provider.str_val(dr["element_type"]), acode = db_provider.str_val(dr["attribute_code"])
        , atype = db_provider.str_val(dr["attribute_type"]);
      int aid = db_provider.int_val(dr["attribute_id"]);
      bool content_txt_xml = db_provider.int_val(dr["content_txt_xml"]) == 1;

      return new attribute((element.type_element)Enum.Parse(typeof(element.type_element), etype), aid, acode
        , (attribute.attribute_type)Enum.Parse(typeof(attribute.attribute_type), atype), content_txt_xml);
    }

    public void set_attribute(element e, attribute a, string qry_val) {
      db_conn.exec(core.parse(config.get_query("elements.set-attribute").text
        , new Dictionary<string, object>() { { "attr_type", a.type.ToString() }, { "attr_value", qry_val }
        , { "id_element", e.id }, { "attr_code", a.code }, { "element_type", e.type.ToString() } }));
    }

    public attribute load_attribute(element.type_element te, string code) {
      DataRow dr = db_conn.first_row(core.parse(config.get_query("elements.element-attribute").text
        , new Dictionary<string, object>() { { "type", te.ToString() }, { "code", code } }));
      return create_attribute(dr);
    }

    public List<element> load_childs(long element_id, bool also_deleted = false) {
      int max_lvl_els = 0, max_lvl = 0;
      return load_elements(out max_lvl_els, out max_lvl, element_id, also_deleted: also_deleted, only_childs: true);
    }

    public element load_element(long element_id) {
      int max_lvl_els = 0, max_lvl = 0;
      List<element> els = load_elements(out max_lvl_els, out max_lvl, element_id, childs: false);
      return els.Count > 0 ? els[0] : null;
    }

    public List<element> load_elements(out int out_max_lvl_els, out int out_max_lvl, long? element_id = null, int? max_level = null, bool only_childs = false, bool childs = true, bool also_deleted = false, string key_page = null) {

      List<element> els = new List<element>(); out_max_lvl_els = -1; out_max_lvl = -1;
      string sql = !element_id.HasValue ? core.parse(config.get_query("elements.open-roots-elements").text
          , new Dictionary<string, object>() { { "filter_level", max_level.HasValue ? "(h.in_list = 1 or (h.in_list = 0 and h.livello <= " + (max_level.Value + 1).ToString() + "))" : "1 = 1" }
         , { "id_utente", this.user_id }, { "filter_deleted", !also_deleted ? "isnull(h.deleted, 0) = 0" : "1 = 1" } })
        : core.parse(config.get_query("elements.open-elements").text
          , new Dictionary<string, object>() { { "id_element", element_id }
          , { "filter_head", only_childs ? "and 1 = 0" : "" }, { "filter_childs", !childs ? "and 1 = 0" : "" }
          , { "filter_level", max_level.HasValue ? "(h.in_list = 1 or (h.in_list = 0 and h.livello <= " + (max_level.Value + 1).ToString() + "))" : "1 = 1" }
          , { "id_utente", this.user_id }, { "filter_deleted", !also_deleted ? "isnull(h.deleted, 0) = 0" : "1 = 1" } });
      DataTable dt = db_conn.dt_table(sql);
      foreach (DataRow re in dt.Rows) {

        // element
        long parent_id = db_provider.long_val(re["parent_id"]), contents_id = db_provider.long_val(re["elements_contents_id"])
          , id = db_provider.long_val(re["element_id"]);
        int livello = db_provider.int_val(re["livello"]), order = db_provider.int_val(re["order"]);
        string element_type = db_provider.str_val(re["element_type"]), title = db_provider.str_val(re["element_title"]);
        bool has_childs = db_provider.int_val(re["has_childs"]) == 1, in_list = db_provider.int_val(re["in_list"]) == 1
          , has_child_elements = db_provider.int_val(re["has_child_elements"]) == 1;
        DateTime? dt_ins = db_provider.dt_val(re["dt_ins"]), dt_upd = db_provider.dt_val(re["dt_upd"]);

        if (out_max_lvl_els < livello && !in_list) out_max_lvl_els = livello;
        if (out_max_lvl < livello) out_max_lvl = livello;

        element e = els.FirstOrDefault(x => x.id == id);
        if (e == null) {
          e = new element(this.core, (element.type_element)Enum.Parse(typeof(element.type_element), element_type), title, livello
            , id, parent_id, contents_id, has_childs, has_child_elements, in_list, dt_ins, dt_upd, order: order
            , sham: max_level.HasValue ? livello == max_level + 1 && has_childs && !in_list : false);
          e.key_page = key_page;
          if (livello == 0 && element_id.HasValue) e.back_element_id = parent_id != 0 ? parent_id : (long?)null;
          els.Add(e);
        }

        // attribute
        string attribute_code = db_provider.str_val(re["attribute_code"])
          , attribute_type = db_provider.str_val(re["attribute_type"]);
        bool content_txt_xml = db_provider.int_val(re["content_txt_xml"]) == 1;
        if (attribute_code != "") {
          object val = re["val_" + attribute_type];
          e.set_attribute(attribute_code, (attribute.attribute_type)Enum.Parse(typeof(attribute.attribute_type), attribute_type), val, content_txt_xml);
        }
      }

      // hierarchy
      List<element> res = new List<element>(els.Where(x => x.level == 0));
      for (int l = 1; l <= out_max_lvl; l++) {
        foreach (element el in els.Where(x => x.level == l)) {
          element pe = els.FirstOrDefault(x => x.id == el.parent_id);
          pe.add_child(el);
        }
      }

      return res;
    }

    public List<element> load_xml(string xml, int? back_element_id = null) {
      return element.load_xml(this.core, load_attributes(), xml, back_element_id);
    }


    public List<element> get_all(List<element> els, List<element> l = null) {
      List<element> res = l == null ? new List<element>() : l;
      foreach (element el in els) {
        if (el.id == 0 || (el.id > 0 && res.FirstOrDefault(x => x.type == el.type && x.id == el.id) == null))
          res.Add(el);
        foreach (element c in el.childs) {
          if (c.id == 0 || (c.id > 0 && res.FirstOrDefault(x => x.id == c.id) == null))
            res.Add(c);
          get_all(new List<element>() { (element)c }, res);
        }
      }
      return res;
    }

    public void check_and_del(List<element> els_bck, List<element> els) {

      // check ids univoci
      List<element> all = get_all(els);
      foreach (element c in all) {
        if (c.id > 0 && all.Count(x => x.id == c.id) > 1)
          throw new Exception(string.Format("l'elemento con id {1} è duplicato più volte e non è possibile!"
            , c.type.ToString(), c.id));
      }

      // ciclo pulizia
      DataRow dr = db_conn.first_row(core.parse(config.get_query("elements.id-deleted").text, new Dictionary<string, object>() { { "id_utente", this.user_id } }));
      int id_deleted = db_provider.int_val(dr["id_deleted"]);
      foreach (element c in get_all(els_bck)) {
        if (all.FirstOrDefault(x => x.id == c.id) == null) {
          db_conn.exec(core.parse(config.get_query("elements.delete-element").text
            , new Dictionary<string, object>() { { "id_element", c.id }, { "id_utente", this.user_id }, { "id_deleted", id_deleted } }));
        }
      }

    }

    public void save_elements(List<element> els, string key_page = "", int parent_id = 0, int? first_order = null, int? last_order = null) {

      // correggo l'order
      if (first_order.HasValue && last_order.HasValue) {
        int diff = els.Count - ((last_order.Value - first_order.Value) + 1);
        if (diff != 0) {
          db_conn.exec(core.parse(config.get_query("elements.refresh-orders").text
            , new Dictionary<string, object>() { { "filter_id_element", parent_id > 0 ? " = " + parent_id.ToString() : " = -1" }
            , { "id_utente", this.user_id }, { "diff_order", diff }, { "filter_order", last_order.Value } }));
        }
      }

      // salvataggio elementi
      foreach (element el in els) {
        save_element(el, key_page);
        el.order = el.order_xml + (first_order.HasValue ? first_order.Value : 0);
        db_conn.exec(core.parse(config.get_query("elements.save-child").text
          , new Dictionary<string, object>() { { "id_parent", parent_id > 0 ? parent_id.ToString() : "-1" }
            , { "id_element", el.id }, { "order", el.order_xml + (first_order.HasValue ? first_order.Value : 0) } }));
      }
    }

    public long save_element(element e, string key_page = "") {

      // from_id
      e.undeleted = false;
      if (e.from_id > 0) {
        DataRow dr = db_conn.first_row(core.parse(config.get_query("elements.deleted-element").text
          , new Dictionary<string, object>() { { "id_element", e.from_id }, { "type_element", e.type.ToString() } }));
        if (dr != null) { e.id = e.from_id; e.key_page = key_page; e.undeleted = true; }
      }

      // element
      e.added = false;
      if (e.id == 0) {
        e.added = true;
        e.id = int.Parse(db_conn.exec(core.parse(config.get_query("elements.save-element").text
          , new Dictionary<string, object>() { { "id_utente", this.user_id }, { "type", e.type.ToString() }, { "title", e.title } }), true));
        e.key_page = key_page;
      } else
        db_conn.exec(core.parse(config.get_query("elements.update-element").text
          , new Dictionary<string, object>() { { "type", e.type.ToString() }, { "title", e.title }, { "id_element", e.id } }));

      // set attributes
      foreach (attribute a in e.attributes) {
        object val = e.attribute_value(a.code); string qry_val = null;
        if (val != null) {
          if (a.type == attribute.attribute_type.datetime)
            qry_val = db_provider.dt_qry(Convert.ToDateTime(val));
          else if (a.type == attribute.attribute_type.integer)
            qry_val = Convert.ToInt32(val).ToString();
          else if (a.type == attribute.attribute_type.real)
            qry_val = Convert.ToDouble(val).ToString();
          else qry_val = db_provider.str_qry(val.ToString());

          set_attribute(e, a, qry_val);
        } else {
          if (!e.added) db_conn.exec(core.parse(config.get_query("elements.delete-attributes").text
            , new Dictionary<string, object>() { { "id_element", e.id }, { "attr_type", a.type.ToString() }
            , { "element_type", e.type.ToString() } }));
        }
      }

      // childs
      if (!e.sham) {
        if (!e.added) db_conn.exec(core.parse(config.get_query("elements.reset-contents").text
         , new Dictionary<string, object>() { { "id_element", e.id } }));

        foreach (element ec in e.childs) {
          save_element(ec, key_page);
          db_conn.exec(core.parse(config.get_query("elements.save-child-content").text
            , new Dictionary<string, object>() { { "id_element", e.id }, { "id_child", ec.id }, { "order", ec.order_xml } }));
        }
      } // elementi figli sham 
      else {
        if (e.undeleted)
          db_conn.exec(core.parse(config.get_query("elements.undelete-childs").text
            , new Dictionary<string, object>() { { "id_element", e.id } }));
      }

      return e.id;
    }
  }
}