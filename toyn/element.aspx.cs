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
    try {

      if (Request.Headers["toyn-post"] != null) {
        json_result res = new json_result(json_result.type_result.ok);

        try {
          string json = String.Empty;
          Request.InputStream.Position = 0;
          using (var inputStream = new StreamReader(Request.InputStream)) {
            json = inputStream.ReadToEnd();
          }

          if (!string.IsNullOrEmpty(json)) {
            JObject jo = JObject.Parse(json);
            if (jo["action"].ToString() == "save_element") {
              string xml = jo["xml"].ToString();
              xml_doc d = new xml_doc() { xml = xml };
              d.save("c:\\tmp\\o.xml");
            }
          } else throw new Exception("nessun dato da elaborare!");

        } catch (Exception ex) { res = new json_result(json_result.type_result.error, ex.Message); }

        Response.Clear();
        Response.ContentType = "application/json";
        Response.Write(JsonConvert.SerializeObject(res));
        Response.Flush();
        Response.End();

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
        if (el == null) throw new Exception("ELEMENTO INESISTENTE!");

        // client vars
        url_xml.Value = master.url_cmd("xml element id:" + element_id);

        // document
        StringBuilder sb = new StringBuilder();
        parse_doc(el, sb);
        contents_doc.InnerHtml = sb.ToString();
        contents_xml.Visible = false;

        // menu
        sb.Clear();
        parse_menu(el, sb);
        menu.InnerHtml = sb.ToString();

      }
        // xml
      else if (c.action == "xml" && c.obj == "element") {
        string element_id = c.sub_cmd("id");
        if (element_id == "") throw new Exception("NON HAI SPECIFICATO QUALE ELEMENTO EDITARE!");
        element el = load_element(int.Parse(element_id));
        if (el == null) throw new Exception("ELEMENTO INESISTENTE!");

        // client vars
        url_view.Value = master.url_cmd("view element id:" + element_id);

        // xml document
        contents_xml.Visible = true;
        contents.Visible = false;
        xml_doc doc = new xml_doc() { xml = "<el/>" };
        add_element_node(doc.root_node, el, true);
        doc_xml.Value = doc.xml;

        // menu
        StringBuilder sb = new StringBuilder();
        parse_menu(el, sb);
        menu.InnerHtml = sb.ToString();

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) {
      blocks blk = new blocks();
      blk.add("err-label", "ERRORE: " + ex.Message);
      contents_xml.Visible = false;
      contents.Visible = true;
      contents.InnerHtml = blk.parse_blocks(_core);
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
         , db_provider.str_val(re["element_title"]), db_provider.str_val(re["element_ref"]), db_provider.int_val(re["has_childs_elements"]) == 1, db_provider.int_val(re["back_element_id"]));
        elements.Add(el);

        if (db_provider.int_val(re["element_level"]) == 0) res = el;

        // element_parent
        if (db_provider.int_val(re["element_parent_id"]) > 0)
          elements.First(e => e.element_id == db_provider.int_val(re["element_parent_id"])).add_element(db_provider.int_val(re["element_content_id"]), el);
      }
        // child
      else {
        element_content.element_content_type et = (element_content.element_content_type)Enum.Parse(typeof(element_content.element_content_type), db_provider.str_val(re["element_type"]));
        element ep = elements.FirstOrDefault(e => e.element_id == db_provider.int_val(re["element_id"]));
        if (et == element_content.element_content_type.account)
          ep.add_account(db_provider.int_val(re["child_content_id"]),
            new account(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_value"]), db_provider.str_val(re["child_notes"])));
        else if (et == element_content.element_content_type.title)
          ep.add_title(db_provider.int_val(re["child_content_id"]),
            new title(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_text"]), db_provider.str_val(re["child_ref"])));
        else if (et == element_content.element_content_type.text)
          ep.add_text(db_provider.int_val(re["child_content_id"]),
            new text(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_text"]), db_provider.str_val(re["child_style"])));
        else if (et == element_content.element_content_type.value)
          ep.add_value(db_provider.int_val(re["child_content_id"]),
            new value(ep, db_provider.int_val(re["child_id"]), db_provider.str_val(re["child_value"]), db_provider.str_val(re["child_ref"]), db_provider.str_val(re["child_notes"])));
      }
    }

    return res;
  }

  #region menu

  protected string element_ref_id(element e) { return e.has_element_content ? e.element_content.content_id.ToString() : "root"; }

  protected void parse_menu(element e, StringBuilder sb) {
    bool root = false;
    if (sb.Length == 0) { sb.AppendFormat("<ul class='nav flex-column'>\n"); root = true; }
    if (e.has_title) parse_title_menu(e.element_level, element_ref_id(e), e.title, sb
      , open_element_id: (e.element_level == _max_level && e.has_child_elements ? e.element_id : 0), back_element_id: e.back_element_id);
    bool first = true;
    foreach (element_content ec in e.childs.OrderBy(x => x.content_id)) {
      if (ec.content_type == element_content.element_content_type.title) {
        if (first && e.element_level >= 1) { sb.Append("<ul>"); first = false; }
        parse_title_menu(e.element_level + 1, ec.content_id.ToString(), ec.title, sb);
      } else if (ec.content_type == element_content.element_content_type.element) {
        if (first && e.element_level >= 1) { sb.Append("<ul>"); first = false; }
        parse_menu(ec.element_child, sb);
      }
    }
    sb.AppendFormat("{0}\n", !first ? "</ul>" : "");
    if (root) { sb.AppendFormat("</ul>\n"); }
  }

  protected void parse_title_menu(int level, string ref_id, title ce, StringBuilder sb, bool close = true, int open_element_id = 0, int back_element_id = 0) {
    sb.AppendFormat(@"<li>{6}<a {3} style='{4}' href='#title_{1}'>{0}</a>{5}{2}"
      , ce.text, ref_id, close ? "</li>" : ""
      , level == 0 ? "class='h5'" : "", level == 0 ? "color:steelblue;"
        : (level == 1 ? "color:skyblue;margin-top:10px;padding-top:5px;border-top:1pt solid dimgray;display:block;" : "font-size:90%;color:lightcyan;")
        , open_element_id > 0 ? "<a style='margin-left:5px;font-size:90%;color:lightcyan;' href=\"" + get_url_cmd("view element id:" + open_element_id.ToString()) + "\">(...)</a>" : ""
        , back_element_id > 0 ? "<a href=\"" + get_url_cmd("view element id:" + back_element_id.ToString())
        + "\" style='margin-right:10px;'><img src='images/left-chevron-24.png' width='20' height='20'></a>" : "");
  }

  #endregion

  #region document

  protected void parse_doc(element e, StringBuilder sb) {
    if (e.has_title) parse_title(e.element_level, element_ref_id(e), e.element_code, e.element_type, e.title, sb);
    foreach (element_content ec in e.childs.OrderBy(x => x.content_id)) {
      if (ec.content_type == element_content.element_content_type.title)
        parse_title(e.element_level + 1, ec.content_id.ToString(), "", "", ec.title, sb);
      else if (ec.content_type == element_content.element_content_type.text)
        parse_text(e.element_level + 1, ec.text, sb);
      else if (ec.content_type == element_content.element_content_type.value)
        parse_value(e.element_level + 1, ec.value, sb);
      else if (ec.content_type == element_content.element_content_type.account)
        parse_account(e.element_level + 1, ec.account, sb);
      else if (ec.content_type == element_content.element_content_type.element)
        parse_doc(ec.element_child, sb);
    }

    // {@cmdurl='view cmds'}
    if (e.element_level == _max_level && e.has_child_elements)
      sb.AppendFormat("<a style='font-weight:bold;font-size:90%;display:block;' href=\"{0}\">vedi sotto-elementi...</a>"
        , get_url_cmd("view element id:" + e.element_id.ToString()));
  }

  protected void parse_title(int level, string ref_id, string code, string type, title ce, StringBuilder sb) {
    bool view_level = false;
    string a = string.Format("<a id='title_{0}' class='anchor'></a>", ref_id), st = (level == 1 ? "style='border-top:1pt solid lightgrey;margin-top:45px;padding-top:15px;margin-bottom:0px;padding-bottom:0px;'"
      : (level >= 2 ? "style='color:dimgray;margin-bottom:0px;padding-bottom:0px;'" : ""));
    if (ce.title_ref != "")
      sb.AppendFormat("{5}<{0} {4}><a href='{2}' {3}>{1}</a></{0}>", level == 0 ? "h1" : (level == 1 ? "h2"
        : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5"))), (view_level ? level.ToString() + ": " : "") + ce.text
        , ce.title_ref, !ce.title_ref_cmd ? "target='blank'" : "", st, a);
    else
      sb.AppendFormat("{3}<{0} {2}><span>{1}</span></{0}>", level == 0 ? "h1" : (level == 1 ? "h2"
        : (level == 2 ? "h3" : (level == 3 ? "h4" : "h5"))), (view_level ? level.ToString() + ": " : "") + ce.text, st, a);

    if (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(type)) {
      string hc = !string.IsNullOrEmpty(code) ? string.Format("<span style='font-size:90%;color:darkgray;margin-right:5px;'><b>element code</b>: {0}</span>", code) : "";
      string tp = !string.IsNullOrEmpty(type) ? string.Format("<span style='font-size:90%;color:darkgray;margin-right:5px;'><b>element type</b>: {0}</span>", type) : "";
      sb.AppendFormat("<p style='padding:0px;margin:0px;margin-bottom:15px;'>{0}{1}</p>", tp, hc);
    } else sb.AppendFormat("<br/>");
  }

  protected void parse_text(int level, text te, StringBuilder sb) {
    string style = te.is_style() ? string.Format("style='{0}'"
      , string.Join("", te.get_styles().Select(s => s == text.text_styles.bold ? "font-weight:bold;"
        : (s == text.text_styles.underline ? "font-style:italic;" : "")))) : "";
    sb.AppendFormat("<p class='lead' {1}>{0}</p>", te.text_content, style);
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

  #region xml document

  protected void add_element_node(xml_node nd, element e, bool root = false) {
    xml_node ne = root ? nd : nd.add_node("el");
    ne.set_attr("id", e.element_id.ToString());
    ne.set_attr("title", e.title.text);
    ne.set_attr("ref", e.title.title_ref_value);
    ne.set_attr("code", e.element_code);
    ne.set_attr("type", e.element_type);
    foreach (element_content ec in e.childs.OrderBy(x => x.content_id)) {
      if (ec.content_type == element_content.element_content_type.title)
        add_title_node(ne, ec.title);
      else if (ec.content_type == element_content.element_content_type.text)
        add_text_node(ne, ec.text);
      else if (ec.content_type == element_content.element_content_type.value)
        add_value_node(ne, ec.value);
      else if (ec.content_type == element_content.element_content_type.account)
        add_account_node(ne, ec.account);
      else if (ec.content_type == element_content.element_content_type.element)
        add_element_node(ne, ec.element_child);
    }
  }

  protected void add_title_node(xml_node el, title t) {
    xml_node nd = el.add_node("title", t.text);
    nd.set_attr("ref", t.title_ref_value);
  }
  protected void add_text_node(xml_node el, text t) {
    xml_node nd = el.add_node("text");
    nd.text = t.text_content;
    nd.set_attr("style", t.text_style);
  }
  protected void add_value_node(xml_node el, value v) {
    xml_node nd = el.add_node("val", v.value_content);
    nd.set_attr("ref", v.value_ref_value);
    nd.set_attr("notes", v.value_notes);
  }

  protected void add_account_node(xml_node el, account a) {
    xml_node nd = el.add_node("account");
    nd.set_attr("user", a.account_user);
    nd.set_attr("password", a.account_password);
    nd.set_attr("notes", a.account_notes);
  }

  #endregion


  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
  }
}