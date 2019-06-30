using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace molello.classes {
  public class item_label :item {

    protected string _label;
    public string label { get { return _label; } set { _label = value; } }

    public item_label (int id, string label) : base(id, item_type.label, item_display.inline) { _label = label; }
    public item_label (HtmlAgilityPack.HtmlNode node) : base(node, item_display.inline) { _label = node.InnerText; }

    public override string html_item () { 
      return html_base(this, string.Format(@"<span class='item-label' {1}>{0}</span>", _label, attrs_base(this.tp, "ETICHETTA..."))); 
    }

    public override void exec_insert (int node_id) {
      if (string.IsNullOrEmpty(_label)) return;
      base_dal.exec_qry("qry-nodes.add-item-label", new Dictionary<string, object>() { { "node_id", node_id }
        , { "item_type", this.tp.ToString() }, { "item_label", _label } });
    }
  }
}
