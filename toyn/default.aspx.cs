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
using mlib.db;
using mlib.tools;
using mlib.xml;

public partial class _default : tl_page {

  protected override void OnInit (EventArgs e) {
    base.OnInit(e);
    lbl_docuteck.Visible = sec_dtck.Visible = is_admin;
  }

  protected override void OnLoad (EventArgs e) {
    base.OnLoad(e);

    lbl_name.InnerText = is_user && !string.IsNullOrEmpty(_user.name) ? _user.name.Substring(0, 1).ToUpper() + _user.name.Substring(1).ToLower() : "";

    view_cmds.HRef = master.url_cmd("view cmds");

    // compongo la docuteck
    xml_doc dtk = new xml_doc(Path.Combine(core.base_path, "dtk.xml"));
    blocks bl = new blocks(); nano_node ul = bl.add_xml("<ul/>");
    foreach (xml_node el in dtk.nodes("/docuteck/el"))
      add_dtk_el(ul, el);

    body_dtk.InnerHtml = bl.parse_blocks(_core);
  }

  protected nano_node add_dtk_el (nano_node ul, xml_node el, int level = 0) {
    config.level lev = config.exists_level(level) ? config.get_level(level) : config.get_max_level();
    nano_node li = ul.add_xml(string.Format(@"<li brd-l='2pt solid " + lev.color + "'><title-" + lev.title_size + " des=\"" 
      + (el.get_attr("type") != "" ? " - " + el.get_attr("type") : "")
      + "\" bg-color='" + lev.color + "'>{0}</title-" + lev.title_size + "></li>", el.get_attr("name")));
    if (el.get_attr("des") != "") li.add_xml("<p>" + el.get_attr("des") + "</p>");
    if (el.exists_node("values/val"))
      li.add_xml("<span>values:</span>" + string.Join("", el.nodes("values/val").Select(v => "<label-sm>" + v.text + "</label-sm>")));
    nano_node ul2 = null;
    foreach (xml_node el2 in el.nodes("el")) {
      if (ul2 == null) ul2 = li.add_xml("<ul/>");
      add_dtk_el(ul2, el2, level + 1);
    }
    return li;
  }

  protected override void OnUnload (EventArgs e) {
    base.OnUnload(e);
  }

}