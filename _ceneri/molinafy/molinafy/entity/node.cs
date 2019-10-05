using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using mlib;

namespace molinafy {
  public class node {
    protected int? _id_node; 
    protected string _title;
    protected List<node> _sub_nodes = null;

    public bool is_root { get { return !_id_node.HasValue; } }
    public int? id_node { get { return _id_node; } }
    public string title { get { return _title; } }
    public List<node> sub_nodes { get { return _sub_nodes; } }

    public node () { _id_node = null; _title = ""; _sub_nodes = null; }

    public node (int id_node, string title) { _id_node = id_node; _title = title; _sub_nodes = null; }
  }
}
