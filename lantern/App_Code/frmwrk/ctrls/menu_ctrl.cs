using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace deeper.frmwrk.ctrls
{
  public class menu_ctrl : page_ctrl
  {
    public menu_ctrl(page_cls page, XmlNode defNode, bool render = true) :
      base(page, defNode, render) { }

    public override void add() {
      base.add();

      try {
        //_page.addSection("emptyMenu", 0);

        // produco il documento di menu ripulito dei permessi
        XmlDocument doc = new XmlDocument();
        doc.LoadXml("<root/>");
        doc.DocumentElement.AppendChild(doc.ImportNode(rootNode, true));

        // elaboro tutti i permessi delle voci di menu
        while (doc.SelectSingleNode("/root//*[@if]") != null) {
          XmlNode node = doc.SelectSingleNode("/root//*[@if]");
          if (!_cls.page.eval_cond_id(node.Attributes["if"].Value))
            node.ParentNode.RemoveChild(node);
          else node.Attributes.Remove(node.Attributes["if"]);
        }

        XmlNode menu = doc.SelectSingleNode("/root/menu");

        string html = "<div class='app-bar "
          + (menu.Attributes["style"] != null ? menu.Attributes["style"].Value : "") + "' data-role='appbar'>";

        // voce principale
        XmlNode pageNode = menu.Attributes["page"] != null ?
          _cls.page.cfg_node("/root/pages/page[@name='" + menu.Attributes["page"].Value + "']") : null;

        string href = menu.Attributes["href"] != null ? _cls.page.parse(menu.Attributes["href"].Value)
          : (pageNode != null ? _cls.page.getPageRef(menu.Attributes["page"].Value
            , menu.Attributes["page_args"] != null ? menu.Attributes["page_args"].Value : null) : "")
          , title = menu.Attributes["title"] != null ? menu.Attributes["title"].Value
          : (pageNode != null ? pageNode.Attributes["title"].Value : "");

        if (href != "" && title != "")
          html += "<a href=\"" + href + "\" class='app-bar-element'>"
            + (pageNode != null && pageNode.Attributes["icon"] != null ? "<span class='mif-" + pageNode.Attributes["icon"].Value + "'></span> " : "") + title + "</a>"
            + "<span class='app-bar-divider'></span>";

        // costruzione menu - ciclo voci livello 1
        XmlNodeList voices = menu.SelectNodes("*");
        for (int i = 0; i < voices.Count; i++)
          html += voices[i].Name == "voice" ? (new voiceMenu(voices[i], _cls)).buildHtmlForBar(i < (voices.Count - 1))
            : (voices[i].Name == "icon" ? (new iconMenu(voices[i], _cls)).buildHtml() : "");

        html += "</div>";

        _cls.addHtmlSection(html, 0);
      }
      catch (Exception ex) { _cls.addError(ex, 0); }
    }
  }

  // voiceMenu
  class voiceMenu
  {
    protected string _title = "", _href = "", _pageName = "";
    protected XmlNode _voice = null;
    protected page_cls _page = null;
    protected bool _right = false, _sep = false, _disabled = false;

    public string title {
      get {
        return _title != "" ? _title : (_pageName != ""
          ? _page.page.cfg_node("/root/pages/page[@name='" + _pageName + "']").Attributes["title"].Value
          : "-- titolo non specificato --");
      }
    }

    public bool disabled { get { return _disabled; } }
    public string href { get { return _href; } }
    public string cldisabled { get { if (!_disabled) return ""; return " class='disabled'"; } }
    public bool hasSubvoices { get { return _voice.SelectNodes("voice|icon").Count > 0; } }

    public voiceMenu(XmlNode voiceNode, page_cls page) { voiceMenuCstr(voiceNode, false, page); }

    public voiceMenu(XmlNode voiceNode, bool right, page_cls page) { voiceMenuCstr(voiceNode, right, page); }

    protected void voiceMenuCstr(XmlNode voiceNode, bool right, page_cls page) {
      _page = page;
      _voice = voiceNode;
      _right = right;
      if (voiceNode.Attributes["title"] != null) _title = voiceNode.Attributes["title"].Value;
      if (voiceNode.Attributes["page"] != null) _pageName = voiceNode.Attributes["page"].Value;

      if (voiceNode.Attributes["disabled"] != null && bool.Parse(voiceNode.Attributes["disabled"].Value) == true)
        _disabled = true;

      _href = "#";
      if (!_disabled) _href = voiceNode.Attributes["href"] != null ? _page.page.parse(voiceNode.Attributes["href"].Value)
          : (_pageName != "" ? _page.page.getPageRef(_pageName, voiceNode.Attributes["page_args"] != null ? voiceNode.Attributes["page_args"].Value : null) : _href);

      if (_voice.Attributes["sep"] != null) _sep = bool.Parse(_voice.Attributes["sep"].Value);
    }

    public string buildHtmlForBar(bool thereNext) {
      return "<ul class='app-bar-menu'>" + buildHtml(thereNext) + "</ul>"
        + "<span class=\"app-bar-divider\"></span>";
    }

    public string buildHtml(bool thereNext) {
      if (!hasSubvoices || disabled)
        return " <li" + cldisabled + (disabled ? " style='color:lightgray'" : "")
          + "><a href=\"" + href + "\">" + title + "</a></li>" + (_sep && thereNext ? "<li class=\"divider\"></li>" : "");
      
      // ciclo sottovoci (voice, sep) 
      return "<li><a href='#' class='dropdown-toggle'>" + title + "</a>"
        + " <ul class='d-menu" + (_right ? " place-right" : "") + "' data-role='dropdown'>"
        + (string.Concat(_voice.SelectNodes("*").Cast<XmlNode>()
          .Select(voice => voice.Name == "voice" ? (new voiceMenu(voice, _page)).buildHtml(thereNext)
            : (voice.Name == "icon" ? (new iconMenu(voice, _page)).buildHtml(false) : ""))))
        + "</ul></li>" + (_sep && thereNext ? "<li class=\"divider\"></li>" : "");
    }
  }
  
  // iconMenu
  class iconMenu
  {
    protected string _title = "", _text = "", _href = ""
      , _icon = null, _javascript = "";
    protected XmlNode _iconNode = null;
    protected page_cls _page = null;
    protected bool _right = false;

    public bool hasSubVoices { get { return _iconNode.SelectNodes("voice|icon").Count > 0; } }

    public iconMenu(XmlNode iconNode, page_cls page) { iconMenuCstr(iconNode, false, page); }

    public iconMenu(XmlNode iconNode, bool right, page_cls page) { iconMenuCstr(iconNode, right, page); }

    protected void iconMenuCstr(XmlNode iconNode, bool right, page_cls page) {
      _page = page;
      _iconNode = iconNode;
      _icon = iconNode.Attributes["name"].Value;

      if (iconNode.Attributes["text"] != null) _text = _page.page.parse(iconNode.Attributes["text"].Value);

      if (iconNode.Attributes["javascript"] != null)
        _javascript = _page.page.parse(iconNode.Attributes["javascript"].Value);

      if (iconNode.Attributes["title"] != null) _title = _page.page.parse(iconNode.Attributes["title"].Value);

      _href = "#";
      if (iconNode.Attributes["href"] != null && !hasSubVoices)
        _href = _page.page.parse(iconNode.Attributes["href"].Value);

      if (iconNode.Attributes["right"] != null && bool.Parse(iconNode.Attributes["right"].Value))
        _right = true;
    }

    public string buildHtml() { return buildHtml(true); }

    public string buildHtml(bool forBar) {

      string clRight = _right ? " place-right" : "";
      string html = (_right && forBar ? "<span class='app-bar-divider" + clRight + "'></span>" : "")
        + "<div class='app-bar-element fg-white" + clRight + "'>"
        + "    <a " + (hasSubVoices ? " class='dropdown-toggle fg-white' " : " class='fg-white'")
        + " href=\"" + (_javascript != "" ? "javascript:" + _javascript : _href)
        + "\" title=\"" + _title + "\"><span class='fg-white " + _icon + "'></span> " + _text + "</a>";

      // sottovoci
      if (hasSubVoices) {
        string subHtml = "";
        XmlNodeList svoices = _iconNode.SelectNodes("*");
        for (int i = 0; i < svoices.Count; i++)
          subHtml += svoices[i].Name == "voice" ? (new voiceMenu(svoices[i], _right, _page)).buildHtml(i < (svoices.Count - 1))
          : (svoices[i].Name == "icon" ? (new iconMenu(svoices[i], _right, _page)).buildHtml(false) : "");
        html += "    <ul class='d-menu" + clRight + "' data-role='dropdown'>" + subHtml + "</ul>";
      }

      return html + "</div>" + ((!_right && forBar) ? "<span class='app-bar-divider" + clRight + "'></span>" : "");
    }
  }
}

/*        
                

        protected enum pagesStyles
        {
            dark, blue
        }

        protected pagesStyles menuStyle()
        {
            string style = _roots.value("/root/menu", "style");
            if (style == "")
                return pagesStyles.blue;

            return (pagesStyles)Enum.Parse(typeof(pagesStyles), style);
        }


*/