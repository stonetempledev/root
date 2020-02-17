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
              List<element> els_bck = element.load_xml(this.core, jo["xml_bck"].ToString(), parent_id)
                , els = element.load_xml(this.core, jo["xml"].ToString(), parent_id);
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
                  if (n.name == child.xml_elements.hide_element.ToString()) {
                    if (n.get_attr("id") != "" && n.get_attr("from_id") == "")
                      n.set_attr("from_id", n.get_attr("id"));
                  } else if (Enum.GetNames(typeof(child.xml_elements)).Contains(n.name))
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
        string element_id = c.sub_cmd("id");
        List<element> els = load_element(element_id != "" ? int.Parse(element_id) : (int?)null);

        // client vars
        url_xml.Value = master.url_cmd("xml element" + (element_id != "" ? " id:" + element_id : ""));

        // document
        StringBuilder sb = new StringBuilder();
        parse_doc(els, sb);
        contents_doc.InnerHtml = sb.ToString();
        contents_xml.Visible = false;
        sb.Clear();

        // menu
        int max_level = element.max_level(els);
        parse_menu(els, sb, false, max_level: max_level);
        menu.InnerHtml = sb.ToString();

      }
        // xml
      else if (c.action == "xml" && c.obj == "element") {
        string element_id = c.sub_cmd("id");
        List<element> els = load_element(element_id != "" ? int.Parse(element_id) : (int?)null);

        // client vars
        int max_level = element.max_level(els);
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

  protected List<child> get_all(List<element> els, List<child> l = null) {
    List<child> res = l == null ? new List<child>() : l;
    foreach (element el in els) {
      if (el.id == 0 || (el.id > 0 && res.FirstOrDefault(x => x.type == el.type && x.id == el.id) == null))
        res.Add(el);
      foreach (child c in el.childs.Select(c => c.child)) {
        if (c.id == 0 || (c.id > 0 && res.FirstOrDefault(x => x.type == c.type && x.id == c.id) == null))
          res.Add(c);
        if (c.type == child.content_type.element)
          get_all(new List<element>() { (element)c }, res);
      }
    }
    return res;
  }

  protected void check_and_del(List<element> els_b, List<element> els) {
    // documento aggiornato
    List<child> all = get_all(els);

    // check ids
    foreach (child c in all) {
      if (c.id > 0 && all.Count(x => x.type == c.type && x.id == c.id) > 1)
        throw new Exception(string.Format("l'elemento '{0}' con id {1} è duplicato più volte e non è possibile!"
          , c.type.ToString(), c.id));
    }

    // ciclo pulizia
    foreach (child c in get_all(els_b)) {
      if (all.FirstOrDefault(x => x.type == c.type && x.id == c.id) == null) {

        if (c.type == child.content_type.element)
          db_conn.exec(string.Format(@"delete from [elements] where element_id = {0}", c.id));
        else if (c.type == child.content_type.account)
          db_conn.exec(string.Format(@"delete from [accounts] where account_id = {0}", c.id));
        else if (c.type == child.content_type.title)
          db_conn.exec(string.Format(@"delete from [titles] where title_id = {0}", c.id));
        else if (c.type == child.content_type.text)
          db_conn.exec(string.Format(@"delete from [texts] where text_id = {0}", c.id));
        else if (c.type == child.content_type.value)
          db_conn.exec(string.Format(@"delete from [values] where value_id = {0}", c.id));
        else throw new Exception("tipo elemento '" + c.type.ToString() + "' non gestito durante le cancellazioni");

        db_conn.exec(string.Format(@"delete from elements_contents 
          where child_content_type = '{0}' and child_id = {1}", c.type.ToString(), c.id));
      }
    }
  }

  protected void save_elements(List<element> els, int parent_id = 0) {

    if (els.FirstOrDefault(e => e.id > 0) != null)
      db_conn.exec(string.Format(@"delete from elements_contents 
        where element_id {0} and child_content_type = {1} and child_id in ({2})"
        , parent_id > 0 ? " = " + parent_id.ToString() : " is null", db_provider.str_qry(child.content_type.element.ToString()), string.Join(",", els.Where(e => e.id > 0).Select(e => e.id))));

    foreach (element el in els) {
      save_element(el);

      db_conn.exec(string.Format(@"insert into elements_contents (element_id, child_content_type, child_id)
        values ({0}, {1}, {2})", parent_id > 0 ? parent_id.ToString() : "null", db_provider.str_qry(el.type.ToString()), el.id));
    }
  }

  protected void save_element(element e) {

    // elements
    if (e.id == 0) {
      e.id = int.Parse(db_conn.exec(string.Format(@"insert into [elements] (element_type, element_code, element_title, element_ref)
        values ({0}, {1}, {2}, {3})", db_provider.str_qry(e.element_type.ToString()), db_provider.str_qry(e.element_code.ToString())
          , db_provider.str_qry(e.title.text), db_provider.str_qry(e.title.title_ref_value)), true));
    } else
      db_conn.exec(string.Format(@"update [elements] set element_type = {0}, element_code = {1}
          , element_title = {2}, element_ref = {3} where element_id = {4}"
        , db_provider.str_qry(e.element_type.ToString()), db_provider.str_qry(e.element_code.ToString())
        , db_provider.str_qry(e.title.text), db_provider.str_qry(e.title.title_ref_value), e.id));

    // childs: elements, title, text, values, accounts
    db_conn.exec(string.Format("delete from elements_contents where element_id = {0}", e.id));

    foreach (element_content ec in e.childs) {
      // element
      if (ec.child.type == child.content_type.element) {
        save_element(ec.child as element);
      }
        // title
      else if (ec.child.type == child.content_type.title) {
        title t = (title)ec.child;
        if (t.id == 0)
          t.id = int.Parse(db_conn.exec(string.Format(@"insert into titles (title_text, title_ref)
            values ({0}, {1})", db_provider.str_qry(t.text.ToString())
              , db_provider.str_qry(t.title_ref_value.ToString())), true));
        else
          db_conn.exec(string.Format(@"update titles set title_text = {0}, title_ref = {1}
            where title_id = {2}"
            , db_provider.str_qry(t.text), db_provider.str_qry(t.title_ref_value), t.id));
      }
        // text
      else if (ec.child.type == child.content_type.text) {
        text t = (text)ec.child;
        if (t.id == 0)
          t.id = int.Parse(db_conn.exec(string.Format(@"insert into texts (text_style, text_content)
            values ({0}, {1})", db_provider.str_qry(t.text_style.ToString())
              , db_provider.str_qry(t.text_content.ToString())), true));
        else
          db_conn.exec(string.Format(@"update texts set text_style = {0}, text_content = {1}
            where text_id = {2}"
            , db_provider.str_qry(t.text_style), db_provider.str_qry(t.text_content), t.id));
      }
        // value
      else if (ec.child.type == child.content_type.value) {
        value v = (value)ec.child;
        if (v.id == 0)
          v.id = int.Parse(db_conn.exec(string.Format(@"insert into [values] (value_content, value_ref, value_notes)
            values ({0}, {1}, {2})", db_provider.str_qry(v.value_content), db_provider.str_qry(v.value_ref)
              , db_provider.str_qry(v.value_notes)), true));
        else
          db_conn.exec(string.Format(@"update [values] set value_content = {0}, value_ref = {1}, value_notes = {2}
            where value_id = {3}"
            , db_provider.str_qry(v.value_content), db_provider.str_qry(v.value_ref)
            , db_provider.str_qry(v.value_notes), v.id));
      }
        // account
      else if (ec.child.type == child.content_type.account) {
        account a = (account)ec.child;
        if (a.id == 0)
          a.id = int.Parse(db_conn.exec(string.Format(@"insert into accounts (account_user, account_password, account_notes)
            values ({0}, {1}, {2})", db_provider.str_qry(a.account_user.ToString())
              , db_provider.str_qry(a.account_password.ToString())
              , db_provider.str_qry(a.account_notes)), true));
        else
          db_conn.exec(string.Format(@"update accounts set account_user = {0}, account_password = {1}
          , account_notes = {2} where account_id = {3}"
            , db_provider.str_qry(a.account_user.ToString())
            , db_provider.str_qry(a.account_password), db_provider.str_qry(a.account_notes), a.id));
      } else throw new Exception("elemento '" + ec.child.type.ToString() + "' non gestito per il salvataggio!");

      // contents
      db_conn.exec(string.Format(@"insert into elements_contents (element_id, child_content_type, child_id)
         values ({0}, {1}, {2})", e.id, db_provider.str_qry(ec.child.type.ToString()), ec.child.id));
    }
  }

  protected List<element> load_element(int? element_id = null) {
    List<element> res = new List<element>();

    DataTable dt = db_conn.dt_table(core.parse(config.get_query("open-element").text
      , new Dictionary<string, object>() { { "filter_element", element_id.HasValue 
        ? " = " + element_id.ToString() : " in (select element_id from vw_elements where child_id is null and parent_id is null)" } }));
    List<element> elements = new List<element>();
    foreach (DataRow re in dt.Rows) {
      // element
      if (db_provider.int_val(re["child_id"]) == 0) {
        element el = new element(_core, db_provider.int_val(re["element_level"]), db_provider.int_val(re["element_id"])
          , db_provider.int_val(re["parent_id"]), db_provider.str_val(re["element_type"]), db_provider.str_val(re["element_code"])
          , db_provider.str_val(re["element_title"]), db_provider.int_val(re["element_content_id"]), db_provider.str_val(re["element_ref"]), db_provider.int_val(re["has_childs_elements"]) == 1
          , db_provider.int_val(re["back_element_id"]), db_provider.int_val(re["hide_element"]) == 1);
        elements.Add(el);

        if (db_provider.int_val(re["element_level"]) == 0) res.Add(el);

        // element_parent
        if (db_provider.int_val(re["element_parent_id"]) > 0)
          elements.First(e => e.id == db_provider.int_val(re["element_parent_id"])).add_element(el, db_provider.int_val(re["element_content_id"]));
      }
        // child
      else {
        child.content_type et = (child.content_type)Enum.Parse(typeof(child.content_type), db_provider.str_val(re["element_type"]));
        element ep = elements.FirstOrDefault(e => e.id == db_provider.int_val(re["element_id"]));
        if (et == child.content_type.account)
          ep.add_account(new account(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_value"]), db_provider.str_val(re["child_notes"])), db_provider.int_val(re["child_content_id"]));
        else if (et == child.content_type.title)
          ep.add_title(new title(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_text"]), db_provider.str_val(re["child_ref"])), db_provider.int_val(re["child_content_id"]));
        else if (et == child.content_type.text)
          ep.add_text(new text(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_text"]), db_provider.str_val(re["child_style"])), db_provider.int_val(re["child_content_id"]));
        else if (et == child.content_type.value)
          ep.add_value(new value(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_value"]), db_provider.str_val(re["child_ref"]), db_provider.str_val(re["child_notes"])), db_provider.int_val(re["child_content_id"]));
      }
    }

    return res;
  }

  #region menu

  protected string element_ref_id(element e) { return "el_" + e.id.ToString(); }

  protected void parse_menu(List<element> els, StringBuilder sb, bool for_xml = false, int? max_level = null) {
    foreach (element el in els.OrderBy(x => x.content_id))
      parse_menu(el, sb, true, for_xml, max_level, true);
  }

  protected void parse_menu(element e, StringBuilder sb, bool root = false, bool for_xml = false, int? max_level = null, bool first_child_title = false) {
    if (root) sb.AppendFormat("<ul class='nav flex-column' style='padding:0px;margin-top:10px;' >\n");
    if (e.has_title) {
      parse_title_menu(e.element_level, element_ref_id(e), e.title, sb
        , first_child: !root ? first_child_title : false
        , open_element_id: (e.element_level == max_level + 1 && e.has_child_elements ? e.id : 0)
        , back_element_id: e.back_element_id, for_xml: for_xml);
      if (!root) first_child_title = false;
    }
    bool first = true;
    foreach (element_content ec in e.childs.OrderBy(x => x.content_id)) {
      if (ec.child.type == child.content_type.title) {
        if (first && e.element_level >= 1) { sb.Append("<ul>"); first = false; }
        parse_title_menu(e.element_level + 1, "tit_" + ec.child.id.ToString(), ec.title, sb, first_child: first_child_title, for_xml: for_xml);
        first_child_title = false;
      } else if (ec.child.type == child.content_type.element && !ec.element_child.hide_element) {
        if (first && e.element_level >= 1) { sb.Append("<ul>"); first = false; }
        parse_menu(ec.element_child, sb, false, for_xml, max_level, first_child_title);
        first_child_title = false;
      }
    }
    sb.AppendFormat("{0}\n", !first ? "</ul>" : "");
    if (root) { sb.AppendFormat("</ul>\n"); }
  }

  protected void parse_title_menu(int level, string ref_id, title ce, StringBuilder sb, bool first_child = false, bool close = true, int open_element_id = 0
    , int back_element_id = 0, bool for_xml = false) {
    sb.AppendFormat(@"<li style='{7}'><div style='display:block;{8}'>{6}<a {3} style='{4}' href='#{1}'>{0}{5}</a></div>{2}"
    , ce.text, ref_id, close ? "</li>" : "", level == 0 ? "class='h5'" : ""
    , level == 0 ? "color:steelblue;"
      : (level == 1 ? "color:skyblue;margin-top:10px;padding-top:5px;padding-left:3px;" : "font-size:90%;color:lightcyan;")
    , open_element_id > 0 ? "<a style='float:right;margin-right:3px;' href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + open_element_id.ToString()) + "\">"
      + "<img src='images/right-arrow-16.png' style='margin-bottom:4px;'></a>" : ""
    , back_element_id > 0 ? "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + back_element_id.ToString())
      + "\" style='margin-right:10px;'><img src='images/left-chevron-24.png' style='margin-left:3px;margin-bottom:4px;' width='20' height='20'></a>" : ""
    , level == 0 ? "background-color:#444a50;padding-left:3px;padding-top:4px;padding-bottom:4px;" : ""
    , level == 1 && !first_child ? "border-top:1pt solid dimgray;" : "");
  }

  #endregion

  #region document

  protected void parse_doc(List<element> els, StringBuilder sb) {
    foreach (element e in els.OrderBy(x => x.content_id))
      parse_doc(e, sb);
  }

  protected void parse_doc(element e, StringBuilder sb) {
    if (e.has_title) parse_title_element(e, sb);
    if (!e.hide_element) {
      foreach (element_content ec in e.childs.OrderBy(x => x.content_id)) {
        if (ec.child.type == child.content_type.title)
          parse_title(e.element_level + 1, "tit_" + ec.child.id.ToString(), ec.title, sb);
        else if (ec.child.type == child.content_type.text)
          parse_text(e.element_level + 1, ec.text, sb);
        else if (ec.child.type == child.content_type.value)
          parse_value(e.element_level + 1, ec.value, sb);
        else if (ec.child.type == child.content_type.account)
          parse_account(e.element_level + 1, ec.account, sb);
        else if (ec.child.type == child.content_type.element)
          parse_doc(ec.element_child, sb);
      }
    }
  }

  protected void parse_title(int level, string ref_id, title ce, StringBuilder sb, element hide_element = null) {
    bool view_level = false;
    string a = string.Format("<a id='{0}' class='anchor'></a>", ref_id)
      , st = (level == 1 ? "style='color:dimgray;border-top:1pt solid lightgrey;margin-top:45px;padding-top:15px;margin-bottom:0px;padding-bottom:0px;'"
      : (level >= 2 ? "style='color:dimgray;margin-bottom:0px;padding-bottom:0px;'" : "style='color:dimgray;'"));
    string title_ref = ce.title_ref; bool title_ref_cmd = ce.title_ref_cmd;
    if (hide_element != null) { title_ref = get_url_cmd("view element id:" + hide_element.id.ToString()); title_ref_cmd = true; }
    if (title_ref != "")
      sb.AppendFormat("{5}<{0} {4}><a href='{2}' {3}>{1}</a></{0}>"
        , level == 0 ? "h1" : (level == 1 ? "h2" : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5")))
        , (view_level ? level.ToString() + ": " : "") + ce.text + (hide_element != null ? "..." : "")
        , title_ref, !title_ref_cmd ? "target='blank'" : "", st, a);
    else
      sb.AppendFormat("{3}<{0} {2}><span>{1}</span></{0}>", level == 0 ? "h1" : (level == 1 ? "h2"
        : (level == 2 ? "h3" : (level == 3 ? "h4" : "h5"))), (view_level ? level.ToString() + ": " : "") + ce.text, st, a);

    sb.AppendFormat("<br/>");
  }

  protected void parse_title_element(element e, StringBuilder sb) {
    int level = e.element_level; string ref_id = element_ref_id(e);
    string code = e.element_code; string type = e.element_type;
    bool view_level = false;
    string a = string.Format("<a id='{0}' class='anchor'></a>", ref_id)
      , st = (level == 1 ? "style='border-top:1pt solid lightgrey;margin-top:45px;padding-top:15px;margin-bottom:0px;padding-bottom:0px;'"
      : (level >= 2 ? "style='margin-bottom:0px;padding-bottom:0px;'" : "style='background-color:whitesmoke;'"));
    string title_ref = e.title.title_ref; bool title_ref_cmd = e.title.title_ref_cmd;
    if (e.hide_element) { title_ref = get_url_cmd("view element id:" + e.id.ToString()); title_ref_cmd = true; }
    if (title_ref != "")
      sb.AppendFormat("{5}<{0} {4}><a href='{2}' {3}>{1}</a></{0}>", level == 0 ? "h1" : (level == 1 ? "h2"
        : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5"))), (view_level ? level.ToString() + ": " : "") + e.title.text + (e.hide_element ? "..." : "")
        , title_ref, !title_ref_cmd ? "target='blank'" : "", st, a);
    else
      sb.AppendFormat("{3}<{0} {2}><span>{1}</span></{0}>", level == 0 ? "h1" : (level == 1 ? "h2"
        : (level == 2 ? "h3" : (level == 3 ? "h4" : "h5"))), (view_level ? level.ToString() + ": " : "") + e.title.text, st, a);

    if ((!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(type)) && !e.hide_element) {
      sb.AppendFormat("<p class='lead' style='font-size:90%;color:darkgray;padding:0px;margin:0px;margin-bottom:15px;'>{0}{2}{1}</p>"
        , !string.IsNullOrEmpty(code) ? string.Format("code: <span style='font-weight:bold;'>{0}</span>", code) : ""
        , !string.IsNullOrEmpty(type) ? string.Format("type: <span style='font-weight:bold;'>{0}</span>", type) : ""
        , !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(type) ? ", " : "");
    } else sb.AppendFormat("<br/>");
  }

  protected void parse_text(int level, text te, StringBuilder sb) {
    string fs = level > 1 ? "font-size:95%;" : "", style = (te.is_style() ? string.Join(""
      , te.get_styles().Select(s => s == text.text_styles.bold ? "font-weight:bold;"
        : (s == text.text_styles.underline ? "font-style:italic;" : ""))) : "") + fs;
    sb.AppendFormat("<p class='lead' level='{2}' {1}>{0}</p>", te.text_content, style != "" ? "style='" + style + "'" : "", level);
  }

  protected void parse_value(int level, value v, StringBuilder sb) {
    string hn = !string.IsNullOrEmpty(v.value_notes) ? string.Format("<span style='margin-left:10px;color:silver;font-style:italic;'>({0})</span>", v.value_notes) : "";
    string st = "style='margin-left:5px;'";
    if (v.value_ref != "")
      sb.AppendFormat("<li {4}><a href='{1}' {2}>{0}</a>{3}</li>", v.value_content, v.value_ref
        , !v.value_ref_cmd ? "target='blank'" : "", hn, st);
    else
      sb.AppendFormat("<li {2}><span>{0}</span>{1}</li>", v.value_content, hn, st);
  }

  protected void parse_account(int level, account a, StringBuilder sb) {
    string hn = !string.IsNullOrEmpty(a.account_notes) ? string.Format("<span style='margin-left:10px;color:silver;font-style:italic;'>({0})</span>", a.account_notes) : "";
    string st = "style='margin-left:5px;'";
    sb.AppendFormat("<li {3}><span>{0}{1}</span>{2}</li>", a.account_user
      , !string.IsNullOrEmpty(a.account_password) ? "/" + a.account_password : "", hn, st);
  }

  #endregion

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
  }
}