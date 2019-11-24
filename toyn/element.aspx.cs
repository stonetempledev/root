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

              // carico l'xml
              element el = element.load_xml(this.core, jo["xml"].ToString());

              // salvo il documento

            } else if (jo["action"].ToString() == "check_paste_xml") {
              StringBuilder text_xml = new StringBuilder();
              foreach (JValue j in jo["text_xml"] as JArray)
                text_xml.AppendLine(j.Value.ToString());
              xml_doc d = new xml_doc(); try { d.load_xml("<root>" + text_xml.ToString() + "</root>"); } catch { d = null; }
              if (d != null) {
                foreach (xml_node n in d.nodes("//*")) {
                  if (Enum.GetNames(typeof(child.xml_elements)).Contains(n.name))
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
        //Response.Flush();
        //Response.End();
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
        if (element_id == "") throw new Exception("NON HAI SPECIFICATO QUALE ELEMENTO APRIRE!");
        element el = load_element(int.Parse(element_id));
        int max_level = el.max_level();
        if (el == null) throw new Exception("ELEMENTO INESISTENTE!");

        // client vars
        url_xml.Value = master.url_cmd("xml element id:" + element_id);

        // document
        StringBuilder sb = new StringBuilder();
        parse_doc(max_level, el, sb);
        contents_doc.InnerHtml = sb.ToString();
        contents_xml.Visible = false;

        // menu
        sb.Clear();
        parse_menu(max_level, el, sb);
        menu.InnerHtml = sb.ToString();

      }
        // xml
      else if (c.action == "xml" && c.obj == "element") {
        string element_id = c.sub_cmd("id");
        if (element_id == "") throw new Exception("NON HAI SPECIFICATO QUALE ELEMENTO EDITARE!");
        element el = load_element(int.Parse(element_id));
        int max_level = el.max_level();
        if (el == null) throw new Exception("ELEMENTO INESISTENTE!");

        // client vars
        url_view.Value = master.url_cmd("view element id:" + element_id);

        // xml document
        contents_xml.Visible = true;
        contents.Visible = false;
        doc_xml.Value = el.get_xml(max_level).xml;

        // menu
        StringBuilder sb = new StringBuilder();
        parse_menu(max_level, el, sb, true);
        menu.InnerHtml = sb.ToString();

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) {
      log.log_err(ex);
      if (!elab_request) {
        blocks blk = new blocks();
        blk.add("err-label", "ERRORE: " + ex.Message);
        contents_xml.Visible = false;
        contents.Visible = true;
        contents.InnerHtml = blk.parse_blocks(_core);
      }
    }
  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

  protected element load_element(int element_id) {
    element res = null;

    DataTable dt = db_conn.dt_table(config.get_query("open-element").text, core, new Dictionary<string, object>() { { "element_id", element_id } });
    List<element> elements = new List<element>();
    foreach (DataRow re in dt.Rows) {
      // element
      if (db_provider.int_val(re["child_id"]) == 0) {
        element el = new element(_core, db_provider.int_val(re["element_level"]), db_provider.int_val(re["element_id"]), db_provider.str_val(re["element_type"]), db_provider.str_val(re["element_code"])
         , db_provider.str_val(re["element_title"]), db_provider.str_val(re["element_ref"]), db_provider.int_val(re["has_childs_elements"]) == 1, db_provider.int_val(re["back_element_id"])
         , db_provider.int_val(re["hide_element"]) == 1);
        elements.Add(el);

        if (db_provider.int_val(re["element_level"]) == 0) res = el;

        // element_parent
        if (db_provider.int_val(re["element_parent_id"]) > 0)
          elements.First(e => e.id == db_provider.int_val(re["element_parent_id"])).add_element(el, db_provider.int_val(re["element_content_id"]));
      }
        // child
      else {
        element_content.element_content_type et = (element_content.element_content_type)Enum.Parse(typeof(element_content.element_content_type), db_provider.str_val(re["element_type"]));
        element ep = elements.FirstOrDefault(e => e.id == db_provider.int_val(re["element_id"]));
        if (et == element_content.element_content_type.account)
          ep.add_account(new account(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_value"]), db_provider.str_val(re["child_notes"])), db_provider.int_val(re["child_content_id"]));
        else if (et == element_content.element_content_type.title)
          ep.add_title(new title(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_text"]), db_provider.str_val(re["child_ref"])), db_provider.int_val(re["child_content_id"]));
        else if (et == element_content.element_content_type.text)
          ep.add_text(new text(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_text"]), db_provider.str_val(re["child_style"])), db_provider.int_val(re["child_content_id"]));
        else if (et == element_content.element_content_type.value)
          ep.add_value(new value(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_value"]), db_provider.str_val(re["child_ref"]), db_provider.str_val(re["child_notes"])), db_provider.int_val(re["child_content_id"]));
      }
    }

    return res;
  }

  #region menu

  protected string element_ref_id(element e) { return e.has_element_content ? e.element_content.content_id.ToString() : "root"; }

  protected void parse_menu(int max_level, element e, StringBuilder sb, bool for_xml = false) {
    bool root = false;
    if (sb.Length == 0) { sb.AppendFormat("<ul class='nav flex-column'>\n"); root = true; }
    if (e.has_title) parse_title_menu(e.element_level, element_ref_id(e), e.title, sb
      , open_element_id: (e.element_level == max_level && e.has_child_elements ? e.id : 0)
      , back_element_id: e.back_element_id, for_xml: for_xml);
    bool first = true;
    foreach (element_content ec in e.childs.OrderBy(x => x.content_id)) {
      if (ec.content_type == element_content.element_content_type.title) {
        if (first && e.element_level >= 1) { sb.Append("<ul>"); first = false; }
        parse_title_menu(e.element_level + 1, ec.content_id.ToString(), ec.title, sb, for_xml: for_xml);
      } else if (ec.content_type == element_content.element_content_type.element && !ec.element_child.hide_element) {
        if (first && e.element_level >= 1) { sb.Append("<ul>"); first = false; }
        parse_menu(max_level, ec.element_child, sb, for_xml);
      }
    }
    sb.AppendFormat("{0}\n", !first ? "</ul>" : "");
    if (root) { sb.AppendFormat("</ul>\n"); }
  }

  protected void parse_title_menu(int level, string ref_id, title ce, StringBuilder sb, bool close = true, int open_element_id = 0
    , int back_element_id = 0, bool for_xml = false) {
    sb.AppendFormat(@"<li>{6}<a {3} style='{4}' href='#title_{1}'>{0}</a>{5}{2}"
      , ce.text, ref_id, close ? "</li>" : ""
      , level == 0 ? "class='h5'" : "", level == 0 ? "color:steelblue;"
        : (level == 1 ? "color:skyblue;margin-top:10px;padding-top:5px;border-top:1pt solid dimgray;display:block;" : "font-size:90%;color:lightcyan;")
        , open_element_id > 0 ? "<a style='margin-left:5px;font-size:90%;color:lightcyan;' href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + open_element_id.ToString()) + "\">(...)</a>" : ""
        , back_element_id > 0 ? "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + back_element_id.ToString())
        + "\" style='margin-right:10px;'><img src='images/left-chevron-24.png' width='20' height='20'></a>" : "");
  }

  #endregion

  #region document

  protected void parse_doc(int max_level, element e, StringBuilder sb) {
    if (e.has_title) parse_title_element(e, sb);
    if (!e.hide_element) {
      foreach (element_content ec in e.childs.OrderBy(x => x.content_id)) {
        if (ec.content_type == element_content.element_content_type.title)
          parse_title(e.element_level + 1, ec.content_id.ToString(), ec.title, sb);
        else if (ec.content_type == element_content.element_content_type.text)
          parse_text(e.element_level + 1, ec.text, sb);
        else if (ec.content_type == element_content.element_content_type.value)
          parse_value(e.element_level + 1, ec.value, sb);
        else if (ec.content_type == element_content.element_content_type.account)
          parse_account(e.element_level + 1, ec.account, sb);
        else if (ec.content_type == element_content.element_content_type.element)
          parse_doc(max_level, ec.element_child, sb);
      }
    }
  }

  protected void parse_title(int level, string ref_id, title ce, StringBuilder sb, element hide_element = null) {
    bool view_level = false;
    string a = string.Format("<a id='title_{0}' class='anchor'></a>", ref_id)
      , st = (level == 1 ? "style='color:dimgray;border-top:1pt solid lightgrey;margin-top:45px;padding-top:15px;margin-bottom:0px;padding-bottom:0px;'"
      : (level >= 2 ? "style='color:dimgray;margin-bottom:0px;padding-bottom:0px;'" : "style='color:dimgray;'"));
    string title_ref = ce.title_ref; bool title_ref_cmd = ce.title_ref_cmd;
    if (hide_element != null) { title_ref = get_url_cmd("view element id:" + hide_element.element_content.element.id.ToString()); title_ref_cmd = true; }
    if (title_ref != "")
      sb.AppendFormat("{5}<{0} {4}><a href='{2}' {3}>{1}</a></{0}>", level == 0 ? "h1" : (level == 1 ? "h2"
        : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5"))), (view_level ? level.ToString() + ": " : "") + ce.text + (hide_element != null ? "..." : "")
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
    string a = string.Format("<a id='title_{0}' class='anchor'></a>", ref_id)
      , st = (level == 1 ? "style='border-top:1pt solid lightgrey;margin-top:45px;padding-top:15px;margin-bottom:0px;padding-bottom:0px;'"
      : (level >= 2 ? "style='margin-bottom:0px;padding-bottom:0px;'" : ""));
    string title_ref = e.title.title_ref; bool title_ref_cmd = e.title.title_ref_cmd;
    if (e.hide_element) { title_ref = get_url_cmd("view element id:" + e.element_content.element.id.ToString()); title_ref_cmd = true; }
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
        , !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(type) ? ", ": "");
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