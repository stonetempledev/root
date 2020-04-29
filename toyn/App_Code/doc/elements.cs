using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib;
using mlib.db;
using mlib.tools;

namespace toyn {
  public class elements : bo {

    public elements() {
    }

    public List<attribute> load_attributes() {
      List<attribute> attrs = new List<attribute>();
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("elements.elements-attributes")).Rows)
        attrs.Add(create_attribute(dr));
      return attrs;
    }

    public List<element> load_types_elements() {
      List<element> els = new List<element>();
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("elements.types-attributes")).Rows) {
        string des = db_provider.str_val(dr["element_des"]), childs_types = db_provider.str_val(dr["childs_types"]);
        element.type_element et = (element.type_element)Enum.Parse(typeof(element.type_element), db_provider.str_val(dr["element_type"]));
        element e = els.FirstOrDefault(x => x.type == et);
        if (els.FirstOrDefault(x => x.type == et) == null) {
          e = new element(this.core) { type = et, des = des, childs_types = childs_types };
        }
        e.attributes.Add(create_attribute(dr));
        els.Add(e);
      }
      return els;
    }

    public element load_type_element(element.type_element et, long id = 0, string[,] attrs = null, Dictionary<string, string> attrs2 = null) {
      element etype = null;
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("elements.type-attributes"
        , new Dictionary<string, object>() { { "type", et.ToString() } })).Rows) {
        string des = db_provider.str_val(dr["element_des"]), childs_types = db_provider.str_val(dr["childs_types"]);
        if (etype == null) {
          etype = new element(this.core) { id = id, type = et, des = des, childs_types = childs_types };
        }
        attribute a = create_attribute(dr);
        if (attrs != null) {
          for (int i = 0; i < attrs.GetLength(0); i++) {
            if (attrs[i, 0] == a.code) a.value = attrs[i, 1];
          }
        }
        if (attrs2 != null && attrs2.ContainsKey(a.code))
          a.value = attrs2[a.code];
        etype.attributes.Add(a);
      }
      return etype;
    }

    public attribute create_attribute(DataRow dr) {
      string etype = db_provider.str_val(dr["element_type"]), acode = db_provider.str_val(dr["attribute_code"])
        , atype = db_provider.str_val(dr["attribute_type"]);
      int aid = db_provider.int_val(dr["attribute_id"]);
      bool content_txt_xml = db_provider.int_val(dr["content_txt_xml"]) == 1
        , data_content = db_provider.int_val(dr["data_content"]) == 1
        , static_value = db_provider.int_val(dr["static_value"]) == 1;

      return new attribute((element.type_element)Enum.Parse(typeof(element.type_element), etype), aid, acode
        , (attribute.attribute_type)Enum.Parse(typeof(attribute.attribute_type), atype), content_txt_xml, static_value, data_content);
    }

    public void store_element(long element_id) {
      db_conn.exec(core.parse_query("elements.store-element"
        , new Dictionary<string, object>() { { "id_element", element_id } }));
    }

    public void unstore_element(long element_id) {
      db_conn.exec(core.parse_query("elements.unstore-element"
        , new Dictionary<string, object>() { { "id_element", element_id } }));
    }

    public void set_attribute(element.type_element et, long id, string code, object val) {
      set_attribute(et, id, load_attribute(et, code), val);
    }

    protected bool set_attribute(element.type_element et, long id, attribute a, object val, bool delete = true) {
      string qry_val = null;
      if (a.static_value) return false;
      if (val != null) {
        try {
          if (a.type == attribute.attribute_type.datetime)
            qry_val = db_provider.dt_qry(Convert.ToDateTime(val));
          else if (a.type == attribute.attribute_type.integer)
            qry_val = Convert.ToInt32(val).ToString();
          else if (a.type == attribute.attribute_type.real)
            qry_val = Convert.ToDouble(val).ToString();
          else qry_val = db_provider.str_qry(val.ToString());
        } catch {
          throw new Exception(string.Format("il valore {0} dev'essere di tipo {3} e non è corretto. elemento '{1}' id: {2}"
            , val.ToString(), et.ToString(), id, a.type.ToString()));
        }
        exec_set_attribute(et, id, a.type, a.code, qry_val);
        return true;
      } else { if (delete) { exec_del_attribute(id, a.type, a.code); return true; } }
      return false;
    }

    protected void exec_del_attribute(long id, attribute.attribute_type at, string code) {
      db_conn.exec(core.parse_query("elements.delete-attribute"
        , new Dictionary<string, object>() { { "id_element", id }, { "attr_type", at.ToString() }
            , { "attr_code", code } }));
    }

    protected void exec_set_attribute(element.type_element et, long id, attribute.attribute_type at, string code, string qry_val) {
      db_conn.exec(core.parse_query("elements.set-attribute"
        , new Dictionary<string, object>() { { "attr_type", at }, { "attr_value", qry_val }
          , { "id_element", id }, { "attr_code", code }, { "element_type", et } }));
    }

    public attribute load_attribute(element.type_element te, string code) {
      return create_attribute(db_conn.first_row(core.parse_query("elements.element-attribute"
        , new Dictionary<string, object>() { { "type", te.ToString() }, { "code", code } })));
    }

    public void load_childs(element e, bool also_deleted = false, int? max_level = null) {
      int max_lvl_els = 0, max_lvl = 0;
      List<element> els = load_elements(out max_lvl_els, out max_lvl, e.id, max_level, also_deleted: also_deleted, only_childs: true, force_level: e.level);
      els.Where(x => x.level == e.level + 1).ToList().ForEach(ec => { e.add_child(ec); });
    }

    public element load_element(long element_id, bool childs = false) {
      int max_lvl_els = 0, max_lvl = 0;
      List<element> els = load_elements(out max_lvl_els, out max_lvl, element_id, childs: childs);
      return els.Count > 0 ? els[0] : null;
    }

    public List<element> load_elements(long? element_id = null, int? max_level = null, bool only_childs = false, bool childs = true, bool also_deleted = false, string key_page = null) {
      int out_max_lvl_els = 0, out_max_lvl = 0;
      return load_elements(out out_max_lvl_els, out out_max_lvl, element_id, max_level, only_childs, childs, also_deleted, key_page);
    }

    public List<element> load_elements(out int out_max_lvl_els, out int out_max_lvl, long? element_id = null, int? max_level = null, bool only_childs = false, bool childs = true, bool also_deleted = false, string key_page = null, int? force_level = null) {

      List<element> els = new List<element>(); out_max_lvl_els = -1; out_max_lvl = -1;
      string sql = !element_id.HasValue ? core.parse_query("elements.open-roots-elements"
          , new Dictionary<string, object>() { { "filter_level", max_level.HasValue ? "(h.in_list = 1 or (h.in_list = 0 and h.livello <= " + (max_level.Value + 1).ToString() + "))" : "1 = 1" }
         , { "id_utente", this.user_id }, { "filter_deleted", !also_deleted ? "isnull(h.deleted, 0) = 0" : "1 = 1" } })
        : core.parse_query("elements.open-elements"
          , new Dictionary<string, object>() { { "id_element", element_id }
          , { "filter_head", only_childs ? "and 1 = 0" : "" }, { "filter_childs", !childs ? "and 1 = 0" : "" }
          , { "filter_level", max_level.HasValue ? "(h.in_list = 1 or (h.in_list = 0 and h.livello <= " + (max_level.Value + 1).ToString() + "))" : "1 = 1" }
          , { "id_utente", this.user_id }, { "filter_deleted", !also_deleted ? "isnull(h.deleted, 0) = 0" : "1 = 1" } });
      DataTable dt = db_conn.dt_table(sql);
      foreach (DataRow re in dt.Rows) {

        // element
        long parent_id = db_provider.long_val(re["parent_id"]), contents_id = db_provider.long_val(re["elements_contents_id"])
          , id = db_provider.long_val(re["element_id"]);
        int livello = force_level.HasValue ? force_level.Value + db_provider.int_val(re["livello"]) : db_provider.int_val(re["livello"])
          , order = db_provider.int_val(re["order"]);
        string element_type = db_provider.str_val(re["element_type"]), title = db_provider.str_val(re["element_title"])
          , des = db_provider.str_val(re["element_des"]), element_code = db_provider.str_val(re["element_code"]);
        bool has_childs = db_provider.int_val(re["has_childs"]) == 1, in_list = db_provider.int_val(re["in_list"]) == 1
          , has_child_elements = db_provider.int_val(re["has_child_elements"]) == 1;
        DateTime? dt_ins = db_provider.dt_val(re["dt_ins"]), dt_upd = db_provider.dt_val(re["dt_upd"])
          , dt_stored = db_provider.dt_val(re["dt_stored"]);

        if (out_max_lvl_els < livello && !in_list) out_max_lvl_els = livello;
        if (out_max_lvl < livello) out_max_lvl = livello;

        element e = els.FirstOrDefault(x => x.id == id);
        if (e == null) {
          e = new element(this.core, (element.type_element)Enum.Parse(typeof(element.type_element), element_type), element_code, title, livello
            , id, parent_id, contents_id, has_childs, has_child_elements, in_list, des, dt_ins, dt_upd, dt_stored, order: order
            , sham: max_level.HasValue ? livello == max_level + 1 && has_childs && !in_list : false);
          e.key_page = key_page;
          if (livello == 0 && element_id.HasValue) e.back_element_id = parent_id != 0 ? parent_id : (long?)null;
          els.Add(e);
        }

        // attribute
        string attribute_code = db_provider.str_val(re["attribute_code"])
          , attribute_type = db_provider.str_val(re["attribute_type"]);
        bool content_txt_xml = db_provider.int_val(re["content_txt_xml"]) == 1
          , static_value = db_provider.int_val(re["static_value"]) == 1;
        if (attribute_code != "" && !static_value) {
          object val = re["val_" + attribute_type];
          e.set_attribute(attribute_code, (attribute.attribute_type)Enum.Parse(typeof(attribute.attribute_type), attribute_type), val, content_txt_xml, static_value);
        }
      }

      // hierarchy
      List<element> res = new List<element>(els.Where(x => x.level ==
        (only_childs ? (force_level.HasValue ? force_level.Value + 1 : 1)
          : (force_level.HasValue ? force_level.Value + 0 : 0))));
      for (int l = 1; l <= out_max_lvl; l++) {
        foreach (element el in els.Where(x => x.level == l)) {
          element pe = els.FirstOrDefault(x => x.id == el.parent_id);
          if (pe != null) pe.add_child(el);
        }
      }

      return res;
    }

    public List<element> load_parents_elements(long element_id, long from_parent_id = 0, bool with_childs = false, int? max_level = null) {
      List<element> els = new List<element>();

      bool first = true; int diff_level = 0, max_livello = 0; element el = null;
      foreach (DataRow re in db_conn.dt_table(core.parse_query("elements.open-parents-elements"
        , new Dictionary<string, object>() { { "element_id", element_id }, { "id_utente", this.user_id }
          , { "parent_id", from_parent_id > 0 && element_id != from_parent_id ? (object)from_parent_id : null } })).Rows) {

        int livello = db_provider.int_val(re["livello"]);
        if (first) { diff_level = element_id != from_parent_id ? 0 - livello : 0; first = false; }
        livello += diff_level;

        // element
        long parent_id = db_provider.long_val(re["parent_id"]), contents_id = db_provider.long_val(re["elements_contents_id"])
          , id = db_provider.long_val(re["element_id"]);
        int order = db_provider.int_val(re["order"]);
        string element_type = db_provider.str_val(re["element_type"]), element_code = db_provider.str_val(re["element_code"])
          , title = db_provider.str_val(re["element_title"]), des = db_provider.str_val(re["element_des"]);
        bool has_childs = db_provider.int_val(re["has_childs"]) == 1
          , has_child_elements = db_provider.int_val(re["has_child_elements"]) == 1;
        DateTime? dt_ins = db_provider.dt_val(re["dt_ins"]), dt_upd = db_provider.dt_val(re["dt_upd"])
          , dt_stored = db_provider.dt_val(re["dt_stored"]);

        element e = els.FirstOrDefault(x => x.id == id);
        if (e == null) {
          e = new element(this.core, (element.type_element)Enum.Parse(typeof(element.type_element), element_type), element_code, title, livello
            , id, parent_id, contents_id, has_childs, has_child_elements, false, des, dt_ins, dt_upd, dt_stored, order: order);
          els.Add(e);
          if (id == element_id) el = e;
          if (livello > max_livello) max_livello = livello;
        }

        // attribute
        string attribute_code = db_provider.str_val(re["attribute_code"])
          , attribute_type = db_provider.str_val(re["attribute_type"]);
        bool content_txt_xml = db_provider.int_val(re["content_txt_xml"]) == 1
          , static_value = db_provider.int_val(re["static_value"]) == 1;
        if (attribute_code != "" && !static_value) {
          object val = re["val_" + attribute_type];
          e.set_attribute(attribute_code, (attribute.attribute_type)Enum.Parse(typeof(attribute.attribute_type), attribute_type)
            , val, content_txt_xml, static_value);
        }
      }

      // childs
      if (with_childs && el != null)
        load_childs(el, max_level: (max_level.HasValue ? max_level - max_livello : 0));

      return els;
    }

    public List<long> get_child_elements_ids(long element_id) {
      return db_conn.dt_table(core.parse_query("elements.open-elements"
        , new Dictionary<string, object>() { { "id_element", element_id }
        , { "filter_head", "and 1 = 0" }, { "filter_childs", "" }, { "filter_level", "1 = 1" }
        , { "id_utente", this.user_id }, { "filter_deleted", "isnull(h.deleted, 0) = 0" } })).Rows
          .Cast<DataRow>().Select(r => db_provider.long_val(r["element_id"])).ToList();
    }

    public List<long> get_parent_elements_ids(long element_id) {
      return db_conn.dt_table(core.parse_query("elements.parent-elements"
        , new Dictionary<string, object>() { { "id_element", element_id }, { "id_utente", this.user_id } })).Rows
          .Cast<DataRow>().Select(r => db_provider.long_val(r["parent_id"])).ToList();
    }

    public List<element> load_xml(string xml, int? back_element_id = null) {
      return element.load_xml(this.core, load_types_elements(), xml, back_element_id);
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
      DataRow dr = db_conn.first_row(core.parse_query("elements.id-deleted", new Dictionary<string, object>() { { "id_utente", this.user_id } }));
      int id_deleted = db_provider.int_val(dr["id_deleted"]);
      foreach (element c in get_all(els_bck)) {
        if (all.FirstOrDefault(x => x.id == c.id) == null) {
          db_conn.exec(core.parse_query("elements.delete-element"
            , new Dictionary<string, object>() { { "id_element", c.id }, { "id_utente", this.user_id }, { "id_deleted", id_deleted } }));
        }
      }

    }

    public void save_elements(List<element> els, string key_page = "", int parent_id = 0, int? first_order = null, int? last_order = null) {

      try {

        db_conn.begin_trans();

        List<element> el_types = load_types_elements();

        // correggo l'order
        if (first_order.HasValue && last_order.HasValue) {
          int diff = els.Count - ((last_order.Value - first_order.Value) + 1);
          if (diff != 0) {
            db_conn.exec(core.parse_query("elements.refresh-orders"
              , new Dictionary<string, object>() { { "filter_id_element", parent_id > 0 ? " = " + parent_id.ToString() : " = -1" }
            , { "id_utente", this.user_id }, { "diff_order", diff }, { "filter_order", last_order.Value } }));
          }
        }

        // salvataggio elementi
        foreach (element el in els) {
          save_element2(el, el_types, key_page);
          el.order = el.order_xml + (first_order.HasValue ? first_order.Value : 0);
          db_conn.exec(core.parse_query("elements.save-child"
            , new Dictionary<string, object>() { { "id_parent", parent_id > 0 ? parent_id.ToString() : "-1" }
            , { "id_element", el.id }, { "order", el.order_xml + (first_order.HasValue ? first_order.Value : 0) } }));
        }

        db_conn.commit();
      } catch (Exception ex) { db_conn.rollback(); throw ex; }
    }

    public List<long> check_exists_elements(List<long> ids) {
      return db_conn.dt_table(core.parse_query("elements.check-exists-elements"
        , new Dictionary<string, object>() { { "ids_elements", string.Join(",", ids) }, { "id_utente", user_id } }))
        .Rows.Cast<DataRow>().Select(r => db_provider.long_val(r["element_id"])).ToList();
    }

    public long save_element(element e, List<element> el_types, List<string> attrs_to_save = null, bool save_childs = true) {
      long res = 0; bool close_trans = false;
      try {
        if (!db_conn.is_trans()) { db_conn.begin_trans(); close_trans = true; }
        res = save_element2(e, el_types, "", attrs_to_save, save_childs);
        if (close_trans) db_conn.commit();
      } catch (Exception ex) { if (close_trans) db_conn.rollback(); throw ex; }
      return res;
    }

    protected long save_element2(element e, List<element> el_types, string key_page = "", List<string> attrs_to_save = null, bool save_childs = true) {

      // type
      element etype = el_types.FirstOrDefault(x => x.type == e.type);
      if (etype == null) throw new Exception("elemento '" + e.type + "' non configurato correttamente!");

      // from_id
      e.undeleted = false;
      if (e.from_id > 0) {
        DataRow dr = db_conn.first_row(core.parse_query("elements.deleted-element"
          , new Dictionary<string, object>() { { "id_element", e.from_id }, { "type_element", e.type.ToString() } }));
        if (dr != null) { e.id = e.from_id; e.key_page = key_page; e.undeleted = true; }
      }

      // element
      e.added = false;
      if (e.id == 0) {
        e.added = true;
        e.id = int.Parse(db_conn.exec(core.parse_query("elements.save-element"
          , new Dictionary<string, object>() { { "id_utente", this.user_id }, { "code", e.code }, { "type", e.type.ToString() }, { "title", e.title } }), true));
        e.key_page = key_page;
      } else
        db_conn.exec(core.parse_query("elements.update-element"
          , new Dictionary<string, object>() { { "type", e.type.ToString() }, { "title", e.title }, { "code", e.code }, { "id_element", e.id } }));

      // set attributes
      foreach (attribute a in e.attributes) {
        if (attrs_to_save != null && !attrs_to_save.Contains(a.code)) continue;
        set_attribute(e.type, e.id, a, a.value, !e.added);
      }

      // delete attributes
      if (!e.added) {
        etype.attributes.ForEach(at => {
          bool salta = attrs_to_save != null && !attrs_to_save.Contains(at.code);
          if (!salta && e.attributes.FirstOrDefault(x => x.code == at.code) == null)
            exec_del_attribute(e.id, at.type, at.code);
        });
      }

      // childs
      if (save_childs) {
        if (!e.sham) {
          if (!e.added) db_conn.exec(core.parse_query("elements.reset-contents"
           , new Dictionary<string, object>() { { "id_element", e.id } }));

          foreach (element ec in e.childs) {
            save_element2(ec, el_types, key_page);
            db_conn.exec(core.parse_query("elements.save-child-content"
              , new Dictionary<string, object>() { { "id_element", e.id }, { "id_child", ec.id }, { "order", ec.order_xml } }));
          }
        } // elementi figli sham 
        else {
          if (e.undeleted)
            db_conn.exec(core.parse_query("elements.undelete-childs"
              , new Dictionary<string, object>() { { "id_element", e.id } }));
        }
      }

      return e.id;
    }

    public void refresh_order_element(long element_id, db_provider conn = null) {
      (conn != null ? conn : this.db_conn).exec(
        core.parse_query("elements.refresh-orders-element"
          , new Dictionary<string, object>() { { "element_id", element_id } }));
    }

    public void refresh_order_child(long child_id, db_provider conn = null) {
      (conn != null ? conn : this.db_conn).exec(
        core.parse_query("elements.refresh-orders-child"
         , new Dictionary<string, object>() { { "child_id", child_id } }));
    }

    public List<long> next_elements_to(long id, List<element.type_element> t_end) {
      List<long> ids = new List<long>();
      foreach (DataRow r in db_conn.dt_table(core.parse_query("elements.next-elements"
        , new Dictionary<string, object>() { { "id_element", id } })).Rows) {
        long idc = db_provider.long_val(r["child_element_id"]);
        element.type_element type = (element.type_element)Enum.Parse(typeof(element.type_element), db_provider.str_val(r["element_type"]));
        if (idc == id) { ids.Add(idc); continue; }
        if (t_end.Contains(type)) break;
        ids.Add(idc);
      }
      return ids;
    }

    public element find_element(long id, List<element> els) {
      foreach (element e in els) {
        element res = e.find_element(id);
        if (res != null) return res;
      }
      return null;
    }
  }
}