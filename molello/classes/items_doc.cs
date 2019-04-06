using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace molello.classes {
  public class items_doc {
    protected int _node_id = 0;
    public int node_id { get { return _node_id; } }

    protected List<item> _items = null;
    public List<item> items { get { return _items; } }

    public items_doc (int node_id) {
      _node_id = node_id;
      _items = classes.node.dal.get_items_node(_node_id);
      reorder_items();
    }

    protected void reorder_items () {
      int igrp = 0, level = 0; item iprev = null;
      for (int ii = 0; ii < _items.Count; ii++) {
        item it = _items[ii]; string okey = ii.ToString("000000");
        if (iprev == null) { it.group.set(level, igrp, okey); } 
        else {
          // level
          if (iprev.tp == item.item_type.title && it.tp != item.item_type.title) level++;
          else if (iprev.tp != item.item_type.title && it.tp == item.item_type.title) level--;

          // grp
          if (iprev.tp == item.item_type.todo && it.tp == item.item_type.todo) { } else igrp++;

          // okey
          if (it.tp == item.item_type.todo)
            okey = ((item_todo)it).stato.order.ToString("000") + ii.ToString("000");

          it.group.set(level, igrp, okey);
        }
        
        iprev = it;
      }

      _items = _items.OrderBy(x => x.group.grp.ToString("000") + "." + x.group.okey).ToList();
    }

    public string html () {
      StringBuilder res = new StringBuilder();
      foreach (classes.item i in _items)
        res.Append(i.html_item());
      if (res.Length == 0) res.Append((new classes.item_text(0, "")).html_item());
      return res.ToString();
    }
  }
}
