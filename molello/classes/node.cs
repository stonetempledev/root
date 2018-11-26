using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using mlib.db;

namespace molello.classes {
  public class node {
    protected int _id, _link_id, _n_childs; protected string _title;

    public int id { get { return _id; } }
    public int link_id { get { return _link_id; } }
    public string title { get { return _title; } set { _title = value; } }
    public int n_childs { get { return _n_childs; } }

    public node (int id, string title, int link_id = -1, int n_childs = -1) {
      _id = id; _title = title; _link_id = link_id; _n_childs = n_childs;
    }

    // dal
    public class dal : molello.classes.base_dal {
      public static List<classes.node> get_link_path (int link_id) {
        return base_dal.dt_qry("qry-nodes.get-link-path", new Dictionary<string, object>() { { "link_id", link_id } })
          .Rows.Cast<DataRow>().Select(r => new classes.node(int_val(r["node_id"]), str_val(r["node_title"]), int_val(r["link_id"]), int_val(r["cc_childs"]))).ToList();
      }

      public static classes.node get_node (int link_id) {
        DataRow r = base_dal.dt_qry("qry-nodes.get-node", new Dictionary<string, object>() { { "link_id", link_id } }).Rows[0];
        return new classes.node(int_val(r["node_id"]), str_val(r["node_title"]));
      }

      public static List<classes.node> get_nodes (int parent_link_id = -1) {
        return base_dal.dt_qry("qry-nodes.get-nodes"
          , new Dictionary<string, object>() { { "parent_link_id", parent_link_id > 0 ? parent_link_id.ToString() : null } })
          .Rows.Cast<DataRow>().Select(r => new classes.node(int_val(r["node_id"])
            , str_val(r["node_title"]), int_val(r["link_id"]), int_val(r["cc_childs"]))).ToList();
      }

      public static void add_node (string node_title, int parent_link_id = -1, int parent_node_id = -1) {
        base_dal.exec_qry("qry-nodes.add-node", new Dictionary<string, object>() { { "node_title", node_title }
          , { "parent_link_id", parent_link_id > 0 ? parent_link_id.ToString() : null }
          , { "parent_node_id", parent_node_id > 0 ? parent_node_id.ToString() : null } });
      }

      public static void add_link (int node_id, int parent_link_id = -1, int parent_node_id = -1) {
        base_dal.exec_qry("qry-nodes.add-link", new Dictionary<string, object>() { { "node_id", node_id }
          , { "parent_link_id", parent_link_id > 0 ? parent_link_id.ToString() : null }
          , { "parent_node_id", parent_node_id > 0 ? parent_node_id.ToString() : null } });
      }

      public static void remove_node (int link_id) {
        base_dal.exec_qry("qry-nodes.remove-node", new Dictionary<string, object>() { { "link_id", link_id } });
      }

      public static void set_node (classes.node n) {
        base_dal.exec_qry("qry-nodes.set-node", new Dictionary<string, object>() { { "node_id", n.id }, { "node_title", n.title } });
      }
    }
  }
}
