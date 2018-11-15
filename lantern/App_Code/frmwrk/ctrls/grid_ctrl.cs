using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using deeper.db;
using deeper.frmwrk.ctrls.grid;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.frmwrk.ctrls
{
  public class grid_ctrl : page_ctrl
  {
    static protected string _sess_var_grid = "deeperXmlFilter";
    protected Dictionary<string, object> _summaries;

    public grid_ctrl(page_cls page, XmlNode defNode, bool render = true) :
      base(page, defNode, render) { }

    #region events, base functions

    public override void onInit() {
      base.onInit();

      if (!_cls.page.IsPostBack && _cls.page.cfg_value_bool("/root/gridsettings", "resetonload"))
        reset_filters(_cls.page);
    }

    public override void onLoad() {
      base.onLoad();

      try { loadData(); } catch (Exception ex) {
        _cls.regScript(_cls.scriptStart("var gridEl = document.getElementById('" + _id + "');"
            + " if(gridEl != null) gridEl.style.display = 'none';"));
        _cls.addError(ex, "Errore durante la selezione dati per la griglia '" + rootValue("title", _name) + "': " + ex.Message);
      }
    }

    static public string gridNameFromId(string gridID, lib_page page) {
      GridView grid = (GridView)page.FindControl(gridID);
      return (grid == null || grid.Attributes["grid_name"] == null) ? ""
        : grid.Attributes["grid_name"].ToString();
    }

    public string gridNameFromId(string gridID) { return gridNameFromId(gridID, _cls.page); }

    public override Dictionary<string, string> key_fields() {
      string key = rootValue("cols", "key");
      return (key == "") ? null : key_fields_grid(key);
    }

    public System.Xml.XmlDocument xmlOfGrid() { return xmlOfGrid(_cls, _id); }

    static public System.Xml.XmlDocument xmlOfGrid(page_cls cls, string gridId) {
      System.Xml.XmlDocument docXml = new System.Xml.XmlDocument();
      docXml.LoadXml(get_xml_filters(cls, gridId));
      return docXml;
    }

    static public string get_xml_filters(page_cls cls, string gridId) { return (cls.page.FindControl("Xml" + gridId) as HtmlTextArea).Value; }

    public void set_xml_filters(string gridId, System.Xml.XmlDocument docXml) {
      (_cls.page.FindControl("Xml" + gridId) as HtmlTextArea).Value = docXml.OuterXml;
    }

    static public void reset_filters(lib_page page, string gridId = "", string gridName = "") {
      try {
        if (gridId == "") {
          for (int i = 0; i < page.Session.Count; i++) {
            string varName = page.Session.Keys[i];
            if (varName.Length >= 15 && varName.Substring(0, _sess_var_grid.Length) == _sess_var_grid)
              page.Session[varName] = null;
          }
        } else page.Session[_sess_var_grid + gridId + "_" + gridName] = null;
      } catch { }
    }

    protected string emptyXmlForGrid(string gridName) { return "<root grid_name='" + gridName + "' page='0'><sort/><filter/></root>"; }

    public string htmlButtons() {
      string result = "";

      // pulsanti
      foreach (XmlNode button in rootSelNodes("buttons/button")) {
        if (!_cls.page.eval_cond_id(xmlDoc.node_val(button, "if"))) continue;
        if (button.Attributes["ref"] != null || button.Attributes["open"] != null) {
          string url = _cls.page.parse(button.Attributes["ref"] != null ? button.Attributes["ref"].Value : button.Attributes["open"].Value);
          result += "<a class='add-record' " + (button.Attributes["ref"] != null ? " href=\"" + url + "\"" : " href='.' onclick=\"window.open('" + url + "', '_blank'); return false;\"")
           + (button.Attributes["shortkeys"] != null ? " shortkeys='" + button.Attributes["shortkeys"].Value + "' "
           + " title=\"combinazione: " + button.Attributes["shortkeys"].Value + "\" " : "") + ">" + xmlDoc.node_val(button, "text") + "</a>";
        } else if (button.Attributes["action"] != null) {
          string demandid = "";
          if (button.Attributes["demand"] != null) { demandid = _cls.newIdControl; _cls.xml_topage(demandid, button.Attributes["demand"].Value); }

          result += "<a class='add-record' href=\"javascript:request_grid('" + _name + "', '" + button.Attributes["action"].Value + "', '" + demandid + "')\""
           + (button.Attributes["shortkeys"] != null ? " shortkeys='" + button.Attributes["shortkeys"].Value + "' "
           + " title=\"combinazione: " + button.Attributes["shortkeys"].Value + "\" " : "") + ">" + xmlDoc.node_val(button, "text") + "</a>";
        }
      }

      return result;
    }

    // aggiorna le informazioni legate alla griglia
    protected void update_footer(string gridId) {
      GridView gridView = (GridView)_cls.page.FindControl(gridId);
      GridViewRow pagerRow = gridView.BottomPagerRow;
      if (pagerRow == null)
        return;

      DataTable dataTable = gridView.DataSource as DataTable;
      if (dataTable == null)
        return;

      pagerRow.Visible = true;

      int pageSize = gridView.PageSize, pageDispCount = 10;
      string gridName = gridView.Attributes["grid_name"].ToString();
      Literal lit_summ = (Literal)pagerRow.FindControl("litSummary" + gridId);
      TableCell cell_pages = (TableCell)pagerRow.FindControl("cellPages" + gridId)
        , cellMiddle = (TableCell)pagerRow.FindControl("cellMiddle" + gridId);

      if (dataTable.Rows.Count <= 0)
        return;

      // num pages
      int numberOfRecords = dataTable.Rows.Count, currentPage = (gridView.PageIndex)
        , numberOfPages = numberOfRecords > pageSize ? (int)Math.Ceiling((double)numberOfRecords / (double)pageSize) : 1;
      long top_count = gridView.Attributes["r_count"] != null ? long.Parse(gridView.Attributes["r_count"].ToString()) : -1
        , top_rows = gridView.Attributes["top_rows"] != null ? long.Parse(gridView.Attributes["top_rows"].ToString()) : -1;
      bool hide_count = gridView.Attributes["hide_count"].ToString() == "true";

      // summary
      int ceil = ((currentPage * pageSize) + pageSize);
      lit_summ.Text = htmlButtons() + "<span>Visualizzati " + (top_rows >= 0 ? "i primi " : "") + ((currentPage * pageSize) + 1).ToString().ToString()
        + " - " + (ceil > numberOfRecords ? numberOfRecords.ToString() : ceil.ToString())
        + (!hide_count ? " di " + (top_count >= 0 ? top_count.ToString() : numberOfRecords.ToString()) + " Records" : "") + "</span>";

      // reset filters
      XmlDocument doc_filters = xmlOfGrid(_cls, gridId);
      if (doc_filters.SelectSingleNode("/root/filter/field") != null) {
        HyperLink l_reset = new HyperLink();
        l_reset.CssClass = "middle-link";
        l_reset.Text = _cls.page.cfg_value("/root/gridsettings", "reset_filters_title", "Togli i filtri impostati");
        l_reset.ID = _cls.newIdControl;
        l_reset.Attributes.Add("href", "javascript:resetFilter('" + gridId + "')");

        cellMiddle.Controls.Add(l_reset);
      }

      // esportazione
      if (rootAttrBool("canexport", true)) {
        LinkButton l_exp = new LinkButton();
        l_exp.CssClass = "middle-link";
        l_exp.Text = _cls.page.cfg_value("/root/gridsettings", "exporttitle", "Esporta File Exel");
        l_exp.ID = _cls.newIdControl;
        l_exp.CommandName = "ExportExel";
        l_exp.EnableViewState = true;

        cellMiddle.Controls.Add(l_exp);
      }

      // pages
      {
        // limiti
        int pageShowLimitStart = 1, pageShowLimitEnd = 1;
        if (pageDispCount > numberOfPages) pageShowLimitEnd = numberOfPages;
        else {
          if (currentPage > 4) {
            pageShowLimitEnd = currentPage + (int)(Math.Floor((decimal)pageDispCount / 2));
            pageShowLimitStart = currentPage - (int)(Math.Floor((decimal)pageDispCount / 2));
          } else pageShowLimitEnd = pageDispCount;
        }
        if (pageShowLimitStart < 1) pageShowLimitStart = 1;

        // prima e precedente
        if (currentPage > 0) {
          // prima
          HyperLink objLbFirst = new HyperLink();
          objLbFirst.Text = "Prima";
          objLbFirst.NavigateUrl = "javascript:upd_active_page('" + gridId + "', '0')";
          cell_pages.Controls.Add(objLbFirst);
          cell_pages.Controls.Add(new label("|", "stanghetta").control);

          // precedente
          HyperLink objLbPrevious = new HyperLink();
          objLbPrevious.Text = "Precedente";
          objLbPrevious.NavigateUrl = "javascript:upd_active_page('" + gridId + "', '" + (currentPage - 1).ToString() + "')";
          cell_pages.Controls.Add(objLbPrevious);
          cell_pages.Controls.Add(new label("|", "stanghetta").control);
        }

        // pages
        for (int i = pageShowLimitStart - 1; i < pageShowLimitEnd; i++) {
          if (i < numberOfPages) {
            HyperLink objLb = new HyperLink();
            objLb.Text = (i + 1).ToString();
            objLb.NavigateUrl = "javascript:upd_active_page('" + gridId + "', '" + i.ToString() + "')";
            objLb.CssClass = (currentPage) == i ? "active-page" : "unactive-page";
            cell_pages.Controls.Add(objLb);
            cell_pages.Controls.Add(new label("|", "stanghetta").control);
          }
        }

        // prossima e ultima
        if ((currentPage + 1) != numberOfPages) {
          // prossima
          HyperLink objLbNext = new HyperLink();
          objLbNext.Text = "Prossima";
          objLbNext.NavigateUrl = "javascript:upd_active_page('" + gridId + "', '" + (currentPage + 1).ToString() + "')";
          cell_pages.Controls.Add(objLbNext);
          cell_pages.Controls.Add(new label("|", "stanghetta").control);

          // ultima
          HyperLink objLbLast = new HyperLink();
          objLbLast.Text = "Ultima";
          objLbLast.NavigateUrl = "javascript:upd_active_page('" + gridId + "', '" + (numberOfPages - 1).ToString() + "')";
          cell_pages.Controls.Add(objLbLast);
          cell_pages.Controls.Add(new label("|", "stanghetta").control);
        }
      }
    }

    protected void exportGridToExcelXml(GridView gridView) {
      string gridName = gridView.Attributes["grid_name"];

      // export file
      string exportFile = _cls.page.cfg_value("/root/gridsettings", "exportfile", "Export");
      exportFile = rootAttr("exportfile", exportFile);
      exportFile += ".xml";
      string attachment = "attachment; filename=" + exportFile;

      _cls.page.Response.ClearContent();
      _cls.page.Response.ContentType = "text/xml";
      _cls.page.Response.AddHeader("content-disposition", attachment);
      _cls.page.Response.ContentEncoding = System.Text.Encoding.UTF8;

      // emptyXmlExcel
      System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
      string emptyExcel = _cls.page.cfg_value("/root/gridsettings", "emptyXmlExcel");
      xmlDoc.Load(_cls.page.Server.MapPath(emptyExcel));

      string nameSpace = "urn:schemas-microsoft-com:office:spreadsheet";
      string nameSpaceExcel = "urn:schemas-microsoft-com:office:excel";
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
      nsmgr.AddNamespace("ss", nameSpace);

      XmlNode worksheetNode = xmlDoc.SelectSingleNode("/ss:Workbook/ss:Worksheet", nsmgr);

      // Table
      XmlNode tableNode = worksheetNode.AppendChild(xmlDoc.CreateElement("Table", nameSpace));
      XmlAttribute attr = tableNode.Attributes.Append(xmlDoc.CreateAttribute("FullColumns", nameSpaceExcel));
      attr.InnerText = "1";

      attr = tableNode.Attributes.Append(xmlDoc.CreateAttribute("FullRows", nameSpaceExcel));
      attr.InnerText = "1";

      attr = tableNode.Attributes.Append(xmlDoc.CreateAttribute("DefaultColumnWidth", nameSpace));
      attr.InnerText = "102";

      // Header
      XmlNode row_h = tableNode.AppendChild(xmlDoc.CreateElement("Row", nameSpace));
      foreach (XmlNode col in rootSelNodes("cols/col")) {
        string title = col.Attributes["field"].Value;
        if (col.Attributes["title"] != null)
          title = col.Attributes["title"].Value;

        XmlNode cellNode = row_h.AppendChild(xmlDoc.CreateElement("Cell", nameSpace));
        XmlNode dataNode = cellNode.AppendChild(xmlDoc.CreateElement("Data", nameSpace));
        attr = dataNode.Attributes.Append(xmlDoc.CreateAttribute("Type", nameSpace));
        attr.InnerText = "String";
        dataNode.InnerText = title;
      }

      // Body
      DataTable dt = gridView.DataSource as DataTable;
      if (dt != null) {
        foreach (DataRow row in dt.Rows) {
          XmlNode rowNode = tableNode.AppendChild(xmlDoc.CreateElement("Row", nameSpace));

          foreach (XmlNode col in rootSelNodes("cols/col")) {
            string field = col.Attributes["field"].Value;

            // type
            grid_tmplcol.TypeTemplateCol type = grid_tmplcol.TypeTemplateCol.text;
            if (col.Attributes["type"] != null)
              type = (grid_tmplcol.TypeTemplateCol)Enum.Parse(typeof(grid_tmplcol.TypeTemplateCol), col.Attributes["type"].Value);

            // value
            string value = "";
            if (row[field] != null) {
              if (row[field] != DBNull.Value)
                value = row[field].ToString();
            }

            // format
            string formatDate = "";
            {
              string name = "";
              if (col.Attributes["formatDate"] != null)
                name = col.Attributes["formatDate"].Value;
              if (!page.formatDates(name, out formatDate))
                throw new Exception("non è stato possibile reperire il formato data");
            }

            XmlNode cellNode = rowNode.AppendChild(xmlDoc.CreateElement("Cell", nameSpace));

            // text                        
            XmlNode dataNode = xmlDoc.CreateElement("Data", nameSpace);
            XmlAttribute typeAttr = dataNode.Attributes.Append(xmlDoc.CreateAttribute("Type", nameSpace));
            if (type == grid_tmplcol.TypeTemplateCol.text) {
              typeAttr.InnerText = "String";
              dataNode.InnerText = dbValueToText(value);
            } else if (type == grid_tmplcol.TypeTemplateCol.check) {
              typeAttr.InnerText = "Boolean";
              if (dbValueToCheck(value))
                dataNode.InnerText = "1";
              else
                dataNode.InnerText = "0";
            } else if (type == grid_tmplcol.TypeTemplateCol.date) {
              typeAttr.InnerText = "DateTime";
              dataNode.InnerText = dbValueToDate(value, formatDate);
              if (dataNode.InnerText != "")
                dataNode.InnerText += "T00:00:00.000";

              XmlAttribute styleID = cellNode.Attributes.Append(xmlDoc.CreateAttribute("StyleID", nameSpace));
              styleID.InnerText = "sDate";
            } else if (type == grid_tmplcol.TypeTemplateCol.integer) {
              typeAttr.InnerText = "Number";
              dataNode.InnerText = dbValueToInt(value);
            } else if (type == grid_tmplcol.TypeTemplateCol.euro) {
              typeAttr.InnerText = "Number";
              dataNode.InnerText = dbValueToEuro(value);
            }

            if (dataNode.InnerText != "")
              cellNode.AppendChild(dataNode);
          }
        }
      }

      xmlDoc.Save(_cls.page.Response.Output);

      _cls.page.Response.End();
    }

    protected string get_sort_expr(System.Xml.XmlDocument docXml, bool with_order_by = false, string alias = "") {
      string sql = string.Join(",", docXml.SelectNodes("/root/sort/field").Cast<XmlNode>()
        .Select(field => {
          string fld_name = field.Attributes["name"].Value, fld_expr = fld_name.IndexOf('.') > 0 && alias != "" ?
            alias + fld_name.Substring(fld_name.IndexOf('.')) : (alias != "" ? alias + "." : "") + fld_name;
          return fld_expr + " " + field.Attributes["direction"].Value;
        }));
      return sql != "" ? (with_order_by ? "ORDER BY " + sql : sql) : "";
    }

    static public bool find_sort_field(System.Xml.XmlDocument docXml, string fieldName, out string direction) {
      int iField = 0; return find_sort_field(docXml, fieldName, out iField, out direction);
    }

    static public bool find_sort_field(System.Xml.XmlDocument docXml, string fieldName, out int iField, out string direction) {
      iField = 1;
      direction = "";

      // ricerca field
      foreach (System.Xml.XmlNode field in docXml.SelectNodes("/root/sort/field")) {
        if (field.Attributes["name"].Value == fieldName) { direction = field.Attributes["direction"].Value; return true; }
        iField++;
      }

      return false;
    }

    protected string get_where_expr(System.Xml.XmlDocument docXml, string def = "", bool for_query = false) {

      db_schema db = for_query ? page.page.conn_db_user() : null;

      string filter = "";
      foreach (System.Xml.XmlNode field in docXml.SelectNodes("/root/filter/field")) {
        string filter_val = field.InnerText.Trim(), fld_name = field.Attributes["name"].Value
          , type = rootValue("cols/col[@field='" + fld_name + "']", "type");

        // {!field!}
        filter_val = filter_val.Replace("{!field!}", fld_name);

        // {!date:<VALUE>}

        //string tmpFilter = "";
        //if (type == "" || type == "text" || type == "link") {
        //  if (compare == "") compare = "LIKE";
        //  tmpFilter = fieldName + " " + compare + " '" + filter_fld + "'";
        //} else if (type == "check") {
        //  if (filter_fld != "" && filter_fld != "-1")
        //    tmpFilter = filter_fld == "0" ? "(" + fieldName + " = " + filter_fld + " OR " + fieldName + " IS NULL)"
        //      : fieldName + " = " + filter_fld;
        //} else if (type == "integer" || type == "euro" || type == "real") {
        //  if (compare == "") compare = "=";
        //  tmpFilter = fieldName + " " + compare + " " + filter_fld;
        //} else if (type == "date") {
        //  if (compare == "") compare = "=";

        //  DateTime date = Convert.ToDateTime(filter_fld);
        //  if (for_query) tmpFilter = compare == "=" ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
        //    "(" + fieldName + " >= {0} AND " + fieldName + " < {1}#)", db.val_toqry(date.ToString("yyyy/MM/dd"), fieldType.DATETIME)
        //     , db.val_toqry(date.AddDays(1).ToString("yyyy/MM/dd"), fieldType.DATETIME))
        //    : string.Format(System.Globalization.CultureInfo.InvariantCulture, fieldName + " " + compare + " #{0}#", db.val_toqry(date.ToString("yyyy/MM/dd"), fieldType.DATETIME));
        //  else tmpFilter = compare == "=" ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
        //    "(" + fieldName + " >= #{0:MM/dd/yyyy}# AND " + fieldName + " < #{1:MM/dd/yyyy}#)", date, date.AddDays(1))
        //    : string.Format(System.Globalization.CultureInfo.InvariantCulture, fieldName + " " + compare + " #{0:MM/dd/yyyy}#", date);
        //}


        //bool close_cond = false, first_cond = true, do_while = true;
        //do {
        //  // filter field condition
        //  string filter_cond = "", filter_fld = filter_val;
        //  do_while = false;
        //  foreach (string cond in (new string[] { " AND ", " OR " })) {
        //    int end = filter_val.ToUpper().IndexOf(cond);
        //    if (end > 0) {
        //      filter_fld = filter_val.Substring(0, end);
        //      filter_val = filter_val.Substring(end + cond.Length);
        //      filter_cond = cond.Trim();
        //      do_while = true;
        //      break;
        //    }
        //  }

        //  // compare
        //  string compare = "";
        //  if (filter_fld.Length > 8 && filter_fld.Substring(0, 8).ToLower() == "not like") {
        //    compare = "not like";
        //    filter_fld = filter_fld.Substring(8);
        //  } else if (filter_fld.Length > 4 && filter_fld.Substring(0, 4).ToLower() == "like") {
        //    compare = "like";
        //    filter_fld = filter_fld.Substring(4);
        //  } else if (filter_fld.Length > 2 && (filter_fld.Substring(0, 2) == "!=" || filter_fld.Substring(0, 2) == "<>")) {
        //    compare = "<>";
        //    filter_fld = filter_fld.Substring(2);
        //  } else if (filter_fld.Length > 2 && (filter_fld.Substring(0, 2) == ">=" || filter_fld.Substring(0, 2) == "<=")) {
        //    compare = filter_fld.Substring(0, 2);
        //    filter_fld = filter_fld.Substring(2);
        //  } else if (filter_fld.Length > 1 && (filter_fld.Substring(0, 1) == "<" || filter_fld.Substring(0, 1) == ">"
        //     || filter_fld.Substring(0, 1) == "=")) {
        //    compare = filter_fld.Substring(0, 1);
        //    filter_fld = filter_fld.Substring(1);
        //  }

        //  // type
        //  string fieldName = field.Attributes["name"].Value
        //    , type = rootValue("cols/col[@field='" + fieldName + "']", "type");

        //  // filter
        //  string tmpFilter = "";
        //  if (type == "" || type == "text" || type == "link") {
        //    if (compare == "") compare = "LIKE";
        //    tmpFilter = fieldName + " " + compare + " '" + filter_fld + "'";
        //  } else if (type == "check") {
        //    if (filter_fld != "" && filter_fld != "-1")
        //      tmpFilter = filter_fld == "0" ? "(" + fieldName + " = " + filter_fld + " OR " + fieldName + " IS NULL)"
        //        : fieldName + " = " + filter_fld;
        //  } else if (type == "integer" || type == "euro" || type == "real") {
        //    if (compare == "") compare = "=";
        //    tmpFilter = fieldName + " " + compare + " " + filter_fld;
        //  } else if (type == "date") {
        //    if (compare == "") compare = "=";

        //    DateTime date = Convert.ToDateTime(filter_fld);
        //    if (for_query) tmpFilter = compare == "=" ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
        //      "(" + fieldName + " >= {0} AND " + fieldName + " < {1}#)", db.val_toqry(date.ToString("yyyy/MM/dd"), fieldType.DATETIME)
        //       , db.val_toqry(date.AddDays(1).ToString("yyyy/MM/dd"), fieldType.DATETIME))
        //      : string.Format(System.Globalization.CultureInfo.InvariantCulture, fieldName + " " + compare + " #{0}#", db.val_toqry(date.ToString("yyyy/MM/dd"), fieldType.DATETIME));
        //    else tmpFilter = compare == "=" ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
        //      "(" + fieldName + " >= #{0:MM/dd/yyyy}# AND " + fieldName + " < #{1:MM/dd/yyyy}#)", date, date.AddDays(1))
        //      : string.Format(System.Globalization.CultureInfo.InvariantCulture, fieldName + " " + compare + " #{0:MM/dd/yyyy}#", date);
        //  }

        //  // filter result
        //  if (tmpFilter != "") {
        //    if (!first && first_cond) filter += " AND ";

        //    if (filter_cond != "") {
        //      if (first_cond) { filter += "("; close_cond = true; }
        //      filter += tmpFilter + " " + filter_cond + " ";
        //    } else filter += tmpFilter;
        //  }

        //  first = first_cond = false;
        //} while (do_while);

        //if (close_cond) filter += ")";
      }

      return filter != "" ? filter : def;
    }


    protected string get_filter_expr(System.Xml.XmlDocument docXml, string def = "", bool for_query = false, string alias = "") {

      db_schema db = for_query ? page.page.conn_db_user() : null;

      string filter = "";
      bool first = true;
      foreach (System.Xml.XmlNode field in docXml.SelectNodes("/root/filter/field")) {
        string filter_val = field.InnerText.Trim();
        bool close_cond = false, first_cond = true, do_while = true, user_filter = xmlDoc.node_bool(field, "user_filter");
        do {
          // filter field condition
          string filter_cond = "", filter_fld = filter_val;
          do_while = false;
          foreach (string key_word in (new string[] { " AND ", " OR " })) {
            int end = filter_val.ToUpper().IndexOf(key_word);
            if (end > 0) {
              filter_fld = filter_val.Substring(0, end);
              filter_val = filter_val.Substring(end + key_word.Length);
              filter_cond = key_word.Trim();
              do_while = true;
              break;
            }
          }

          // compare
          string compare = "";
          if (filter_fld.Length > 8 && filter_fld.Substring(0, 8).ToLower() == "not like") {
            compare = "not like";
            filter_fld = filter_fld.Substring(8);
          } else if (filter_fld.Length > 4 && filter_fld.Substring(0, 4).ToLower() == "like") {
            compare = "like";
            filter_fld = filter_fld.Substring(4);
          } else if (filter_fld.Length > 2 && (filter_fld.Substring(0, 2) == "!=" || filter_fld.Substring(0, 2) == "<>")) {
            compare = "<>";
            filter_fld = filter_fld.Substring(2);
          } else if (filter_fld.Length > 2 && (filter_fld.Substring(0, 2) == ">=" || filter_fld.Substring(0, 2) == "<=")) {
            compare = filter_fld.Substring(0, 2);
            filter_fld = filter_fld.Substring(2);
          } else if (filter_fld.Length > 1 && (filter_fld.Substring(0, 1) == "<" || filter_fld.Substring(0, 1) == ">"
             || filter_fld.Substring(0, 1) == "=")) {
            compare = filter_fld.Substring(0, 1);
            filter_fld = filter_fld.Substring(1);
          }

          // type
          string fld_name = field.Attributes["name"].Value, type = rootValue("cols/col[@field='" + fld_name + "']", "type")
            , fld_expr = fld_name.IndexOf('.') > 0 && alias != "" ? alias + fld_name.Substring(fld_name.IndexOf('.'))
              : (alias != "" ? alias + "." : "") + fld_name;

          // filter
          string tmpFilter = "";

          if (type == "" || type == "text" || type == "link") {
            if (compare == "") compare = "LIKE";
            tmpFilter = fld_expr + " " + compare + (user_filter ? " '%" + filter_fld + "%'" : " '" + filter_fld + "'");
          } else if (type == "check") {
            if (filter_fld != "" && filter_fld != "-1")
              tmpFilter = filter_fld.ToLower() == "false" || filter_fld.ToLower() == "falso" || filter_fld.ToLower() == "no" || filter_fld == "0"
                ? "(" + fld_expr + " = 0 OR " + fld_expr + " IS NULL)"
                : fld_expr + " = " + (filter_fld.ToLower() == "vero" || filter_fld.ToLower() == "true" || filter_fld.ToLower() == "si" ? "1" : filter_fld);
          } else if (type == "integer" || type == "euro" || type == "real") {
            if (compare == "") compare = "=";
            tmpFilter = fld_expr + " " + compare + " " + filter_fld;
          } else if (type == "date") {
            if (compare == "") compare = "=";

            DateTime date = Convert.ToDateTime(filter_fld);
            if (for_query) tmpFilter = compare == "=" ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
              "(" + fld_expr + " >= {0} AND " + fld_expr + " < {1})", db.val_toqry(date.ToString("yyyy/MM/dd"), fieldType.DATETIME)
               , db.val_toqry(date.AddDays(1).ToString("yyyy/MM/dd"), fieldType.DATETIME))
              : string.Format(System.Globalization.CultureInfo.InvariantCulture, fld_expr + " " + compare + " {0}", db.val_toqry(date.ToString("yyyy/MM/dd"), fieldType.DATETIME));
            else tmpFilter = compare == "=" ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
              "(" + fld_expr + " >= #{0:MM/dd/yyyy}# AND " + fld_expr + " < #{1:MM/dd/yyyy}#)", date, date.AddDays(1))
              : string.Format(System.Globalization.CultureInfo.InvariantCulture, fld_expr + " " + compare + " #{0:MM/dd/yyyy}#", date);
          }

          // filter result
          if (tmpFilter != "") {
            if (!first && first_cond) filter += " AND ";

            if (filter_cond != "") {
              if (first_cond) { filter += "("; close_cond = true; }
              filter += tmpFilter + " " + filter_cond + " ";
            } else filter += tmpFilter;
          }

          first = first_cond = false;
        } while (do_while);

        if (close_cond) filter += ")";
      }

      return filter != "" ? filter : def;
    }

    public DataColumnCollection columnsForGrid(string gridName) {
      string sql = _cls.page.parse(rootValue("select"));

      // elenco delle colonne
      db_provider db = _cls.page.conn_db_user();

      DataSet ds = db.dt_set("SELECT * FROM (" + sql + ") TBL WHERE 1 = 0");
      if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
        return null;

      return ds.Tables[0].Columns;
    }

    protected void save_filter(string gridId, string gridName, string xml) {
      _cls.page.Session[_sess_var_grid + gridId + "_" + gridName] = xml;
    }

    public bool exist_filter() {
      string id = _sess_var_grid + _id + "_" + _name;
      if (_cls.page.Session[id] == null
          || (_cls.page.Session[id] != null && _cls.page.Session[id].ToString() == ""))
        return false;

      // <root gridname="gridSpese" page="0"><sort /><filter /></root>
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(_cls.page.Session[id].ToString());
      if (doc.SelectSingleNode("/root/filter").FirstChild == null)
        return false;

      return true;
    }

    protected string load_filter(string gridId, string gridName) {
      if (_cls.page.Session[_sess_var_grid + gridId + "_" + gridName] != null) {
        string tmp = _cls.page.Session[_sess_var_grid + gridId + "_" + gridName] as string;
        if (tmp != null && tmp != "") return tmp;
      }
      return "";
    }

    public Dictionary<string, string> key_fields_grid(string gridName) {
      string keyGrid = rootValue("cols", "key");
      if (keyGrid == "")
        return null;

      return key_fields(keyGrid);
    }

    public override bool evalControlRequest(xmlDoc doc, xmlDoc outxml) {
      bool evaluated = false;

      if (base.evalControlRequest(doc, outxml))
        return true;

      // aggioramento database
      string type = doc.get_value("/request/pars/par[@name='type']");
      if (type == "execQueries") {
        evaluated = true;

        // keys
        Dictionary<string, string> values = new Dictionary<string, string>();
        foreach (string keyValue in doc.get_value("/request/pars/par[@name='keys']").Split(','))
          values.Add(keyValue.Split('=')[0], keyValue.Split('=')[1]);

        // queries da eseguire
        _cls.exec_updates(doc.get_value("/request/pars/par[@name='queries']"), _name, values);
      } else if (type == "execAction") evaluated = _cls.action(doc.get_value("/request/pars/par[@name='action']"), _name, "");
      else if (type == "execActionItem") evaluated = _cls.action(doc.get_value("/request/pars/par[@name='action']")
          , _name, doc.get_value("/request/pars/par[@name='keys']"));
      else if (type == "delRecord") {
        evaluated = true;

        // keys
        Dictionary<string, string> keys = new Dictionary<string, string>();
        foreach (string keyValue in doc.get_value("/request/pars/par[@name='keys']").Split(','))
          keys.Add(keyValue.Split('=')[0].ToLower(), keyValue.Split('=')[1]);

        // tabella
        db_schema db = _cls.page.conn_db_user();
        if (!db.exist_schema || !db.exist_meta)
          throw new Exception("non è possibile risalire alla tabella da gestire ed ai collegamenti fra le tabelle!");

        // vedo se il record è collegato a qualche altra tabella
        string pkey = doc.get_value("/request/pars/par[@name='primarykey']"), table = db.schema.tableFromPk(pkey);
        int rc = db.rel_tables_count(table, keys[pkey.ToLower()]).Count;
        if (rc > 0) {
          long id = long.Parse(keys[pkey.ToLower()]);
          if (!db.susbstitute(table, id) || doc.get_bool("/request/pars/par[@name='force']")) {
            try {
              db.begin_trans();
              db.remove_links(table, id, "", false, true, false);
              db.exec(_cls.page.parse(db.buildDelQry(table, id), _name));
              db.commit();
            } catch (Exception ex) { db.rollback(); throw ex; }
          } else {
            outxml.root_add("related_records").InnerText = "true";
            outxml.root_add("url_delpage").InnerText = strings.combineurl(
                strings.combineurl(_cls.page.getPageRef("checkdelete"), "fld=" + pkey), "val=" + keys[pkey.ToLower()]);
          }
        }
          // se non è collegata nessuna tabella procedo con l'eliminazione
        else db.exec(_cls.page.parse(db.buildDelQry(table, long.Parse(keys[pkey.ToLower()])), _name));
      }

      return evaluated;
    }

    public override bool ctrlDataBound(object sender, EventArgs e) {
      if (base.ctrlDataBound(sender, e))
        return true;

      if (!(sender is GridView))
        return false;

      GridView gridView = (GridView)sender;
      if (gridView.ID != _id)
        return false;

      update_footer(gridView.ID);

      return true;
    }

    public override bool ctrlRowCommand(object sender, CommandEventArgs e) {
      if (base.ctrlRowCommand(sender, e))
        return true;

      if (!(sender is GridView))
        return false;

      GridView gridView = (GridView)sender;
      if (gridView.ID != _id)
        return false;

      if (e.CommandName.Equals("ExportExel")) {
        if (_cls.page.cfg_value("/root/gridsettings", "exporttype", "xml") == "xml") exportGridToExcelXml(gridView);
        else export_to_csv(add_csv(new StringBuilder(), get_data(true), this.page, rootNode.SelectNodes("cols/col"))
          , this.page, rootAttr("exportfile", _cls.page.cfg_value("/root/gridsettings", "exportfile", "Export")));
      }

      return true;
    }

    #endregion

    #region controls

    public override void add() {
      base.add();

      try {
        HtmlControl par_ele = parentOnAdd();
        if (par_ele == null || !_cls.page.eval_cond_id(rootAttr("if"))) return;

        // filter        
        string ukey = _cls.page.parse(rootAttr("unload_key", _cls.page.cfg_value("/root/gridsettings", "unload_key")), _name)
          , saved_filter = !_cls.page.IsPostBack ? _cls.get_key(ukey, "active_filter") : "";
        string xml_filters = !string.IsNullOrEmpty(saved_filter) ? saved_filter : load_filter(_id, _name);
        if (xml_filters == "") xml_filters = emptyXmlForGrid(_name);
        _cls.xml_topage("Xml" + _id, xml_filters);

        // griglia
        GridView grid = new GridView();

        if (rootAttr("title") != "") grid.Caption = rootAttr("title");
        (new attrs(new string[,] { { "grid_name", _name }, { "top_rows", rootAttr("top") }, { "oncontextmenu", "grid_contextmenu()" }
          , { "hide_count", rootAttrBool("hide_count") ? "true" : "false" }, { "unload_key", ukey } })).add_toctrl(grid);

        grid.EnableViewState = false;
        grid.ID = _id;
        grid.GridLines = GridLines.None;
        grid.AutoGenerateColumns = true;
        grid.ShowFooter = false;

        // page size            
        int pageSize = rootAttrInt("pagesize", -1);
        if (pageSize <= 0) pageSize = _cls.page.cfg_value_int("/root/gridsettings", "pagesize", -1);

        grid.AllowPaging = true;
        grid.PageSize = pageSize > 0 ? pageSize : _cls.page.cfg_value_int("/root/gridsettings", "nopagesize");

        // updatePage, deletePage
        string key = rootValue("cols", "key");
        if (rootSelNodes("cols/action").Count > 0 && key == "")
          throw new Exception("la griglia '" + _name + "' impostata come editabile non ha specificato l'indice univoco");

        // templates            
        grid.EmptyDataTemplate = new grid_tmplempty(_cls.page, _id);
        grid.EmptyDataRowStyle.CssClass = "pager";
        grid.PagerTemplate = new grid_tmplpgr(_cls.page, _id);

        grid.CssClass = "metroGrid" + (rootAttr("class") != "" ? " " + rootAttr("class") : "");
        grid.PagerStyle.CssClass = "pager";
        grid.FooterStyle.CssClass = "footer";
        grid.AlternatingRowStyle.CssClass = "alt";
        if (rootExist("cols/col[@summary]")) grid.ShowFooter = true;
        grid.RowCommand += new GridViewCommandEventHandler(_cls.page.ctrl_RowCommand);
        grid.DataBound += new EventHandler(_cls.page.ctrl_DataBound);

        if (_addstyles != null)
          foreach (KeyValuePair<string, string> addstyle in _addstyles)
            grid.Style.Add(addstyle.Key, addstyle.Value);

        par_ele.Controls.Add(grid);

        // sono specificate le colonne da visualizzare
        System.Xml.XmlNodeList cols = rootSelNodes("cols/col");
        if (cols.Count > 0) {
          grid.AutoGenerateColumns = false;

          foreach (System.Xml.XmlNode col in cols)
            if (page.page.eval_cond_id(xmlDoc.node_val(col, "if"))) grid_tmplcol.addToGrid(_cls, grid, col);

          // agginuta azioni alla fine
          foreach (XmlNode action in rootSelNodes("cols/action")) {
            if (!page.page.eval_cond_id(xmlDoc.node_val(action, "if"))) continue;

            string icon = action.Attributes["icon"].Value, des = xmlDoc.node_val(action, "des")
              , demandid = action.Attributes["demand"] == null ? "" : _cls.xml_topage(action.Attributes["demand"].Value);

            // lancio pagina
            if (action.Attributes["pageref"] != null || action.Attributes["pageopen"] != null)
              grid_tmplcol.add_action(_cls, grid, action, icon, des, key, _cls.page.parse(action.Attributes["pageref"] != null ?
                action.Attributes["pageref"].Value : action.Attributes["pageopen"].Value), action.Attributes["pageopen"] != null);
            // lancio url
            else if (action.Attributes["url_field"] != null)
              grid_tmplcol.add_action_url(_cls, grid, action, icon, des, xmlDoc.node_val(action, "url_field"), xmlDoc.node_val(action, "url_field_title"));
            // richiesta http
            else if (action.Attributes["pagerequest"] != null)
              grid_tmplcol.add_action_request(_cls, grid, action, icon, des, key
                  , _cls.page.parse(action.Attributes["pagerequest"].Value)
                  , xmlDoc.node_val(action, "queries"), xmlDoc.node_val(action, "action"), demandid);
            // cancellazione record
            else if (action.Attributes["primarykey"] != null && xmlDoc.node_val(action, "type", "del") == "del")
              grid_tmplcol.add_action_del(_cls, grid, action, icon, des, key, demandid);
            // elementi correlati
            else if (action.Attributes["primarykey"] != null && xmlDoc.node_val(action, "type") == "linked")
              grid_tmplcol.add_action_linked(_cls, grid, action, icon, des, key, demandid);
          }
        }
          // costruzione dinamica della griglia
        else {
          grid.AutoGenerateColumns = false;
          foreach (DataColumn sqlcol in columnsForGrid(_name)) {
            XmlNode col = _cls.page.cfg_node("/root/gridsettings/dbtypes/col[@dbtype='" + sqlcol.DataType.ToString() + "']");
            if (col == null)
              throw new Exception("il tipo '" + sqlcol.DataType.ToString() + "' non è censito all'interno dei tipi di colonne definito nel config in /root/gridsettings/dataTypes.");

            grid_tmplcol.addToGrid(_cls, grid, col);
          }
        }

        _cls.regScript(_cls.scriptStart("init_grid('" + _cls.pageName + "', '" + _name + "')"));

      } catch (Exception ex) { _cls.addError(ex); }
    }

    #endregion

    #region data

    public DataTable get_data(bool no_top = false, Dictionary<string, object> sums = null) {
      GridView grid = (GridView)_cls.page.FindControl(_id);
      if (grid == null) return null;

      string gridName = grid.Attributes["grid_name"].ToString(), hide_count = grid.Attributes["hide_count"].ToString()
        , top = grid.Attributes["top_rows"] != null && !no_top ? grid.Attributes["top_rows"].ToString() : "";

      // filter
      System.Xml.XmlDocument doc = xmlOfGrid();
      if (doc.DocumentElement != null && doc.DocumentElement.Attributes != null
          && doc.DocumentElement.Attributes["reset"] != null && doc.DocumentElement.Attributes["reset"].Value == "true") {
        doc.LoadXml(emptyXmlForGrid(gridName));

        save_filter(grid.ID, gridName, doc.InnerXml);
        set_xml_filters(grid.ID, doc);
      } else save_filter(grid.ID, gridName, doc.InnerXml);

      // trattamento preliminare select      
      sql_select sql = _cls.sql_sel(_name, rootAttr("selects"));
      if (sql != null && sql.type == sql_select.type_sql.select) {
        int begin_sql = sql.command.IndexOf("{!begin_sql!}"), end_sql = sql.command.IndexOf("{!end_sql!}");
        string sql_prefix = begin_sql > 0 ? sql.command.Substring(0, begin_sql) : "", sql_sel = begin_sql > 0 ? sql.command.Substring(begin_sql + 13, end_sql - begin_sql - 13) : sql.command
          , sql_suffix = end_sql > 0 ? sql.command.Substring(end_sql + 11, sql.command.Length - end_sql - 11) : "";
        string count = _cls.dt_from_sql(sql_prefix + "select count(*) from (" + sql_sel + ") tbl where "
            + get_filter_expr(doc, "1 = 1", true, "tbl") + sql_suffix, _name).Rows[0][0].ToString();
        if (hide_count != "true") grid.Attributes.Add("r_count", count);
        grid.Attributes.Add("n_pages", (int.Parse(count) > grid.PageSize ? 
            (int)Math.Ceiling((double)int.Parse(count) / (double)grid.PageSize) : 1).ToString());

        sql.command = sql_prefix + "select " + (top != "" ? "top " + top : "") + " tbl.* from (" + sql_sel + ") tbl where " + get_filter_expr(doc, "1 = 1", true, "tbl")
          + " " + get_sort_expr(doc, true, "tbl") + sql_suffix;

        // raggruppamenti
        if (sums != null)
          foreach (XmlNode col in rootSelNodes("cols/col[@summary]"))
            sums.Add(col.Attributes["field"].Value, _cls.dt_from_sql(sql_prefix + "select " + col.Attributes["summary"].Value
              + " from (" + sql_sel + ") tbl where " + get_filter_expr(doc, "1 = 1", true, "tbl") + sql_suffix, _name).Rows[0][0]);
      }

      // caricamento dati
      DataTable dt_ids = _cls.dt_from_sql(sql, _name);
      DataTable dt = dt_ids != null ? dt_ids
        : (rootNode.Attributes["loadscript"] != null ? dt_from_xmlscript(rootNode.Attributes["loadscript"].Value) : null);

      return dt;
    }

    protected void loadData() {
      GridView grid = (GridView)_cls.page.FindControl(_id);
      if (grid == null) return;

      if (_summaries == null) _summaries = new Dictionary<string, object>();
      else _summaries.Clear();

      DataTable dt = get_data(false, _summaries);
      if (dt != null) {
        grid.DataSource = dt;
        set_pages(xmlOfGrid(), grid);
        grid.DataBind();
      }
    }

    protected bool set_pages(XmlDocument doc, GridView grid) {
      DataTable dataTable = grid.DataSource as DataTable;
      if (dataTable == null)
        return false;

      string gridName = grid.Attributes["grid_name"].ToString();
      int page_i = Convert.ToInt32(doc.DocumentElement.Attributes["page"].Value);

      DataView dv = new DataView(dataTable);
      DataTable dt = dv.ToTable();

      // num pages
      int numberOfRecords = dt.Rows.Count;
      int numberOfPages = (numberOfRecords > grid.PageSize) ?
        numberOfPages = (int)Math.Ceiling((double)numberOfRecords / (double)grid.PageSize) : numberOfPages = 1;

      if (page_i >= numberOfPages) {
        page_i = numberOfPages - 1;
        doc.DocumentElement.Attributes["page"].Value = page_i.ToString();
        set_xml_filters(grid.ID, doc);
      }

      grid.PageIndex = page_i;
      grid.DataSource = dt;

      return true;
    }

    protected DataTable dt_from_xmlscript(string script) {
      DataTable dt = new DataTable();

      // ciclo colonne
      XmlNodeList cols = rootSelNodes("cols/col");
      foreach (XmlNode col in cols) {
        string field = col.Attributes["field"].Value;

        // type
        grid_tmplcol.TypeTemplateCol type = (grid_tmplcol.TypeTemplateCol)Enum.Parse(
            typeof(grid_tmplcol.TypeTemplateCol), xmlDoc.node_val(col, "type", "text"));

        // dbtype
        string dbType = xmlDoc.node_val(_cls.page.cfg_node("/root/gridsettings/dbtypes/col[@type='" + type.ToString() + "']"), "dbtype");
        if (dbType == "")
          throw new Exception("il tipo '" + type.ToString() + "' non è censito all'interno dei tipi di colonne definito nel config in /root/gridsettings.");

        dt.Columns.Add(new DataColumn(field, Type.GetType(dbType)));
      }

      // ciclo keys
      Dictionary<string, string> keys = key_fields();
      foreach (string key in keys.Keys) {
        if (!dt.Columns.Contains(key))
          dt.Columns.Add(new DataColumn(key, Type.GetType("System.String")));
      }

      // ciclo nodi
      int count = 0;
      object nodes = listFromXmlScript(script, out count);
      for (int i = 0; i < count; i++) {
        XmlNode row = indexNode(nodes, i);

        List<object> values = new List<object>();
        foreach (DataColumn col in dt.Columns) {
          XmlAttribute attr = row.Attributes[col.ColumnName];
          if (attr != null)
            values.Add(attr.Value);
          else
            values.Add(null);
        }

        dt.Rows.Add(values.ToArray());
      }

      return dt;
    }

    public object summary(string field) { return _summaries == null || !_summaries.ContainsKey(field) ? null : _summaries[field]; }

    public static StringBuilder add_csv(StringBuilder sb, DataTable dt, page_cls cls, XmlNodeList grid_cols = null) {

      // cols
      Dictionary<string, object[]> cols = new Dictionary<string, object[]>();
      if (grid_cols != null)
        foreach (XmlNode col in grid_cols)
          cols.Add(xmlDoc.node_val(col, "field"), new object[] { col.Attributes["type"] != null ? (grid_tmplcol.TypeTemplateCol)Enum.Parse(typeof(grid_tmplcol.TypeTemplateCol), col.Attributes["type"].Value)
           : grid_tmplcol.TypeTemplateCol.text, xmlDoc.node_val(col, "title", xmlDoc.node_val(col, "field")) });
      else if (dt != null)
        foreach (DataColumn col in dt.Columns)
          cols.Add(col.ColumnName, new object[] { col.DataType == typeof(DateTime) ? grid_tmplcol.TypeTemplateCol.date 
            : col.DataType == typeof(int) || col.DataType == typeof(long) ? grid_tmplcol.TypeTemplateCol.integer 
            : col.DataType == typeof(double) || col.DataType == typeof(Decimal) ? grid_tmplcol.TypeTemplateCol.euro 
            : col.DataType == typeof(bool) ? grid_tmplcol.TypeTemplateCol.check : grid_tmplcol.TypeTemplateCol.text, col.ColumnName });

      // Header
      foreach (KeyValuePair<string, object[]> col in cols) sb.Append(((object[])col.Value)[1].ToString() + ";");
      sb.Append("\r\n");

      // Body
      if (dt != null) {
        // format
        string formatDate = "";
        if (!cls.formatDates(cls.page.cfg_value("/root/dateformats", "exportexcel"), out formatDate))
          throw new Exception("non è stato possibile reperire il formato data");

        foreach (DataRow row in dt.Rows) {
          foreach (KeyValuePair<string, object[]> col in cols) {
            grid_tmplcol.TypeTemplateCol type = (grid_tmplcol.TypeTemplateCol)((object[])col.Value)[0];
            string value = row[col.Key] != null && row[col.Key] != DBNull.Value ? row[col.Key].ToString() : "";
            sb.Append((value != "" ? (type == grid_tmplcol.TypeTemplateCol.text ? "=\"" + dbValueToText(value) + "\""
              : type == grid_tmplcol.TypeTemplateCol.date ? dbValueToDate(value, formatDate)
              : type == grid_tmplcol.TypeTemplateCol.integer ? dbValueToInt(value)
              : type == grid_tmplcol.TypeTemplateCol.euro ? Convert.ToDecimal(value).ToString()
              : type == grid_tmplcol.TypeTemplateCol.check ? (dbValueToCheck(value) ? "1" : "0") : "") : "") + ";");
          }
          sb.Append("\r\n");
        }
      }

      return sb;
    }

    public static void export_to_csv(StringBuilder sb, page_cls cls, string file_name) {

      // export file
      string attachment = "attachment; filename=" + file_name + ".csv";

      cls.page.Response.Clear();
      cls.page.Response.Buffer = true;
      cls.page.Response.ContentType = "application/text";
      //_page.page.Response.Charset = "";
      cls.page.Response.ContentEncoding = Encoding.ASCII;
      cls.page.Response.AddHeader("content-disposition", attachment);

      cls.page.Response.Output.Write(sb.ToString());
      //_page.page.Response.Output.Write(Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(sb.ToString())));
      cls.page.Response.Flush();
      cls.page.Response.End();
    }

    #endregion


  }
}

// 1154