using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace molello.classes {
  public class item_text : item {

    protected string _text;
    public string text { get { return _text; } set { _text = value; } }

    public item_text (int id, string text) : base(id, item_type.text) { _text = text; }
    public item_text (HtmlAgilityPack.HtmlNode node) : base(node) { _text = node.InnerText; }

    public override string html_item () {
      return html_base(this, string.Format(@"<div class='item text' {1}>{0}</div>"
        , _text, attrs_base(this.tp))); 
    }
    public override void exec_insert (int node_id) {
      if (string.IsNullOrEmpty(_text)) return;
      base_dal.exec_qry("qry-nodes.add-item-text", new Dictionary<string, object>() { { "node_id", node_id }
        , { "item_type", this.tp.ToString() }, { "item_text", _text } });
    }
  }
}
