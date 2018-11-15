using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Data;
using deeper.db;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.frmwrk.ctrls
{
  public class form_ctrl : page_ctrl
  {
    DataRow _dr = null;

    public form_ctrl(page_cls page, XmlNode defNode, bool render = true) :
      base(page, defNode, render) { }

    #region requests

    public override bool evalControlRequest(xmlDoc doc, xmlDoc outxml) {
      bool evaluated = false;
      if (base.evalControlRequest(doc, outxml))
        return true;

      string type = doc.get_value("/request/pars/par[@name='type']");
      if (type == "reload_combo") {
        evaluated = true;
        outxml.load_xml(combo_sel_xml(doc.get_value("/request/pars/par[@name='select']")));
      }

      return evaluated;
    }
    #endregion

    #region init, events

    public override void add() {
      base.add();

      try {

        ctrl par_el = new ctrl(parentOnAdd());
        if (par_el == null) return;

        // if
        if (!_cls.page.eval_cond_id(rootAttr("if"))) return;
        db_schema conn = page.page.conn_db_user();

        // datarow
        DataTable dt = _cls.dt_from_ids(rootAttr("selects"), _name);
        _dr = _dr == null ? (dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null) : _dr;

        // aggiungo i dati per i combo in alto
        foreach (XmlNode select in rootSelNodes("queries/select[@forcombos]")) {

          // vedo se il combo viene usato 
          string id = select.Attributes["name"].Value;
          if (rootSelNodes("contents//combo[@select='" + id + "']")
            .Cast<XmlNode>().FirstOrDefault(x => !if_node(rowOfControl(x), _dr)) == null)
            continue;

          // text area
          string fieldId, fieldVal, fieldDes;
          string xml = combo_sel_xml(id, out fieldId, out fieldVal, out fieldDes);
          par_el.add_at(0, new txt_area(_cls.newIdControl, xml
            , new attrs(new string[,] { { "forcombos", id }, { "fieldid", fieldId }, { "fieldval", fieldVal }
              , { "fielddes", fieldDes } }), new styles(new object[,] { { "display", "none" } })));
        }

        // scripts lato client
        foreach (XmlNode script in rootSelNodes("jscripts/jscript"))
          _cls.regScript(page.page.parse(script.InnerText, _name));

        // contents
        tbl tbl_frm = new tbl(_id, "metroForm" + (rootAttr("class") != "" ? " " + rootAttr("class") : ""), null, null
          , new styles(new object[,] { { "width", rootAttr("width") }, { "height", rootAttr("height") } }), _cls.page.parse(rootAttr("title")));
        int cols = 0;
        {
          ctrl hdiv = par_el.add(new ctrl(new HtmlGenericControl("div")));
          if (_addstyles != null) tbl_frm.add_styles(_addstyles);
          hdiv.add(tbl_frm);

          string[] flds_width = rootAttr("flds-width") != "" ? rootAttr("flds-width").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : (string[])null;

          bool enabled_form = _cls.page.eval_cond_id(_name, rootAttr("enabled_if"), _dr);

          // precalcoli
          foreach (System.Xml.XmlNode row in rootSelNodes("contents/row")) {
            if (!_cls.page.eval_cond_id(_name, xmlDoc.node_val(row, "if"), _dr))
              continue;

            // fields
            int icol = 0;
            foreach (System.Xml.XmlNode field in row.SelectNodes("field")) {
              if (!_cls.page.eval_cond_id(_name, xmlDoc.node_val(field, "if"), _dr))
                continue; 

              icol += field.Attributes["colspan"] != null ? Convert.ToInt32(field.Attributes["colspan"].Value) : 1;
            }

            cols = icol > cols ? icol : cols;
          }

          // contents
          int ictrl = 0, irow = 0;
          foreach (System.Xml.XmlNode row in rootSelNodes("contents/row")) {
            if (row.Attributes["if"] != null && !_cls.page.eval_cond_id(_name, row.Attributes["if"].Value, _dr))
              continue;

            bool enabled_row = _cls.page.eval_cond_id(_name, xmlDoc.node_val(row, "enabled_if"), _dr);

            // tr
            tbl_row rowTable = tbl_frm.add_row(new tbl_row(null, row.Attributes["tooltip"] != null ? row.Attributes["tooltip"].Value : null
              , row.Attributes["style"] != null ? new styles(row.Attributes["style"].Value) : null));

            // field
            int icol = 0;
            foreach (System.Xml.XmlNode field in row.SelectNodes("field")) {
              if (field.Attributes["if"] != null && !_cls.page.eval_cond_id(_name, field.Attributes["if"].Value, _dr))
                continue;

              ctrl parcell = rowTable.add_cell(new tbl_cell(null, null, null, null
                , field.Attributes["width"] != null ? new styles(new object[,] { { "width", field.Attributes["width"].Value } }) : null
                , "fldContents" + (field.Attributes["class"] != null ? " " + field.Attributes["class"].Value : "")
                , field.Attributes["colspan"] != null ? Convert.ToInt32(field.Attributes["colspan"].Value) : (xmlDoc.node_bool(field, "maxspan") ? cols : 1)
                , field.Attributes["tooltip"] != null ? field.Attributes["tooltip"].Value : ""
                , xmlDoc.node_bool(field, "right") ? HorizontalAlign.Right : HorizontalAlign.NotSet, VerticalAlign.Middle));
              if (field.Attributes["width"] == null && field.Attributes["colspan"] == null && flds_width != null && icol < flds_width.Length) 
                parcell.add_style("width", flds_width[icol].Trim());

              // fieldset
              if (field.Attributes["title"] != null) {
                Panel panel = new Panel();
                panel.GroupingText = field.Attributes["title"].Value;
                parcell.assign(parcell.add(panel));
              }

              // controls
              foreach (System.Xml.XmlNode c_node in field.SelectNodes("*")) {
                if (c_node.Attributes["if"] != null && !_cls.page.eval_cond_id(_name, c_node.Attributes["if"].Value, _dr))
                  continue;

                string type = xmlDoc.node_val(c_node, "type"), name = xmlDoc.node_val(c_node, "name"), fieldName = xmlDoc.node_val(c_node, "field")
                  , fieldDes = xmlDoc.node_val(c_node, "fielddes"), width = xmlDoc.node_val(c_node, "width"), align = xmlDoc.node_val(c_node, "align")
                  , text = xmlDoc.node_val(c_node, "text"), defValue = xmlDoc.node_val(c_node, "defvalue"), style = xmlDoc.node_val(c_node, "style")
                  , jsblur = xmlDoc.node_val(c_node, "jsblur"), jskeydown = xmlDoc.node_val(c_node, "jskeydown"), jsselect = xmlDoc.node_val(c_node, "jsselect")
                  , maxlength = xmlDoc.node_val(c_node, "maxlength");
                string enabled_if = c_node.Name == "input" || c_node.Name == "list" || c_node.Name == "combo" ? xmlDoc.node_val(c_node, "enabled_if") : "";
                bool enabled = !enabled_form || !enabled_row ? false : (c_node.Attributes["enabled"] != null ? xmlDoc.node_bool(c_node, "enabled")
                  : _cls.page.eval_cond_id(_name, enabled_if, _dr)), hide = xmlDoc.node_bool(c_node, "hide", false);

                // label
                if (c_node.Name == "label") {

                  string lblText = "";
                  if (_dr != null && fieldName != "" && !_cls.page.IsPostBack) {
                    lblText = valueFromDataRow(_dr, fieldName);
                    if (c_node.Attributes["dateformat"] != null && lblText != "")
                      lblText = DateTime.Parse(lblText).ToString(c_node.Attributes["dateformat"].Value);
                  } else if (text != "") lblText = text;

                  string url = enabled ? (c_node.Attributes["ref"] != null ? _cls.page.parse(c_node.Attributes["ref"].Value, _name) : (
                    c_node.Attributes["open"] != null ? "javascript:window.open('" + _cls.page.parse(c_node.Attributes["open"].Value, _name) + "', '_blank');" : null)) : null;
                  styles st = new styles(width != "" ? new object[,] { { "width", width }, { "display", "inline-block" } } : null, style != "" ? style : null);
                  if (align != "") st.add_style("text-align", align);
                  parcell.add(url == null ? new label(lblText, "", xmlDoc.node_val(c_node, "title"), null, st, _cls.newIdControl).lbl
                    : new link(lblText, xmlDoc.node_val(c_node, "title"), null, c_node.Attributes["open"] == null ? url : "."
                      , c_node.Attributes["open"] != null ? new attrs(new string[,] { { "onclick", url + "return false;" } }) : null, st, null).w_ctrl);
                }
                // input
                else if (c_node.Name == "input" && (type == "int" || type == "euro" || type == "migliaia"
                  || type == "real" || type == "text" || type == "pwd")) {

                  text input = new text("", "txtField", "", new attrs(new string[,] { { "field_type", type }, { "field_name", fieldName }
                    , { "onfocus", "focus_input(this)" }, { "onblur", "blur_input(this); " + (jsblur != "" ? jsblur : "") }
                    , { "form_name", _name }, { "autocomplete", "off" }, { "onkeydown", "return onkeydowninput(event, this, '" + _name + "')" } })
                    , null, _cls.newIdControl);
                  if (!enabled) input.add_attr("readonly", "readonly");
                  else input.add_attr("onkeyup", xmlDoc.node_val(c_node, "jskeyup"));
                  if (hide && type == "text") input.add_style("display", "none");
                  if (type == "pwd") input.txt.TextMode = TextBoxMode.Password;
                  if (jskeydown != "") input.add_attr("onkeydown", jskeydown);
                  if (maxlength != "" && type == "text" || type == "pwd") {
                    if (maxlength.IndexOf('.') < 0) input.add_attr("maxlength", maxlength);
                    else {
                      schema_field sch = conn.schema.table_field(maxlength.Substring(0, maxlength.IndexOf('.')), maxlength.Substring(maxlength.IndexOf('.') + 1));
                      if (sch != null && sch.MaxLength.HasValue) input.add_attr("maxlength", sch.MaxLength.Value.ToString());
                    }
                  }

                  // value
                  if (!_cls.page.IsPostBack) {
                    if (_dr != null && _dr[fieldName] != null)
                      input.txt.Text = valueFromDataRow(_dr, fieldName);
                    else if (input.txt.Text == "" && defValue != "")
                      input.txt.Text = defValue;

                    if (input.txt.Text != "") {
                      if (type == "euro") input.txt.Text = page_ctrl.dbValueToEuro(input.txt.Text);
                      else if (type == "migliaia") input.txt.Text = page_ctrl.dbValueToMigliaia(input.txt.Text);
                      else if (type == "real") input.txt.Text = double.Parse(input.txt.Text).ToString();
                    }
                  }

                  if (type == "int" || type == "euro" || type == "migliaia" || type == "real")
                    input.add_style("text-align", "right");

                  if (width != "") { input.add_style("width", width); input.add_style("display", "inline-block"); }

                  parcell.add(input);

                  // jquery init                            
                  if (type == "euro") _cls.regScript(_cls.scriptStart("initEuro('" + _name + "', '" + fieldName + "')"));
                  else if (type == "migliaia") _cls.regScript(_cls.scriptStart("initMigliaia('" + _name + "', '" + fieldName + "')"));
                }
                // input: date
                else if (c_node.Name == "input" && type == "date") {
                  tbl tbl_cmb = new tbl(null, null, null, null, new styles("witdh:100%"));
                  tbl_row tr = tbl_cmb.add_row(new tbl_row(null, null, width != "" ?
                    new styles("width:" + width + ";display:inline-block;") : null));
                  parcell.add(tbl_cmb);

                  // textbox
                  string initDate = "", clientFmt = "";
                  {
                    html_ctrl div = new html_ctrl("div", new attrs(new string[,] { { "class", "input-control text dateField" } })
                      , new styles("width:100%"));
                    parcell.add(div);

                    text input = new ctrls.text("", "dateField", "", new attrs(new string[,] {{"field_type", type}
                      , { "field_name", fieldName }, { "form_name", _name }, { "autocomplete", "off" } })
                      , new styles("text-align:right;"), _cls.newIdControl);
                    if (!enabled) input.add_attr("readonly", "readonly");
                    if (enabled) input.add_attrs(new string[,] { { "onkeyup", xmlDoc.node_val(c_node, "jskeyup") } 
                      , { "onkeydown", "return onkeydowndate(event, this, '" + _name + "')" }, { "onfocus", "datefocus(this)" }
                      , { "onblur", "dateblur(this)" } });

                    // value                        
                    string value = !_cls.page.IsPostBack ? (_dr != null && _dr[fieldName] != null ? valueFromDataRow(_dr, fieldName)
                      : (input.txt.Text == "" && defValue != "" ? defValue : "")) : "";

                    // format
                    {
                      string serverFmt = "";
                      if (!page.formatDates(xmlDoc.node_val(c_node, "formatDate"), out serverFmt, out clientFmt))
                        throw new Exception("non è stato possibile reperire il formato data da applicare!");

                      DateTime? dtValue = null;
                      try {
                        if (value != "") {
                          dtValue = DateTime.Parse(value);
                          value = dtValue.Value.ToString(serverFmt);
                        }
                      } catch { value = ""; dtValue = null; }

                      if (dtValue.HasValue) initDate = dtValue.Value.ToString("yyyy-MM-dd");
                    }

                    div.add(input);

                    tr.add_cell(new tbl_cell(null, null, new List<ctrl> { div }, null, new styles("width:100%")));
                  }

                  // pulsante calendario
                  if (enabled && !hide) {
                    link lnk = new link("", null, "mif-calendar iconCalendar", "javascript:clickCalendar('" + _name + "', '" + fieldName + "')"
                      , new attrs(new string[,] { { "onmouseover", "overCalendar('" + _name + "', '" + fieldName + "')"} } ));
                    lnk.hlink.Width = Unit.Percentage(100);

                    tr.add(new tbl_cell(null, null, new List<ctrl>() { lnk }, null, new styles("width:100%")));
                  }

                  // input value
                  parcell.add(new text("", "", "", new attrs(new string[,] { { "form_name_date", _name }
                      , { "field_name_date", fieldName } }), new styles("display:none"), _cls.newIdControl));

                  // jquery init                                    
                  _cls.regScript(_cls.scriptStart("initDate('" + _name + "', '" + fieldName + "', '" + initDate + "', '" + clientFmt + "')"));
                }
                // input: check
                else if (c_node.Name == "input" && type == "check") {
                  CheckBox input = new CheckBox();
                  input.ID = _cls.newIdControl;
                  if (!enabled)
                    input.Attributes.Add("readonly", "readonly");
                  input.CssClass = "checkField";
                  input.Attributes.Add("field_type", type);
                  input.Attributes.Add("field_name", fieldName);
                  input.Attributes.Add("form_name", _name);
                  if (enabled) input.Attributes.Add("onkeyup", xmlDoc.node_val(c_node, "jskeyup"));

                  if (!_cls.page.IsPostBack) {
                    string value = "";
                    if (_dr != null && _dr[fieldName] != null)
                      value = valueFromDataRow(_dr, fieldName);
                    else if (value == "" && defValue != "")
                      value = defValue;
                    input.Checked = dbValueToCheck(value);
                  }

                  if (width != "") {
                    input.Style.Add("width", width);
                    input.Style.Add("display", "inline-block");
                  }

                  parcell.add(input);
                }
                // input: upload
                else if (c_node.Name == "input" && type == "upload") {
                  Table tblUpload = new Table();
                  tblUpload.Style.Add("width", "100%");
                  TableRow tblRow = new TableRow();
                  tblUpload.Rows.Add(tblRow);
                  if (width != "") {
                    tblUpload.Style.Add("width", width);
                    tblUpload.Style.Add("display", "inline-block");
                  }

                  parcell.add(tblUpload);

                  // controllo principale
                  {
                    TableCell tblCell = new TableCell();
                    tblCell.Style.Add("width", "100%");
                    {
                      FileUpload input = new FileUpload();
                      input.ID = _cls.newIdControl;
                      input.Style.Add("display", "none");
                      input.Attributes.Add("field_name", fieldName);
                      input.Attributes.Add("form_name", _name);
                      input.Attributes.Add("subname", "upload-ctrl");
                      input.Attributes.Add("onchange", "selFile('" + _name + "', '" + fieldName + "')");
                      tblCell.Controls.Add(input);

                      // hidden
                      TextBox tmp = new TextBox();
                      tmp.ID = _cls.newIdControl;
                      tmp.Style.Add("display", "none");
                      tmp.Attributes.Add("field_name", fieldName + "_tmp");
                      tmp.Attributes.Add("form_name", _name);
                      tblCell.Controls.Add(tmp);

                      tblRow.Cells.Add(tblCell);
                    }

                    // input
                    {
                      TextBox input = new TextBox();
                      input.ID = _cls.newIdControl;
                      input.Attributes.Add("readonly", "readonly");
                      input.CssClass = "uploadField";
                      input.Attributes.Add("form_name", _name);
                      input.Attributes.Add("field_type", type);
                      input.Attributes.Add("field_name", fieldName);
                      input.Attributes.Add("subname", "input-ctrl");
                      input.Style.Add("width", "100%");
                      input.Style.Add("display", "inline-block");

                      tblRow.Cells.Add(tblCell);
                      tblCell.Controls.Add(input);
                    }
                  }

                  // button
                  {
                    Literal btn = new Literal();
                    btn.ID = _cls.newIdControl;
                    btn.Text = "<input class='uploadBtn' type='button' "
                     + " form_name='" + _name + "' field_name='" + fieldName + "'"
                     + " subname='btn-ctrl' value='sfoglia...' onclick=\"$('input[form_name=" + _name + "][field_name=" + fieldName + "][subname=upload-ctrl]').click();\"></input>";

                    TableCell tblCell = new TableCell();
                    tblCell.Style.Add("width", "100%");
                    tblRow.Cells.Add(tblCell);
                    tblCell.Controls.Add(btn);
                  }
                }
                // list
                else if (c_node.Name == "list") {
                  // values
                  text txt = new ctrls.text(!_cls.page.IsPostBack && _dr != null && _dr[fieldName] != null ? valueFromDataRow(_dr, fieldName) : "", "", ""
                    , new attrs(new string[,] { { "field_name", fieldName }, { "form_name", _name }, { "field_type", c_node.Name }, { "lst_sel", xmlDoc.node_val(c_node, "select") }
                      , { "xmldoc", _cls.findControlByAttribute<HtmlTextArea>("forcombos", xmlDoc.node_val(c_node, "select")).ID } })
                    , new styles(new object[,] { { "display", "none" } }), _cls.newIdControl);
                  if (width != "") txt.add_style("width", width);
                  parcell.add(txt);

                  // des
                  parcell.add(new html_ctrl("div", new attrs(new string[,] { { "field_name", fieldName + "_des" }, { "form_name", _name }, { "can_remove", enabled ? "true" : "false" } })));

                  _cls.regScript(_cls.scriptStart("init_list('" + _name + "', '" + fieldName + "')"));
                }
                // combo
                else if (c_node.Name == "combo") {
                  tbl tblc = new tbl(null, null, null, null, new styles(new object[,] { { "width", "100%" } }));
                  tbl_row tblr = tblc.add_row(new tbl_row());
                  if (width != "") tblc.add_styles(new object[,] { { "width", width }, { "display", "inline-block" } });

                  parcell.add(tblc);

                  string ctrlName = (fieldName != "") ? fieldName : "fld" + _cls.newIdControl, title = xmlDoc.node_val(c_node, "title")
                    , onsel_desfield = xmlDoc.node_val(c_node, "onsel_desfield");

                  // input principale
                    tbl_cell tblCell = tblr.add_cell(new tbl_cell(null, null, null, null, new styles(new object[,] { { "width", "100%" } })));
                  text input = (text)tblCell.add(new text("", "comboField", title, new attrs(new string[,] { { "form_name", _name }, {"field_type", "combo"}, {"field_name", ctrlName}
                    , { "lst_sel", xmlDoc.node_val(c_node, "select") }, {"xmldoc", _cls.findControlByAttribute<HtmlTextArea>("forcombos", xmlDoc.node_val(c_node, "select")).ID}
                    , { "onfocus", "combo_onfocus(this)" }, { "onblur", "combo_onblur('" + _name + "', '" + ctrlName + "');" 
                        + (jsblur != "" ? jsblur : "") }, { "autocomplete", "off"}, { "tolist", xmlDoc.node_val(c_node, "tolist") != "" ? "true" : "false" } })
                    , new styles(new object[,] { { "width", "100%" }, { "display", "inline-block" } }), _cls.newIdControl));
                  if (!enabled) input.add_attr("readonly", "readonly");
                  if (onsel_desfield != "") input.add_attr("onsel_desfield", onsel_desfield);
                  if (jsselect != "") input.add_attr("onsel_item", jsselect);

                    // value 
                  if (_dr != null && fieldDes != "" && _dr[fieldDes] != null && !_cls.page.IsPostBack)
                      input.txt.Text = valueFromDataRow(_dr, fieldDes);

                  // freccina
                  tblr.add_cell(new tbl_cell(null, null, new List<ctrl>() { new div(null, "<span class='mif-arrow-down comboFreccina'" + (title != "" ? " title=\"" + title + "\"" : "")
                    + (enabled ? " onclick=\"return show_combo('" + _name + "', '" + ctrlName + "')\" combo-freccina='true'>" : "") + "</span>")}));


                  // input id
                  text iid = (text)parcell.add(new text("", "", "", new attrs(new string[,] {{"form_name_cmb", _name}, {"field_name_cmb", ctrlName}})
                    , new styles(new object[,] { { "display", "none" } }), _cls.newIdControl));

                  // value 
                  if (_dr != null && fieldName != "" && _dr[fieldName] != null && !_cls.page.IsPostBack)
                    iid.txt.Text = valueFromDataRow(_dr, fieldName).ToString();

                  string reloadkey = xmlDoc.node_val(c_node, "onsel_setkey"), resetkeys = xmlDoc.node_val(c_node, "onsel_resetkeys");
                  if (reloadkey != "") input.add_attrs(new string[,] { { "onsel_setkey", reloadkey }, { "key_value", page.page.keystr(reloadkey) } });
                  if (resetkeys != "") input.add_attrs(new string[,] { { "onsel_resetkeys", resetkeys } });

                  // jquery init                                
                  _cls.regScript(_cls.scriptStart(string.Format("init_combo('{0}', '{1}', '{2}')", _name, ctrlName, xmlDoc.node_val(c_node, "tolist"))));
                } else if (c_node.Name == "button") {
                  // if
                  if ((c_node.Attributes["if"] != null && !_cls.page.eval_cond_id(_name, c_node.Attributes["if"].Value, _dr)))
                    continue;

                  string value = c_node.Attributes["value"].Value;
                  string demandid = (type == "submit" || type == "action") && c_node.Attributes["demand"] != null ?
                    _cls.xml_topage(_cls.newIdControl, c_node.Attributes["demand"].Value) : "";

                  if (type == "submit") {
                    button sub = new button(value, "btnForm", xmlDoc.node_val(c_node, "tooltip"), new attrs(new string[,] { { "form_name", _name }
                      , { "noexit", xmlDoc.node_bool(c_node, "noexit") ? "true" : "false" }, { "btn_name", name }, { "btn_type", type }, { "onclick", "return click_button(this, '" + type + "')" } })
                      , style != "" ? new styles(style) : null, new EventHandler(_cls.page.ctrl_Click));
                    if (c_node.Attributes["noconfirm"] != null && bool.Parse(c_node.Attributes["noconfirm"].Value)) sub.add_attr("noconfirm", "true");
                    if (c_node.Attributes["ref"] != null) sub.add_attr("ref", c_node.Attributes["ref"].Value);
                    parcell.add(sub);
                  } else if (type == "action") {
                    button sub = new button(value, "btnForm", xmlDoc.node_val(c_node, "tooltip")
                      , new attrs(new string[,] { { "form_name", _name }, { "btn_name", name }, { "btn_type", type }, { "onclick", "return click_button(this, '" + type + "')" }
                        , { "checks", xmlDoc.node_bool(c_node, "check") ? "true" : "false" }, { "actionname", c_node.Attributes["action"].Value }})
                        , style != "" ? new styles(style) : null);
                    if (c_node.Attributes["noconfirm"] != null
                      && bool.Parse(c_node.Attributes["noconfirm"].Value)) sub.add_attr("noconfirm", "true");
                    if (c_node.Attributes["ref"] != null) sub.add_attr("ref", c_node.Attributes["ref"].Value);
                    if (demandid != "") sub.add_attr("onclick", "return request_form(this, '" + _name + "', null, '" + demandid + "')");
                    sub.btn.Click += new EventHandler(_cls.page.ctrl_Click);
                    parcell.add(sub);
                  } else if (type == "exit") {
                    button submit = new button(value, "btnForm", xmlDoc.node_val(c_node, "tooltip"), new attrs(new string[,] { { "form_name", _name }
                      , { "btn_name", name }, { "btn_type", type }, { "onclick", "return click_button(this, '" + type + "')" } })
                      , style != "" ? new styles(style) : null, new EventHandler(_cls.page.ctrl_Click));
                    parcell.add(submit);
                  }
                } else if (c_node.Name == "newline") {
                  Literal literal = new Literal();
                  literal.ID = _cls.newIdControl;
                  literal.Text = "<br/>";

                  parcell.add(literal);
                }

                ictrl++;
              }

              icol += field.Attributes["colspan"] != null ? Convert.ToInt32(field.Attributes["colspan"].Value) : 1;
            }

            irow++;
          }
        }

        // blur onsel combo
        if (_cls.page.IsPostBack) {
          // ciclo combo della form
          XmlNodeList combos = rootSelNodes("contents//combo");
          foreach (XmlNode combo in combos) {
            bool enabled = xmlDoc.node_bool(combo, "enabled", true);
            if (combo.Attributes["onsel_desfield"] != null && enabled)
              _cls.regScript(_cls.scriptStart("combo_onblur('" + _name + "', '" + combo.Attributes["field"].Value + "')"));
          }
        }
      } catch (Exception ex) { _cls.addError(ex); }
      }

    protected string combo_sel_xml(string id) {
      string fieldId, fieldVal, fieldDes;
      return combo_sel_xml(id, out fieldId, out fieldVal, out fieldDes);
    }

    protected string combo_sel_xml(string id, out string fieldId, out string fieldVal, out string fieldDes) {

      // fields
      string[] flds = rootSelNode("queries/select[@forcombos][@name='" + id + "']")
        .Attributes["forcombos"].Value.Split(',');
      fieldId = flds[0].Trim(); fieldVal = flds[1].Trim(); fieldDes = flds.Length > 2 ? flds[2].Trim() : "";

      return db_provider.set_todoc(_cls.dt_from_id(id, _name)).InnerXml;
    }
    protected XmlNode rowOfControl(XmlNode ctrl) {
      while (true && ctrl.ParentNode != null)
        if (ctrl.Name == "row")
          return ctrl;
        else
          ctrl = ctrl.ParentNode;

      return null;
    }

    public void insertAttach(XmlNode attach) {
      if (attach.Name != "form-attach")
        throw new Exception("elemento '" + attach.Name + "' non attaccabile!");

      // <contents>
      importMainNode(attach, "contents");

      // <jscripts>
      importMainNode(attach, "jscripts");

      // <queries>
      importMainNode(attach, "queries");

      // <include>
      importMainNode(attach, "include");

      // <scripts> 
      importMainNode(attach, "scripts");

      // <sections>
      importMainNode(attach, "sections");
    }

    protected void importMainNode(XmlNode attach, string name) {
      XmlNode main = attach.SelectSingleNode(name);
      if (main == null)
        return;

      XmlNode parent = rootNode.SelectSingleNode(name) != null ? rootNode.SelectSingleNode(name)
          : rootNode.AppendChild(rootNode.OwnerDocument.CreateElement(name));

      foreach (XmlNode child in main.ChildNodes)
        parent.AppendChild(parent.OwnerDocument.ImportNode(child, true));
    }

    public override bool ctrlClick(object sender, EventArgs e) {
      if (base.ctrlClick(sender, e))
        return true;

      return false;
    }

    protected bool if_node(XmlNode row, DataRow dr) {
      return row.Attributes["if"] != null && !_cls.page.eval_cond_id(_name, row.Attributes["if"].Value, dr);
    }

    public bool convalidaForm(out string err) {
      bool result = true;
      err = "";

      //db_provider db = null;
      try {
        // rows
        foreach (XmlNode row in rootSelNodes("contents/row")) {
          if (if_node(row, _dr)) continue;

          // field
          foreach (System.Xml.XmlNode fld in row.SelectNodes("field")) {
            if (if_node(fld, _dr)) continue;

          // ciclo campi della form
            foreach (XmlNode input in row.SelectNodes("field//input[@field] | field//combo[@field]")) {
              if (if_node(input, _dr)) continue;

              string field = input.Attributes["field"].Value, tpinput = input.Name == "input" ? xmlDoc.node_val(input, "type") : "";

            // valore del campo
            bool required = false;
            string type = "", errField = "", value = "";
              if (input.Name == "input") {
                if (!ctrlValue(input, _name, out value, out type, out required, out errField))
                  throw new Exception("il campo '" + ((errField != "") ? errField : field) + "' non è stato trovato");
              } else if (input.Name == "combo") {
            if (!ctrlValue(input, _name, out value, out type, out required, out errField))
              throw new Exception("il campo '" + ((errField != "") ? errField : field) + "' non è stato trovato");

                if (value == "" && xmlDoc.node_bool(input, "can_edit") &&
                  !ctrlValue(input, _name, out value, out type, out required, out errField, true))
                  throw new Exception("il campo '" + ((errField != "") ? errField : field) + "' non è stato trovato");
              }

            // required
            if (value == "" && required)
              throw new Exception("è richiesto il campo '" + ((errField != "") ? errField : field) + "'!");

            // value 
            int tmp = 0;
            if ((value != "" && type == "int" && !int.TryParse(value, out tmp))
                || (value != "" && type == "euro" && !page_ctrl.isEuro(value))
                || (value != "" && type == "migliaia" && !page_ctrl.isMigliaia(value))
                || (value != "" && type == "real" && !page_ctrl.isDouble(value)))
              throw new Exception("il campo '" + ((errField != "") ? errField : field) + "' non è valido!");

            // condizioni specificate                    
            foreach (XmlNode ifNode in input.SelectNodes("if")) {
                if (ifNode.Attributes["if"] != null && !_cls.page.eval_cond_id(_name, ifNode.Attributes["if"].Value))
                continue;

              DataTable dt = ifNode.Attributes["select"] != null ?
                  _cls.page.classPage.dt_from_id(ifNode.Attributes["select"].Value, _name) : null;
                if (ifNode.InnerText != "" ? bool.Parse(_cls.page.parse(ifNode.InnerText, _name
                , dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null)) : dt.Rows.Count > 0)
                throw new Exception(ifNode.Attributes["message"].Value);
            }
          }
        }
        }
      } catch (Exception ex) { result = false; err = ex.Message; }

      return result;
    }

    public void submitDataForm() { _cls.exec_updates(rootAttr("updates"), _name); }

    #endregion

    #region controls

    public override bool fieldValue(string field, out string value, bool force_input = false) {
      if (base.fieldValue(field, out value, force_input))
        return true;

      value = "";
      if (ctrlValue(_name, field, out value, force_input))
        return true;

      return false;
    }

    protected string ctrlValue(string formName, string fieldName) {
      string value = "";
      return ctrlValue(formName, fieldName, out value) ? value : "";
    }

    protected bool ctrlValue(string formName, string fieldName, out string value, bool force_input = false) {
      string type = "";
      bool required = false;
      string errField = "";

      value = "";

      return ctrlValue(formName, fieldName, out value, out type, out required, out errField, force_input);
    }

    protected bool ctrlValue(string formName, string fieldName, out string value, out string type, out bool required, out string errField, bool force_input = false) {
      errField = "";
      required = false;
      type = "";
      value = "";
      Control ctrl = inputControl(formName, fieldName, out type, out required, out errField);
      if (ctrl == null)
        return false;

      return controlValue(ctrl, formName, fieldName, type, out value, force_input);
    }

    protected bool ctrlValue(System.Xml.XmlNode ctrlNode, string formName, out string value, out string type, out bool required, out string errField, bool force_input = false) {
      errField = "";
      required = false;
      type = "";
      value = "";
      Control ctrl = inputControl(ctrlNode, formName, out type, out required, out errField);
      if (ctrl == null)
        return false;

      return controlValue(ctrl, formName, ctrlNode.Attributes["field"].Value, type, out value, force_input);
    }

    /// <summary>
    /// Ottenere il valore della form
    /// </summary>
    /// <param name="force_input">vogliamo forzare la lettura della casella di testo per i casi particolari</param>
    protected bool controlValue(Control ctrl, string form, string field, string type, out string value, bool force_input = false) {
      value = "";

      if (type == "pwd" || type == "list" || type == "text" || type == "int"
        || type == "euro" || type == "migliaia" || type == "real") value = ((TextBox)ctrl).Text;
      else if (type == "textarea") value = ((HtmlTextArea)ctrl).InnerText;
      else if (type == "date") value = _cls.findControlByAttributes<TextBox>("field_name_date", field, "form_name_date", form).Text;
      else if (type == "check") value = ((CheckBox)ctrl).Checked.ToString();
      else if (type == "combo") value = !force_input ? _cls.findControlByAttributes<TextBox>("field_name_cmb", field, "form_name_cmb", form).Text :
          ((TextBox)ctrl).Text;
      else if (type == "upload") value = ((FileUpload)ctrl).FileName;
      else return false;

      return true;
    }

    public Control inputControl(string fieldName) {
      string type = "";
      bool required = false;
      string errField = "";

      return inputControl(_name, fieldName, out type, out required, out errField);
    }

    protected Control inputControl(System.Xml.XmlNode ctrlNode, string formName, out string typeInput, out bool required, out string errField) {
      typeInput = ctrlNode.Name == "combo" ? "combo"
          : ctrlNode.Name == "list" ? "list" : xmlDoc.node_val(ctrlNode, "type");
      required = xmlDoc.node_bool(ctrlNode, "required", false);
      errField = xmlDoc.node_val(ctrlNode, "errField");

      if (!_cls.existControl(formName)) return null;

      string fieldName = ctrlNode.Attributes["field"].Value;
      return (typeInput == "check" ? (Control)_cls.findControlByAttributes<CheckBox>("field_name", fieldName, "form_name", formName)
          : typeInput == "upload" ? (Control)_cls.findControlByAttributes<FileUpload>("field_name", fieldName, "form_name", formName)
          : typeInput == "textarea" ? (Control)_cls.findHtmlControlByAttributes<HtmlTextArea>("field_name", fieldName, "form_name", formName)
          : (Control)_cls.findControlByAttributes<TextBox>("field_name", fieldName, "form_name", formName));
    }

    protected Control inputControl(string frm_name, string fld_name, out string tp_input, out bool required, out string errField) {
      tp_input = errField = "";required = false;

      if (!_cls.existControl(frm_name)) return null;

      // ricerco il nodo xml del controllo in base alle condizioni impostate
      foreach (System.Xml.XmlNode c_node in rootSelNodes("contents//*[@field='" + fld_name + "']")) {
        if (c_node.ParentNode.ParentNode.Name != "row") throw new Exception("la struttura xml del documento del form '" + frm_name + "' non è corretta!");

        if(!_cls.page.eval_cond_id(frm_name, xmlDoc.node_val(c_node.ParentNode.ParentNode, "if"))
          || !_cls.page.eval_cond_id(frm_name, xmlDoc.node_val(c_node.ParentNode, "if"))
          || !_cls.page.eval_cond_id(frm_name, xmlDoc.node_val(c_node, "if"))) continue;

        return inputControl(c_node, frm_name, out tp_input, out required, out errField);
      }

      return null;
    }

    public string upload_save(string name_ctrl, string path) { ((FileUpload)inputControl(name_ctrl)).SaveAs(path); return path; }

    public string upload_filename(string name_ctrl) { return ((FileUpload)inputControl(name_ctrl)).FileName; }

    public void upload_set_path(string name_ctrl, string path = "") { _cls.input_ctrl(_name, name_ctrl + "_tmp").Text = path; }

    public string upload_path(string name_ctrl) { return _cls.input_ctrl(_name, name_ctrl + "_tmp").Text; }

    #endregion

  }
}

// 883
