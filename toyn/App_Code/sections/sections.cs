using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;

namespace toyn {
  public class sections : bo {

    public sections() {
    }

    public page_sections load_home_page() {

      page_sections ps = new page_sections();

      // main section
      DataTable dt = db_conn.dt_table(core.parse(config.get_query("sections.homepage-vars").text
        , new Dictionary<string, object>() { { "id_utente", this.user_id } }));
      string title = db_provider.str_val(dt.Select("name='home-page.title'")[0]["value"]);
      ps.title = title != "" ? title : "";

      // macro sections
      ps.macro_sections = load_macros_sections();

      return ps;
    }

    #region macro_sections

    public List<macro_section> load_macros_sections(int? id = null) {
      List<macro_section> res = dt_macro_sections(id).Rows.Cast<DataRow>().Select(x => macro_section_from_dr(x)).ToList();

      // sections
      foreach (section s in load_sections(id_ms: id)) {
        macro_section ms = res.FirstOrDefault(x => x.id == s.id_ms);
        if (ms == null) throw new Exception("la macro sezione '" + ms.id + "' impostata per la sezione '" + s.id + "' non esiste!");
        ms.add_section(s);
      }

      return res;
    }

    public macro_section load_macro_section(int id) {
      List<macro_section> l = load_macros_sections(id);
      if (l == null || l.Count == 0) throw new Exception("la macro sezione '" + id + "' non esiste!");
      return l[0];
    }

    protected DataTable dt_macro_sections(int? id = null) {
      return db_conn.dt_table(core.parse_query("sections.load-macro-sections"
        , new string[,] { { "id_utente", user_id.ToString() }, { "id", id.HasValue ? id.ToString() : "" } }));
    }

    protected macro_section macro_section_from_dr(DataRow dr) {
      return new macro_section() {
        id = db_provider.int_val(dr["id_macro_section"]), title = db_provider.str_val(dr["title"]),
        notes = db_provider.str_val(dr["notes"]), order = db_provider.int_val(dr["order"])
      };
    }

    #endregion

    #region sections

    public List<section> load_sections(int? id = null, int? id_ms = null) {
      List<section> res = new List<section>();
      foreach (DataRow ra in dt_sections(id, id_ms).Rows) {
        int id_section = db_provider.int_val(ra["id_section"]);
        section s = res.FirstOrDefault(x => x.id == id_section);
        if (s == null) { s = section_from_dr(ra); res.Add(s); }

        string attribute_code = db_provider.str_val(ra["attribute_code"]);
        if (attribute_code != "") {
          string attribute_type = db_provider.str_val(ra["attribute_type"]);
          object val = ra["val_" + attribute_type] != DBNull.Value ? ra["val_" + attribute_type] : null;
          s.set_attribute(attribute_code
            , (section_attr.section_attr_type)Enum.Parse(typeof(section_attr.section_attr_type), attribute_type), val);
        }
      }
      return res;
    }

    public section load_section(int id) { List<section> l = load_sections(id); return l.Count > 0 ? l[0] : null; }

    public void update_section(section s, bool save_attrs = true) {
      bool trans = false;
      try {
        trans = db_conn.check_begin_trans();
        db_conn.exec(core.parse_query("sections.upd-section"
          , new string[,] { { "title", s.title }, { "notes", s.notes }, { "cols", s.cols.ToString() }, { "id", s.id.ToString() } }));
        if (save_attrs) {
          foreach (section_attr a in s.attributes)
            set_attribute(s.type, s.id, a, a.value, true);
        }
        if (trans) db_conn.commit();
      } catch (Exception ex) { if (trans) db_conn.rollback(); throw ex; }
    }

    protected DataTable dt_sections(int? id = null, int? id_ms = null) {
      return db_conn.dt_table(core.parse_query("sections.load-sections"
        , new string[,] { { "id_utente", user_id.ToString() }, { "id", id.HasValue ? id.ToString() : "" }
          , { "id_ms", id_ms.HasValue ? id_ms.ToString() : "" }}));
    }

    protected section section_from_dr(DataRow dr) {
      return new section() {
        id = db_provider.int_val(dr["id_section"]), id_ms = db_provider.int_val(dr["id_macro_section"]),
        type = (section.type_section)Enum.Parse(typeof(section.type_section), db_provider.str_val(dr["type"])),
        title = db_provider.str_val(dr["title"]), notes = db_provider.str_val(dr["notes"]), order = db_provider.int_val(dr["order"]),
        dt_ins = db_provider.dt_val(dr["dt_ins"]), dt_upd = db_provider.dt_val(dr["dt_upd"]),
        emphasis = db_provider.str_val(dr["emphasis"]), cols = db_provider.int_val(dr["cols"])
      };
    }

    protected void exec_set_attribute(section.type_section st, long id, section_attr.section_attr_type at, string code, string qry_val) {
      db_conn.exec(core.parse_query("sections.set-attribute"
        , new Dictionary<string, object>() { { "attr_type", at }, { "attr_value", qry_val }
          , { "id_section", id }, { "attr_code", code }, { "section_type", st } }));
    }

    protected void exec_del_attribute(section.type_section st, long id, section_attr.section_attr_type at, string code) {
      db_conn.exec(core.parse_query("sections.delete-attribute"
        , new Dictionary<string, object>() { { "id_section", id }, { "attr_type", at.ToString() }
            , { "attr_code", code }, { "section_type", st }}));
    }

    protected bool set_attribute(section.type_section st, long id, section_attr a, object val, bool delete = true) {
      string qry_val = null;
      if (val != null) {
        try {
          if (a.type == section_attr.section_attr_type.datetime)
            qry_val = db_provider.dt_qry(Convert.ToDateTime(val));
          else if (a.type == section_attr.section_attr_type.integer)
            qry_val = Convert.ToInt32(val).ToString();
          else qry_val = db_provider.str_qry(val.ToString());
        } catch {
          throw new Exception(string.Format("il valore {0} dev'essere di tipo {3} e non è corretto. elemento '{1}' id: {2}"
            , val.ToString(), st.ToString(), id, a.type.ToString()));
        }
        exec_set_attribute(st, id, a.type, a.code, qry_val);
        return true;
      } else { if (delete) { exec_del_attribute(st, id, a.type, a.code); return true; } }
      return false;
    }

    #endregion

    #region statics

    public string set_main_title(string txt) { return set_setting("home-page.title", txt); }

    #endregion
  }
}