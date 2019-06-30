using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using mlib.db;
using mlib.tools;
using mlib.xml;
using mlib.tiles;

public partial class _nodes : tl_page {
    protected override void OnInit(EventArgs e) {
        base.OnInit(e);

        // elab cmd
        if (!IsPostBack) {

            // inits
            url_node.Value = master.url_cmd("view nodes");

            // elab cmds
            blocks blk = new blocks();
            try {
                cmd c = new cmd(qry_val("cmd"));

                // check
                if (string.IsNullOrEmpty(qry_val("cmd"))) return;

                if (c.action == "add" && c.obj == "node") {
                    throw new Exception("lavori in corso...");
                    //      c.set_sub_cmds("to");
                    //      Dictionary<string, object> flds = new Dictionary<string, object>() { { "title", c.sub_obj() }, { "notes", c.sub_obj(1) }
                    //        , { "to", c.sub_cmd("to") }};

                    //      // vedo se c'è già
                    //      if (db_conn.check_exists(config.get_query("exists-node"), core, flds))
                    //        throw new Exception("C'È GIÀ UN NODO CHE SI CHIAMA '" + c.sub_obj() + "'");

                    //      // aggiunta nodo
                    //db_conn.exec_qry(config.get_query("add-node"), core, flds);

                    //      build_nodes(blk);
                } else if (c.action == "view" && c.obj == "nodes") {
                    int node_id = int.Parse(c.sub_cmd("id", "-1"));
                    xml_doc doc = load_node(node_id);

                    // top bar
                    blocks tb = new blocks();
                    List<xml_node> nodes = doc.nodes("/root/path/node");
                    for (int i = 0; i < nodes.Count; i++)
                        tb.add_xml("<" + (i == nodes.Count - 1 ? "p-node-last" : "p-node")
                            + " id_node='" + nodes[i].get_attr("id") + "' title=\"" + nodes[i].get_attr("title") + "\" />");
                    path_nodes.InnerHtml = tb.parse_blocks(_core);

                    // left menu
                    blocks menu = new blocks();
                    foreach (xml_node nd in doc.nodes("/root/nodes/node"))
                        parse_node(menu, null, nd);
                    mnu_nodes.InnerHtml = menu.parse_blocks(_core);
                } else throw new Exception("COMANDO NON RICONOSCIUTO!");

            } catch (Exception ex) {
                blk.add("err-label", ex.Message);
            }
            contents.InnerHtml = blk.parse_blocks(_core);
        }
    }

    // load_node
    // 
    // documento xml contenente le informazioni circa i nodi
    //
    // format xml:
    //  <root id_node='' title='' dt='yyyy-mm-dd' path_ids=''>
    //    <path>
    //     <node id='' title='' path_ids=''/>
    //    </path>
    //    <nodes>
    //     <node id_node='' title='' dt='yyyy-mm-dd' notes=''/>
    //    </nodes>
    //  </root>
    protected xml_doc load_node(int node_id = -1) {
        xml_doc doc = new xml_doc() { xml = "<root><path/><nodes/></root>" };

        // path
        string path_ids = ""; Dictionary<int, string> path = new Dictionary<int, string>() { { -1, "ROOT" } };
        doc.add_xml("/root/path", "<node title='ROOT' id='-1'/>");
        if (node_id >= 0) {
            DataRow dr = db_conn.dt_table(config.get_query("path-node").text, core, new Dictionary<string, object>() { { "node_id", node_id } }).Rows[0];
            path_ids = dr["path_ids"].ToString();
            string[] ids = path_ids.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                , titles = dr["path_titles"].ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ids.Length; i++)
                doc.add_node("/root/path", "node").set_attrs(new Dictionary<string, string>() { { "id", ids[i] }, { "title", titles[i] } });
        }

        // childs
        if (node_id == -1) doc.root_node.set_attrs(new Dictionary<string, string>() { { "title", "ROOT" }, { "id_node", "-1" } });
        xml_node nodes = doc.node("/root/nodes");
        foreach (DataRow r in node_id < 0 ? db_conn.dt_table(config.get_query("view-nodes").text).Rows
            : db_conn.dt_table(config.get_query("view-nodes-childs").text, core, new Dictionary<string, object>() { { "path_node", path_ids } }).Rows) {
            int id_node = (int)r["id_node"], parent_id = r["parent_id"] != DBNull.Value ? (int)r["parent_id"] : -1;
            bool set_root = doc.root_value("id_node") != "";
            xml_node p_nd = doc.node("//*[@id_node=" + parent_id.ToString() + "]");
            xml_node nd = !set_root ? doc.root_node : (p_nd.name == "root" ? nodes.add_node("node") : p_nd.add_node("node"));
            nd.set_attr("id_node", id_node.ToString());
            nd.set_attr("dt", Convert.ToDateTime(r["dt_node"]).ToString("yyyy-MM-dd"));
            nd.set_attr("title", (string)r["title"]);
            nd.set_attr("notes", r["notes"] != DBNull.Value ? (string)r["notes"] : "");
        }

        return doc;
    }

    protected void parse_node(blocks bl, nano_node parent_el, xml_node nd, int level = 0) {
        config.level lev = config.exists_level(level) ? config.get_level(level) : config.get_max_level();
        string xml = string.Format("<{0} title=\"{1}\" href=\"" + master.url_cmd("view nodes" + (nd.get_attr("id_node") != "-1" ? " id:" + nd.get_attr("id_node") : "")) + "\"/>"
            , !nd.has_child() ? "v-menu" : "v-menu-childs", nd.get_attr("title"), nd.get_attr("id_node"));
        nano_node p = bl != null ? bl.add_xml(xml) : parent_el.add_xml(xml);
        foreach (xml_node nd2 in nd.nodes("node"))
            parse_node(null, p, nd2, level + 1);
    }

    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
    }

}