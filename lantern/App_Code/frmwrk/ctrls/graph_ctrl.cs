using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Xml;
using deeper.db;
using deeper.lib;

namespace deeper.frmwrk.ctrls {
  public class graph_ctrl : page_ctrl {
    public graph_ctrl(page_cls page, XmlNode defNode, bool render = true) :
      base(page, defNode, render) { }

    public override Dictionary<string, string> key_fields() {
      return string.IsNullOrEmpty(rootAttr("key")) ? null : key_fields(rootAttr("key"));
    }

    public override void add() {
      base.add();

      try {
        HtmlControl parentElement = parentOnAdd();
        if (parentElement == null) return;

        // ctrl
        string ctrl = _cls.newIdControl, name = xmlDoc.node_val(_defNode, "name");
        _cls.addHtmlSection(string.Format(@"<table style='{2}'><tr>
              <td style='text-align:left;'>{5}</td><td style='text-align:center;'><span style='font-size:150%;'>{4}</span></td><td style='text-align:right;'>{6}</td></tr>
            <tr><td colspan='3'><div id='{0}' grname='{1}' style='width:100%;{3}'></div></td></tr></table>"
          , ctrl, name, rootAttr("width") != "" ? "width:" + rootAttr("width") + ";" : "width:100%;"
          , rootAttr("height") != "" ? "height:" + rootAttr("height") + ";" : "", rootAttr("title")
          , xmlDoc.node_bool(_defNode, "scroll-x") ? "<span style='border-radius:25px;border:1pt solid;padding:3px;cursor:pointer;' class='mif-arrow-left' onclick=\"scroll_left('" + ctrl + "')\"></span>" : ""
          , xmlDoc.node_bool(_defNode, "scroll-x") ? "<span style='border-radius:25px;border:1pt solid;padding:3px;cursor:pointer;' class='mif-arrow-right' onclick=\"scroll_right('" + ctrl + "')\"></span>" : ""));

        // vars, data
        xmlDoc doc = xmlDoc.create_fromxml("<root><datas/></root>");
        load_data(doc.node("/root/datas"), 0);
        _cls.xml_topage("data_" + name, doc.doc.InnerXml);

      } catch (Exception ex) { _cls.addError(ex); }
    }

    protected void load_data(XmlNode nd_datas, int i_x) {
      xmlDoc.set_attr(nd_datas, "interval", rootAttr("interval"));
      xmlDoc.set_attr(nd_datas, "format-value", rootAttr("format-value"));

      foreach (XmlNode data in rootSelNodes("datas/data")) {
        XmlNode nd_data = xmlDoc.add_node(nd_datas, "data");
        xmlDoc.set_attr(nd_data, "onclick", xmlDoc.node_val(data, "onclick"));
        xmlDoc.set_attr(nd_data, "axis-type", xmlDoc.node_val(data, "axis-type", "primary"));
        xmlDoc.set_attr(nd_data, "fill-opacity", xmlDoc.node_val(data, "fill-opacity", "1"));
        xmlDoc.set_attr(nd_data, "marker-size", xmlDoc.node_val(data, "marker-size", "7"));
        xmlDoc.set_attr(nd_data, "line-thikness", xmlDoc.node_val(data, "line-thikness", "2"));
        xmlDoc.set_attr(nd_data, "type", xmlDoc.node_val(data, "type", "line"));
        xmlDoc.set_attr(nd_data, "color-line", xmlDoc.node_val(data, "color-line"));
        xmlDoc.set_attr(nd_data, "tp-grid", xmlDoc.node_val(data, "tp-grid"));
        xmlDoc.set_attr(nd_data, "lg-grid", xmlDoc.node_val(data, "lg-grid"));
        sql_select sql = _cls.sql_sel(_name, xmlDoc.node_val(data, "selects"));
        if (sql != null && rootAttr("type") == "days-2d") {
          string date_fld = sql.data_fields[0], val_fld = sql.data_fields[1];
          foreach (DataRow dr in _cls.dt_from_sql(sql, _name, new Dictionary<string, string>() { { "i_x", i_x.ToString() } }).Rows) {
            if (dr[date_fld] != DBNull.Value) {
              XmlNode row = xmlDoc.add_node(nd_data, "row");
              xmlDoc.add_node(row, "x").InnerText = Convert.ToDateTime(dr[date_fld]).ToString("yyyy-MM-dd");
              xmlDoc.add_node(row, "y").InnerText = dr[val_fld] != DBNull.Value ? Convert.ToDecimal(dr[val_fld]).ToString("F") : "{null}";
            }
          }
        }
      }
    }

    #region requests

    public override bool evalControlRequest(xmlDoc doc, xmlDoc outxml) {
      bool evaluated = false;
      if (base.evalControlRequest(doc, outxml))
        return true;

      string type = doc.get_value("/request/pars/par[@name='type']");
      if (type == "graph.scroll") {
        evaluated = true;
        int i_x = doc.get_int("/request/pars/par[@name='i_x']");

        outxml.load_xml("<datas/>");
        load_data(outxml.node("/datas"), i_x);
      }

      return evaluated;
    }

    #endregion

  }
}