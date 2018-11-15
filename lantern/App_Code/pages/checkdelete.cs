using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using deeper.frmwrk;
using deeper.db;
using deeper.lib;

namespace deeper.pages
{
  public class checkdelete : deeper.frmwrk.page_cls
  {
    protected deeper.db.db_schema _db = null;
    protected string _table = "";
    protected deeper.db.meta_table _meta = null;
    protected bool _exists = false, _only_linked = false, _viewtbl = false;

    public checkdelete(deeper.frmwrk.lib_page page, XmlNode pageNode)
      : base(page, pageNode) { }

    public override void onInit(object sender, EventArgs e, bool request = false, bool addControls = true) {
      _db = page.query_param("dbname") != "" ? page.conn_db(page.query_param("dbname")) : page.conn_db_user();
      if (!_db.exist_schema || !_db.exist_meta)
        throw new Exception("attenzione, il database non ha lo schema xml e non è possibile gestire i collegamenti fra le tabelle!");

      _viewtbl = page.pageName == "table_view";
      _only_linked = page.query_param("lnk") == "1";
      _table = page.query_param("tbl") != "" ? page.query_param("tbl") : _db.schema.tableFromPk(page.query_param("fld"));
      _meta = _db.meta_doc.meta_tbl(_table);
      _exists = !_viewtbl ? _db.get_count("select count(*) from [" + _table + "] "
          + " where [" + page.query_param("fld") + "] = " + page.query_param("val")) > 0 : false;

      // esiste l'elemento?
      if (addControls) {
        if (!_viewtbl) {
          if (!_exists) regScript(scriptStartAlert("L'elemento è già stato rimosso in precedenza!", "Messaggio", "navToPrev()"));
          else {
            // conteggio elementi correlati
            long count = _db.rel_tables_count(_table, page.query_param("val")).Sum(kv => kv.Value);
            if (_only_linked && count == 0)
              regScript(scriptStartAlert("Non ci sono elementi correlati!", "Messaggio", "navToPrev()"));
            else setPageDoc(docForDel(count, _db.count_notnull_remove_links(_table, long.Parse(page.query_param("val")))
              , page.query_param("iddmain")));
          }
        } else setPageDoc(addGrid(_db, xmlDoc.doc_from_xml("<page schema='xmlschema.ctrls'><contents/></page>")
          , _table, "grid0", "tabella " + (_meta != null ? _meta.title : _table), "", meta_link.types_link.normal, "", "", "=", true, true));
      }

      base.onInit(sender, e, request, addControls);
    }

    public override string titlePage(XmlNode pageNode, Dictionary<string, string> fields = null
        , System.Xml.XmlNode row = null, System.Data.DataRow dr = null) { return string.Format(base.titlePage(pageNode, fields, row, dr), _meta != null ? _meta.title : _table); }

    public override string desPage(XmlNode pageNode, Dictionary<string, string> fields = null
        , System.Xml.XmlNode row = null, System.Data.DataRow dr = null) {
      string base_des = base.desPage(pageNode, fields, row, dr);

      return _viewtbl ? base_des : desElement(_db, _table, page.query_param("fld"), page.query_param("val"));
    }

    protected string desElement(deeper.db.db_schema db, string table, string fldprimary, string value) {
      deeper.db.meta_table metatbl = db.meta_doc.meta_tbl(table);
      if (!_exists)
        return metatbl.single;

      string sqlfields = "", sqljoins = "";
      foreach (KeyValuePair<string, Dictionary<string, string>> col
          in db.meta_doc.tableCols(table, new List<meta_doc.col_type> { /*meta_doc.col_type.diretta, */ meta_doc.col_type.linked, meta_doc.col_type.info })) // false, true, true, false, true
            {
        if (bool.Parse(col.Value["diretta"]))
          sqlfields += " tbl.[" + col.Key + "], ";
        else if (bool.Parse(col.Value["linked"])) {
          string indice = col.Value["indice"], lnktable = col.Value["linkedtable"];

          if (col.Value["linkedtype"] == "list")
            sqlfields += fnc_ids(db, table, lnktable, "tbl.[" + col.Key + "]")
                + " AS [" + col.Key + "_" + lnktable + "], ";
          else {
            sqljoins += " left join [" + lnktable + "] tbl" + indice + " ON "
                + " tbl" + indice + ".[" + db.schema.pkOfTable(lnktable) + "] = tbl.[" + col.Key + "] ";

            // colonne dirette linkedtable
            Dictionary<string, Dictionary<string, string>> cols2 = db.meta_doc.tableCols(lnktable, new List<meta_doc.col_type> { meta_doc.col_type.diretta, meta_doc.col_type.info }); // false, true, false, false, true
            foreach (KeyValuePair<string, Dictionary<string, string>> col2 in cols2)
              if (bool.Parse(col2.Value["diretta"]))
                sqlfields += " tbl" + indice + ".[" + col2.Key + "] AS [" + col2.Key + "_" + lnktable + "], ";
          }
        }
      }

      System.Data.DataTable dt = db.dt_table("select " + sqlfields.Substring(0, sqlfields.Length - 2)
          + " from [" + table + "] tbl " + sqljoins + " where tbl.[" + fldprimary + "] = " + value);

      string result = "";
      foreach (System.Data.DataColumn col in dt.Columns)
        result += dt.Rows[0][col.ColumnName].ToString() != "" ?
            (result != "" ? ", " : "") + dt.Rows[0][col.ColumnName].ToString() : "";

      return result != "" ? metatbl.single + ": " + result : metatbl.single;
    }

    protected xmlDoc docForDel(long count, long c_notnull, string id_replace) {
      xmlDoc doc = xmlDoc.doc_from_xml("<page schema='xmlschema.ctrls'><contents/></page>");

      // form
      if (!_only_linked) {
      if (count > 0) {
        XmlNode form = doc.add_xml("/page/contents", "<form name='main' title='Elemento sostitutivo' selects='load' width='800px' height='100px'>"
            + "<contents><row><field><label text='sostituisci con:'/></field>"
            + "  <field><input field='id_elemento' type='text' hide='true'/>"
            + "   <input field='des_elemento' type='text' enabled='false'/></field></row>"
            + " <row><field colspan='2'><label text='numero elementi correlati: " + count.ToString() + "'/></field></row>"
            + " <row><field class='footer' maxspan='true' right='true'>"
            + (!_only_linked ? "<button type='action' action='remove-all' tooltip='eliminazione o sostituzione di tutti gli elementi collegati' value='Togli gli elementi'/>" : "")
            + " <button type='action' action='replace-all' value='" + ((id_replace != "") ? "Sostituisci tutto" : "Togli ogni riferimento")
            + "' " + (id_replace == "" && c_notnull > 0 ? "demand='Alcune tabelle hanno il campo obbligatorio e non saranno aggiornate, vuoi continuare ugualmente?'" : "") + "/>"
            + " <button type='exit' value='Esci'/></field></row></contents>"
            + "<queries><select name='load'><![CDATA[" + (id_replace != "" ? "select '" + id_replace + "' as id_elemento"
                + ", '" + desElement(_db, _table, page.query_param("fld").ToUpper(), id_replace) + "' as des_elemento"
                : "select '' as id_elemento, '' as des_elemento") + "]]></select></queries></form>");
        } else {
        XmlNode form = doc.add_xml("/page/contents", "<form name='main' selects='load' width='800px' height='100px'>"
            + "<contents><row><field><label text='cancellazione:'/></field>"
            + "  <field><input field='id_elemento' type='text' hide='true'/>"
            + "   <input field='des_elemento' type='text' enabled='false'/></field></row>"
            + " <row><field class='footer' maxspan='true' right='true'><button type='action' action='remove-element' tooltip='eliminazione elemento' value='Rimuovi'/>"
            + "   <button type='exit' value='Esci'/></field></row></contents>"
            + "<queries><select name='load'><![CDATA[select '" + page.query_param("val") + "' as id_elemento"
            + " , '" + desElement(_db, _table, page.query_param("fld").ToUpper(), page.query_param("val")) + "' as des_elemento]]></select>"
            + "</queries></form>");
      }
      }

      if (count > 0) {
        // dettagli
        if (!_only_linked) doc.add_xml("/page/contents", "<section name='titleDetail'><![CDATA[<h3>Dettaglio elementi collegati</h3><br/>]]></section>");

        // tabs
        XmlNode tabs = xmlDoc.set_attr(doc.add_node("/page/contents", "tabs"), "name", "tablestab");

        // main grid
        if (!_only_linked) addGrid(_db, doc, _table, "grid" + tabs.ChildNodes.Count.ToString(), "tabella " + _meta.title, "tablestab." + addTab(tabs, _meta.title)
           , meta_link.types_link.normal, page.query_param("fld"), page.query_param("val"), "<>", true, false, false, "iddmain"
           , (!_only_linked ? new Dictionary<string, Dictionary<string, string>>() { {"set-substitute-element"
              , new Dictionary<string, string>() { {"des", "imposta come elemento sostitutivo..."}, {"icon", "mif-arrow-right"}, {"url", "{@currurlargs='nohstr=1'}"} } } } : null));

        // aggiunta griglie collegate
        foreach (deeper.db.meta_link lnk in _db.meta_doc.table_links(_table)
          .Where(t => _db.count_links(t, page.query_param("val")) > 0)) addGrid(_db, doc, lnk.table_link
            , "grid" + tabs.ChildNodes.Count.ToString(), "tabella " + lnk.title
            , "tablestab." + addTab(tabs, lnk.title), lnk.type, lnk.field.ToUpper()
            , page.query_param("val"), "=", true, _only_linked, !_only_linked);
      }

      return doc;
    }

    public override bool action(string actionName, string formName, string keys = "", string noConfirm = "", string refurl = "") {
      if (base.action(actionName, formName, keys, noConfirm))
        return true;

      if (actionName == "remove-element") {
        _db.exec("delete from [" + _table + "] where [" + page.query_param("fld") + "] = " + page.query_param("val"));
        regScript(scriptStartAlert("L'elemento è stato rimosso con successo!", "Messaggio"
            , null, refurl != null ? page.parse(refurl) : page.urlPrev()));

        return true;
      } else if (actionName == "remove-all") {
        try {
          _db.begin_trans();

          long records = _db.remove_links(_table, page.query_long("val"), ((deeper.frmwrk.ctrls.form_ctrl)control("main")).fieldValue("id_elemento"));
          records += _db.exec("delete from [" + _table + "] where [" + page.query_param("fld") + "] = " + page.query_param("val"));
          regScript(scriptStartAlert("Sono stati tolti con successo '" + records.ToString() + "' elementi.", "Messaggio"
              , null, refurl != null ? page.parse(refurl) : page.urlPrev()));

          _db.commit();
        } catch (Exception ex) { _db.rollback(); throw ex; }
        return true;
      } else if (actionName == "replace-all") {
        long records = _db.remove_links(_table, page.query_long("val")
          , ((deeper.frmwrk.ctrls.form_ctrl)control("main")).fieldValue("id_elemento"), true, false);
        regScript(scriptStartAlert("Sono stati aggiornati con successo '" + records.ToString() + "' elementi."
            , "Messaggio", "reloadPage()"));

        return true;
      }

      return false;
    }

    protected string addTab(XmlNode tabs, string title) {
      string tabName = "tab" + tabs.ChildNodes.Count.ToString();
      {
        XmlNode tab = tabs.AppendChild(tabs.OwnerDocument.CreateElement("tab"));
        xmlDoc.set_attr(tab, "name", tabName);
        xmlDoc.set_attr(tab, "title", title);
        xmlDoc.set_attr(tab, "des", "tabella");
      }

      return tabName;
    }

    protected xmlDoc addGrid(deeper.db.db_schema db, xmlDoc page_doc, string table, string id, string title, string tab = "", meta_link.types_link type_link = meta_link.types_link.normal
      , string primaryfield = "", string value = "", string operatore = "=", bool with_dip = false, bool with_linked = false, bool delCol = false, string par_subst_key = "iddtbl"
      , Dictionary<string, Dictionary<string, string>> actions = null) {

      string pk_table = db.schema.pkOfTable(table);
      XmlNode grid = page_doc.add_xml("/page/contents", "<grid name='" + id + "' title=\"" + title + "\" "
          + " selects='load' pagesize='15' " + (tab != "" ? " tab='" + tab + "'" : "") + " top='100'/>");

      // cols, query
      deeper.db.meta_table metatbl = db.meta_doc.meta_tbl(table);
      Dictionary<string, Dictionary<string, string>> grd_cols = db.meta_doc.tableCols(table, new List<meta_doc.col_type> { meta_doc.col_type.primary, /*meta_doc.col_type.diretta,*/ meta_doc.col_type.info });
      XmlNode cols = xmlDoc.add_xml(grid, db.schema.there_pk(table) ? "<cols key='" + pk_table.ToLower() + "(" + par_subst_key + ")'/>" : "<cols/>");
      string sqlfields = "", sqljoins = "";
      if (grd_cols.Count > 0) {
        //sqlfields += "tbl.[" + pk_table + "], ";

        // colonne dirette
        foreach (KeyValuePair<string, Dictionary<string, string>> item in grd_cols) {

        // colonne linked
          if (item.Value["linkedtable"] != "") {
          string lnktable = item.Value["linkedtable"];

          if (item.Value["linkedtype"] == "list") {
              xmlDoc.add_xml(cols, "<col title='" + db.meta_doc.titleCol(item.Key) + "'"
                + " field='" + item.Key + "_" + lnktable + "'/>");

              sqlfields += fnc_ids(db, table, lnktable, "tbl.[" + item.Key + "]")
                + " AS [" + item.Key + "_" + lnktable + "], ";
          } else {
              string indice = item.Value["indice"];
            sqljoins += " left join [" + lnktable + "] tbl" + indice + " ON "
                + " tbl" + indice + ".[" + db.schema.pkOfTable(lnktable) + "] = tbl.[" + item.Key + "] ";

            // colonne dirette linkedtable
              Dictionary<string, Dictionary<string, string>> tblcols = db.meta_doc.tableCols(lnktable, new List<meta_doc.col_type> { /*meta_doc.col_type.diretta, meta_doc.col_type.linked, *//*meta_doc.col_type.service,*/ meta_doc.col_type.info });
            foreach (KeyValuePair<string, Dictionary<string, string>> item2 in tblcols) {
                string indice2 = item2.Value["indice"], lnktable2 = item2.Value["linkedtable"];

              if (!bool.Parse(item2.Value["linked"])) {
                  xmlDoc.add_xml(cols, "<col title='" + titleCol(db.meta_doc, item2.Key, lnktable, item.Key) + "'"
                    + " field='" + item.Key + "_" + item2.Key + "_" + lnktable + "'"
                    + " type='" + gridTypeCol(db, item2.Value["type"]) + "'/>");

                sqlfields += " tbl" + indice + ".[" + item2.Key + "] AS [" + item.Key + "_" + item2.Key + "_" + lnktable + "], ";
              } else {
                if (item2.Value["linkedtype"] == "list") {
                    xmlDoc.add_xml(cols, "<col title='" + titleCol(db.meta_doc, item2.Key, lnktable, item.Key) + "'"
                      + " field='" + item2.Key + "_" + lnktable2 + "'/>");

                    sqlfields += fnc_ids(db, lnktable, lnktable2, "tbl" + indice + ".[" + item2.Key + "]")
                      + " AS [" + item2.Key + "_" + lnktable2 + "], ";
                } else {
                  sqljoins += " left join [" + lnktable2 + "] tbl" + indice + "_" + indice2
                      + " ON tbl" + indice + "_" + indice2 + ".[" + db.schema.pkOfTable(lnktable2) + "]"
                      + " = tbl" + indice + ".[" + item2.Key + "] ";

                  Dictionary<string, Dictionary<string, string>> tbl2cols = db.meta_doc.tableCols(lnktable2, new List<meta_doc.col_type> { meta_doc.col_type.diretta, meta_doc.col_type.info }, false);
                  foreach (KeyValuePair<string, Dictionary<string, string>> item3 in tbl2cols) {
                      xmlDoc.add_xml(cols, "<col title='" + titleCol(db.meta_doc, item3.Key, lnktable2, item.Key) + "'"
                        + " field='" + item.Key + "_" + item2.Key + "_" + item3.Key + "_" + lnktable2 + "'"
                        + " type='" + gridTypeCol(db, item3.Value["type"]) + "'/>");

                    sqlfields += " tbl" + indice + "_" + indice2 + ".[" + item3.Key + "] AS [" + item.Key + "_" + item2.Key + "_" + item3.Key + "_" + lnktable2 + "], ";
                  }
                }
              }
            }
          }
          } else {
            xmlDoc.add_xml(cols, "<col title='" + db.meta_doc.titleCol(item.Key) + "' field='" + item.Key + "'"
                + " type='" + gridTypeCol(db, item.Value["type"]) + "'/>");

            sqlfields += "tbl.[" + item.Key + "], ";
          }
        }

        // colonne di servizio
        foreach (KeyValuePair<string, Dictionary<string, string>> item in
          db.meta_doc.tableCols(table, new List<meta_doc.col_type> { meta_doc.col_type.service })) {
          xmlDoc.add_xml(cols, "<col title='" + db.meta_doc.titleCol(item.Key) + "' field='" + item.Key + "'"
                  + " type='" + gridTypeCol(db, item.Value["type"]) + "'/>");

          sqlfields += "tbl.[" + item.Key + "], ";
        }

        // colonna dipendenze
        if (with_dip) {
          xmlDoc.add_xml(cols, "<col title='Dipendenze' field='dipendenze' type='integer'/>");

          string flddip = "";
          foreach (deeper.db.meta_link lnk in db.meta_doc.table_links(table))
            flddip += (flddip != "" ? " union all " : "") + "select count(*) as conteggio from [" + lnk.table_link + "] "
              + (lnk.type == meta_link.types_link.list ? " where charindex('[' + cast(tbl.[" + pk_table + "] as varchar) + ']', [" + lnk.field + "]) > 0"
                : " where [" + lnk.field + "] = tbl.[" + pk_table + "]");

          sqlfields += flddip != "" ? string.Format("(select sum(conteggio) from ({0}) tblconteggi) as Dipendenze, ", flddip) : "(select 0) as Dipendenze, ";
        }

        if (with_linked)
          xmlDoc.add_xml(cols, "<action des='elementi correlati...' icon='mif-arrow-right' type='linked' primarykey='" + pk_table + "'/>");

        // colonna azioni
        if (actions != null)
          foreach (KeyValuePair<string, Dictionary<string, string>> act in actions)
            xmlDoc.add_xml(cols, "<action action='" + act.Key + "' des=\"" + act.Value["des"] + "\" pageref=\"" + act.Value["url"] + "\" icon='" + act.Value["icon"] + "'/>");

        // colonna di eliminazione singolo elemento
        if (delCol) xmlDoc.add_xml(cols, "<action des=\"elimina " + metatbl.single + "...\" "
             + " icon='mif-cross' primarykey='" + pk_table + "'/>");
      } else {
        // colonne dirette
        foreach (db.schema_field fld in db.table_fields(table)) {
          xmlDoc.add_xml(cols, "<col title='" + db.meta_doc.titleCol(fld.Name) + "' field='" + fld.Name + "'"
              + " type='" + gridTypeCol(db, fld.OriginalType) + "'/>");

          sqlfields += "tbl.[" + fld.Name + "], ";
        }
      }

      if (sqlfields != "") xmlDoc.set_attr(xmlDoc.add_node(xmlDoc.add_node(grid, "queries"), "select"), "name", "load")
              .InnerText = "select " + sqlfields.Substring(0, sqlfields.Length - 2) + " from [" + table + "] tbl " + sqljoins
                  + (primaryfield != "" && operatore != "" && value != "" ? (type_link == meta_link.types_link.list && operatore == "=" ?
                  "where charindex('[" + value + "]', [" + primaryfield + "]) > 0" : " where tbl.[" + primaryfield + "] " + operatore + " " + value) : "");

      return page_doc;
    }
    protected string fnc_ids(db_schema db, string main_table, string table, string field) {
      return _page.parse(db.meta_doc.functionIds(table, field)
        , new Dictionary<string, string>() { { "main_tbl", main_table }, { "field_ids", field } }, db);
    }

    protected string titleCol(meta_doc meta, string col, string table, string maincol = "") {
      return meta.titleCol(col);
      //string title = meta.doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "single",
      //    meta.doc.get_value_throw("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "title", "titolo tabella '" + table + "' non specificato"));
      //if (maincol != "" && meta.doc.exist("/root/fields/field[@nameupper='" + maincol.ToUpper() + "']"))
      //  title = meta.doc.get_value("/root/fields/field[@nameupper='" + maincol.ToUpper() + "']", "title");

      //if (meta.doc.get_bool("/root/fields/field[@nameupper='" + col.ToUpper() + "']", "nosingle"))
      //  return meta.doc.get_value("/root/fields/field[@nameupper='" + col.ToUpper() + "']", "title", col);

      //string title2 = meta.doc.get_value("/root/fields/field[@nameupper='" + col.ToUpper() + "']", "title", col);
      //return title2.Trim().ToLower() == title.Trim().ToLower() ? title : title2.Trim() + " - " + title.Trim();
    }

  }
}