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
              int parent_id = jo["parent_id"].ToString() != "" ? Convert.ToInt32(jo["parent_id"]) : 0, max_level = Convert.ToInt32(jo["max_level"]);
              List<element> els_bck = element.load_xml(this.core, load_attributes(), jo["xml_bck"].ToString(), parent_id)
                , els = element.load_xml(this.core, load_attributes(), jo["xml"].ToString(), parent_id);
              check_and_del(els_bck, els);
              save_elements(els, parent_id);

              res.doc_xml = element.get_doc(els).xml;

              // menu
              StringBuilder sb = new StringBuilder();
              parse_menu(els, sb, true, max_level);
              res.menu_html = sb.ToString();

            } else if (jo["action"].ToString() == "check_paste_xml") {
              StringBuilder text_xml = new StringBuilder();
              foreach (JValue j in jo["text_xml"] as JArray)
                text_xml.AppendLine(j.Value.ToString());
              xml_doc d = new xml_doc(); try { d.load_xml("<root>" + text_xml.ToString() + "</root>"); } catch { d = null; }
              if (d != null) {
                foreach (xml_node n in d.nodes("//*")) {
                  if (n.get_bool("sham")) {
                    if (n.get_attr("id") != "" && n.get_attr("from_id") == "")
                      n.set_attr("from_id", n.get_attr("id"));
                  } else if (Enum.GetNames(typeof(element.type_element)).Contains(n.name))
                    n.set_attr("id", "");
                }
                text_xml.Clear();
                foreach (xml_node cn in d.root_node.childs())
                  text_xml.AppendLine(cn.node.OuterXml);
                res.contents = text_xml.ToString();
              } else res.contents = text_xml.ToString();
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

        // test - load xml -> save into table
        if (element_id == "") {
          db_conn.exec(string.Format(@"truncate table [elements];
            truncate table [elements_contents];
            truncate table [elements_attrs_datetime];
            truncate table [elements_attrs_integer];
            truncate table [elements_attrs_link];
            truncate table [elements_attrs_text];
            truncate table [elements_attrs_real];
            truncate table [elements_attrs_varchar];"));
          save_elements(element.load_xml(this.core, load_attributes(), File.ReadAllText("C:\\tmp\\toyn\\test.xml")));
        }

        List<element> els = load_elements(out max_level, element_id != "" ? int.Parse(element_id) : (int?)null, _max_level);

        // client vars
        url_xml.Value = master.url_cmd("xml element" + (element_id != "" ? " id:" + element_id : ""));
        max_lvl.Value = max_level.ToString();

        // document
        StringBuilder sb = new StringBuilder();
        parse_elements(els, sb);
        contents_doc.InnerHtml = sb.ToString();
        contents_xml.Visible = false;
        sb.Clear();

        // menu
        parse_menu(els, sb, false, max_level);
        menu.InnerHtml = sb.ToString();

      }
        // xml
      else if (c.action == "xml" && c.obj == "element") {

        string element_id = c.sub_cmd("id"); int max_level = 0;
        List<element> els = load_elements(out max_level, element_id != "" ? int.Parse(element_id) : (int?)null, _max_level);

        // client vars
        url_view.Value = master.url_cmd("view element" + (element_id != "" ? " id:" + element_id : ""));
        id_element.Value = element_id.ToString();
        parent_id.Value = els.Count > 0 ? els[0].parent_id.ToString() : "";
        max_lvl.Value = max_level.ToString();

        // xml document
        contents_xml.Visible = true;
        contents.Visible = false;
        doc_xml.Value = doc_xml_bck.Value = element.get_doc(els).root_node.inner_xml;

        // menu
        StringBuilder sb = new StringBuilder();
        parse_menu(els, sb, true, max_level);
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

  protected void check_and_del(List<element> els_b, List<element> els) {
    // documento aggiornato
    List<element> all = get_all(els);

    // check ids univoci
    foreach (element c in all) {
      if (c.id > 0 && all.Count(x => x.id == c.id) > 1)
        throw new Exception(string.Format("l'elemento '{0}' con id {1} è duplicato più volte e non è possibile!"
          , c.type.ToString(), c.id));
    }

    // ciclo pulizia
    foreach (element c in get_all(els_b)) {
      if (all.FirstOrDefault(x => x.type == c.type && x.id == c.id) == null) {

        string dels = "";
        foreach (var v in Enum.GetValues(typeof(attribute.attribute_type)))
          dels += string.Format("{1}delete from [elements_attrs_{2}] where element_id = {0};"
            , c.id, dels != "" ? "\n" : "", v.ToString());

        db_conn.exec(string.Format(@"{1}
          delete from [elements] where element_id = {0};
          delete from [elements_contents] where element_id = {0} or child_element_id = {0};", c.id, dels));
      }
    }
  }

  protected void save_elements(List<element> els, int parent_id = 0) {
    foreach (element el in els)
      save_element(el);
  }

  protected long save_element(element e) {

    // elements
    bool added = false;
    if (e.id == 0) {
      added = true;
      e.id = int.Parse(db_conn.exec(string.Format(@"insert into [elements] (element_type, element_title)
        values ({0}, {1})", db_provider.str_qry(e.type.ToString()), db_provider.str_qry(e.title)), true));
    } else
      db_conn.exec(string.Format(@"update [elements] set element_type = {0}, element_title = {1} 
        where element_id = {2}", db_provider.str_qry(e.type.ToString()), db_provider.str_qry(e.title), e.id));

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

        db_conn.exec(string.Format(@"if exists (select top 1 1 from [elements_attrs_{0}] ea
          join attributes a on a.attribute_id = ea.attribute_id
          where ea.element_id = {2} and a.attribute_code = '{3}')
            update ea set ea.value = {1} 
             from [elements_attrs_{0}] ea
             join attributes a on a.attribute_id = ea.attribute_id
             where ea.element_id = {2} and a.attribute_code = '{3}';
          else
            insert into [elements_attrs_{0}] (element_id, attribute_id, [value])
             select {2} as element_id, a.attribute_id, {1} as value
             from attributes a 
             where a.element_type = '{4}' and a.attribute_code = '{3}'"
          , a.type.ToString(), qry_val, e.id, a.code, e.type.ToString()));
      } else {
        if (!added)
          db_conn.exec(string.Format(@"delete ea from [elements_attrs_{0}] ea 
            join attributes a on a.attribute_id = ea.attribute_id
            where ea.element_id = {1} and a.element_type = '{2}'"
            , a.type.ToString(), e.id, e.type.ToString()));
      }
    }

    // childs
    if (!e.sham) {
      if (!added) db_conn.exec(string.Format("delete from elements_contents where element_id = {0}", e.id));
      foreach (element ec in e.childs) {
        save_element(ec);
        db_conn.exec(string.Format(@"insert into elements_contents (element_id, child_element_id)
         values ({0}, {1})", e.id, ec.id));
      }
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

  protected List<element> load_elements(out int out_max_level, int? element_id = null, int? max_level = null) {
    List<element> els = new List<element>(); out_max_level = -1;
    string sql = !element_id.HasValue ? core.parse(config.get_query("open-roots-element").text
      , new Dictionary<string, object>() { { "filter_level", max_level.HasValue ? "h.livello <= " + max_level.Value.ToString() : "1 = 1" } })
      : core.parse(config.get_query("open-element").text
        , new Dictionary<string, object>() { { "id_element", element_id }
        , { "filter_level", max_level.HasValue ? "h.livello <= " + max_level.Value.ToString() : "1 = 1" }});
    DataTable dt = db_conn.dt_table(sql);
    List<element> elements = new List<element>(); element e = null;
    foreach (DataRow re in dt.Rows) {

      // element
      long parent_id = db_provider.long_val(re["parent_id"]), contents_id = db_provider.long_val(re["elements_contents_id"])
        , id = db_provider.long_val(re["element_id"]);
      int livello = db_provider.int_val(re["livello"]);
      string element_type = db_provider.str_val(re["element_type"]), title = db_provider.str_val(re["element_title"]);
      bool has_childs = db_provider.int_val(re["has_childs"]) == 1
        , has_child_elements = db_provider.int_val(re["has_child_elements"]) == 1;

      if (out_max_level < livello) out_max_level = livello;

      if (e == null || e.id != id) {
        e = new element(_core, (element.type_element)Enum.Parse(typeof(element.type_element), element_type), title, livello
          , id, parent_id, contents_id, has_childs, has_child_elements, sham: max_level.HasValue ? livello >= max_level : false);
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

  protected void parse_menu(List<element> els, StringBuilder sb, bool for_xml = false, int? max_level = null) {
    foreach (element el in els.Where(x => x.type == element.type_element.element || x.type == element.type_element.title)
      .OrderBy(x => x.content_id)) parse_menu(el, sb, true, for_xml, max_level);
  }

  protected void parse_menu(element e, StringBuilder sb, bool root = false, bool for_xml = false, int? max_level = null) {
    if (root) sb.AppendFormat("<ul class='nav flex-column' style='padding:0px;margin-top:10px;' >\n");
    if (e.has_title) {
      parse_title_menu(e, e.title, sb
        , open_element_id: (e.level == max_level && e.has_childs ? e.id : 0)
        , back_element_id: e.back_id, for_xml: for_xml);
    }

    bool first = true;
    foreach (element ec in e.childs.OrderBy(x => x.content_id)) {
      if (ec.type == element.type_element.title) {
        if (first && e.level >= 1) { sb.Append("<ul>"); first = false; }
        parse_title_menu(ec, ec.has_content ? ec.content : "<titolo>", sb, for_xml: for_xml);
      } else if (ec.type == element.type_element.element) {
        if (first && e.level >= 1) { sb.Append("<ul>"); first = false; }
        parse_menu(ec, sb, false, for_xml, max_level);
      }
    }

    sb.AppendFormat("{0}\n", !first ? "</ul>" : "");
    if (root) { sb.AppendFormat("</ul>\n"); }
  }

  protected void parse_title_menu(element e, string title, StringBuilder sb, bool close = true
    , long open_element_id = 0, long back_element_id = 0, bool for_xml = false) {
    int level = e.level;
    string ref_id = "ele_" + e.id.ToString(), reference = e.get_attribute_string("ref");

    sb.AppendFormat(@"<li style='{7}'><div style='display:block;'>{6}<a {3} style='{4}' href='#{1}'>{0}{5}</a></div>{2}"
      , title, ref_id, close ? "</li>" : "", e.type == element.type_element.element && e.level == 0 ? "class='h5'" : ""
    , level == 0 ? "color:steelblue;"
      : (level == 1 ? "color:skyblue;margin-top:10px;padding-top:5px;padding-left:3px;" : "font-size:90%;color:lightcyan;")
    , open_element_id > 0 ? "<a style='margin-left:15px;' href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + open_element_id.ToString()) + "\">"
      + "<img src='images/right-arrow-16.png' style='margin-bottom:4px;'></a>" : ""
    , back_element_id > 0 ? "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + back_element_id.ToString())
      + "\" style='margin-right:10px;'><img src='images/left-chevron-24.png' style='margin-left:3px;margin-bottom:4px;' width='20' height='20'></a>" : ""
    , "");
  }

  #endregion

  #region document

  protected void parse_elements(List<element> els, StringBuilder sb) {
    foreach (element e in els.OrderBy(x => x.content_id))
      parse_element(e, sb);
  }

  protected void parse_element(element e, StringBuilder sb) {
    if (e.has_title && e.type == element.type_element.element)
      parse_type_element(e, sb);
    if (!e.sham) {
      foreach (element ec in e.childs.OrderBy(x => x.content_id)) {
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
      , st = (level == 1 ? "style='color:dimgray;border-top:1pt solid lightgrey;margin-top:45px;padding-top:15px;margin-bottom:0px;padding-bottom:0px;'"
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
      , st = (level == 1 ? "style='background-color:azure;border-top:1pt solid lightgrey;margin-top:45px;padding-top:5px;margin-bottom:0px;padding-bottom:5px;'"
      : (level >= 2 ? "style='margin-bottom:0px;padding-bottom:0px;background-color:whitesmoke;'" : "style='background-color:aliceblue;'"));
    string open = e.sham ? "<a style='margin-left:20px;' href=\"" + get_url_cmd("view element id:" + e.id.ToString()) + "\"><img src='images/right-arrow-24-black.png' style='margin-bottom:4px;'></a>" : "";

    string tag = level == 0 ? "h1" : (level == 1 ? "h2"
          : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5")));
    if (reference != "")
      sb.AppendFormat("{5}<{0} {4}><a href='{2}' {3}>{1}</a></{0}>"
        , tag, title
        , reference, !title_ref_cmd ? "target='blank'" : "", st, a);
    else
      sb.AppendFormat("{3}<{0} {2}><span>{1}</span>{4}</{0}>", tag, title, st, a, open);

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