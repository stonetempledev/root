using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using mlib.db;
using mlib.tools;
using mlib.xml;
using mlib.tiles;
using toyn;

public partial class _element : tl_page {

  protected int _max_level = 3;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // elab requests & cmds
    bool elab_request = false;
    try {

      if (Request.Headers["toyn-post"] != null) {
        elab_request = true;
        json_result res = new json_result(json_result.type_result.ok);

        try {

          string json = String.Empty;
          Request.InputStream.Position = 0;
          using (var inputStream = new StreamReader(Request.InputStream)) {
            json = inputStream.ReadToEnd();
          }

          if (!string.IsNullOrEmpty(json)) {
            JObject jo = JObject.Parse(json);
            // save_element
            if (jo["action"].ToString() == "save_element") {

              // carico l'xml e salvo tutto
              string element_id = jo["element_id"].ToString(), key_page = jo["key_page"].ToString();
              int parent_id = jo["parent_id"].ToString() != "" ? Convert.ToInt32(jo["parent_id"].ToString()) : 0
                , max_level = Convert.ToInt32(jo["max_level"]);
              int? first_order = jo["first_order"].ToString() != "" ? int.Parse(jo["first_order"].ToString()) : (int?)null;
              int? last_order = jo["last_order"].ToString() != "" ? int.Parse(jo["last_order"].ToString()) : (int?)null;
              List<element> els_bck = element.load_xml(this.core, load_attributes(), jo["xml_bck"].ToString(), element_id != "" ? parent_id : (int?)null)
                , els = element.load_xml(this.core, load_attributes(), jo["xml"].ToString(), element_id != "" ? parent_id : (int?)null);

              // salvataggio
              check_and_del(els_bck, els);
              save_elements(els, key_page, parent_id, first_order, last_order);

              // vedo se ci sono elementi figli
              int ml = element.max_level(els);
              foreach (element ec in element.find_elements(els, ml).Where(x => x.id > 0)) {
                DataRow dr = db_conn.first_row(core.parse(config.get_query("has-childs").text
                  , new Dictionary<string, object>() { { "id_element", ec.id } }));
                ec.has_childs = db_provider.int_val(dr["has_childs"]) == 1;
                ec.has_child_elements = db_provider.int_val(dr["has_child_elements"]) == 1;
              }

              // xml
              res.doc_xml = element.get_doc(els).xml;
              res.vars.Add("first_order", els.Count > 0 ? els[0].order.ToString() : "");
              res.vars.Add("last_order", els.Count > 0 ? els[els.Count - 1].order.ToString() : "");

              // menu
              StringBuilder sb = new StringBuilder();
              parse_menu(element_id, els, sb, true, max_level);
              res.menu_html = sb.ToString();

            } else if (jo["action"].ToString() == "check_paste_xml") {
              StringBuilder text_xml = new StringBuilder();
              foreach (JValue j in jo["text_xml"] as JArray)
                text_xml.AppendLine(j.Value.ToString());
              string key_page = jo["key_page"].ToString(), doc_xml = jo["doc_xml"].ToString(), paste_xml = text_xml.ToString();

              // elaboro l'xml
              if (!string.IsNullOrEmpty(paste_xml)) {

                // elenco ids da controllare
                List<string> ids = new List<string>();
                int ni = 0;
                while (true) {
                  int fi = paste_xml.IndexOf(" id=\"", ni);
                  if (fi < 0) break;
                  int start = fi + 5, end = paste_xml.IndexOf("\"", start);
                  ni = start;
                  if (end < 0) break;
                  ids.Add(paste_xml.Substring(start, end - start));
                }

                // ids da cancellare
                List<string> ids_to_del = new List<string>();
                foreach (string val in ids) {
                  string kp = val.Split(new char[] { ':' })[1];
                  int id = int.Parse(val.Split(new char[] { ':' })[0]);

                  // se c'è già l'id
                  if (doc_xml.IndexOf("id=\"" + id.ToString() + ":") >= 0)
                    ids_to_del.Add(val);
                  else {
                    DataRow dr = db_conn.first_row(core.parse(config.get_query("exists-element").text
                    , new Dictionary<string, object>() { { "id_element", id } }));
                    if (kp != key_page && dr != null) ids_to_del.Add(val);
                    else if (dr == null) ids_to_del.Add(val);
                  }
                }

                foreach (string id_del in ids_to_del)
                  paste_xml = paste_xml.Replace("id=\"" + id_del + "\"", "from_id=\"" + id_del + "\"");
              }
              res.contents = paste_xml;
            }
          } else throw new Exception("nessun dato da elaborare!");

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        Response.Clear();
        Response.ContentType = "application/json";
        Response.Write(JsonConvert.SerializeObject(res));
        Response.Flush();
        Response.SuppressContent = true;
        HttpContext.Current.ApplicationInstance.CompleteRequest();

        return;
      }

      cmd c = new cmd(qry_val("cmd"));

      // check cmd
      if (string.IsNullOrEmpty(qry_val("cmd"))) return;

      // view
      if (c.action == "view" && c.obj == "element") {

        string element_id = c.sub_cmd("id"); int max_level = 0;

        List<element> els = load_elements(out max_level, element_id != "" ? int.Parse(element_id) : (int?)null, _max_level);

        // client vars
        url_xml.Value = master.url_cmd("xml element" + (element_id != "" ? " id:" + element_id : ""));
        max_lvl.Value = max_level.ToString();

        // document
        StringBuilder sb = new StringBuilder();
        parse_elements(element_id, els, sb);
        contents_doc.InnerHtml = sb.ToString();
        contents_xml.Visible = false;
        sb.Clear();

        // menu
        parse_menu(element_id, els, sb, false, max_level);
        menu.InnerHtml = sb.ToString();

      }
        // xml
      else if (c.action == "xml" && c.obj == "element") {

        string element_id = c.sub_cmd("id"), kp = strings.random_hex(4); int max_level = 0;
        List<element> els = load_elements(out max_level, element_id != "" ? int.Parse(element_id) : (int?)null, _max_level, key_page: kp);

        // client vars
        url_view.Value = master.url_cmd("view element" + (element_id != "" ? " id:" + element_id : ""));
        id_element.Value = element_id.ToString();
        parent_id.Value = els.Count > 0 ? els[0].parent_id.ToString() : "";
        first_order.Value = els.Count > 0 ? els[0].order.ToString() : "";
        last_order.Value = els.Count > 0 ? els[els.Count - 1].order.ToString() : "";
        max_lvl.Value = max_level.ToString();
        key_page.Value = kp;

        // xml document
        contents_xml.Visible = true;
        contents.Visible = false;
        doc_xml.Value = doc_xml_bck.Value = element.get_doc(els).root_node.inner_xml;

        // menu
        StringBuilder sb = new StringBuilder();
        parse_menu(element_id, els, sb, true, max_level);
        menu.InnerHtml = sb.ToString();
      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) {
      log.log_err(ex);
      if (!elab_request) master.err_txt(ex.Message);
    }
  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

  protected List<element> get_all(List<element> els, List<element> l = null) {
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

  protected void check_and_del(List<element> els_bck, List<element> els) {

    // check ids univoci
    List<element> all = get_all(els);
    foreach (element c in all) {
      if (c.id > 0 && all.Count(x => x.id == c.id) > 1)
        throw new Exception(string.Format("l'elemento con id {1} è duplicato più volte e non è possibile!"
          , c.type.ToString(), c.id));
    }

    // ciclo pulizia
    DataRow dr = db_conn.first_row(core.parse(config.get_query("id-deleted").text, new Dictionary<string, object>() { { "id_utente", _user.id } }));
    int id_deleted = db_provider.int_val(dr["id_deleted"]);
    foreach (element c in get_all(els_bck)) {
      if (all.FirstOrDefault(x => x.id == c.id) == null) {
        db_conn.exec(core.parse(config.get_query("delete-element").text
          , new Dictionary<string, object>() { { "id_element", c.id }, { "id_utente", _user.id }, { "id_deleted", id_deleted } }));
      }
    }

  }

  protected void save_elements(List<element> els, string key_page, int parent_id = 0, int? first_order = null, int? last_order = null) {

    // correggo l'order
    if (first_order.HasValue && last_order.HasValue) {
      int diff = els.Count - ((last_order.Value - first_order.Value) + 1);
      if (diff != 0) {
        db_conn.exec(core.parse(config.get_query("refresh-orders").text
          , new Dictionary<string, object>() { { "filter_id_element", parent_id > 0 ? " = " + parent_id.ToString() : " = -1" }
            , { "id_utente", _user.id }, { "diff_order", diff }, { "filter_order", last_order.Value } }));
      }
    }

    // salvataggio elementi
    foreach (element el in els) {
      save_element(el, key_page);
      el.order = el.order_xml + (first_order.HasValue ? first_order.Value : 0);
      db_conn.exec(core.parse(config.get_query("save-child").text
        , new Dictionary<string, object>() { { "id_parent", parent_id > 0 ? parent_id.ToString() : "-1" }
            , { "id_element", el.id }, { "order", el.order_xml + (first_order.HasValue ? first_order.Value : 0) } }));
    }
  }

  protected long save_element(element e, string key_page = "") {

    // from_id
    e.undeleted = false;
    if (e.from_id > 0) {
      DataRow dr = db_conn.first_row(core.parse(config.get_query("deleted-element").text
        , new Dictionary<string, object>() { { "id_element", e.from_id }, { "type_element", e.type.ToString() } }));
      if (dr != null) { e.id = e.from_id; e.key_page = key_page; e.undeleted = true; }
    }

    // element
    e.added = false;
    if (e.id == 0) {
      e.added = true;
      e.id = int.Parse(db_conn.exec(core.parse(config.get_query("save-element").text
        , new Dictionary<string, object>() { { "id_utente", _user.id }, { "type", e.type.ToString() }, { "title", e.title } }), true));
      e.key_page = key_page;
    } else
      db_conn.exec(core.parse(config.get_query("update-element").text
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

        db_conn.exec(core.parse(config.get_query("set-attribute").text
          , new Dictionary<string, object>() { { "attr_type", a.type.ToString() }, { "attr_value", qry_val }
            , { "id_element", e.id }, { "attr_code", a.code }, { "element_type", e.type.ToString() } }));
      } else {
        if (!e.added) db_conn.exec(core.parse(config.get_query("delete-attributes").text
          , new Dictionary<string, object>() { { "id_element", e.id }, { "attr_type", a.type.ToString() }
            , { "element_type", e.type.ToString() } }));
      }
    }

    // childs
    if (!e.sham) {
      if (!e.added) db_conn.exec(core.parse(config.get_query("reset-contents").text
       , new Dictionary<string, object>() { { "id_element", e.id } }));

      foreach (element ec in e.childs) {
        save_element(ec, key_page);
        db_conn.exec(core.parse(config.get_query("save-child-content").text
          , new Dictionary<string, object>() { { "id_element", e.id }, { "id_child", ec.id }, { "order", ec.order_xml } }));
      }
    } // elementi figli sham 
    else {
      if (e.undeleted)
        db_conn.exec(core.parse(config.get_query("undelete-childs").text
          , new Dictionary<string, object>() { { "id_element", e.id } }));
    }

    return e.id;
  }

  protected List<attribute> load_attributes() {
    List<attribute> attrs = new List<attribute>();
    foreach (DataRow dr in db_conn.dt_table(core.parse(config.get_query("elements-attributes").text)).Rows) {
      string etype = db_provider.str_val(dr["element_type"]), acode = db_provider.str_val(dr["attribute_code"])
        , atype = db_provider.str_val(dr["attribute_type"]);
      int aid = db_provider.int_val(dr["attribute_id"]);
      bool content_txt_xml = db_provider.int_val(dr["content_txt_xml"]) == 1;

      attrs.Add(new attribute((element.type_element)Enum.Parse(typeof(element.type_element), etype), aid, acode
        , (attribute.attribute_type)Enum.Parse(typeof(attribute.attribute_type), atype), content_txt_xml));
    }
    return attrs;
  }

  protected List<element> load_childs(long element_id, bool also_deleted = false) {
    int out_max_level = 0; return load_elements(out out_max_level, element_id, also_deleted: also_deleted, only_childs: true);
  }

  protected List<element> load_elements(out int out_max_level, long? element_id = null, int? max_level = null, bool only_childs = false, bool also_deleted = false, string key_page = null) {

    List<element> els = new List<element>(); out_max_level = -1;
    string sql = !element_id.HasValue ? core.parse(config.get_query("open-roots-element").text
        , new Dictionary<string, object>() { { "filter_level", max_level.HasValue ? "h.livello <= " + (max_level.Value + 1).ToString() : "1 = 1" }
         , {"id_utente", _user.id }, { "filter_deleted", !also_deleted ? "isnull(h.deleted, 0) = 0" : "1 = 1" } })
      : core.parse(config.get_query("open-element").text
        , new Dictionary<string, object>() { { "id_element", element_id }, { "filter_only_childs", only_childs ? "and 1 = 0" : "" }
          , { "filter_level", max_level.HasValue ? "h.livello <= " + max_level.Value.ToString() : "1 = 1" }
          , {"id_utente", _user.id }, { "filter_deleted", !also_deleted ? "isnull(h.deleted, 0) = 0" : "1 = 1" } });
    DataTable dt = db_conn.dt_table(sql);
    foreach (DataRow re in dt.Rows) {

      // element
      long parent_id = db_provider.long_val(re["parent_id"]), contents_id = db_provider.long_val(re["elements_contents_id"])
        , id = db_provider.long_val(re["element_id"]);
      int livello = db_provider.int_val(re["livello"]), order = db_provider.int_val(re["order"]);
      string element_type = db_provider.str_val(re["element_type"]), title = db_provider.str_val(re["element_title"]);
      bool has_childs = db_provider.int_val(re["has_childs"]) == 1
        , has_child_elements = db_provider.int_val(re["has_child_elements"]) == 1;

      if (out_max_level < livello) out_max_level = livello;

      element e = els.FirstOrDefault(x => x.id == id);
      if (e == null) {
        e = new element(_core, (element.type_element)Enum.Parse(typeof(element.type_element), element_type), title, livello
          , id, parent_id, contents_id, has_childs, has_child_elements, order: order
          , sham: max_level.HasValue ? livello == max_level + 1 && has_childs : false);
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
    for (int l = 1; l <= out_max_level; l++) {
      foreach (element el in els.Where(x => x.level == l)) {
        element pe = els.FirstOrDefault(x => x.id == el.parent_id);
        pe.add_child(el);
      }
    }

    return res;
  }

  #region menu

  protected void parse_menu(string active_element_id, List<element> els, StringBuilder sb, bool for_xml = false, int? max_level = null) {
    foreach (element el in els.Where(x => x.type == element.type_element.element || x.type == element.type_element.title))
      parse_menu(active_element_id, el, sb, true, for_xml, max_level);
  }

  protected void parse_menu(string active_element_id, element e, StringBuilder sb, bool root = false, bool for_xml = false, int? max_level = null) {
    if (root) sb.AppendFormat("<ul class='nav flex-column' style='padding:0px;margin-top:10px;' >\n");
    if (e.has_title) {
      parse_title_menu(e, e.title, sb
        , open_element_id: (e.level == max_level && e.has_childs ? e.id : 0)
        , back_element_id: e.back_element_id.HasValue && active_element_id != "" ? e.back_element_id.Value : 0, for_xml: for_xml);
    }

    bool first = true;
    foreach (element ec in e.childs) {
      if (ec.type == element.type_element.title) {
        if (first && e.level >= 1) { sb.Append("<ul>"); first = false; }
        parse_title_menu(ec, ec.has_content ? ec.content : "<titolo>", sb, for_xml: for_xml);
      } else if (ec.type == element.type_element.element) {
        if (first && e.level >= 1) { sb.Append("<ul>"); first = false; }
        parse_menu(active_element_id, ec, sb, false, for_xml, max_level);
      }
    }

    sb.AppendFormat("{0}\n", !first ? "</ul>" : "");
    if (root) { sb.AppendFormat("</ul>\n"); }
  }

  protected void parse_title_menu(element e, string title, StringBuilder sb, bool close = true
    , long open_element_id = 0, long back_element_id = 0, bool for_xml = false) {
    int level = e.level;
    string ref_id = "ele_" + e.id.ToString(), reference = e.get_attribute_string("ref")
      , rf = !for_xml ? "#" + ref_id : "javascript:got_to_id(" + e.id.ToString() + ")";

    sb.AppendFormat(@"<li style='{7}'><div style='display:block;'>{6}<a {3} style='{4}' href='{1}'>{0}{5}</a></div>{2}"
      , (e.type == element.type_element.element ? "<span class='menu-el-arrow'>&#10148;</span>" : "") + title
      , rf, close ? "</li>" : "", e.type == element.type_element.element && e.level == 0 ? "class='h5'" : ""
      , level == 0 ? "color:steelblue;"
        : (level == 1 ? "color:skyblue;margin-top:10px;padding-top:5px;padding-left:3px;" : "font-size:90%;color:lightcyan;")
      , open_element_id > 0 ? "<a style='margin-left:15px;' href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + open_element_id.ToString()) + "\">"
        + "<img src='images/right-arrow-16.png' style='margin-bottom:4px;'></a>" : ""
      , back_element_id != 0 ? (back_element_id > 0 ? "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + back_element_id.ToString())
         : "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element"))
        + "\" style='margin-right:10px;'><img src='images/left-chevron-24.png' style='margin-left:3px;margin-bottom:4px;' width='20' height='20'></a>" : ""
      , "");
  }

  #endregion

  #region document

  protected void parse_elements(string active_element_id, List<element> els, StringBuilder sb) {
    foreach (element e in els)
      parse_element(e, sb);
  }

  protected void parse_element(element e, StringBuilder sb) {
    if (e.has_title && e.type == element.type_element.element)
      parse_type_element(e, sb);
    if (!e.sham) {
      foreach (element ec in e.childs) {
        if (ec.type == element.type_element.title)
          parse_type_title(ec, sb);
        else if (ec.type == element.type_element.text)
          parse_type_text(ec, sb);
        else if (ec.type == element.type_element.element)
          parse_element(ec, sb);
      }
    }
  }

  protected void parse_type_title(element e, StringBuilder sb, element hide_element = null) {
    if (e.type != element.type_element.title) throw new Exception("elemento di tipo errato!");

    int level = e.level;
    string ref_id = "ele_" + e.id.ToString(), title = e.has_content ? e.content : "<titolo>"
      , reference = get_ref(e.get_attribute_string("ref"));
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));
    string a = string.Format("<a id='{0}' class='anchor'></a>", ref_id)
      , st = (level == 1 ? "style='color:dimgray;border-top:1pt solid lightgrey;margin-top:20px;padding-top:15px;margin-bottom:0px;padding-bottom:0px;'"
      : (level >= 2 ? "style='color:dimgray;margin-bottom:0px;padding-bottom:0px;'" : "style='color:dimgray;'"));
    if (hide_element != null) { reference = get_url_cmd("view element id:" + hide_element.id.ToString()); title_ref_cmd = true; }
    if (reference != "")
      sb.AppendFormat("{5}<{0} {4}><a href='{2}' {3}>{1}</a></{0}>"
        , level == 0 ? "h1" : (level == 1 ? "h2" : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5")))
        , title, reference, !title_ref_cmd ? "target='blank'" : "", st, a);
    else
      sb.AppendFormat("{3}<{0} {2}><span>{1}</span></{0}>", level == 0 ? "h1" : (level == 1 ? "h2"
        : (level == 2 ? "h3" : (level == 3 ? "h4" : "h5"))), title, st, a);

    sb.AppendFormat("<br/>");
  }

  protected bool reference_cmd(string reference) { return reference.StartsWith("{@cmdurl='"); }
  protected string value_ref(string reference) { return get_ref(reference); }
  protected string get_ref(string ref_url) {
    return ref_url.StartsWith("{@cmdurl='") ? _core.config.var_value_par("vars.router-cmd"
      , ref_url.Substring(10, ref_url.Length - 12)) : ref_url;
  }

  protected void parse_type_element(element e, StringBuilder sb) {
    if (e.type != element.type_element.element) throw new Exception("elemento di tipo errato!");

    int level = e.level;
    string code = e.get_attribute_string("code"), type = e.get_attribute_string("type")
      , reference = get_ref(e.get_attribute_string("ref")), title = e.title;
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));
    string a = string.Format("<a id='{0}' class='anchor'></a>", "ele_" + e.id.ToString())
      , st = (level == 1 ? "style='background-color:azure;border-top:1pt solid lightgrey;margin-top:30px;padding-top:5px;margin-bottom:0px;padding-bottom:5px;'"
      : (level >= 2 ? "style='margin-bottom:0px;padding-bottom:0px;background-color:whitesmoke;'" : "style='background-color:aliceblue;'"));
    string open = e.sham ? "<a style='margin-left:20px;' href=\"" + get_url_cmd("view element id:" + e.id.ToString()) + "\"><img src='images/right-arrow-24-black.png' style='margin-bottom:4px;'></a>" : "";

    string tag = level == 0 ? "h1" : (level == 1 ? "h2" : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5")));
    if (reference != "")
      sb.AppendFormat("{5}<{0} {4}><a href='{2}' {3}><span class='doc-el-arrow'>&#10148;</span>{1}</a></{0}>"
        , tag, title, reference, !title_ref_cmd ? "target='blank'" : "", st, a);
    else
      sb.AppendFormat("{3}<{0} {2}><span><span class='doc-el-arrow'>&#10148;</span>{1}</span>{4}</{0}>"
        , tag, title, st, a, open);

    if ((!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(type)) && !e.sham) {
      sb.AppendFormat("<p class='lead' style='font-size:90%;color:darkgray;padding:0px;margin:0px;margin-bottom:15px;'>{0}{2}{1}</p>"
        , !string.IsNullOrEmpty(code) ? string.Format("code: <span style='font-weight:bold;'>{0}</span>", code) : ""
        , !string.IsNullOrEmpty(type) ? string.Format("type: <span style='font-weight:bold;'>{0}</span>", type) : ""
        , !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(type) ? ", " : "");
    } else sb.AppendFormat("<br/>");
  }

  public enum text_styles { underline, bold }
  protected text_styles[] get_styles(string styles) {
    if (string.IsNullOrEmpty(styles)) return new text_styles[] { };
    string ts = styles.Replace("][", ",").Replace("[", "").Replace("]", "");
    return ts.Split(new char[] { ',' }).Select(x => (text_styles)Enum.Parse(typeof(text_styles), x)).ToArray();
  }
  public bool is_style(string styles) { return !string.IsNullOrEmpty(styles); }

  protected void parse_type_text(element e, StringBuilder sb) {
    if (e.type != element.type_element.text) throw new Exception("elemento di tipo errato!");

    int level = e.level;
    string content = e.get_attribute_string("content"), el_style = e.get_attribute_string("style");

    string fs = level > 1 ? "font-size:95%;" : "", style = (is_style(el_style) ? string.Join(""
      , get_styles(el_style).Select(s => s == text_styles.bold ? "font-weight:bold;"
        : (s == text_styles.underline ? "font-style:italic;" : ""))) : "") + fs;
    sb.AppendFormat("<p class='lead' level='{2}' {1}>{0}</p>", content, style != "" ? "style='" + style + "'" : "", level);
  }

  #endregion
}