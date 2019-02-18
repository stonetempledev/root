using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace molello.classes {
  public class item_todo : item {

    protected string _stato, _cosa;
    public string stato { get { return _stato; } set { _stato = value; } }
    public string cosa { get { return _cosa; } set { _cosa = value; } }

    public item_todo (int id, string stato, string cosa) : base(id, item_type.todo, item_display.block) { _stato = stato; _cosa = cosa; }
    public item_todo (HtmlAgilityPack.HtmlNode node) : base(node, item_display.block) {
      _stato = node.SelectNodes("div[@i-subtype='todo-stato']")[0].InnerText;
      _cosa = node.SelectNodes("div[@i-subtype='todo-cosa']")[0].InnerText;
    }

    public override string html_item () {
      return html_base(this, string.Format(@"<div class='item-todo' {0}>
        <div i-subtype='todo-stato' class='todo-state' {1}>{2}</div>
        <div i-subtype='todo-cosa' class='todo-cosa' {3}>{4}</div></div>"
        , attrs_base(this.tp), attrs_editable("STATO TODO..."), _stato
        , attrs_editable("TESTO TODO..."), _cosa));
    }

    public override void exec_insert (int node_id) {
      if (string.IsNullOrEmpty(_stato) || string.IsNullOrEmpty(_cosa)) return;
      base_dal.exec_qry("qry-nodes.add-item-todo", new Dictionary<string, object>() { { "node_id", node_id }
        , { "item_type", this.tp.ToString() }, { "item_stato", _stato }, { "item_cosa", _cosa } });
    }
  }
}
