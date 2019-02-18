using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using mlib.db;

namespace molello.classes {
  public class node {
    protected int _id, _link_id, _parent_link_id, _n_childs, _n_links, _livello;
    protected string _title;
    protected bool _assigned = false;

    public int id { get { return _id; } }
    public int link_id { get { return _link_id; } }
    public int parent_link_id { get { return _parent_link_id; } set { _parent_link_id = value; } }
    public string title { get { return _title; } set { _title = value; } }
    public int n_childs { get { return _n_childs; } }
    public int n_links { get { return _n_links; } }
    public bool assigned { get { return _assigned; } set { _assigned = value; } }
    public int livello { get { return _livello; } set { _livello = value; } }

    public node (int id, string title, int link_id = -1, int n_childs = -1, int n_links = -1, int parent_link_id = -1, int livello = -1) {
      _id = id; _title = title; _link_id = link_id; _n_childs = n_childs; _n_links = n_links; _parent_link_id = parent_link_id; _livello = livello;
    }

    // dal
    public class dal : molello.classes.base_dal {
      public static List<classes.node> get_link_path (int link_id) {
        return get_link_path(link_id.ToString());
      }

      public static List<classes.node> get_link_path (string link_ids, string filter_nodes = null) {
        return base_dal.dt_qry(filter_nodes == null ? "qry-nodes.get-link-path" : "qry-nodes.get-link-path-filter"
          , new Dictionary<string, object>() { { "link_ids", link_ids }, { "filter_nodes", filter_nodes } })
            .Rows.Cast<DataRow>().Select(r => new classes.node(int_val(r["node_id"]), str_val(r["node_title"])
              , int_val(r["link_id"]), int_val(r["cc_childs"]), int_val(r["cc_links"]), int_val(r["parent_link_id"]), int_val(r["livello"]))).ToList();
      }

      public static List<classes.node> get_sublink_path (int link_id) {
        return get_sublink_path(link_id.ToString());
      }

      public static List<classes.node> get_sublink_path (string link_ids, string filter_nodes = null) {
        return base_dal.dt_qry(filter_nodes == null ? "qry-nodes.get-sublink-path" : "qry-nodes.get-sublink-path-filter"
          , new Dictionary<string, object>() { { "link_ids", link_ids }, { "filter_nodes", filter_nodes } })
            .Rows.Cast<DataRow>().Select(r => new classes.node(int_val(r["node_id"]), str_val(r["node_title"])
              , int_val(r["link_id"]), int_val(r["cc_childs"]), int_val(r["cc_links"]), int_val(r["parent_link_id"]), int_val(r["livello"]))).ToList();
      }

      public static classes.node get_node (int link_id) {
        DataRow r = base_dal.dt_qry("qry-nodes.get-links-node", new Dictionary<string, object>() { { "link_ids", link_id } }).Rows[0];
        return new classes.node(int_val(r["node_id"]), str_val(r["node_title"]), link_id
          , int_val(r["cc_childs"]), int_val(r["cc_links"]), int_val(r["parent_link_id"]));
      }

      public static bool get_last_nav (out int link_id, out int node_id) {
        link_id = node_id = -1;
        DataTable dt = base_dal.dt_qry("qry-nodes.get-last-nav");
        if (dt != null && dt.Rows != null && dt.Rows.Count > 0) {
          link_id = int_val(dt.Rows[0]["link_id"]);
          node_id = int_val(dt.Rows[0]["node_id"]);
          return true;
        }
        return false;
      }

      public static List<item> get_items_node (int node_id) {
        List<item> res = new List<item>();
        foreach (DataRow dr in base_dal.dt_qry("qry-nodes.get-items-node", new Dictionary<string, object>() { { "node_id", node_id } }).Rows) {
          int id = int_val(dr["item_id"]);
          classes.item.item_type tp = (classes.item.item_type)Enum.Parse(typeof(classes.item.item_type), str_val(dr["item_type"]));
          if (tp == item.item_type.text) res.Add(new item_text(id, str_val(dr["item_text"])));
          else if (tp == item.item_type.title) res.Add(new item_title(id, str_val(dr["item_title"])));
          else if (tp == item.item_type.label) res.Add(new item_label(id, str_val(dr["item_label"])));
          else if (tp == item.item_type.todo) res.Add(new item_todo(id, str_val(dr["item_stato"]), str_val(dr["item_cosa"])));
          else throw new Exception("item type '" + tp.ToString() + "' not supported for select!");
        }
        return res;
      }

      public static List<classes.node> get_links_node (string lids) {
        List<classes.node> res = new List<node>();
        foreach (DataRow r in base_dal.dt_qry("qry-nodes.get-links-node", new Dictionary<string, object>() { { "link_ids", lids } }).Rows) {
          int lid = int_val(r["link_id"]);
          res.Add(new classes.node(int_val(r["node_id"]), str_val(r["node_title"]), lid
            , int_val(r["cc_childs"]), int_val(r["cc_links"]), int_val(r["parent_link_id"])));
        }
        return res;
      }

      public static List<classes.node> get_nodes (int parent_link_id = -1) {
        return base_dal.dt_qry("qry-nodes.get-nodes"
          , new Dictionary<string, object>() { { "parent_link_id", parent_link_id > 0 ? parent_link_id.ToString() : null } })
          .Rows.Cast<DataRow>().Select(r => new classes.node(int_val(r["node_id"])
            , str_val(r["node_title"]), int_val(r["link_id"]), int_val(r["cc_childs"]), int_val(r["cc_links"]), parent_link_id)).ToList();
      }

      public static void nav_link(int link_id){
        base_dal.exec_qry("qry-nodes.nav-link", new Dictionary<string, object>() { { "link_id", link_id } });
      }

      public static List<int> get_all_links_node (int node_id, int out_link_id = -1) {
        return get_all_links_node(node_id.ToString(), out_link_id);
      }

      public static List<int> get_all_links_node (string node_ids, int out_link_id = -1) {
        return base_dal.dt_qry("qry-nodes.get-all-nodes-link", new Dictionary<string, object>() { { "node_ids", node_ids }, { "out_link_id", out_link_id } })
          .Rows.Cast<DataRow>().Select(r => int_val(r["link_id"])).ToList();
      }

      public static int add_node (string node_title, int parent_link_id = -1) {
        return int.Parse(base_dal.exec_qry("qry-nodes.add-node", new Dictionary<string, object>() { { "node_title", node_title }
          , { "parent_link_id", parent_link_id > 0 ? parent_link_id.ToString() : null } }, true));
      }

      public static int add_link (int node_id, int parent_link_id = -1) {
        return int.Parse(base_dal.exec_qry("qry-nodes.add-link", new Dictionary<string, object>() { { "node_id", node_id }
          , { "parent_link_id", parent_link_id > 0 ? parent_link_id.ToString() : null } }, true));
      }

      public static int copy_node (int node_id, int parent_link_id = -1) {
        return int.Parse(base_dal.exec_qry("qry-nodes.copy-node", new Dictionary<string, object>() { { "node_id", node_id }
          , { "parent_link_id", parent_link_id > 0 ? parent_link_id.ToString() : null } }, true));
      }

      public static void remove_links (string link_ids) {
        base_dal.exec_qry("qry-nodes.remove-links", new Dictionary<string, object>() { { "link_ids", link_ids } });
      }

      public static void set_node (classes.node n) {
        base_dal.exec_qry("qry-nodes.set-node", new Dictionary<string, object>() { { "node_id", n.id }, { "node_title", n.title } });
      }

      public static void save_items (int node_id, List<classes.item> items) {
        base_dal.exec_qry("qry-nodes.reset-items", new Dictionary<string, object>() { { "node_id", node_id } });
        foreach (classes.item i in items) i.exec_insert(node_id);
        base_dal.exec_qry("qry-nodes.updated-node", new Dictionary<string, object>() { { "node_id", node_id } });
      }
    }
  }
}
