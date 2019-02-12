using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace molello.classes {
  public class item_title : item {

    protected string _title;
    public string title { get { return _title; } set { _title = value; } }

    public item_title (int id, string title) : base(id, item_type.title) { _title = title; }
    public item_title (HtmlAgilityPack.HtmlNode node) : base(node) { _title = node.InnerText; }

    public override string html_item () {
      return html_base(this, string.Format(@"<div class='item title' {1}>{0}</div>", _title, attrs_base(this.tp)));
    }
    public override void exec_insert (int node_id) {
      if (string.IsNullOrEmpty(_title)) return;
      base_dal.exec_qry("qry-nodes.add-item-title", new Dictionary<string, object>() { { "node_id", node_id }
        , { "item_type", this.tp.ToString() }, { "item_title", _title } });
    }
  }
}
