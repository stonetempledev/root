using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.frmwrk.ctrls.grid
{
  //A customized class for displaying the Template Column
  public class grid_tmplcol : ITemplate
  {
    // nota bene: i valori di questo enum corrispondono con l'attributo @type delle elemento col presente nel config.xml
    public enum TypeTemplateCol
    {
      text, integer, euro, real, check, date, date_hour, link, action, action_url, request, tabledel, tablelink
    }

    protected page_cls _page = null;
    protected XmlNode _colNode;
    protected ListItemType _templateType;
    protected TypeTemplateCol _typeCtrl;
    protected string _col_title, _col_des, _fld_name, _grid_id, _key, _icon
      , _page_url, _demandid, _req_action, _req_queries, _field_title, _field_url;
    protected static int _iField = 1;
    protected int _width = -1;
    protected bool _open_url = false;

    static public void addToGrid(page_cls page, GridView gridData, XmlNode col) {
      string field = col.Attributes["field"].Value, tooltipfld = xmlDoc.node_val(col, "tooltipfld"), title = xmlDoc.node_val(col, "title", field)
         , type = xmlDoc.node_val(col, "type", "text"), des = xmlDoc.node_val(col, "des");
      int widthCols = page.page.cfg_value_int("/root/gridsettings", "widthcols", -1), width = xmlDoc.node_int(col, "width", widthCols);
      bool hide_titles = page.page.cfg_value_bool("/root/gridsettings", "hideitem_tooltip", false);

      TypeTemplateCol typeCol = (TypeTemplateCol)Enum.Parse(typeof(TypeTemplateCol), type);

      if (typeCol == TypeTemplateCol.text || typeCol == TypeTemplateCol.check || typeCol == TypeTemplateCol.integer
          || typeCol == TypeTemplateCol.date || typeCol == TypeTemplateCol.date_hour || typeCol == TypeTemplateCol.link
          || typeCol == TypeTemplateCol.euro || typeCol == TypeTemplateCol.real) {
        TemplateField fieldTmpl = new TemplateField();
        fieldTmpl.HeaderTemplate = createHeader(page, gridData.ID, col, title, field, typeCol, width, des);
        fieldTmpl.ItemTemplate = createItem(page, gridData.ID, col, field, typeCol, width, des, "", "", "", false, "", "", "", !hide_titles ? tooltipfld : "");
        if (col.Attributes["summary"] != null)
          fieldTmpl.FooterTemplate = createFooter(page, gridData.ID, col, title, field, typeCol, width, des);

        if (width > 0) {
          fieldTmpl.HeaderStyle.Width = Unit.Pixel(width);
          fieldTmpl.ItemStyle.Width = Unit.Pixel(width);
          fieldTmpl.ItemStyle.CssClass = "item-tbl";
          fieldTmpl.HeaderStyle.CssClass = "header-tbl";
        }

        if (typeCol == TypeTemplateCol.check || typeCol == TypeTemplateCol.date || typeCol == TypeTemplateCol.date_hour)
          fieldTmpl.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        else if (typeCol == TypeTemplateCol.link)
          fieldTmpl.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
        else if (typeCol == TypeTemplateCol.integer || typeCol == TypeTemplateCol.euro || typeCol == TypeTemplateCol.real) {
          fieldTmpl.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
          if (fieldTmpl.FooterTemplate != null)
            fieldTmpl.FooterStyle.HorizontalAlign = HorizontalAlign.Right;
        }

        gridData.Columns.Add(fieldTmpl);
      } else
        throw new Exception("Il tipo di campo '" + type + "' specificato per il campo '" + title + "' non è ancora supportato!");
    }

    static protected grid_tmplcol createHeader(page_cls page, string gridId, XmlNode colNode, string colTitle, string fieldName
      , TypeTemplateCol typeControl, int width = -1, string colDes = "") {
      return new grid_tmplcol(page, gridId, colNode, ListItemType.Header, colTitle, fieldName, typeControl, width, colDes);
    }

    static protected grid_tmplcol createFooter(page_cls page, string gridId, XmlNode colNode, string colTitle, string fieldName
      , TypeTemplateCol typeControl, int width = -1, string colDes = "") {
      return new grid_tmplcol(page, gridId, colNode, ListItemType.Footer, colTitle, fieldName, typeControl, width, colDes);
    }

    static protected grid_tmplcol createItem(page_cls page, string gridId, XmlNode colNode, string fieldName
      , TypeTemplateCol typeControl, int width, string colDes, string key = "", string icon = "", string page_url = "", bool open_url = false
      , string req_queries = "", string action = "", string demandid = "", string tooltipfld = "", string field_url = "") {
      return new grid_tmplcol(page, gridId, colNode, ListItemType.Item, "", fieldName, typeControl
          , width, colDes, key, icon, page_url, open_url, req_queries, action, demandid, tooltipfld, field_url);
    }

    protected grid_tmplcol(page_cls page, string gridId, XmlNode colNode, ListItemType type, string colTitle, string fieldName
      , TypeTemplateCol typeControl, int width, string colDes = "", string key = "", string icon = "", string page_url = "", bool open_url = false
      , string requestQueries = "", string action = "", string demandid = "", string tooltipfld = "", string field_url = "") {
      _page = page; _grid_id = gridId; _colNode = colNode; _templateType = type; _col_title = colTitle;
      _fld_name = fieldName; _typeCtrl = typeControl; _width = width; _col_des = colDes; _key = key;
      _icon = icon; _page_url = page_url; _open_url = open_url; _demandid = demandid; _req_queries = requestQueries;
      _req_action = action; _field_title = tooltipfld; _field_url = field_url;
    }

    public string HeaderText { get { return _col_title; } }

    static public void add_action_url(page_cls page, GridView gridData, XmlNode actionNode, string iconName, string des, string url_field, string url_field_title) {
      TemplateField fieldTmpl = new TemplateField();
      fieldTmpl.HeaderTemplate = createHeader(page, gridData.ID, actionNode, "", "", TypeTemplateCol.action_url);
      fieldTmpl.ItemTemplate = createItem(page, gridData.ID, actionNode, "", TypeTemplateCol.action_url, -1, des, "", iconName, "", false
        , "", "", "", url_field_title, url_field);

      fieldTmpl.ItemStyle.CssClass = "item-action";
      fieldTmpl.HeaderStyle.CssClass = "header-action";

      gridData.Columns.Add(fieldTmpl);
    }

    static public void add_action(page_cls page, GridView gridData, XmlNode actionNode, string iconName, string title, string key, string page_url, bool open_url = false) {
      TemplateField fieldTmpl = new TemplateField();
      fieldTmpl.HeaderTemplate = createHeader(page, gridData.ID, actionNode, "", "", TypeTemplateCol.action);
      fieldTmpl.ItemTemplate = createItem(page, gridData.ID, actionNode, "", TypeTemplateCol.action, -1, title, key, iconName, page_url, open_url);

      fieldTmpl.ItemStyle.CssClass = "item-action";
      fieldTmpl.HeaderStyle.CssClass = "header-action";

      gridData.Columns.Add(fieldTmpl);
    }

    static public void add_action_request(page_cls page, GridView gridData, XmlNode actionNode, string iconName, string title, string key, string urlPageRequest, string queries, string action, string demandid) {
      TemplateField fieldTmpl = new TemplateField();
      fieldTmpl.HeaderTemplate = createHeader(page, gridData.ID, actionNode, "", "", TypeTemplateCol.request);
      fieldTmpl.ItemTemplate = createItem(page, gridData.ID, actionNode, "", TypeTemplateCol.request, -1, title, key, iconName, urlPageRequest, false, queries, action, demandid);

      fieldTmpl.ItemStyle.CssClass = "item-action";
      fieldTmpl.HeaderStyle.CssClass = "header-action";

      gridData.Columns.Add(fieldTmpl);
    }

    static public void add_action_del(page_cls page, GridView gridData, XmlNode actionNode, string iconName, string title, string key, string demandid) {
      TemplateField fieldTmpl = new TemplateField();
      fieldTmpl.HeaderTemplate = createHeader(page, gridData.ID, actionNode, "", "", TypeTemplateCol.tabledel);
      fieldTmpl.ItemTemplate = createItem(page, gridData.ID, actionNode, "", TypeTemplateCol.tabledel, -1, title, key, iconName, page.page.getCurrentUrl(true), false, "", "", demandid);

      fieldTmpl.ItemStyle.CssClass = "item-action";
      fieldTmpl.HeaderStyle.CssClass = "header-action";

      gridData.Columns.Add(fieldTmpl);
    }

    static public void add_action_linked(page_cls page, GridView gridData, XmlNode actionNode, string iconName, string title, string key, string demandid) {
      TemplateField fieldTmpl = new TemplateField();
      fieldTmpl.HeaderTemplate = createHeader(page, gridData.ID, actionNode, "", "", TypeTemplateCol.tablelink);
      fieldTmpl.ItemTemplate = createItem(page, gridData.ID, actionNode, "", TypeTemplateCol.tablelink, -1, title, key, iconName, page.page.getCurrentUrl(true), false, "", "", demandid);

      fieldTmpl.ItemStyle.CssClass = "item-action";
      fieldTmpl.HeaderStyle.CssClass = "header-action";

      gridData.Columns.Add(fieldTmpl);
    }

    protected XmlNode refItem() {
      if (_colNode == null)
        return null;

      foreach (XmlNode reflink in _colNode.SelectNodes("refitem"))
        if (_page.page.eval_cond_id(xmlDoc.node_val(reflink, "if")))
          return reflink;

      return null;
    }

    protected XmlNode refHeader() {
      if (_colNode == null)
        return null;

      foreach (XmlNode reflink in _colNode.SelectNodes("refheader"))
        if (_page.page.eval_cond_id(xmlDoc.node_val(reflink, "if")))
          return reflink;

      return null;
    }

    void ITemplate.InstantiateIn(System.Web.UI.Control container) {
      switch (_templateType) {
        case ListItemType.Header: {
            _iField++;

            if (_typeCtrl != TypeTemplateCol.action && _typeCtrl != TypeTemplateCol.action_url
              && _typeCtrl != TypeTemplateCol.request && _typeCtrl != TypeTemplateCol.tabledel
              && _typeCtrl != TypeTemplateCol.tablelink) {

              html_ctrl fld = new html_ctrl("div", new attrs(new string[,] { { "class", "header-col" } }));

              // titolo colonna
              attrs atrs = new attrs(new string[,] { { "colgrid", "true" }, { "oncontextmenu", "col_contextmenu('" + _grid_id + "', '" + _fld_name + "')" } });
              XmlNode h_node = refHeader();
              ctrl c_title = h_node != null ? (ctrl)new link(_col_title, xmlDoc.node_val(h_node, "title") != "" ? xmlDoc.node_val(h_node, "title") : _col_des, "title-col"
                  , _page.page.parse(h_node.Attributes["ref"].Value), atrs) : (ctrl)new label(_col_title, "title-col", _col_des, atrs);
              fld.add(c_title.control);

              // freccine ordinamento
              string direction;
              bool sorted = grid_ctrl.find_sort_field(grid_ctrl.xmlOfGrid(_page, _grid_id), _fld_name, out direction);
              ctrl f_ctrl = new div(null, "<div class='arrows' title=\".\" "
                + " field_name='" + _fld_name + "' onclick=\"updateGridSort('" + _grid_id + "', '" + _fld_name + "')\" oncontextmenu=\"col_contextmenu('" + _grid_id + "', '" + _fld_name + "')\">"
                + (!sorted ? "<a class='mif-arrow-up arrow-col'></a>" : (direction.ToLower() == "asc" ? "<a class='mif-arrow-up arrow-col-active'></a>"
                : "<a class='mif-arrow-down arrow-col-active'></a>")) + "</div>");
              fld.add(f_ctrl.control);

              container.Controls.Add(fld.control);
            }
          }
          break;

        case ListItemType.Item: {
            _iField++;

            // creazione del controllo
            WebControl ctrl = null;
            bool notTransparent = false;
            if (_typeCtrl == TypeTemplateCol.text) ctrl = refItem() != null ?
              new link(new EventHandler(textlink_DataBinding)).w_ctrl : new text(new EventHandler(text_DataBinding), true).w_ctrl;
            else if (_typeCtrl == TypeTemplateCol.integer) ctrl = refItem() != null ?
              new link(new EventHandler(intlink_DataBinding)).w_ctrl : new label(new EventHandler(int_DataBinding)).lbl;
            else if (_typeCtrl == TypeTemplateCol.euro) ctrl = refItem() != null ?
              new link(new EventHandler(eurolink_DataBinding)).w_ctrl : new label(new EventHandler(euro_DataBinding)).lbl;
            else if (_typeCtrl == TypeTemplateCol.real) ctrl = refItem() != null ?
              new link(new EventHandler(reallink_DataBinding)).w_ctrl : new label(new EventHandler(real_DataBinding)).lbl;
            else if (_typeCtrl == TypeTemplateCol.check) ctrl = new check(_page.newIdControl, new EventHandler(check_DataBinding), false).w_ctrl;
            else if (_typeCtrl == TypeTemplateCol.date || _typeCtrl == TypeTemplateCol.date_hour) {
              if (refItem() != null) throw new Exception("refitem non supportato per la colonna 'date'");
              else ctrl = new label(new EventHandler(date_DataBinding)).lbl;
            } else if (_typeCtrl == TypeTemplateCol.link) ctrl = new link(new EventHandler(link_DataBinding)).w_ctrl;
            else if (_typeCtrl == TypeTemplateCol.action) {
              notTransparent = true;
              ctrl = new link(new EventHandler(action_DataBinding), _icon + " icon-btn").w_ctrl;
            } else if (_typeCtrl == TypeTemplateCol.action_url) {
              notTransparent = true;
              ctrl = new link(new EventHandler(actionurl_DataBinding), _icon + " icon-btn").w_ctrl;
            } else if (_typeCtrl == TypeTemplateCol.request) {
              notTransparent = true;
              ctrl = new link(new EventHandler(request_DataBinding), _icon + " icon-btn").w_ctrl;
            } else if (_typeCtrl == TypeTemplateCol.tabledel) {
              notTransparent = true;
              ctrl = new link(new EventHandler(requestDel_DataBinding), _icon + " icon-btn").w_ctrl;
            } else if (_typeCtrl == TypeTemplateCol.tablelink) {
              notTransparent = true;
              ctrl = new link(new EventHandler(requestLink_DataBinding), _icon + " icon-btn").w_ctrl;
            } else
              throw new Exception("campo '" + _typeCtrl.ToString() + "' non supportato!");

            // aggiunta controllo alla griglia
            if (ctrl != null) {
              if (_typeCtrl == TypeTemplateCol.action || _typeCtrl == TypeTemplateCol.action_url
                || _typeCtrl == TypeTemplateCol.request || _typeCtrl == TypeTemplateCol.tabledel
                || _typeCtrl == TypeTemplateCol.tablelink) ctrl.ToolTip = _col_des;
              if (xmlDoc.node_bool(_colNode, "show_title")) ctrl.Attributes.Add("show_title", "true");
              ctrl.Style.Add(HtmlTextWriterStyle.Width, "95%");
              if (!notTransparent) ctrl.Style.Add(HtmlTextWriterStyle.BackgroundColor, "transparent");
              container.Controls.Add(ctrl);
            }
          }
          break;

        case ListItemType.EditItem:
          //...
          break;

        case ListItemType.Footer: {
            _iField++;

            // creazione del controllo
            WebControl ctrl = null;
            if (_typeCtrl == TypeTemplateCol.euro) ctrl = new label(new EventHandler(eurosum_DataBinding)).lbl;
            else if (_typeCtrl == TypeTemplateCol.real) ctrl = new label(new EventHandler(realsum_DataBinding)).lbl;
            else throw new Exception("campo '" + _typeCtrl.ToString() + "' non supportato!");

            // aggiunta controllo alla griglia
            if (ctrl != null) {
              ctrl.ToolTip = _col_des;
              ctrl.Style.Add(HtmlTextWriterStyle.Width, "95%");
              ctrl.Style.Add(HtmlTextWriterStyle.BackgroundColor, "transparent");
              container.Controls.Add(ctrl);
            }
          }
          break;
      }

    }

    string field_no_alias(string fld) { return fld.IndexOf('.') >= 0 ? fld.Substring(fld.IndexOf('.') + 1) : fld; }

    void text_DataBinding(object sender, EventArgs e) {
      TextBox label = (TextBox)sender;
      GridViewRow container = (GridViewRow)label.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null) label.ToolTip = tooltip.ToString();
      } else label.ToolTip = _page.page.parse(label.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      label.Text = page_ctrl.dbValueToText(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name)));
    }

    void textlink_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      // tooltip            
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null) link.ToolTip = tooltip.ToString();
      } else link.ToolTip = _page.page.parse(xmlDoc.node_val(refItem(), "title")
          , grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      string keyurl = buildRowKeyForUrl(container);

      XmlNode ref_item = refItem();
      string urlpage = _page.page.parse(ref_item.Attributes["ref"] != null ? ref_item.Attributes["ref"].Value : ref_item.Attributes["open"].Value);
      urlpage = urlpage.IndexOf('?') < 0 ? urlpage + "?" + keyurl : urlpage + "&" + keyurl;
      link.NavigateUrl = ref_item.Attributes["ref"] != null ? urlpage : ".";
      if (ref_item.Attributes["open"] != null) link.Attributes.Add("onclick", "window.open('" + urlpage + "', '_blank'); return false;");
      link.Text = page_ctrl.dbValueToText(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name)));
    }

    void link_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          link.ToolTip = tooltip.ToString();
      } else link.ToolTip = _page.page.parse(link.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      // fieldref
      string fieldRef = _colNode != null && _colNode.Attributes["fieldref"] != null ?
        _colNode.Attributes["fieldref"].Value : "";

      object titleLink = DataBinder.Eval(container.DataItem, field_no_alias(_fld_name));
      object refLink = DataBinder.Eval(container.DataItem, fieldRef);
      if (refLink != null && refLink.ToString() != "") {
        link.Text = page_ctrl.dbValueToText(titleLink);
        link.NavigateUrl = page_ctrl.dbValueToText(refLink);
        link.Target = "_blank";
      } else {
        link.Text = page_ctrl.dbValueToText(titleLink);
        link.CssClass = "linkNoRef";
      }
    }

    void request_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      link.Text = "";
      link.Width = Unit.Percentage(100);
      link.NavigateUrl = "javascript:requestRowGrid('" + grid_ctrl.gridNameFromId(_grid_id, _page.page) + "', '" + _page_url + "', '"
          + buildRowKey(container, true) + "', '" + _req_queries + "', '" + _req_action + "', '" + _demandid + "')";
    }

    void requestLink_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      string prkey = _colNode.Attributes["primarykey"].Value;
      link.Text = "";
      link.Width = Unit.Percentage(100);
      link.NavigateUrl = strings.combineurl(_page.page.getPageRef("checkdelete"), "lnk=1&fld=" + prkey
          + "&val=" + DataBinder.Eval(container.DataItem, prkey).ToString());
    }


    void requestDel_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      link.Text = "";
      link.Width = Unit.Percentage(100);
      link.NavigateUrl = "javascript:requestDelRowGrid('" + grid_ctrl.gridNameFromId(_grid_id, _page.page) + "', '" + _page_url
          + "', '" + buildRowKey((GridViewRow)link.NamingContainer, true) + "', '" + _colNode.Attributes["primarykey"].Value
          + "', " + xmlDoc.node_val(_colNode, "force", "false") + ", '" + _demandid + "')";
    }

    void actionurl_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          link.ToolTip = tooltip.ToString();
      } else link.ToolTip = _page.page.parse(link.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      link.Text = "";
      link.Width = Unit.Percentage(100);
      object url = DataBinder.Eval(container.DataItem, _field_url);
      if (url != null && url != DBNull.Value) { link.NavigateUrl = url.ToString(); link.Target = "_blank"; } else link.Visible = false;
    }

    void action_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;

      link.Text = "";
      link.Width = Unit.Percentage(100);
      link.NavigateUrl = !_open_url ? strings.combineurl(_page_url, buildRowKeyForUrl((GridViewRow)link.NamingContainer)) : ".";
      if (_open_url)
        link.Attributes.Add("onclick", "javascript:window.open('" + strings.combineurl(_page_url, buildRowKeyForUrl((GridViewRow)link.NamingContainer)) + "'); return false;");
    }

    protected string buildRowKeyForUrl(GridViewRow gridrow) {
      string gridName = grid_ctrl.gridNameFromId(_grid_id, _page.page);

      string keyurl = "";
      foreach (KeyValuePair<string, string> pair in ((grid_ctrl)_page.control(gridName)).key_fields_grid(gridName))
        keyurl += (keyurl != "" ? "&" : "") + pair.Value + "=" + DataBinder.Eval(gridrow.DataItem, pair.Key).ToString();
      return keyurl;
    }

    protected string buildRowKey(GridViewRow gridrow, bool names = false) {
      return buildRowKey(gridrow, ",", names);
    }

    protected string buildRowKey(GridViewRow gridrow, string separator, bool names = false) {
      string gridName = grid_ctrl.gridNameFromId(_grid_id, _page.page);

      string keyurl = "";
      Dictionary<string, string> keys = ((grid_ctrl)_page.control(gridName)).key_fields_grid(gridName);
      foreach (KeyValuePair<string, string> pair in keys)
        keyurl += (keyurl != "" ? separator : "") + (names ? pair.Key + "=" : "")
          + DataBinder.Eval(gridrow.DataItem, pair.Key).ToString();
      return keyurl;
    }

    void int_DataBinding(object sender, EventArgs e) {
      Label label = (Label)sender;
      GridViewRow container = (GridViewRow)label.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          label.ToolTip = tooltip.ToString();
      } else label.ToolTip = _page.page.parse(label.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      label.Text = page_ctrl.dbValueToInt(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name)),
        _colNode != null && _colNode.Attributes["format"] != null ? _colNode.Attributes["format"].Value : "");
    }

    void eurosum_DataBinding(object sender, EventArgs e) {
      Label label = (Label)sender;
      GridViewRow container = (GridViewRow)label.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          label.ToolTip = tooltip.ToString();
      } else
        label.ToolTip = _page.page.parse(label.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      string gridName = grid_ctrl.gridNameFromId(_grid_id, _page.page);
      label.Text = page_ctrl.dbValueToEuro(((grid_ctrl)_page.control(gridName)).summary(field_no_alias(_fld_name)));
    }

    void euro_DataBinding(object sender, EventArgs e) {
      Label label = (Label)sender;
      GridViewRow container = (GridViewRow)label.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          label.ToolTip = tooltip.ToString();
      } else label.ToolTip = _page.page.parse(label.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);
      label.Text = page_ctrl.dbValueToEuro(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name)));
    }

    void realsum_DataBinding(object sender, EventArgs e) {
      Label label = (Label)sender;
      GridViewRow container = (GridViewRow)label.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          label.ToolTip = tooltip.ToString();
      } else label.ToolTip = _page.page.parse(label.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      object dataValue = ((grid_ctrl)_page.control(grid_ctrl.gridNameFromId(_grid_id, _page.page))).summary(field_no_alias(_fld_name));
      if (dataValue.ToString() != "") label.Text = double.Parse(dataValue.ToString()).ToString();
    }

    void real_DataBinding(object sender, EventArgs e) {
      Label label = (Label)sender;
      GridViewRow container = (GridViewRow)label.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          label.ToolTip = tooltip.ToString();
      } else
        label.ToolTip = _page.page.parse(label.ToolTip, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      // format
      string format = _colNode != null && _colNode.Attributes["format"] != null ? _colNode.Attributes["format"].Value : "";
      object dataValue = DataBinder.Eval(container.DataItem, field_no_alias(_fld_name));
      if (dataValue.ToString() != "")
        label.Text = format == "" ? double.Parse(dataValue.ToString()).ToString()
          : double.Parse(dataValue.ToString()).ToString(format);
    }

    void intlink_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          link.ToolTip = tooltip.ToString();
      } else link.ToolTip = _page.page.parse(refItem().Attributes["des"].Value
          , grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      // url
      string keyurl = buildRowKeyForUrl(container);
      string urlpage = _page.page.parse(refItem().Attributes["ref"].Value);
      link.NavigateUrl = urlpage.IndexOf('?') < 0 ? urlpage + "?" + keyurl
        : urlpage + "&" + keyurl;

      link.Text = page_ctrl.dbValueToInt(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name))
        , _colNode != null && _colNode.Attributes["format"] != null ? _colNode.Attributes["format"].Value : "");
    }

    void eurolink_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          link.ToolTip = tooltip.ToString();
      } else
        link.ToolTip = _page.page.parse(refItem().Attributes["des"].Value, grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      // url
      string keyurl = buildRowKeyForUrl(container);
      string urlpage = _page.page.parse(refItem().Attributes["ref"].Value);
      link.NavigateUrl = urlpage.IndexOf('?') < 0 ? urlpage + "?" + keyurl : urlpage + "&" + keyurl;

      // value
      link.Text = page_ctrl.dbValueToEuro(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name)));
    }

    void reallink_DataBinding(object sender, EventArgs e) {
      HyperLink link = (HyperLink)sender;
      GridViewRow container = (GridViewRow)link.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          link.ToolTip = tooltip.ToString();
      } else link.ToolTip = _page.page.parse(refItem().Attributes["des"].Value
          , grid_ctrl.gridNameFromId(_grid_id, _page.page), container);

      // url
      string keyurl = buildRowKeyForUrl(container);
      string urlpage = _page.page.parse(refItem().Attributes["ref"].Value);
      link.NavigateUrl = urlpage.IndexOf('?') < 0 ? urlpage + "?" + keyurl : urlpage + "&" + keyurl;

      // value
      string format = _colNode != null && _colNode.Attributes["format"] != null ? _colNode.Attributes["format"].Value : "";
      object dataValue = DataBinder.Eval(container.DataItem, field_no_alias(_fld_name));
      if (dataValue.ToString() != "") link.Text = format == "" ? double.Parse(dataValue.ToString()).ToString()
          : double.Parse(dataValue.ToString()).ToString(format);
    }

    void date_DataBinding(object sender, EventArgs e) {
      Label label = (Label)sender;
      GridViewRow container = (GridViewRow)label.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          label.ToolTip = tooltip.ToString();
      }

      // format
      string format = "";
      if (!_page.formatDates(_colNode != null && _colNode.Attributes["formatDate"] != null ?
        _colNode.Attributes["formatDate"].Value : _page.formatFromGridType(_colNode.Attributes["type"].Value), out format))
        throw new Exception("non è stato possibile reperire il formato data");

      label.Text = page_ctrl.dbValueToDate(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name)), format);
    }

    void check_DataBinding(object sender, EventArgs e) {
      CheckBox checkdata = (CheckBox)sender;
      GridViewRow container = (GridViewRow)checkdata.NamingContainer;

      // tooltip
      if (_field_title != "") {
        object tooltip = DataBinder.Eval(container.DataItem, _field_title);
        if (tooltip != null)
          checkdata.ToolTip = tooltip.ToString();
      }

      checkdata.Checked = page_ctrl.dbValueToCheck(DataBinder.Eval(container.DataItem, field_no_alias(_fld_name)));
    }
  }
}
