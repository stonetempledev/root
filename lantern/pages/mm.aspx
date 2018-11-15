<%@ Page Language="C#" AutoEventWireup="true" CodeFile="mm.aspx.cs" Inherits="_mm" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        html, body
        {
            height: 100%;
            margin: 0;
            padding: 0;
        }
        #my_container
        {
            padding: 0;
            margin: 0;
            width: 100%;
            height: 100%;
            background-color: azure;
        }
    </style>
    <script type="text/javascript">
        $(document).ready(function() {
            load_doc();
            $(window).resize(function() {
                //resize_doc();
            });
        });

        var _network = null, _all_nodes, _highlightActive = false, _nodes_set, _edges_set, _xml_doc;

        // carica il documento e lo visualizza
        function load_doc() {

            try {
                _xml_doc = new domDocument(document.getElementById("xml_doc").value);

                // nodes
                var lst_nodes = [], i = 0, level = 0;
                _xml_doc.selNodes("/mm/struct/node").forEach(function(nd) {
                    load_nodes(lst_nodes, nd, i, level);
                });
                _nodes_set = new vis.DataSet(lst_nodes);

                // edges
                var lst_edges = [];
                _xml_doc.selNodes("/mm/struct/node").forEach(function(nd) {
                    load_edges(lst_edges, nd, i);
                });
                _edges_set = new vis.DataSet(lst_edges);

                redraw_all(document.getElementById('my_container'), lst_nodes, lst_edges);

            } catch (e) { metro_alert("load_doc(): " + e); }
        }

        // carica i nodi
        function load_nodes(eles, nd, i, level) {

            var ele = nd.selNode("/mm/elements/ele[@id='" + nd.getAttributeText("ele") + "']");
            var type = nd.selNode("/mm/types/type[@id='" + ele.getAttributeText("type") + "']");
            var new_ele = { id: i, label: ele.getAttributeText("title"), font: { size: parseInt(type.getAttributeText("font_size")) }
                , shape: type.getAttributeText("shape"), level: level, color: { border: type.getAttributeText("color_border")
                , background: type.getAttributeText("color_bck"), highlight: { border: type.getAttributeText("color_hight_border")
                , background: type.getAttributeText("color_hight_bck")}}
                };

            if (nd.getAttributeText("x") != "") new_ele.x = parseFloat(nd.getAttributeText("x"));
            if (nd.getAttributeText("y") != "") new_ele.y = parseFloat(nd.getAttributeText("y"));

            eles.push(new_ele);

            nd.setAttributeText("vis_id", i.toString());
            i++;

            nd.selNodes("node").forEach(function(nd_child) { i = load_nodes(eles, nd_child, i, level + 1); });

            return i;
        }

        // carica i collegamenti
        function load_edges(eles, nd_from) {

            var ele_from = nd_from.selNode("/mm/elements/ele[@id='" + nd_from.getAttributeText("ele") + "']");
            nd_from.selNodes("node").forEach(function(nd_child) {
                var ele_to = nd_child.selNode("/mm/elements/ele[@id='" + nd_child.getAttributeText("ele") + "']");
                eles.push({ from: nd_from.getAttributeText("vis_id"), to: nd_child.getAttributeText("vis_id"), arrows: 'to' });

                load_edges(eles, nd_child);
            });
        }

        // visualizza il documento
        function redraw_all(container, lst_nodes, lst_edges) {
            var options = { nodes: { shape: 'dot'
            , scaling: { min: 10, max: 30, label: { min: 8, max: 30, drawThreshold: 12, maxVisible: 20} }
            , font: { size: 12, face: 'tahoma' }
            }, edges: { width: 0.15, color: { inherit: 'from' }, smooth: { type: 'continuous'} },
                interaction: { tooltipDelay: 200, hideEdgesOnDrag: true }, physics: { enabled: false }
            };

            _network = new vis.Network(container, { nodes: _nodes_set, edges: _edges_set }, options);

            _all_nodes = _nodes_set.get({ returnType: "Object" });

            _network.on("release", on_click);
        }

        // salvataggio documento
        function save_doc() {
            _xml_doc.rootNode().setAttributeText("action", "save");
            var out = _xml_doc.sendToRequestPage("default.aspx");
            if (out.rootNode().getAttributeText("result") == "error") metro_alert("Errore: " + out.selNode("/response/err").text());
            else metro_alert("Documento salvato con successo!");
        }

        // click nodo
        function on_click(params) {
            if (params.nodes.length > 0) {
                var nd = _xml_doc.selNode("/mm/struct//node[@vis_id='" + params.nodes[0] + "']");
                nd.setAttributeText("x", params.pointer.canvas.x);
                nd.setAttributeText("y", params.pointer.canvas.y);
            }
        }

    </script>
</head>
<body oncontextmenu="return false">
    <form mainform='true' runat="server">
    </form>
    <!-- document -->
    <div id="my_container">
    </div>
</body>
</html>
