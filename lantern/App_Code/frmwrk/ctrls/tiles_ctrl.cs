using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Xml;
using deeper.db;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.frmwrk.ctrls
{
  public class tiles_ctrl : page_ctrl
  {
    public tiles_ctrl(page_cls page, XmlNode defNode, bool render = true) :
      base(page, defNode, render) { }

    public override void add() {
      base.add();

      try {
        HtmlControl parentElement = parentOnAdd();
        if (parentElement == null)
          return;

        // ciclo tiles
        foreach (XmlNode mode in rootSelNodes("contents/*")) {
          if (mode.Attributes["if"] != null && !_cls.page.eval_cond_id(mode.Attributes["if"].Value))
            continue;

          string ele = mode.Attributes["href"] == null ? "div" : "a";
          string html = "<" + ele + " lw-tile='" + (xmlDoc.node_bool(mode, "onhover") ? "onhover" : (xmlDoc.node_bool(mode, "exclude") ? "exclude" : "normal")) 
            + "' style='margin: 5px;' data-mode='" + mode.Name + "' class='mjlive-tile "
            + attr(mode, "classes") + "'" + attr(mode, "delay", " data-delay='{{value}}'") + attr(mode, "direction", " data-direction='{{value}}'")
            + attr(mode, "slide-direction", " data-slide-direction='{{value}}'") + attr(mode, "href", " href=\"{{value}}\"", true) + ">";

          foreach (XmlNode tile in mode.SelectNodes("tile")) {
            string subele = tile.Attributes["href"] == null ? "div" : "a";

            html += "<" + subele + attr(tile, "href", " href=\"{{value}}\"", true) + ">";
            if (tile.Attributes["imgpage"] != null) {
              XmlNode node = _cls.page.nodePage(tile.Attributes["imgpage"].Value);
              html += attr(node, "image", "<div class='tl-icon' style=\"background: url({{value}}) no-repeat center;\">&nbsp;</div>", true)
                + attr(node, "icon", "<div class='tl-icon'><span style='font-size:27pt;color:white;margin-left:10px;padding-top:10px;' class='mif-{{value}}'></span></div>");
            }

            html += attr(tile, "title", "<div class='tl-title'><span style='display:table-cell;' oncontextmenu='return false;'>{{value}}</span></div>", true)
             + attr(tile, "des", "<div class='tl-des'><span style='display:table-cell;' oncontextmenu='return false;'>{{value}}</span></div>", true)
             + attr(tile, "subref", "<div class='tl-des'><span style='display:table-cell;cursor:hand;text-decoration:underline;' oncontextmenu='return false;' onclick=\"window.location='{{value}}'; return false;\">" + attr(tile, "subref_title") + "</span></div>", true)
             + "</" + subele + ">";
          }

          html += "</" + ele + ">";

          _cls.addHtmlSection(html, parentElement);
        }

        // client script
        string clientScript = page.page.cfg_value("/root/tilessettings/setting[@name='clientScript']");
        if (clientScript != "")
          _cls.regScript(_cls.clientScript(clientScript));
      }
      catch (Exception ex) { _cls.addError(ex); }
    }
  }
}
