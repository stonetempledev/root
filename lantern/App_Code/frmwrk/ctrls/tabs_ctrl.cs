using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using deeper.db;
using deeper.frmwrk.ctrls;

namespace deeper.frmwrk.ctrls
{
  public class tabs_ctrl : page_ctrl
  {
    public tabs_ctrl(page_cls page, XmlNode defNode, bool render = true) :
      base(page, defNode, render) { }

    public override void add() {
      base.add();

      try {
        HtmlControl par_element = parentOnAdd();
        if (par_element == null)
          return;

        // tab attivo
        string ukey = _cls.page.parse(rootAttr("unload_key"), _name);
        text txt = new text(_cls.get_key(ukey, "active_tab", "0"), "", "", new attrs(new string[,] { { "tab_name", _name }, { "active_tab", "true" }, { "unload_key", ukey } })
          , new styles(new object[,] { { "visibility", "hidden" }, { "display", "none" } }), _name + "_activetab");
        par_element.Controls.AddAt(0, txt.control);

        // div
        html_ctrl tab = new html_ctrl("div", new attrs(new string[,] { { "class", "tabcontrol2" + (rootAttr("class") != "" ? " " + rootAttr("class") : "") } 
          , {"data-role", "tabcontrol"}}));

          // tabs, frames
        html_ctrl tabs = new html_ctrl("ul", new attrs(new string[,] { { "class", "tabs" } }));
        html_ctrl frames = new html_ctrl("div", new attrs(new string[,] { { "class", "frames" } }));

            int i = 0;
            foreach (XmlNode tabNode in rootSelNodes("tab")) {
          if (!evalIfs(tabNode)) continue;

          html_ctrl li = new html_ctrl("li", new attrs(new string[,] { { "onclick", "set_active_tab('" + _name + "', " + i.ToString() + ")" } })
            , null, _name + "__tab" + i.ToString(), "<a href='#" + _name + "_" + tabNode.Attributes["name"].Value + "'>" + tabNode.Attributes["title"].Value + "</a>");
          if (tabNode.Attributes["des"] != null) li.add_attr("title", tabNode.Attributes["des"].Value);
          tabs.add(li);

          html_ctrl frame = new html_ctrl("div", new attrs(new string[,] { { "class", "frame" } })
            , null, _name + "_" + tabNode.Attributes["name"].Value);
          frames.add(frame);

              i++;
            }

        tab.add(tabs);
        tab.add(frames);

        par_element.Controls.Add(tab.control);

        _cls.regScript(_cls.scriptStart("init_tab('" + _cls.pageName + "', '" + _name + "')"));

      } catch (Exception ex) { _cls.addError(ex); }
    }

    public override void onLoad() { if (isActiveTab()) setActiveLinguetta(getActiveTab()); }

    protected void setActiveLinguetta(int index) {
      for (int i = 0; i < rootSelNodes("tab").Count; i++) {
        HtmlControl l = (HtmlControl)_cls.page.FindControl(_name + "__tab" + i.ToString());
        if (l != null) {
          if (i == index)
            if (l.Attributes["class"] == null) l.Attributes.Add("class", "active"); else l.Attributes["class"] = "active";
          else
            if (l.Attributes["class"] == null) l.Attributes.Add("class", ""); else l.Attributes["class"] = "";
        }
      }
    }

    public HtmlControl findTab(string name) { return (HtmlControl)_cls.page.FindControl(_name + "_" + name); }

    public bool isActiveTab() { return _cls.page.FindControl(_name + "_activetab") != null; }

    public int getActiveTab() { return int.Parse(((TextBox)_cls.page.FindControl(_name + "_activetab")).Text); }
  }
}
