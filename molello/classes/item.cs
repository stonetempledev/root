using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace molello.classes {
  public class item {
    public enum item_type { none, title, text, label }
    public enum item_display { block, inline }

    protected static List<item_type> _types = null;
    public static List<item_type> types () { if (_types == null) _types = Enum.GetValues(typeof(item_type)).Cast<item_type>().ToList(); return _types; }

    protected int _id = -1;
    protected item_type _type = item_type.none;
    protected item_display _display = item_display.block;

    public int id { get { return _id; } }
    public item_type tp { get { return _type; } }
    public item_display display { get { return _display; } }

    public item (int id, item_type tp) { _id = id; _type = tp; }
    public item (int id, item_type tp, item_display display) { _id = id; _type = tp; _display = display; }
    public item (HtmlAgilityPack.HtmlNode node, item_display display = item_display.block) {
      _id = node.Attributes["i-id"] != null ? int.Parse(node.Attributes["i-id"].Value) : -1;
      _type = (item_type)Enum.Parse(typeof(item_type), node.Attributes["i-type"].Value);
      _display = display;
    }

    public virtual string html_item () { return ""; }
    public virtual bool can_save () { return true; }
    public virtual void exec_insert (int node_id) { }

    #region html

    protected static string attrs_base (item_type tp) {
      return string.Format("i-type='{0}' onkeydown='return i_keydown(this, event)' contenteditable='true' data-text=\"<{1}>\"", tp.ToString()
        , (tp == item_type.text ? "SCRIVI IL TESTO" : (tp == item_type.title ? "SCRIVI IL TITOLO" 
          : (tp == item_type.label ? "SCRIVI L'ETICHETTA" : ""))));
    }

    protected static int _id_item = -1;
    protected static string html_base (item i, string item_html) {
      _id_item++;
      return string.Format(@"<div id='{2}' i-id='{3}' i-parent='true' class='item-parent' style='{4}'
          onmouseover='over_parent(this)' onmouseout='out_parent(this)' onmouseleave='leave_parent(this)'>
        <img i-mc='true' class='item-mc' src='{1}/dot-16.png' style='display:none;' 
          onmouseover='over_mc_item(this)' onmousedown='mc_omdown(this, event)'/>
        <div i-bar='true' class='mc-bar' onmouseleave='out_bar(this)' style='display:none;'>
          <img class='item-mc-bm' src='{1}/refresh-16.png' onmousedown=""mc_bmdown(this, event, 'change-el')""/>
          <img class='item-mc-bm' src='{1}/up-16.png' onmousedown=""mc_bmdown(this, event, 'move-up')""/>
          <img class='item-mc-bm' src='{1}/down-16.png' onmousedown=""mc_bmdown(this, event, 'move-down')""/>
          <img class='item-mc-bm' src='{1}/remove-btn-16.png' onmousedown=""mc_bmdown(this, event, 'remove')""/>
          </div>{0}</div>"
        , item_html, app._core.config.get_var("vars.path-images").value, "parent_" + _id_item.ToString()
        , (i != null ? "item_" + i.id.ToString() : ""), i.display == item_display.inline ? "display: inline;" : "" );
    }

    public static string html_void_item (string tp) {
      item_type t = (item_type)Enum.Parse(typeof(item_type), tp);
      if (t == item_type.text) return (new item_text(0, "")).html_item();
      else if (t == item_type.title) return (new item_title(0, "")).html_item();
      else if (t == item_type.label) return (new item_label(0, "")).html_item();
      else throw new Exception("item type '" + t.ToString() + "' not supported!");
    }

    public static List<item> parse_html (string html) {
      HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
      //htmlDoc.OptionFixNestedTags = true;
      doc.LoadHtml(html);
      List<item> items = new List<item>();
      if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
        throw new Exception("html document incorrect: " + doc.ParseErrors.ElementAt(0).Reason);
      if (doc.DocumentNode != null) {
        foreach (HtmlNode inode in doc.DocumentNode.SelectNodes("//*[@i-type]"))
          items.Add(parse_node(inode));
      }
      return items;
    }

    public static item parse_node (HtmlAgilityPack.HtmlNode node) {
      item_type tp = (item_type)Enum.Parse(typeof(item_type), node.Attributes["i-type"].Value);
      if (tp == item_type.text) return new item_text(node);
      else if (tp == item_type.title) return new item_title(node);
      else if (tp == item_type.label) return new item_label(node);
      else throw new Exception("element parsing '" + tp.ToString() + "' not supported!");
    }

    public static item transform (item i) {
      if (i is item_label) return new item_text(i.id, ((item_label)i).label);
      else if (i is item_text) return new item_title(i.id, ((item_text)i).text);
      else if (i is item_title) return new item_label(i.id, ((item_title)i).title);
      return null;
    }

    #endregion
  }
}
