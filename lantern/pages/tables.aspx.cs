using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;
using deeper.frmwrk;
using deeper.db;
using deeper.lib;

public partial class _tables : deeper.frmwrk.lib_page
{
  #region classi d'appoggio

  /// <summary>
  /// Intervento da effettuare sul database
  /// </summary>
  class action_db
  {
    public enum action_code
    {
      none, act_set_ver_db, act_add_table, act_remove_table, act_add_fnc, act_upd_fnc,
      act_init_history_table, act_init_srvfields_history_table, act_init_list_tags, act_align_data, act_remove_ids, 
      act_add_field, act_add_field_null, act_del_field, act_init_uidx, act_init_uidx_onins, act_init_del_onins, act_upd_field
    }

    // proprietà
    int _id = -1;
    action_code _code = action_code.none;
    Dictionary<string, string> _action_pars = null;
    string _text;

    public int id { get { return _id; } }
    public action_code action { get { return _code; } }
    public Dictionary<string, string> action_pars { get { return _action_pars; } }
    public string text { get { return _text; } set { _text = value; } }

    // costruttori
    action_db(int id, action_code code, string tx, Dictionary<string, string> pars) { _id = id; _code = code; _text = tx; _action_pars = pars; }

    // metodi
    static public action_db add_init_list_tags(List<action_db> actions, string table, string field, string list_table, List<string> values) {
      return action_to_list(actions, "inizializza valori collegati sul campo '" + field + "'", action_code.act_init_list_tags, new Dictionary<string, string>() { { "table", table }, { "field", field }
            , { "list_table", list_table }, { "values", string.Join(",", values.ToArray()) } });
    }

    static public action_db add_remove_ids(List<action_db> actions, string table, string field, string values) {
      return action_to_list(actions, "elimina i records che non hanno la corrispondenza con la tabella collegata per il campo '" + field + "'", action_code.act_remove_ids, new Dictionary<string, string>() { { "table", table }, { "field", field }
            , { "values", values } });
    }

    static public action_db add_align_data_table(List<action_db> actions, string table, string w_codes, string xml_schema, string ids_file) {
      return action_to_list(actions, "allinea i dati della tabella '" + table + "'", action_code.act_align_data
        , new Dictionary<string, string>() { { "table", table }, { "w_codes", w_codes }, { "ids_file", ids_file }, { "db_xml", xml_schema } });
    }
    static public action_db add_add_fnc(List<action_db> actions, string function, string from_ver) { return action_to_list(actions, "aggiungi la funzione '" + function + "'", action_code.act_add_fnc, new Dictionary<string, string>() { { "function", function }, { "from_ver", from_ver } }); }
    static public action_db add_upd_fnc(List<action_db> actions, string function, string from_ver) { return action_to_list(actions, "aggiorna la funzione '" + function + "'", action_code.act_upd_fnc, new Dictionary<string, string>() { { "function", function }, { "from_ver", from_ver } }); }    
    static public action_db add_add_table(List<action_db> actions, string table, string from_ver) { return action_to_list(actions, "aggiungi la tabella '" + table + "'", action_code.act_add_table, new Dictionary<string, string>() { { "table", table }, { "from_ver", from_ver } }); }
    static public action_db add_set_ver_db(List<action_db> actions, string ver, string des) { return action_to_list(actions, "imposta la versione '" + ver + "' al database", action_code.act_set_ver_db, new Dictionary<string, string>() { { "ver", ver }, { "des", des } }); }
    static public action_db add_add_field(List<action_db> actions, string table, string field, string from_ver) { return action_to_list(actions, "aggiungi il campo '" + field + "' della tabella '" + table + "'", action_code.act_add_field, new Dictionary<string, string>() { { "table", table }, { "field", field }, { "from_ver", from_ver } }); }
    static public action_db add_add_field_null(List<action_db> actions, string table, string field, string from_ver) { return action_to_list(actions, "aggiungi il campo '" + field + "' della tabella '" + table + "' forzato a null", action_code.act_add_field_null, new Dictionary<string, string>() { { "table", table }, { "field", field }, { "from_ver", from_ver } }); }
    static public action_db add_del_field(List<action_db> actions, string table, string field) { return action_to_list(actions, "elimina il campo '" + field + "' della tabella '" + table + "'", action_code.act_del_field, new Dictionary<string, string>() { { "table", table }, { "field", field } }); }
    static public action_db add_upd_field(List<action_db> actions, string table, string field) { return action_to_list(actions, "aggiorna il campo '" + field + "' della tabella '" + table + "' dallo schema", action_code.act_upd_field, new Dictionary<string, string>() { { "table", table }, { "field", field } }); }
    static public action_db add_remove_table(List<action_db> actions, string table) { return action_to_list(actions, "elimina la tabella '" + table + "'", action_code.act_remove_table, new Dictionary<string, string>() { { "table", table } }); }
    static public action_db init_history_table(List<action_db> actions, string table) { return action_to_list(actions, "inizializza la tabella di storico relativa alla '" + table + "'", action_code.act_init_history_table, new Dictionary<string, string>() { { "table", table } }); }
    static public action_db init_srvfields_history_table(List<action_db> actions, string table) { return action_to_list(actions, "aggiorna i campi di servizio della tabella di storico '" + table + "'", action_code.act_init_srvfields_history_table, new Dictionary<string, string>() { { "table", table } }); }
    static public action_db init_uidx(List<action_db> actions, string table, string from_ver) { return action_to_list(actions, "inizializza l'indice univoco della tabella '" + table + "'", action_code.act_init_uidx, new Dictionary<string, string>() { { "table", table }, { "from_ver", from_ver } }); }
    static public action_db init_uidx_onins(List<action_db> actions, string table, string from_ver) { return action_to_list(actions, "inizializza l'indice univoco della tabella '" + table + "' sul campo di inserimento", action_code.act_init_uidx_onins, new Dictionary<string, string>() { { "table", table }, { "from_ver", from_ver } }); }
    static public action_db init_del_onins(List<action_db> actions, string table, string from_ver) { return action_to_list(actions, "inizializza il campo della tabella storico '" + table + "' sul campo di inserimento", action_code.act_init_del_onins, new Dictionary<string, string>() { { "table", table }, { "from_ver", from_ver } }); }

    static public action_db action_to_list(List<action_db> actions, string tx, action_code code, Dictionary<string, string> pars) {
      if (actions == null) return null;

      action_db a = find_action(actions, code, pars);
      if (a == null)
        actions.Add(a = new action_db(max_id(actions) + 1, code, tx, pars));

      return a;
    }

    static public int max_id(List<action_db> actions) {
      int id = 0;
      foreach (action_db a in actions)
        if (a.id > id) id = a.id;

      return id;
    }

    static public action_db find_action(List<action_db> actions, action_code code, Dictionary<string, string> pars) {
      return actions.FirstOrDefault(a => a.action == code && equal(a.action_pars, pars));
    }

    static public bool equal(Dictionary<string, string> pars, Dictionary<string, string> pars2) {
      for (int i = 0; i < pars.Count; i++)
        if (pars.Keys.ElementAt(i) != pars2.Keys.ElementAt(i) ||
            pars.Values.ElementAt(i) != pars2.Values.ElementAt(i))
          return false;

      return true;
    }

    public XmlNode create_node(xmlDoc res) {
      XmlNode act = xmlDoc.set_attrs(res.doc.CreateElement("action"), new Dictionary<string, string>() { { "id", _id.ToString() }, { "code", _code.ToString() }
      , {"text", _text}});
      foreach (KeyValuePair<string, string> par in _action_pars)
        xmlDoc.add_xml(act, "<par name='" + par.Key + "'/>").InnerText = par.Value;
      return act;
    }
  }

  /// <summary>
  /// Elenco di segnalazioni legate ad un singolo elemento del db
  /// </summary>
  class ele_db
  {
    public enum ele_type { table, function, stored_procedure }

    ele_type _type; string _name; meta_doc.table_type _table_type = meta_doc.table_type.none;
    List<ele_info> _infos = new List<ele_info>();

    public string name { get { return _name; } }
    public ele_type type { get { return _type; } }
    public List<ele_info> infos { get { return _infos; } }
    public meta_doc.table_type table_type { get { return _table_type; } }

    public ele_db(string name, ele_type type) { _name = name; _type = type; }
    public ele_db(string name, ele_type type, meta_doc.table_type table_type)
      : this(name, type) { _table_type = table_type; }

    public void add_infos(List<ele_info> infos) { foreach (ele_info ele in infos) add_info(ele); }
    public ele_info add_info(ele_info ei) {
      ele_info fnd = _infos.FirstOrDefault(x => x.same_info(ei));
      return fnd != null ? fnd : add(ei);
    }
    protected ele_info add(ele_info ei) { _infos.Add(ei); return ei; }

    public XmlNode create_node(xmlDoc doc) {
      XmlNode result = null;
      XmlNode ndis = xmlDoc.add_node(xmlDoc.set_attrs(result = doc.doc.CreateElement("element"), new Dictionary<string, string>() { { "name", _name }
        , { "type", _type.ToString() }, { "table_type", _table_type.ToString() }}), "infos");
      foreach (ele_info i in _infos) xmlDoc.add_node(ndis, i.create_node(doc));
      return result;
    }

    public static void update_infos(List<ele_db> eles, string name, ele_info info, ele_db.ele_type type) {
      update_infos(eles, name, new List<ele_info>() { info }, type);
    }

    public static void update_infos(List<ele_db> eles, string name, List<ele_info> infos, ele_db.ele_type type, bool reset = false) {
      ele_db el = contains_ele(eles, type, name);
      if (reset && el != null) el.infos.Clear();
      if (infos.Count > 0) (el == null ? ele_db.add_ele(eles, new ele_db(name, type)) : el).add_infos(infos);
    }

    public static void update_info_tables(List<ele_db> eles, string table, ele_info info, meta_doc.table_type type) {
      update_info_tables(eles, table, new List<ele_info>() { info }, type);
    }

    public static void update_info_tables(List<ele_db> eles, string table, List<ele_info> infos, meta_doc.table_type type, bool reset = false) {
      ele_db el = contains_ele(eles, ele_type.table, table);
      if (reset && el != null) el.infos.Clear();
      if (infos != null && infos.Count > 0)
        (el == null ? ele_db.add_ele(eles, new ele_db(table, ele_db.ele_type.table, type))
: el).add_infos(infos);
    }

    public static ele_db contains_ele(List<ele_db> eles, ele_type tp, string name) { return eles.FirstOrDefault(x => x.name.ToLower() == name.ToLower() && x.type == tp); }

    public static ele_db check_add_table(List<ele_db> eles, string table, meta_doc.table_type type) {
      ele_db el = contains_ele(eles, ele_type.table, table);
      return (el != null) ? el : ele_db.add_ele(eles, new ele_db(table, ele_db.ele_type.table, type));
    }

    protected static ele_db add_ele(List<ele_db> eles, ele_db el) { eles.Add(el); return el; }
  }

  /// <summary>
  /// Segnalazione legata ad un elemento del db
  /// </summary>
  class ele_info
  {
    // enum
    public enum info_type { info, error, warning }

    public enum info_code
    {
      // info - data
      none, i_last_upd, i_diff_last_upd, i_last_upd_ref, i_count,
      // align
      i_can_align, err_cant_align, err_cant_align_del_rows,
      // error - schema
      err_no_table_db, err_no_field, err_no_table_history, err_no_service_field, err_history_no_service_field,
      err_history_service_field, err_no_unique_index, err_no_unique_index_onins, err_no_unique_index_rif,
      err_diff_field, err_diff_unique_index, err_idlist_unique_index, err_no_primary_index, err_diff_primary_index,
      err_primary_index, err_data_field, err_no_meta_field, err_no_pk_field,
      // error - data
      err_no_linked_values, err_no_data_table, err_history_schema, err_history_schema_ifields,
      // error - procs
      err_no_func_into_db, err_no_func_into_schema, err_diff_func_into_schema,
      err_no_sp_into_db, err_no_sp_into_schema, err_diff_sp_into_schema,
      // rif
      err_no_table_rif, err_no_table_schema_rif, err_no_field_rif, err_no_field_schema_rif, err_no_table_history_rif,
      err_history_service_field_rif, err_history_no_service_field_rif, err_history_schema_ifields_rif, err_history_schema_rif,
    }

    // proprietà
    List<int> _actions_id = new List<int>();
    info_code _code = info_code.none; info_type _type;
    string _text, _url; Dictionary<string, object> _values = null;

    public object value { get { return _values != null ? _values.Values.First() : null; } }
    public object value_at(int index) { return _values != null ? _values.Values.ElementAt(index) : null; }
    public int c_values { get { return _values != null ? _values.Count : 0; } }
    public List<int> actions_id { get { return _actions_id; } }
    void upgrade_ids(List<action_db> actions) {
      if (actions != null)
        foreach (action_db a in actions)
          if (!_actions_id.Contains(a.id)) _actions_id.Add(a.id);
    }
    public void clear_actions(List<action_db> actions) {
      foreach (int id in actions_id)
        actions.Remove(actions.Find(x => x.id == id));
      _actions_id.Clear();
    }
    public bool same_info(ele_info ele) {
      return _code == ele._code && _type == ele._type && _text == ele._text && _url == ele._url;
    }
    public XmlNode create_node(xmlDoc doc) {
      XmlNode res = xmlDoc.set_attrs(doc.doc.CreateElement("info"), new Dictionary<string, string>() { { "code", _code.ToString() }, { "actions_id", string.Join(",", _actions_id) }
         , { "type", _type.ToString() }, { "url", _url }, { "text", _text } });
      if (_values != null && _values.Count == 1) xmlDoc.set_attr(res, "value", _values.Values.First().ToString());
      else if (_values != null && _values.Count > 1) {
        foreach (KeyValuePair<string, object> v in _values) xmlDoc.set_attrs(xmlDoc.add_node(res, "val"), new Dictionary<string, string>() { { "name", v.Key }, { "val", v.Value.ToString() } });
      }
      return res;
    }

    public info_code code { get { return _code; } set { _code = value; } }
    public info_type type { get { return _type; } set { _type = value; } }
    public string text { get { return _text; } set { _text = value; } }
    public string url { get { return _url; } set { _url = value; } }

    // costruttori
    public ele_info(string tx) { _code = info_code.none; _type = info_type.info; _text = tx; _url = ""; }
    public ele_info(info_code code, info_type tp, string tx) : this(tx) { _code = code; _type = tp; }
    public ele_info(info_code code, info_type tp, string tx, List<action_db> actions) : this(code, tp, tx) { upgrade_ids(actions); }
    public ele_info(info_code code, info_type tp, string tx, action_db action) : this(code, tp, tx) { upgrade_ids(new List<action_db>() { action }); }
    public ele_info(info_code code, string tx) : this(code, info_type.info, tx) { }
    public ele_info(string tx, object value) : this(tx) { _values = new Dictionary<string, object>() { { "1", value } }; }
    public ele_info(info_code code, info_type tp, string tx, object value) : this(code, tp, tx) { _values = new Dictionary<string, object>() { { "1", value } }; }
    public ele_info(info_code code, info_type tp, string tx, object value, string url) : this(code, tp, tx, value) { _url = url; }
    public ele_info(info_code code, info_type tp, string tx, object value, action_db action) : this(code, tp, tx, value) { upgrade_ids(new List<action_db>() { action }); }
    public ele_info(info_code code, info_type tp, string tx, Dictionary<string, object> values, action_db action) : this(code, tp, tx) { _values = values; upgrade_ids(new List<action_db>() { action }); }
    public ele_info(info_code code, string tx, object value) : this(code, info_type.info, tx, value) { }
    public ele_info(info_code code, string tx, object value, string url) : this(code, info_type.info, tx, value, url) { }

    static public ele_info info_war(ele_info.info_code code, string text) { return new ele_info(code, ele_info.info_type.warning, text); }
    static public ele_info info_err(ele_info.info_code code, string text) { return new ele_info(code, ele_info.info_type.error, text); }
    static public ele_info info_err(ele_info.info_code code, string text, string value) { return new ele_info(code, ele_info.info_type.error, text, value); }
    static public ele_info info_err(ele_info.info_code code, string text, action_db action) { return new ele_info(code, ele_info.info_type.error, text, action != null ? new List<action_db>() { action } : null); }
    static public ele_info info_err(ele_info.info_code code, string text, List<action_db> actions) { return new ele_info(code, ele_info.info_type.error, text, actions); }
  }

  #endregion

  #region metodi interni

  protected enum type_check { check, data, sch }

  string par_action(XmlNode action, string par) { return action.SelectSingleNode("par[@name='" + par + "']").InnerText; }
  string lst_pars_action(XmlNode action) {
    return string.Join(", ", action.SelectNodes("par").Cast<XmlNode>().Select(x => string.Format("{0}: {1}"
      , x.Attributes["name"].Value, x.InnerText)));
  }

  // esecuzione azione sul db in seguito alla selezione della pagina web
  void exec_action(db_schema db, System.Xml.XmlNode action) {
    db_xml db_ref = null;
    try {
      logTxt(string.Format("esecuzione azione '{0}', parametri: {1}", xmlDoc.node_val(action, "code"), lst_pars_action(action)));
      if (xmlDoc.node_val(action, "code") == action_db.action_code.act_set_ver_db.ToString())
        db.setInfo("ver", par_action(action, "ver"), 0, par_action(action, "des"));
      else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_remove_table.ToString())
        db.drop_table(par_action(action, "table"));
      else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_init_uidx_onins.ToString()
          || xmlDoc.node_val(action, "code") == action_db.action_code.act_init_del_onins.ToString()) {

        // imposto il datetime2
        string tbl = par_action(action, "table");
        schema_field fld_ins = db.table_fields(tbl, db.meta_doc.field_ins())[0];
        if (fld_ins.TypeField != fieldType.DATETIME2 || fld_ins.Nullable) {
          fld_ins.setOriginalType(db.type, fieldType.DATETIME2);
          fld_ins.Nullable = false;
          db.exec(string.Format("ALTER TABLE {0} ALTER COLUMN {1} DATETIME2 NOT NULL", tbl, db.meta_doc.field_ins()));
          db.schema.set_field(tbl, fld_ins);
          db.schema.save();
        }

        // indice univoco
        string idx_name = "idx" + tbl + "_onins";
        if (xmlDoc.node_val(action, "code") == action_db.action_code.act_init_uidx_onins.ToString()
          && db.table_idxs(tbl, true, idx_name).Count == 0) {

          string pk = db.schema.pkOfTable(tbl);

          // sistemo le doppie      
          while (db.get_count(string.Format("select sum(conteggio) from (select distinct {1}, count(*) as conteggio from {0} group by {1}) tbl "
           + " where tbl.conteggio > 1", tbl, db.meta_doc.field_ins())) > 0) {
            int row_num = 0, mmm = 0, mmm_start = 0;
            foreach (System.Data.DataRow dr in db.dt_table(string.Format("select t1.{2}, convert(varchar, t1.{1}, 20) as {1}, tbl2.conteggio, tbl2.row_num from {0} t1 "
             + " join (select tbl.*, ROW_NUMBER() OVER(ORDER BY [{1}]) AS row_num from (select distinct convert(varchar, {1}, 20) as {1}, count(*) as conteggio from {0} "
             + " group by convert(varchar, {1}, 20)) tbl where tbl.conteggio > 1) tbl2 on convert(varchar, t1.{1}, 20) = convert(varchar, tbl2.{1}, 20) "
             + " order by t1.{1}, t1.{2}", tbl, db.meta_doc.field_ins(), pk)).Rows) {
              int n = int.Parse(dr["row_num"].ToString());
              if (n != row_num) { mmm_start = mmm = int.Parse(dr["conteggio"].ToString()); row_num = n; }
              DateTime new_dtins = DateTime.Parse(dr[db.meta_doc.field_ins()].ToString()).AddMilliseconds(mmm_start - mmm);
              db.exec(string.Format("update {1} set {3} = {2} where {4} = {0}", dr[pk].ToString(), tbl
                , db.val_toqry(db.field_value(new_dtins), fieldType.DATETIME2), db.meta_doc.field_ins(), pk));
              mmm--;
            }
            if (row_num == 0) break;
          }

          // imposto l'indice 
          idx_table new_idx = db.create_index(new idx_table(tbl, idx_name, false, true, false
              , new List<idx_field>() { new idx_field(db.meta_doc.field_ins(), true) }));
          if (db.schema.table_indexes(tbl, true).FindIndex(x => x.Name.ToLower() == idx_name.ToLower()) < 0) {
            db.schema.add_idx(tbl, new_idx);
            db.schema.save();
          }
        }
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_add_table.ToString()) {
        db_ref = db_ref == null ? conn_schema(schema_path_fromconn(query_param("dbname")
          , true, false, false, par_action(action, "from_ver"))) : db_ref;
        db.create_table(db_ref.schema.table_node(par_action(action, "table")));
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_add_fnc.ToString()) {
        db_ref = db_ref == null ? conn_schema(schema_path_fromconn(query_param("dbname")
          , true, false, false, par_action(action, "from_ver"))) : db_ref;
        db.exec(db_ref.schema.function_content(par_action(action, "function")));
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_upd_fnc.ToString()) {
        db_ref = db_ref == null ? conn_schema(schema_path_fromconn(query_param("dbname")
          , true, false, false, par_action(action, "from_ver"))) : db_ref;
        db.exec(db_ref.schema.function_content(par_action(action, "function"))
          .Replace("create function ", "alter function ").Replace("CREATE FUNCTION ", "ALTER FUNCTION "));
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_upd_field.ToString()) {

        // prima tolgo gli eventuali indici collegati
        List<idx_table> idxs = new List<idx_table>(db.table_idxs(par_action(action, "table"))
          .Where(i => i.existField(par_action(action, "field")) != null));
        idxs.ForEach(i => db.drop_index(i.Name, par_action(action, "table")));

        // aggiorno il campo
        db.upd_field(db.schema.table_field(par_action(action, "table"), par_action(action, "field"))
          , par_action(action, "table"));

        // e li ricreo
        idxs.ForEach(i => db.create_index(i));
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_del_field.ToString()) {
        try {
          db.begin_trans();

          db.del_field(par_action(action, "field"), par_action(action, "table"));

          db.commit();
        } catch { db.rollback(); }

      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_add_field.ToString()
        || xmlDoc.node_val(action, "code") == action_db.action_code.act_add_field_null.ToString()) {
        db_ref = db_ref == null ? conn_schema(schema_path_fromconn(query_param("dbname")
          , true, false, false, par_action(action, "from_ver"))) : db_ref;

        try {
          db.begin_trans();

          // tabella di parcheggio
          string tbl = par_action(action, "table"), fld = par_action(action, "field"), tmp_tbl = tbl + "_tmp_add";
          db.exec(string.Format("SELECT * INTO {0} FROM {1}", tmp_tbl, tbl));

          // creo la nuova tabella
          db.drop_table(tbl);
          db.create_table(db_ref.schema.table_node(tbl), true, ""
            , xmlDoc.node_val(action, "code") == action_db.action_code.act_add_field_null.ToString() ? new List<string>() { fld } : null);

          // copio i dati
          if (db.schema.table_autonumber(tbl)) db.set_identity(tbl, true);
          db.exec(string.Format("INSERT INTO {0} ({1}) SELECT {1} FROM {2}"
            , tbl, string.Join(", ", db.table_fields(tmp_tbl).Select(x => x.Name)), tmp_tbl));
          if (db.schema.table_autonumber(tbl)) db.set_identity(tbl, false);

          // elimino la tabella d'appoggio
          db.drop_table(tmp_tbl);

          db.commit();
        } catch(Exception ex) { db.rollback(); }
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_init_history_table.ToString()) {
        db.schema.create_history_table(par_action(action, "table"), db.meta_doc.prefix_del() + par_action(action, "table")
          , db.meta_doc.field_del(), db.meta_doc.field_ref());
        db.create_table(db.schema.table_node(db.meta_doc.prefix_del() + par_action(action, "table")));
        db.schema.save();
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_init_srvfields_history_table.ToString()) {

        if (db.meta_doc.field_del() != null && !db.exist_field(par_action(action, "table"), db.meta_doc.field_del()))
          db.add_field(db.schema.create_schema_field(par_action(action, "table"), db.meta_doc.field_del(), fieldType.DATETIME),
            par_action(action, "table"));

        if (db.meta_doc.field_ref() != null && !db.exist_field(par_action(action, "table"), db.meta_doc.field_ref()))
          db.add_field(db.schema.create_schema_field(par_action(action, "table"), db.meta_doc.field_ref(), fieldType.DATETIME),
            par_action(action, "table"));

        db.schema.save();
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_init_list_tags.ToString()) {
        db.init_list_tags(par_action(action, "table"), par_action(action, "field"), par_action(action, "list_table")
            , par_action(action, "values"));
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_remove_ids.ToString()) {
        db.exec(string.Format("delete from {0} where {1} in ({2})", par_action(action, "table"), par_action(action, "field")
          , par_action(action, "values")));
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_init_uidx.ToString()) {
        db_ref = db_ref == null ? conn_schema(schema_path_fromconn(query_param("dbname")
          , true, false, false, par_action(action, "from_ver"))) : db_ref;
        idx_table idxs = db.tableUniqueIndex(par_action(action, "table"));
        if (idxs != null) db.drop_index(idxs.Name, par_action(action, "table"));
        idx_table idxs_ref = db_ref.tableUniqueIndex(par_action(action, "table"));
        if (idxs_ref != null) db.create_index(idxs_ref);
      } else if (xmlDoc.node_val(action, "code") == action_db.action_code.act_align_data.ToString()) {
        xmlalign_to_table(par_action(action, "table"), db, par_action(action, "db_xml"), par_action(action, "ids_file"));
      } else throw new Exception("l'azione '" + xmlDoc.node_val(action, "code") + "' non è supportata");
    } catch (Exception ex) {
      logErr(string.Format("l'azione '{0}', parametri: {1} è andata in errore controllare il log."
        , xmlDoc.node_val(action, "code"), lst_pars_action(action)));
      throw ex;
    } finally { if (db_ref != null) db_ref.close_conn(); }
  }

  public void xmlalign_to_table(string table, db_schema db, string path_xml_db, string path_xml_rows) {
    db_xml xml_db = null;
    try {
      xml_db = conn_schema(path_xml_db);
      xml_data rows = new xml_data(path_xml_rows);

      // carico le tabelle di tipo 'list'
      List<xml_data> lst_tbls = new List<xml_data>();
      schema_doc sk = File.Exists(schemas_path(Path.GetDirectoryName(path_xml_rows))) ? new schema_doc(schemas_path(Path.GetDirectoryName(path_xml_rows))) : null;
      foreach (string lst_tbl in rows.find_defcols("level=1,pklist=1", "tableref").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        if (lst_tbls.FirstOrDefault(x => x.table.ToLower() == lst_tbl.ToLower()) == null)
          lst_tbls.Add(new xml_data(Path.Combine(list_path(Path.GetDirectoryName(path_xml_rows), lst_tbl)), lst_tbl, sk));

      while (rows.read_row()) {

        // ricerca riga da inserire
        if (!xml_db.find_row_byid(table, xml_db.schema.pkOfTable(table), rows.val_pkid()))
          throw new Exception(string.Format("non è stata trovata la riga con id. per la tabella '{0}'", table));

        // valori da inserire
        Dictionary<string, string> values = db.schema.fields_name(table, true).ToDictionary(x => x.ToUpper(), y => xml_db.get_xmldata(table).val(y));

        // sistemo gli id diretti
        string sql_ids = "";
        foreach (meta_link l in db.meta_doc.links_table(table)) {

          meta_table.align_codes rec = db.meta_doc.table_align_code(l.table);
          string id_val = rows.val(rows.find_defcol(string.Format("level={0},tableref={1},col_name={2}"
            , 2, l.table, l.field), "name"));
          //, l.type != "list" && rec != meta_table.align_codes.folders ? 2 : 1, l.table, l.field), "name"));
          if (string.IsNullOrEmpty(id_val)) id_val = null;
          if (l.type != meta_link.types_link.list) {
            if (rec == meta_table.align_codes.folders) sql_ids += (sql_ids != "" ? "\n union " : "") + string.Format("select isnull(t.{0}, -1) as ID, tbl.* from "
                + "(select '{1}' as FIELD, isnull({2}, -1) AS ID_XML, '{3}' as TIPO) tbl {4}", db.schema.pkOfTable(l.table), l.field
                , db.val_toqry(id_val, fieldType.LONG), l.type, join_qry_ids(l, db, rows));
            else sql_ids += (sql_ids != "" ? "\n union " : "") + string.Format("select isnull(t.{0}, -1) as ID, tbl.* from "
                + "(select '{1}' as FIELD, isnull({2}, -1) AS ID_XML, '{3}' as TIPO) tbl {4}", db.schema.pkOfTable(l.table), l.field
                , db.val_toqry(id_val, fieldType.LONG), l.type, join_qry_ids(l, db, rows));
          } else {
            xml_data lst_rows = lst_tbls.FirstOrDefault(x => x.table.ToLower() == l.table.ToLower());
            string pk_fld = db.schema.pkOfTable(l.table);
            foreach (string id in id_val.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)) {
              if (!lst_rows.find_row_byid(pk_fld, id))
                throw new Exception(string.Format("non è stato trovato il record corrispondente all'id {0} per la tabella {1}", id, l.table));

              sql_ids += (sql_ids != "" ? "\n union " : "") + string.Format("select isnull(t.{0}, -1) as ID, tbl.* from "
                + "(select '{1}' as FIELD, isnull({2}, -1) AS ID_XML, '{3}' as TIPO) tbl {4}"
                , pk_fld, l.field, id, l.type, join_qry_ids(l, db, lst_rows));
            }
          }
        }

        foreach (System.Data.DataRow rid in db.dt_table("select tbl2.* from("
          + (sql_ids != "" ? sql_ids : "select 1 as id, 1 as id_xml") + ") tbl2 where tbl2.id <> tbl2.id_xml").Rows) {

          int id = int.Parse(rid["id"].ToString()), id_xml = int.Parse(rid["id_xml"].ToString());
          if (id < 0)
            throw new Exception(string.Format("il record con il valore {0} relativo al campo {1} del database da importare per la tabella {2}"
              + " non è stato trovato nel database da aggiornare", id_xml, rid["field"].ToString(), table));

          values[rid["field"].ToString().ToUpper()] = (rid["TIPO"] != null && rid["TIPO"] != DBNull.Value && rid["TIPO"].ToString() == "list")
            ? values[rid["field"].ToString().ToUpper()].Replace("[" + id_xml.ToString() + "]", "[" + id.ToString() + "]") : id.ToString();
        }

        // inserisco la riga
        db.begin_trans();

        string sql = "INSERT INTO " + table + "(" + db.schema.qry_fields(table, true) + ") VALUES ("
          + string.Join(",", db.schema.fields_name(table, true)
          .Select(x => db.val_toqry(values[x.ToUpper()], db.field_table(table, x).TypeField, db_schema.null_value))) + ")";
        long idrow = db.exec(sql, true);

        // creo la cartella
        meta_table.align_codes acode = db.meta_doc.table_align_code(table);
        if (acode == meta_table.align_codes.folders) {
          string fld_path = deeper.pages.db_utilities.folder_path(db, this, idrow);
          if (!Directory.Exists(fld_path)) Directory.CreateDirectory(fld_path);
        }
        // copio il file
        else if (acode == meta_table.align_codes.files) {
          System.IO.File.Copy(System.IO.Path.Combine(deeper.pages.db_utilities.folder_path(db, this, long.Parse(values["IDFOLDER"])
            , System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path_xml_db), db_schema._filesFolder)), values["FILENAME"] + "." + values["EXT"])
             , System.IO.Path.Combine(deeper.pages.db_utilities.folder_path(db, this, long.Parse(values["IDFOLDER"])), values["FILENAME"] + "." + values["EXT"]));
        }

        db.commit();
      }
    } catch (Exception ex) { if (db.is_trans()) db.rollback(); logErr(ex); throw ex; } finally { if (xml_db != null) xml_db.close_conn(); }
  }

  string join_qry_ids(meta_link l, db_schema db, xml_data rows, int level = 2, int n_alias = 1) {

    meta_table mt = db.meta_doc.meta_tbl(l.table);

    string al = "t" + (n_alias == 1 ? "" : n_alias.ToString());
    string res = string.Format("left join {0} {1} on ", l.table, al);

    string conds = "";
    if (db.meta_doc.table_align_code(l.table) == meta_table.align_codes.folders) {
      conds += (conds != "" ? " and " : "") + string.Format("dbo.getPathOfFolder({0}.{1}) {2}", al, db.schema.pkOfTable(l.table)
        , db.val_toqry(rows.val(rows.find_defcol(string.Format("align_code=folders,level={0},tableref={1},id_fld={2}", level, l.table, l.field.ToUpper()), "name"))
          , fieldType.VARCHAR, null, true));
    } else {
      foreach (idx_field fld in db.meta_doc.indexUnique(l.table).Fields) {
        meta_link ml = mt.links.FirstOrDefault(x => x.field.ToUpper() == fld.Name.ToUpper());
        if (ml == null)
          conds += (conds != "" ? " and " : "") + string.Format("{0}.{1} {2}", al, fld.Name
            , db.val_toqry(rows.val(rows.find_defcol(string.Format("level={0},tableref={1},id_fld={2},col_name={3}", level, l.table, l.field.ToUpper(), fld.Name), "name"))
              , db.schema.table_field(l.table, fld.Name).TypeField, null, true));
        else {
          conds += (conds != "" ? " and " : "") + string.Format("{0}.{1} = {2}.{3}"
            , al, fld.Name, string.Format("t{0}", n_alias + 1), db.schema.pkOfTable(ml.table));

          res = join_qry_ids(ml, db, rows, level + 1, n_alias += 1) + res;
        }
      }
    }

    return res + conds + (level > 2 ? " " : "");
  }

  // visualizzazione analisi elemento database
  string html_ele(ele_db ele, List<action_db> actions) {
    List<ele_info> infos = ele.infos;
    return ("<div class='rcvry-sec'><span class='rcvry-sec-title'>" + ele.name + "</span><div>")
        + html_section(infos, "note", ele_info.info_type.info) + html_section(infos, "errori", ele_info.info_type.error, System.Drawing.Color.Tomato)
        + html_section(infos, "avvisi", ele_info.info_type.warning) + html_actions(infos, actions) + "</div></div>";
  }

  // visualizzazione sezione di un singolo elemento
  string html_section(List<ele_info> infos, string title, ele_info.info_type tp, System.Drawing.Color? clr = null) {

    string style = clr != null ? " style='color:" + clr.Value.Name.ToLower() + ";'" : "";
    string style_act = clr != null ? " style='color:" + clr.Value.Name.ToLower() + ";font-weight:bold;'" : " style='font-weight:bold;'";

    int n = 0;
    string html = "";
    foreach (ele_info i in infos.Where(x => x.type == tp))
      html += (html == "" ? "<div><span class='rcvry-sec-subtitle'>" + title + ": </span>" : "")
        + (i.url == "" ? "<span class='rcvry-sec-info'" + (i.actions_id.Count == 0 ? style : style_act) + ">" + inc_infos(n, out n) + i.text + "</span>"
          : "<a class='rcvry-sec-info' " + (i.actions_id.Count == 0 ? style : style_act) + " href=\"" + i.url + "\">" + inc_infos(n, out n) + i.text + "</a>");

    return html + (html != "" ? "</div>" : "");
  }

  // visualizzazione azioni legate ad un elemento database
  string html_actions(List<ele_info> infos, List<action_db> actions) {
    string html = string.Concat(infos.SelectMany(i => i.actions_id.Select(x => actions.Find(y => y.id == x))).Select(a => html_action(a)));
    return html != "" ? string.Format("<div><span class='rcvry-sec-subtitle'>azioni: </span>{0}</div>", html) : "";
  }

  string html_action(action_db a) { return "<input type='checkbox' class='rcvry-sec-info' id_action='" + a.id + "' unchecked\">" + a.text + "</a>"; }

  // documento lato client usato per decidere cosa fare sul db
  xmlDoc doc_actions(List<ele_db> eles, List<action_db> actions) {
    xmlDoc res = xmlDoc.doc_from_xml("<root><elements/><actions/></root>");
    eles.ForEach(e => { res.add_node("/root/elements", e.create_node(res)); });
    actions.ForEach(a => { res.add_node("/root/actions", a.create_node(res)); });
    return res;
  }

  string doc_actions_i_value(xmlDoc doc, string el_name, ele_info.info_code i_code, string val_name) {
    return doc.get_value_throw(string.Format("/root/elements/element[@name='{0}']/infos/info[@type='info'][@code='{1}']"
           + "/val[@name='{2}']", el_name, i_code.ToString(), val_name), "val"
           , string.Format("informazione inattesa elemento {0}, codice {1}, valore {2}", el_name, i_code.ToString(), val_name));
  }

  string inc_infos(int n, out int n2) {
    n2 = n + 1;
    //return (n2 > 1 ? "<span>,&nbsp;</span>" : "");
    return (n2 > 1 ? "<span> - " + n2.ToString() + ") </span>" : "");
  }

  // visualizzazione risultati analisi elementi database
  void set_html(db_schema db, db_schema db_ref, type_check type, List<ele_db> eles, List<action_db> actions) {
    try {
      string html = "";

      // infos
      DateTime? last_upd = null;
      IEnumerable<ele_db> lst = eles.Where(x => x.type == ele_db.ele_type.table).OrderBy(x => x.name);
      foreach (ele_db tis in lst) {
        ele_info upd = tis.infos.FirstOrDefault(x => x.code == ele_info.info_code.i_last_upd);
        if (upd != null && (!last_upd.HasValue || last_upd.Value < DateTime.Parse(upd.value.ToString())))
          last_upd = DateTime.Parse(upd.value.ToString());
      }

      if (eles.Where(x => x.infos.Where(y => y.type == ele_info.info_type.error).Count() > 0).Count() == 0)
        html += "<span class='rcvry-sec-title'>" + (type == type_check.check ? "non sono stati riscontrati errori rilevanti nel database"
          : "non sono stati riscontrate differenze rilevanti nel database") + "</span><br>" + (db_ref != null && db.ver_long < db_ref.ver_long ? html_action(action_db.add_set_ver_db(actions, db_ref.ver, "upgrade schema")) + "<br><br>" : "");

      html += db.ver == "" ? "<span class='rcvry-sec-err'>non è specificata la versione del db!</span><br>"
        : "<span class='rcvry-sec-info'>versione db: '" + db.ver + "'</span><br>";

      if (db_ref != null) {
        if (db_ref.ver == "") html += "<span class='rcvry-sec-err'>non è specificata la versione del db di riferimento!</span><br>";
        else
          html += "<span class='rcvry-sec-info'>" + (type == type_check.data ? "versione db dalla quale importare i dati: "
            : "versione db di riferimento: ") + "'" + db_ref.ver + "'</span><br>";
      }

      if (last_upd.HasValue) html += "<span class='rcvry-sec-info'>ultima modifica effettuata: " + last_upd.Value.ToString() + "</span><br>";

      html += "##ACTIONS##";

      // elements
      html += html_eles("Tabelle Dati", eles.Where(x => x.type == ele_db.ele_type.table && x.table_type == meta_doc.table_type.data)
          .OrderBy(x => x.name), actions);

      if (type != type_check.data) {
        html += html_eles("Tabelle Storico", eles.Where(x => x.type == ele_db.ele_type.table && x.table_type == meta_doc.table_type.storico)
            .OrderBy(x => x.name), actions);

        html += html_eles("Altre Tabelle", eles.Where(x => x.type == ele_db.ele_type.table
            && (x.table_type != meta_doc.table_type.storico && x.table_type != meta_doc.table_type.data)).OrderBy(x => x.name), actions);

        html += html_eles("Funzioni", eles.Where(x => x.type == ele_db.ele_type.function).OrderBy(x => x.name), actions);

        html += html_eles("Stored Procedures", eles.Where(x => x.type == ele_db.ele_type.stored_procedure).OrderBy(x => x.name), actions);
      } else {
        foreach (ele_db e in eles.Where(x => x.type == ele_db.ele_type.table && x.table_type == meta_doc.table_type.storico))
          foreach (ele_info i in e.infos) i.clear_actions(actions);
      }

      // esecuzione azioni
      html = html.Replace("##ACTIONS##", actions.Count > 0 ? "<a href='javascript:do_actions(true)'>esegui tutte le azioni (tot. " + actions.Count.ToString() + ")</a>"
        + " - <a href='javascript:do_actions()'>esegui solo le azioni selezionate</a>" : "");

      // documento azioni
      xml_actions.InnerText = doc_actions(eles, actions).doc.InnerXml;

      ctrlsToParent(contents, literal(html));
    } catch (Exception ex) { logErr(ex); throw ex; }
  }

  // visualizzazione gruppo di elementi database
  string html_eles(string title, IEnumerable<ele_db> lst, List<action_db> actions) {
    return lst.Count() > 0 ? lst.Select(x => (lst.First() == x ? "<h2>" + title + "</h2>" : "") + html_ele(x, actions))
        .Aggregate((a, b) => a + b) : "";
  }

  IEnumerable<string> filter_objs(List<string> objs, string filter_objs) {
    return objs.Where(x => !string.IsNullOrEmpty(filter_objs)
      ? filter_objs.ToLower().Split(',').Contains(x.ToLower()) : true);
  }

  IEnumerable<string> filter_objs(string[] objs, string filter_objs) {
    return objs.Where(x => !string.IsNullOrEmpty(filter_objs)
      ? filter_objs.ToLower().Split(',').Contains(x.ToLower()) : true);
  }

  void init_list() {
    try {
      db_schema db = query_param("dbname") != "" ? conn_db(query_param("dbname")) : conn_db_user();
      if (db == null) throw new Exception("non è stato possibile aprire la connessione al db");
      //if (!db.exist_schema) throw new Exception("non è caricato lo schema xml relativo al database!");
      //if (!db.exist_meta) throw new Exception("non è caricato il meta xml relativo al database!");

      string html = "<h4>db connesso: " + db.name + " " + (db.des != "" ? " - " + db.des : "") + "</h4>"
        + "<span class='rcvry-sec-subtitle'>group:</span><span class='rcvry-sec-info'>" + db.group + "</span>, "
        + "<span class='rcvry-sec-subtitle'>type:</span><span class='rcvry-sec-info'>" + db.type.ToString() + "</span>, "
        + "<span class='rcvry-sec-subtitle'>version:</span><span class='rcvry-sec-info'>" + db.ver + "</span><hr>";

      html += "<h4>elenco delle tabelle</h4><br>";
      db.tables().ForEach(tbl => {
        html += string.Format("<a href='{0}'>{1}</a><br><br>", table_url(tbl, db.name), tbl);
        meta_table mt = db.meta_doc.meta_tbl(tbl);
        html += "<span class='rcvry-sec-subtitle'>entità:</span><span class='rcvry-sec-info'>" + (db.exist_meta ? db.meta_doc.table_title(tbl) : "") + "</span>, "
          + "<span class='rcvry-sec-subtitle'>tipo tabella:</span><span class='rcvry-sec-info'>" + mt.type.ToString() + "</span>, "
          + "<span class='rcvry-sec-subtitle'>records:</span><span class='rcvry-sec-info'>" + conn_db(db.name).get_count(string.Format("select count(*) from {0}", tbl)).ToString() + "</span>"
          + "<br><br>";
      });

      ctrlsToParent(contents, literal(html));
    } catch (Exception ex) { logErr(ex); throw ex; }
  }

  // situazione integrità schema e dati delle tabelle del database
  void init_check(type_check type) {
    db_schema db = conn_db(query_param("dbname"));
    if (!db.exist_info) throw new Exception("il database non ha le infos, è necessario inizializzarlo!");
    if (!db.exist_schema) throw new Exception("non è caricato lo schema xml relativo al database!");
    if (!db.exist_meta) throw new Exception("non è caricato il meta xml relativo al database!");

    // tabelle 
    List<ele_db> eles = new List<ele_db>();
    List<action_db> actions = new List<action_db>();
    foreach (string table in filter_objs(db.tables(), db.meta_doc.test_filter()))
      ele_db.update_info_tables(eles, table, check_schema_table(db, table, type, actions), db.meta_doc.type_table(table));
    foreach (string table in filter_objs(db.schema.tables_name(), db.meta_doc.test_filter()))
      if (!db.exist_table(table))
        ele_db.update_info_tables(eles, table
          , ele_info.info_err(ele_info.info_code.err_no_table_db, "la tabella '" + table + "' specificata nello schema xml di riferimento, non esiste nel database"
          , action_db.add_add_table(actions, table, db.ver)), db.meta_doc.type_table(table));

    // funzioni
    foreach (string func in filter_objs(db.functions(), db.meta_doc.test_filter()))
      ele_db.update_infos(eles, func, check_function(db, db.schema, func, actions, type), ele_db.ele_type.function);
    foreach (string func in filter_objs(db.schema.functions_name(), db.meta_doc.test_filter()))
      if (!db.exist_function(func))
        ele_db.update_infos(eles, func
          , ele_info.info_err(ele_info.info_code.err_no_func_into_db, "la funzione '" + func + "' specificata nello schema xml di riferimento, non esiste nel database"
          , action_db.add_add_fnc(actions, func, db.ver)), ele_db.ele_type.function);

    // sp
    foreach (string sp in filter_objs(db.store_procedures(), db.meta_doc.test_filter()))
      ele_db.update_infos(eles, sp, check_sp(db, db.schema, sp, actions, type), ele_db.ele_type.stored_procedure);
    foreach (string sp in filter_objs(db.schema.sps_name(), db.meta_doc.test_filter()))
      if (!db.exist_procedure(sp))
        ele_db.update_infos(eles, sp
          , ele_info.info_err(ele_info.info_code.err_no_sp_into_db, "la store procedure '" + sp + "' specificata nello schema xml di riferimento, non esiste nel database"), ele_db.ele_type.stored_procedure);

    set_html(db, null, type, eles, actions);
  }

  // check integrità della funzione del db rispetto allo schema ed ai dati
  List<ele_info> check_function(db_schema db, schema_doc schema_rif, string function, List<action_db> actions, type_check type) {
    List<ele_info> result = new List<ele_info>();

    // vedo se la funzione corrisponde con lo schema
    if (!schema_rif.existFunction(function))
      result.Add(ele_info.info_err(ele_info.info_code.err_no_func_into_schema, "la funzione '" + function + "' non è specificata nello schema xml di riferimento"));
    // check contenuti
    else if (!same_modules(schema_rif.function_content(function), db.module_text(function)))
      result.Add(ele_info.info_err(ele_info.info_code.err_diff_func_into_schema, "i contenuti della funzione '" + function + "' sono diversi da quelli dello schema xml di riferimento"
        , action_db.add_upd_fnc(actions, function, db.ver)));

    return result;
  }

  // check integrità della s.p. del db rispetto allo schema ed ai dati
  List<ele_info> check_sp(db_schema db, schema_doc schema_rif, string sp, List<action_db> actions, type_check type) {
    List<ele_info> result = new List<ele_info>();

    // vedo se la sp corrisponde con lo schema
    if (!schema_rif.existSP(sp))
      result.Add(ele_info.info_err(ele_info.info_code.err_no_sp_into_schema, "la s.p. '" + sp + "' non è specificata nello schema xml di riferimento"));
    // check contenuti
    else if (!same_modules(schema_rif.sp_content(sp), db.module_text(sp)))
      result.Add(ele_info.info_err(ele_info.info_code.err_diff_sp_into_schema, "la s.p. '" + sp + "' è diversa da quella dello schema xml di riferimento"));

    return result;
  }

  bool same_modules(string first, string second) { return clean_module_text(first).CompareTo(clean_module_text(second)) == 0; }
  string clean_module_text(string text) { return text.Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty); }

  // check integrità della tabella del db rispetto allo schema ed ai dati
  List<ele_info> check_schema_table(db_schema db, string table, type_check type, List<action_db> actions, db_xml db_ref = null) {
    List<ele_info> result = new List<ele_info>();

    // table type
    string table_ref = "";
    meta_doc.table_type tp = db.meta_doc.type_table(table, out table_ref);

    // schema
    if (db_ref != null && type != type_check.check) {
      // struttura
      if (!db_ref.exist_table(table))
        result.Add(ele_info.info_err(ele_info.info_code.err_no_table_rif, "la tabella '" + table + "' non è specificata nello schema xml di riferimento"
            , action_db.add_remove_table(actions, table)));
      else {
        foreach (schema_field field in db.table_fields(table).Where(x => db.schema.existCol(table, x.Name))) {
          schema_field fld2 = db.schema.table_field(table, field.Name);
          if (field.Nullable != fld2.Nullable || field.MaxLength != fld2.MaxLength || field.TypeField != fld2.TypeField)
            result.Add(ele_info.info_err(ele_info.info_code.err_diff_field, "il campo '" + field.Name + "' è diverso dalla tabella di riferimento"
              , action_db.add_upd_field(actions, table, field.Name)));
        }

        foreach (schema_field field in db.table_fields(table).Where(x => !db_ref.exist_field(table, x.Name)))
          result.Add(ele_info.info_err(ele_info.info_code.err_no_field_rif, "il campo '" + field.Name + "' non è specificato nello schema xml di riferimento"
            , action_db.add_del_field(actions, table, field.Name)));

        foreach (schema_field field in db_ref.table_fields(table).Where(x => !db.exist_field(table, x.Name)))
          result.Add(ele_info.info_err(ele_info.info_code.err_no_field, "il campo '" + field.Name + "' presente nello schema xml di riferimento non è presente nel database"
            , !field.Nullable ? new List<action_db>() { action_db.add_add_field(actions, table, field.Name, db_ref.ver), action_db.add_add_field_null(actions, table, field.Name, db_ref.ver) }
             : new List<action_db>() { action_db.add_add_field(actions, table, field.Name, db_ref.ver) }));
      }
    } else if (db_ref == null && type == type_check.check) {
      // struttura
      if (!db.schema.existTable(table))
        result.Add(ele_info.info_err(ele_info.info_code.err_no_table_schema_rif, "la tabella '" + table + "' non è specificata nello schema xml"
            , action_db.add_remove_table(actions, table)));
      else {
        foreach (schema_field field in db.table_fields(table).Where(x => db.schema.existCol(table, x.Name))) {
          schema_field fld2 = db.schema.table_field(table, field.Name);
          if (field.Nullable != fld2.Nullable || field.MaxLength != fld2.MaxLength || field.TypeField != fld2.TypeField)
            result.Add(ele_info.info_err(ele_info.info_code.err_diff_field, "il campo '" + field.Name + "' è diverso dalla tabella di riferimento"
              , action_db.add_upd_field(actions, table, field.Name)));
        }

        foreach (schema_field field in db.table_fields(table).Where(x => !db.schema.existCol(table, x.Name)))
          result.Add(ele_info.info_err(ele_info.info_code.err_no_field_schema_rif, "il campo '" + field.Name + "' non è specificato nello schema xml"
            , action_db.add_del_field(actions, table, field.Name)));

        foreach (schema_field field in db.schema.table_fields(table).Where(x => !db.exist_field(table, x.Name)))
          result.Add(ele_info.info_err(ele_info.info_code.err_no_field, "il campo '" + field.Name + "' presente nello schema xml non è presente nel database"
            , !field.Nullable ? new List<action_db>() { action_db.add_add_field(actions, table, field.Name, db.ver), action_db.add_add_field_null(actions, table, field.Name, db.ver) }
             : new List<action_db>() { action_db.add_add_field(actions, table, field.Name, db.ver) }));
      }
    } else throw new Exception("caso non gestito di check table!");

    // vedo se la tabella corrisponde agli standard
    if (tp == meta_doc.table_type.data || tp == meta_doc.table_type.sistema) {

      // ha la tabella di delete? e la tabella delete è corretta!?!?
      if ((type == type_check.check || type == type_check.data) && tp == meta_doc.table_type.data) {
        if (type == type_check.data) {
          if (db_ref.meta_doc.prefix_del() != null && !db_ref.exist_table(db_ref.meta_doc.prefix_del() + table))
            result.Add(ele_info.info_err(ele_info.info_code.err_no_table_history_rif, "la tabella '" + table + "' non ha la relativa tabella storico"
              + (db_ref.meta_doc.prefix_del() != null ? " '" + (db_ref.meta_doc.prefix_del() + table) + "' sul db di riferimento" : ""), action_db.init_history_table(actions, table)));
          else if (db_ref.meta_doc.prefix_del() != null) result.AddRange(check_del_table(db_ref, actions, db_ref.meta_doc.prefix_del() + table, table, true));
        }
        if (db.meta_doc.prefix_del() != null && !db.exist_table(db.meta_doc.prefix_del() + table))
          result.Add(ele_info.info_err(ele_info.info_code.err_no_table_history, "la tabella '" + table + "' non ha la relativa tabella storico"
            + (db.meta_doc.prefix_del() != null ? " '" + (db.meta_doc.prefix_del() + table) + "'" : ""), action_db.init_history_table(actions, table)));
        else if (db.meta_doc.prefix_del() != null) result.AddRange(check_del_table(db, actions, db.meta_doc.prefix_del() + table, table, false));
      }

      // differenza - conteggio records
      long count = 0, count_ref = 0;
      if (type == type_check.data) {
        count = db.get_count("select count(*) as conteggio from [" + table + "]");
        ((db_xml)db_ref).begin_table(table);
        while (((db_xml)db_ref).read_row(table))
          count_ref++;
        result.Add(new ele_info(ele_info.info_code.i_count, count != count_ref ? ele_info.info_type.warning : ele_info.info_type.info
          , count != count_ref ? ((count_ref == 1 ? "cè una riga nella tabella da importare"
          : count_ref > 0 ? "ci sono " + count_ref.ToString() + " righe nella tabella da importare" : "la tabella da importare è vuota")
          + " mentre " + (count == 1 ? "cè una riga nella tabella da aggiornare" : count > 0 ? "ci sono " + count.ToString() + " righe nella tabella da aggiornare"
          : "la tabella da aggiornare è vuota")) : (count_ref == 1 ? "cè una riga in entrambe le tabelle" : count_ref > 0 ? "ci sono " + count_ref.ToString()
           + " righe in entrambe le tabelle" : "le tabelle sono vuote"), count_ref));
      } else {
        count = db.get_count("select count(*) as conteggio from [" + table + "]");
        result.Add(new ele_info(ele_info.info_code.i_count, count == 1 ? "cè una riga in tabella"
          : count > 0 ? "ci sono " + count.ToString() + " righe in tabella" : "la tabella è vuota", count, table_url(table)));
      }

      // check colonne ins, upd - ultima modifica
      if ((type == type_check.check || type == type_check.data) && tp == meta_doc.table_type.data) {
        if ((db.meta_doc.field_ins() != null && !db.exist_field(table, db.meta_doc.field_ins()))
         || (db.meta_doc.field_upd() != null && !db.exist_field(table, db.meta_doc.field_upd())))
          result.Add(ele_info.info_war(ele_info.info_code.err_no_service_field, "la tabella '" + table + "' non ha una delle colonne di servizio"));
        else {
          // indice univoco sulla dtins
          if (db.meta_doc.uidx_onins() && (db.meta_doc.field_ins() != null && (db.meta_doc.indexUniqueOnIns(table) == null
            || db.schema.table_field(table, db.meta_doc.field_ins()).TypeField != fieldType.DATETIME2
            || db.table_idxs(table, true).FirstOrDefault(idx => idx.Fields.Count == 1 &&
              idx.Fields[0].Name.ToLower() == db.meta_doc.field_ins().ToLower()) == null)))
            result.Add(ele_info.info_err(ele_info.info_code.err_no_unique_index_onins, "la tabella '" + table + "' non ha l'indice univoco sulla colonna di inserimento"
              , action_db.init_uidx_onins(actions, table, db.ver)));

          // date ultima modifica
          DateTime? dt = db.meta_doc.field_ins() != null && db.meta_doc.field_upd() != null && count > 0 ?
            db.get_date("select max(dt) from (select max(" + db.meta_doc.field_ins() + ") as dt from [" + table + "]"
            + " union select max(" + db.meta_doc.field_upd() + ") as dt from [" + table + "]) tbl") : null;
          DateTime? dt_ref = null;
          if (type == type_check.data && ((db_ref.meta_doc.field_ins() != null && db_ref.exist_field(table, db_ref.meta_doc.field_ins()))
            && (db_ref.meta_doc.field_upd() != null && db_ref.exist_field(table, db_ref.meta_doc.field_upd())))) {
            ((db_xml)db_ref).begin_table(table);
            while (((db_xml)db_ref).read_row(table))
              dt_ref = db_provider.max_date(dt_ref, db_provider.max_date(((db_xml)db_ref).get_xmldata(table).date(db.meta_doc.field_ins())
                , ((db_xml)db_ref).get_xmldata(table).date(db.meta_doc.field_upd())));
          }
          if (type == type_check.data && dt.HasValue && dt_ref.HasValue && dt != dt_ref && dt < dt_ref)
            result.Add(new ele_info(ele_info.info_code.i_diff_last_upd, ele_info.info_type.warning
              , "la tabella da importare ha una modifica più recente: " + dt_ref.ToString(), dt_ref));
          else if (type != type_check.data && dt.HasValue) result.Add(new ele_info(ele_info.info_code.i_last_upd, "ultima modifica: " + dt.ToString(), dt));
        }
      }

      // indice univoco
      idx_table itbl = db.tableUniqueIndex(table), itblref = type == type_check.check ? db.meta_doc.indexUnique(table) : db_ref.tableUniqueIndex(table);
      if ((type == type_check.check || type == type_check.data) && itbl == null)
        result.Add(ele_info.info_err(ele_info.info_code.err_no_unique_index, "la tabella '" + table + "' non ha un indice univoco"
          , db.meta_doc.indexUnique(table) != null ? action_db.init_uidx(actions, table, db_ref != null ? db_ref.ver : db.ver) : null));
      else if ((type == type_check.check || type == type_check.data)
        && itbl != null && itbl.Fields.FirstOrDefault(x => db.meta_doc.idlist_field_link(table, x.Name)) != null)
        result.Add(ele_info.info_err(ele_info.info_code.err_idlist_unique_index, "la tabella '" + table + "' ha un campo id list nell'indice univoco"));
      else if (type == type_check.data && db_ref.meta_doc.indexUnique(table) == null)
        result.Add(ele_info.info_err(ele_info.info_code.err_no_unique_index_rif, "la tabella '" + table + "' non ha un indice univoco sullo schema di riferimento, verrà adottato l'indice presente nel database da aggiornare"));
      else if (type == type_check.check || type == type_check.sch || type == type_check.data)
        if (itbl != null && itblref != null && !db.same_idxs(itbl, itblref))
          result.Add(ele_info.info_err(ele_info.info_code.err_diff_unique_index, "la tabella '" + table + "' ha un indice univoco diverso da quello di riferimento"
            , db.meta_doc.indexUnique(table) != null ? action_db.init_uidx(actions, table, db_ref != null ? db_ref.ver : db.ver) : null));

      // chiave primaria 
      if (type == type_check.check || type == type_check.data) {
        idx_table primary = db.schema.indexPrimary(table);
        if (primary == null) result.Add(ele_info.info_err(ele_info.info_code.err_no_primary_index, "la tabella '" + table + "' non ha un indice primario"));
        else if (primary.Fields.Count > 1) result.Add(ele_info.info_err(ele_info.info_code.err_primary_index, "la tabella '" + table + "' ha più di un campo nell'indice primario"));
      } else if (db_ref != null && type == type_check.sch) {
        idx_table pk = db.table_pk(table);
        idx_table pk_ref = db_ref.table_pk(table);
        if (pk != null && pk_ref != null && pk.Fields[0].Name.ToUpper() != pk_ref.Fields[0].Name.ToUpper())
          result.Add(ele_info.info_err(ele_info.info_code.err_diff_primary_index, "la tabella '" + table + "' ha un indice primario diverso da quello di riferimento"));
      }

      // check meta
      meta_table metatbl = db.meta_doc.meta_tbl(table);
      if (type == type_check.check && metatbl != null) {
        foreach (meta_rule rule in metatbl.rules)
          if (rule.type == "nochar" && db.get_count("select count(*) from " + table + " where " + rule.field
              + "  like '%" + rule.value + "%'") > 0)
            result.Add(ele_info.info_err(ele_info.info_code.err_data_field, "la tabella '" + table + "' contiene il carattere sporco '" + rule.value + "'" + " nel campo '" + rule.field + "'."));

        // tabelle collegate
        foreach (meta_link lnk in metatbl.links) {
          if (!db.exist_field(table, lnk.field)) {
            result.Add(ele_info.info_err(ele_info.info_code.err_no_meta_field, "il campo '" + lnk.field + "' specificato nel meta xml non esite per la tabella '" + table + "'."));
            continue;
          }

          string idfield = db.schema.pkOfTable(lnk.table);
          if (idfield == "") {
            result.Add(ele_info.info_err(ele_info.info_code.err_no_pk_field, "non è stato possibile recuperare il campo primario per la tabella '" + lnk.table + "'"));
            continue;
          }

          // id collegati
          if (lnk.type == meta_link.types_link.list) {
            System.Data.DataTable tags = db.dt_table("select distinct t.[" + lnk.field + "] AS field"
                + " from [" + table + "] t where t.[" + lnk.field + "] is not null");

            List<string> list = new List<string>();
            foreach (System.Data.DataRow tag in tags.Rows)
              foreach (string value in tag["field"].ToString().Split(']'))
                if ((value.Length > 0 && value[0] != '[') || value.Length > 0 && value.Substring(1) != ""
                    && (!strings.isNumeric(value.Substring(1)) || db.get_count("select count(*) from [" + lnk.table + "] a"
                        + " where a.[" + idfield + "] = " + value.Substring(1)) == 0)) { if (!list.Contains(value.Substring(1))) list.Add(value.Substring(1)); }

            if (list.Count > 0)
              result.Add(ele_info.info_err(ele_info.info_code.err_no_linked_values, "la tabella '" + table
                + "' contiene " + list.Count.ToString() + " valori '" + string.Join(", ", list.ToArray()) + "' non esistenti nella tabella collegata '" + lnk.table + "' per il campo '" + lnk.field + "'"
                , action_db.add_init_list_tags(actions, table, lnk.field, lnk.table, list)));
          } else if(db.exist_table(lnk.table)) {
            string values = db.get_list("select distinct [" + lnk.field + "] from [" + table + "] a"
                + " where a.[" + lnk.field + "] not in (select [" + idfield + "] from [" + lnk.table + "])"
                + "  and a.[" + lnk.field + "] is not null", lnk.field);

            if (values != "")
              result.Add(ele_info.info_err(ele_info.info_code.err_no_linked_values, "la tabella '" + table
                + "' contiene " + values.Split(',').Length.ToString() + " valori '" + values + "' non esistenti nella tabella collegata '" + lnk.table + "' per il campo '" + lnk.field + "'"
                , action_db.add_remove_ids(actions, table, lnk.field, values)));
          }
        }
      }
    }
    // check tabella storico
    else if (tp == meta_doc.table_type.storico) {
      // conteggio records
      long count = 0;
      if (type != type_check.data) {
        count = db.get_count("select count(*) as conteggio from [" + table + "] where " + db.meta_doc.field_del() + " is not null");
        if (count > 0)
          result.Add(new ele_info(ele_info.info_code.i_count
            , count == 1 ? " cè una riga nel cestino" : "ci sono " + count.ToString() + " righe nel cestino", count));
      }

      // colonna del - ref
      if (type == type_check.check) {
        if (db.meta_doc.field_ref() == null || !db.exist_field(table, db.meta_doc.field_ref()))
          result.Add(ele_info.info_err(ele_info.info_code.err_no_service_field, "la tabella '" + table + "' non ha la colonna di servizio con la data di riferimento"
            , action_db.init_srvfields_history_table(actions, table)));

        if (db.meta_doc.field_del() == null || !db.exist_field(table, db.meta_doc.field_del()))
          result.Add(ele_info.info_err(ele_info.info_code.err_no_service_field, "la tabella '" + table + "' non ha la colonna di servizio con la data di cancellazione"
            , action_db.init_srvfields_history_table(actions, table)));
        // ultima modifica
        else if (count > 0 && db.meta_doc.field_del() != null) {
          object dt = db.get_value("select max(" + db.meta_doc.field_del() + ") as dt from [" + table + "]");
          if (dt != DBNull.Value) result.Add(new ele_info(ele_info.info_code.i_last_upd, "ultima cancellazione: " + dt.ToString(), dt));
        }
      }

      // che ci sia la tabella associata
      if (type == type_check.check && !db.exist_table(table_ref))
        result.Add(ele_info.info_err(ele_info.info_code.err_no_data_table, "la tabella storico '" + table + "' non ha la relativa tabella '" + table_ref + "' associata"));
    }

    return result;
  }

  int get_level_path(System.Data.DataTable folders, int id_folder, int from = -1) {
    string fld_id = "IDFOLDER", fld_idpadre = "IDFOLDERPADRE";
    System.Data.DataRow folder = folders.Select(string.Format("{0}={1}", fld_id, id_folder))[0];
    int result = from >= 0 ? from + 1 : 0;
    return int.Parse(folder[fld_idpadre].ToString()) != int.Parse(folder[fld_id].ToString()) ?
      get_level_path(folders, int.Parse(folder[fld_idpadre].ToString()), result) : result;
  }

  string get_rel_path(System.Data.DataTable folders, int id_folder) {
    string fld_id = "IDFOLDER", fld_idpadre = "IDFOLDERPADRE", fld_name = "FOLDERNAME";
    System.Data.DataRow folder = folders.Select(string.Format("{0}={1}", fld_id, id_folder))[0];
    return int.Parse(folder[fld_idpadre].ToString()) != int.Parse(folder[fld_id].ToString()) ?
      get_rel_path(folders, int.Parse(folder[fld_idpadre].ToString())) + "/" + folder[fld_name].ToString() : folder[fld_name].ToString();
  }

  string get_file_path(System.Data.DataTable files, System.Data.DataTable folders, int id_file) {
    string fld_id_file = "IDFILE", fld_id_folder = "IDFOLDER", fld_ext = "EXT", fld_name = "FILENAME";
    System.Data.DataRow file = files.Select(string.Format("{0}={1}", fld_id_file, id_file))[0];
    return get_rel_path(folders, int.Parse(file[fld_id_folder].ToString()))
      + "/" + file[fld_name].ToString() + (file[fld_ext] != DBNull.Value && file[fld_ext].ToString().Trim() != "" ? "." + file[fld_ext].ToString() : "");
  }

  // check dei dati da importare dalle tabelle xml
  bool check_align_data(string table, db_schema db, db_xml db_ref, List<ele_info> infos, bool del_table
    , string folder_xmls, Dictionary<string, string> tmp_tables, out long to_add, out long to_upd, out string err) {

    err = "";
    to_add = 0; to_upd = 0;

    string tmp_table = tmp_tables["tmp"];
    xmlDoc sch_tmp = null;
    try {

      // checks tmp tables
      if (db.exist_table(tmp_table)) throw new Exception("attenzione! la tabella '" + tmp_table + "' è già presente nel database");

      // tmp_table
      sch_tmp = doc_align_table(tmp_table, db, table, 1);
      //sch_tmp.doc.Save("c:\\tmp\\tbl.xml");
      db.create_table(sch_tmp.node("/root/table"));

      db_ref.begin_table(table, null, true);
      while (db_ref.read_row(table))
        db.exec("INSERT INTO " + tmp_table + " (" + string.Join(",", sch_tmp.nodes("/root/table/cols/col[not(@align_code)]").Cast<XmlNode>()
          .Select(x => x.Attributes["name"].Value)) + ") values(" + values_to_qry(table, sch_tmp.nodes("/root/table/cols/col[not(@align_code)]"), db_ref, db) + ")");

      // popolo i campi recursive folders, files
      System.Data.DataTable folders = null, files = null;
      bool rec_folders = sch_tmp.exist("/root/table/cols/col[@align_code='" + meta_table.align_codes.folders.ToString() + "']")
        , rec_files = sch_tmp.exist("/root/table/cols/col[@align_code='" + meta_table.align_codes.files.ToString() + "']");
      if (rec_folders || rec_files) {

        // tabella d'appoggio folders
        {
          string rec_tbl = db_ref.meta_doc.table_from_code(meta_table.align_codes.folders);
          string tmp_rec_tbl = tmp_tables["folders"];
          if (!db.exist_table(tmp_rec_tbl)) {
            db.create_table(db_ref.schema.table_node(rec_tbl), false, tmp_rec_tbl);
            db_ref.begin_table(rec_tbl);
            db.set_identity(tmp_rec_tbl, true);
            while (db_ref.read_row(rec_tbl))
              db.exec("INSERT INTO " + tmp_rec_tbl + " (" + string.Join(",", db_ref.schema.table_node(rec_tbl).SelectNodes("cols/col").Cast<XmlNode>()
                .Select(x => x.Attributes["name"].Value)) + ") values(" + values_to_qry(rec_tbl, db_ref.schema.table_node(rec_tbl).SelectNodes("cols/col"), db_ref, db, true) + ")");
            db.set_identity(tmp_rec_tbl, false);
          }

          folders = db.dt_table(string.Format("select {0}, {1}, {2} from {3}", "IDFOLDER", "IDFOLDERPADRE", "FOLDERNAME", tmp_rec_tbl));
        }

        // tabella d'appoggio files
        if (rec_files) {
          string rec_tbl = db_ref.meta_doc.table_from_code(meta_table.align_codes.files);
          string tmp_rec_tbl = tmp_tables["files"];
          if (!db.exist_table(tmp_rec_tbl)) {
            db.create_table(db_ref.schema.table_node(rec_tbl), false, tmp_rec_tbl);
            db_ref.begin_table(rec_tbl);
            db.set_identity(tmp_rec_tbl, true);
            while (db_ref.read_row(rec_tbl))
              db.exec("INSERT INTO " + tmp_rec_tbl + " (" + string.Join(",", db_ref.schema.table_node(rec_tbl).SelectNodes("cols/col").Cast<XmlNode>()
                .Select(x => x.Attributes["name"].Value)) + ") values(" + values_to_qry(rec_tbl, db_ref.schema.table_node(rec_tbl).SelectNodes("cols/col"), db_ref, db, true) + ")");
            db.set_identity(tmp_rec_tbl, false);
          }

          files = db.dt_table(string.Format("select {0}, {1}, {2}, {3} from {4}", "IDFILE", "IDFOLDER", "EXT", "FILENAME", tmp_rec_tbl));
        }

        // aggiorno gli id recursives folders
        string pk_fld = sch_tmp.get_value("/root/table/cols/col[@pkfield='1'][@level='1'][@sub_level='0']", "name");
        db.dt_table(string.Format("select * from {0}", tmp_table)).Rows.Cast<System.Data.DataRow>().ToList()
          .ForEach(row => {
            if (rec_folders)
              sch_tmp.nodes("/root/table/cols/col[@align_code='" + meta_table.align_codes.folders.ToString() + "']").Cast<XmlNode>().ToList()
                .ForEach(col => {
                  if (col.Attributes["align_type"] != null && col.Attributes["align_type"].Value == meta_table.align_types.level.ToString())
                    db.exec(string.Format("update {0} set {1} = {2} where {3} = {4}", tmp_table, col.Attributes["name"].Value
                      , db.val_toqry(get_level_path(folders, int.Parse(row[col.Attributes["rel_col_name"].Value].ToString())).ToString()
                      , fieldType.INTEGER)
                      , pk_fld, row[pk_fld].ToString()));
                  else
                    db.exec(string.Format("update {0} set {1} = {2} where {3} = {4}", tmp_table, col.Attributes["name"].Value
                      , db.val_toqry(get_rel_path(folders, int.Parse(row[col.Attributes["rel_col_name"].Value].ToString()))
                      , fieldType.VARCHAR)
                      , pk_fld, row[pk_fld].ToString()));
                });
            if (rec_files)
              sch_tmp.nodes("/root/table/cols/col[@align_code='" + meta_table.align_codes.files.ToString() + "']").Cast<XmlNode>().ToList()
                .ForEach(col => {
                  db.exec(string.Format("update {0} set {1} = {2} where {3} = {4}", tmp_table, col.Attributes["name"].Value
                    , db.val_toqry(get_file_path(files, folders, int.Parse(row[col.Attributes["rel_col_name"].Value].ToString()))
                    , fieldType.VARCHAR)
                    , pk_fld, row[pk_fld].ToString()));
                });
          });
      }

      // righe da aggiungere
      string sqry = build_subqry(sch_tmp, sch_tmp.node("/root/table/cols/col[@level='1'][@pkfield='1']")
           , string.Format("select {0}.{1} from {2} {0} where 1 = 1 ", get_alias(sch_tmp, table, 1), db.schema.pkOfTable(table), table), db);

      // ordinamento per un aggiornamento corretto della gerarchia
      string order_by = "";
      for (int i = 1; i <= sch_tmp.get_int("/root/table", "max_level"); i++) {
        XmlNode col = sch_tmp.node("/root/table/cols/col[@level='" + i.ToString() + "'][@align_code='folders'][@align_type='level']");
        if (col != null) {
          order_by = col.Attributes["name"].Value;

          break;
        }
      }

      // conteggio record da aggiungere - copia tabella list
      if (db.export_data_xml(tmp_table, ids_path(folder_xmls, table), db.table_fields(tmp_table)
        , out to_add, " not exists (" + sqry + ")", true, "tmp", sch_tmp.node("/root/table"), order_by)) {
        foreach (XmlNode col_list in sch_tmp.nodes("/root/table/cols/col[@pklist='1']"))
          if (!System.IO.File.Exists(list_path(folder_xmls, xmlDoc.node_val(col_list, "tableref")))) {
            System.IO.File.Copy(db_ref.data_path(xmlDoc.node_val(col_list, "tableref"))
              , list_path(folder_xmls, xmlDoc.node_val(col_list, "tableref")));
            if (!System.IO.File.Exists(schemas_path(folder_xmls)))
              db_ref.schema.doc.doc.Save(schemas_path(folder_xmls));
          }
      }

      // vedo se ci sono dei records cancellati
      if (del_table && db_ref.read_row(db_ref.meta_doc.prefix_del() + table))
        infos.Add(new ele_info(ele_info.info_code.err_cant_align_del_rows, ele_info.info_type.error, "sono presenti dei records cancellati nel db di riferimento, ma non è possibile per il momento effettuarne l'allineamento"));

      return true;
    } catch (Exception ex) { if (sch_tmp != null) logErr(string.Format("schema {0}: {1}", tmp_table, sch_tmp.doc.OuterXml)); err = ex.Message; return false; } finally { if (db.exist_table(tmp_tables["tmp"])) db.drop_table(tmp_tables["tmp"]); }
  }

  string get_alias(xmlDoc sch_tmp, string tbl_name, int lvl) {
    return sch_tmp.get_value(string.Format("/root/table/cols/col[@tableref='{0}']"
      + "[@pkfield='1'][@level='{1}']", tbl_name, lvl), "alias_tbl");
  }

  string build_subqry(xmlDoc sch_tmp, XmlNode pk_field, string subqry, db_schema db) {
    int i = xmlDoc.node_int(pk_field, "level");
    string table_ref = xmlDoc.node_val(pk_field, "tableref");
    if (i > 1) {
      int i_where = subqry.IndexOf(" where 1 = 1 ");
      subqry = string.Format("{0} \n join {1} {2} on {2}.{3} = {5}.{4}{6}", subqry.Substring(0, i_where), table_ref, xmlDoc.node_val(pk_field, "alias_tbl")
        , xmlDoc.node_val(pk_field, "pk_col_name"), xmlDoc.node_val(pk_field, "col_name")
        , get_alias(sch_tmp, xmlDoc.node_val(pk_field, "parent_tbl"), i - 1), subqry.Substring(i_where));
    }

    // index
    foreach (XmlNode col in sch_tmp.nodes(string.Format("/root/table/cols/col[@keyfield='1'][@pkfield='0'][@tableref='{0}'][@level='{1}'][@sub_level='{2}'][not(@align_code)]"
      , table_ref, i, xmlDoc.node_val(pk_field, "sub_level")))) {
      meta_table.align_codes ac = meta_table.get_align_code(col.Attributes["have_align_cols"]);
      if (ac == meta_table.align_codes.folders) {
        XmlNode align_col = sch_tmp.node(string.Format("/root/table/cols/col[@align_code='folders'][not(@align_type)][@rel_col_name='{0}']"
          , col.Attributes["name"].Value));
        subqry += string.Format(" \n  and isnull(lower(dbo.getPathOfFolder({0}.{1})), '') = isnull(lower(tmp.{2}), '')"
          , xmlDoc.node_val(pk_field, "alias_tbl"), col.Attributes["col_name"].Value, align_col.Attributes["name"].Value);
      } else
        subqry += string.Format(" \n  and isnull({0}.{2}, {3}) = isnull(tmp.{1}, {3})", xmlDoc.node_val(pk_field, "alias_tbl")
          , col.Attributes["name"].Value, col.Attributes["col_name"].Value, db.schema.def_toqry(xmlDoc.node_val(col, "tableref"), xmlDoc.node_val(col, "col_name")));
    }

    // links
    foreach (XmlNode pk2 in pk_field.SelectNodes(string.Format("/root/table/cols/col[@level='{0}'][@parent_tbl='{1}']"
      + "[@pkfield='1'][@keyfield='1']", i + 1, table_ref)))
      subqry = build_subqry(sch_tmp, pk2, subqry, db);

    return subqry;
  }

  // copia dati dalle tabelle xml pronti per la insert
  string values_to_qry(string table, XmlNodeList cols, db_xml dbxml, db_schema db, bool not_align = false) {
    try {
      if (not_align)
        return string.Join(", ", cols.Cast<XmlNode>().Select(col => db.val_toqry(dbxml.get_xmldata(table).val(xmlDoc.node_val(col, "name"))
            , schema_field.originalToType(db.type, xmlDoc.node_val(col, "type")))));

      List<string> id_tbls = new List<string>();
      int min_level = 1; //, level_tbl = min_level;
      string res = "";
      meta_table.align_codes acode = db.meta_doc.table_align_code(table);
      foreach (XmlNode col in cols) {
        int level_col = xmlDoc.node_int(col, "level");
        if (xmlDoc.node_val(col, "id_fld") == "") {
          string val = xmlDoc.node_int(col, "level") == min_level ? dbxml.get_xmldata(table, level_col).val(xmlDoc.node_val(col, "col_name"))
            : (dbxml.is_opened_xmldata(xmlDoc.node_val(col, "parent_tbl"), (xmlDoc.node_int(col, "pkfield") == 1 ? level_col - 1 : level_col)) ?
              dbxml.get_xmldata(xmlDoc.node_val(col, "parent_tbl"), (xmlDoc.node_int(col, "pkfield") == 1 ? level_col - 1 : level_col)).val(xmlDoc.node_val(col, "col_name")) : null);
          if (xmlDoc.node_int(col, "pkfield") == 1 && level_col > min_level) {
            if (!string.IsNullOrEmpty(val)) {
              if (dbxml.find_row_byid(xmlDoc.node_val(col, "tableref"), xmlDoc.node_val(col, "pk_col_name"), val, level_col))
                id_tbls.Add(string.Format("{0}_{1}{2}", xmlDoc.node_val(col, "tableref"), xmlDoc.node_val(col, "level"), xmlDoc.node_val(col, "sub_level")));
              else throw new Exception("non è stata trovata la corrispondenza con il valore " + val + " per il campo '" + xmlDoc.node_val(col, "col_name") + "' nella tabella '" + xmlDoc.node_val(col, "tableref") + "'");
            } else dbxml.close_xmldata(xmlDoc.node_val(col, "tableref"), level_col);
          }
          res += (res != "" ? ", " : "") + db.val_toqry(val, db.schema.table_field(xmlDoc.node_int(col, "level") == min_level ? table
            : xmlDoc.node_int(col, "pkfield") == 1 ? xmlDoc.node_val(col, "parent_tbl") : xmlDoc.node_val(col, "tableref"), xmlDoc.node_val(col, "col_name")).TypeField);
        } else {
          if (id_tbls.Contains(string.Format("{0}_{1}{2}", xmlDoc.node_val(col, "tableref"), xmlDoc.node_val(col, "level"), xmlDoc.node_val(col, "sub_level"))))
            res += (res != "" ? ", " : "") + dbxml.get_xmldata(xmlDoc.node_val(col, "tableref"), level_col).val_to_qry(xmlDoc.node_val(col, "col_name"), db);
          else res += (res != "" ? ", " : "") + "NULL";
        }
      };
      return res;
    } catch (Exception ex) { logErr(ex); throw ex; }
  }

  // costruzione tabella temporanea per l'allineamento dati
  xmlDoc doc_align_table(string tmp_table, db_schema db, string table, int level, int i_sub = 0, xmlDoc doc = null
    , string id_fld = null, bool idx_field = false, string parent_tbl = "", bool force_nullable = false) {
    try {
      if (level > 1 && string.IsNullOrEmpty(id_fld))
        throw new Exception(string.Format("non è specificato l'id field per il livello {0}, tabella {1}, tabella principale {2}", level, table, parent_tbl));

      meta_table mt = db.meta_doc.meta_tbl(table), mt_parent = !string.IsNullOrEmpty(parent_tbl) ? db.meta_doc.meta_tbl(parent_tbl) : null;

      doc = doc == null ? xmlDoc.create_fromxml("<root><table nameupper='" + tmp_table + "' name='" + tmp_table + "'><cols/></table></root>") : doc;

      // min, max level
      doc.set_attr("/root/table", "min_level", !doc.exist("/root/table", "min_level") ? level.ToString()
        : doc.get_int("/root/table", "min_level") > level ? level.ToString() : doc.get_value("/root/table", "min_level"));
      doc.set_attr("/root/table", "max_level", !doc.exist("/root/table", "max_level") ? level.ToString()
        : doc.get_int("/root/table", "max_level") < level ? level.ToString() : doc.get_value("/root/table", "max_level"));

      // id field
      string pkfld = db.schema.pkOfTable(table), name_fld = string.Format("pk_{0}_{1}{2}", pkfld, level, i_sub);
      XmlNode pk_node = level == 1 ? xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.ImportNode(db.schema.field_node(table, pkfld), true))
        , new Dictionary<string, string>() { { "col_name", pkfld }, { "name", name_fld }, { "pkfield", "1" }, { "keyfield", idx_field ? "1" : "0" }
        , { "tableref", table }, { "alias_tbl", string.Format("{0}_{1}{2}", table, level, i_sub) }, { "autonumber", "" }, { "level", level.ToString() }
        , { "sub_level", i_sub.ToString() }, { "parent_tbl", parent_tbl } })
      : xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.ImportNode(db.schema.field_node(parent_tbl, id_fld), true))
        , new Dictionary<string, string>() { { "col_name", id_fld }, { "pk_col_name", pkfld }, { "name", name_fld }, { "pkfield", "1" }, { "keyfield", idx_field ? "1" : "0" }
        , { "tableref", table }, { "alias_tbl", string.Format("{0}_{1}{2}", table, level, i_sub) }, { "autonumber", "" }, { "level", level.ToString() }
        , { "sub_level", i_sub.ToString() }, { "parent_tbl", parent_tbl }, { "nullable", force_nullable ? "true" : ""} });

      if (level > 1 && mt.align_code == meta_table.align_codes.folders) {
        xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.CreateElement("col")), new Dictionary<string, string>() { { "col_name", pkfld }, { "nameupper", pkfld.ToUpper() }
          , { "type", schema_field.typeToOriginal(db.type, fieldType.VARCHAR) }, { "maxlength", "-1" }, { "rel_col_name", name_fld }, { "nullable", "true" }
          , { "align_code", mt.align_code.ToString() } , { "name", string.Format("rec_{0}_{1}{2}", pkfld, level, i_sub) }, { "keyfield", "1" }, { "tableref", table }
          , { "level", level.ToString() } , { "sub_level", i_sub.ToString() }, { "id_fld", id_fld != null ? id_fld.ToUpper() : null }, { "pkfield", "0" }, { "parent_tbl", parent_tbl }});

        xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.CreateElement("col")), new Dictionary<string, string>() { { "col_name", pkfld }, { "tableref", table }, { "pkfield", "0" }
          , { "type", schema_field.typeToOriginal(db.type, fieldType.INTEGER) }, { "rel_col_name", name_fld }, { "nullable", "true" }, { "parent_tbl", parent_tbl }, { "nameupper", pkfld.ToUpper() }
          , { "align_code", mt.align_code.ToString() }, { "align_type", meta_table.align_types.level.ToString() }, { "id_fld", id_fld != null ? id_fld.ToUpper() : null }
          , { "name", string.Format("rec_level_{0}_{1}{2}", pkfld, level, i_sub) }, { "keyfield", "1" }, { "level", level.ToString() } , { "sub_level", i_sub.ToString() }});
      }

      // key fields
      int i_sub2 = 0;
      foreach (idx_field ifld in db.meta_doc.indexUnique(table).Fields) {
        meta_link ml = mt.links.FirstOrDefault(x => x.field.ToUpper() == ifld.Name.ToUpper());
        meta_table.align_codes ml_rec_code = ml != null ? db.meta_doc.meta_tbl(ml.table).align_code : meta_table.align_codes.none;
        if (meta_table.recursive_link(mt_parent, mt)) continue;
        if (ml != null)
          doc_align_table(tmp_table, db, ml.table, level + 1, i_sub2++, doc, ifld.Name, true, table, force_nullable || db.schema.isFieldNullable(table, ifld.Name));
        else {
          string fld_name = string.Format("key_{0}_{1}{2}", ifld.Name, level, i_sub);
          XmlNode col = xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.ImportNode(db.schema.field_node(table, ifld.Name), true))
            , new Dictionary<string, string>() { { "tableref", table }, { "col_name", ifld.Name } , { "name", fld_name }
              , { "keyfield", "1" }, { "level", level.ToString() } , { "sub_level", i_sub.ToString() }, { "id_fld", id_fld != null ? id_fld.ToUpper() : null }, { "pkfield", "0" }
              , { "parent_tbl", parent_tbl }, { "nullable", db.schema.isFieldNullable(table, ifld.Name) || force_nullable ? "true" : "" }
              , { "have_align_cols", ml_rec_code != meta_table.align_codes.none ? ml_rec_code.ToString() : "" }});
          if (ml_rec_code == meta_table.align_codes.files)
            xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.CreateElement("col")), new Dictionary<string, string>() { { "col_name", ifld.Name }, { "nameupper", ifld.Name.ToUpper() }
             , { "type", schema_field.typeToOriginal(db.type, fieldType.VARCHAR) }, { "maxlength", "-1" }, { "rel_col_name", fld_name }, { "nullable", "true" }
             , { "align_code", ml_rec_code.ToString() } , { "name", string.Format("rec_{0}_{1}{2}", ifld.Name, level, i_sub) }, { "keyfield", "1" }, { "tableref", table }
             , { "level", level.ToString() } , { "sub_level", i_sub.ToString() }, { "id_fld", id_fld != null ? id_fld.ToUpper() : null }, { "pkfield", "0" }, { "parent_tbl", parent_tbl }});
        }
      }

      // link fields
      if (level == 1) {
        foreach (meta_link lnk in mt.links.Where(x => !doc.exist("/root/table/cols/col[@nameupper='" + x.field.ToUpper() + "'][@level='2']")))
          if (lnk.type != meta_link.types_link.list && !meta_table.recursive_link(mt_parent, mt))
            doc_align_table(tmp_table, db, lnk.table, level + 1, i_sub2++, doc, lnk.field, false, table, force_nullable || db.schema.isFieldNullable(table, lnk.field));
          else xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.ImportNode(db.schema.field_node(table, lnk.field), true))
            , new Dictionary<string, string>() { { "col_name", lnk.field }, { "pk_col_name", db.schema.pkOfTable(lnk.table) }, { "pklist", "1" }
              , { "tableref", lnk.table }, { "name", string.Format("pks_{0}_{1}{2}", lnk.field, level, i_sub2++) }, { "autonumber", "" }, { "level", level.ToString() } });
      }

      // service fields
      if (level == 1)
        foreach (schema_field fld in db.table_fields(table))
          if (fld.Name.ToUpper() == db.meta_doc.field_ins().ToUpper() || fld.Name.ToUpper() == db.meta_doc.field_upd().ToUpper())
            xmlDoc.set_attrs(doc.add_node("/root/table/cols", doc.doc.ImportNode(db.schema.field_node(table, fld.Name), true))
              , new Dictionary<string, string>() { { "col_name", fld.Name }, { "name", "srv_" + fld.Name }, { "level", level.ToString() }
                , { "pkfield", "0" }, { "keyfield", "0" } });

      return doc;
    } catch (Exception ex) { throw ex; }
  }

  // check sulla tabella di cancellazione records
  List<ele_info> check_del_table(db_schema db, List<action_db> actions, string table, string table_ref, bool rif) {
    List<ele_info> result = new List<ele_info>();

    // colonne di servizio - dtins 
    if ((db.meta_doc.field_ins() != null && !db.exist_field(table, db.meta_doc.field_ins()))
        || (db.meta_doc.field_upd() != null && !db.exist_field(table, db.meta_doc.field_upd()))
        || (db.meta_doc.field_del() != null && !db.exist_field(table, db.meta_doc.field_del()))
        || (db.meta_doc.field_ref() != null && !db.exist_field(table, db.meta_doc.field_ref())))
      result.Add(ele_info.info_war(!rif ? ele_info.info_code.err_history_no_service_field : ele_info.info_code.err_history_no_service_field_rif
        , "la tabella storico " + (rif ? "di riferimento" : "") + " '" + table + "' non ha una delle colonne di servizio"));
    else if (db.schema.table_field(table, db.meta_doc.field_ins()).TypeField != fieldType.DATETIME2
      || db.schema.table_field(table, db.meta_doc.field_ins()).Nullable)
      result.Add(ele_info.info_err(!rif ? ele_info.info_code.err_history_service_field : ele_info.info_code.err_history_service_field_rif
        , "la tabella storico " + (rif ? "di riferimento" : "") + " '" + table + "' ha la colonna di servizio di inserimento errata"
        , action_db.init_del_onins(actions, table, db.ver)));

    // che abbia lo colonne della chiave univoca della tabella pilota
    idx_table iu = db.tableUniqueIndex(table_ref);
    if (iu != null) {
      foreach (string col in iu.Fields.Select(x => x.Name))
        if (!db.exist_field(table, col))
          result.Add(ele_info.info_err(!rif ? ele_info.info_code.err_history_schema_ifields : ele_info.info_code.err_history_schema_ifields_rif
            , "la tabella storico " + (rif ? "di riferimento" : "") + " '" + table + "' non possiede il campo '" + col + "' della chiave univoca nella sua struttura", col));
    }

    // che abbia lo colonne della tabella pilota
    foreach (string col in db.meta_doc.tableCols(table_ref, new List<meta_doc.col_type> { meta_doc.col_type.primary
            , meta_doc.col_type.diretta, meta_doc.col_type.linked }).Keys)
      if (!db.exist_field(table, col)
        && result.Count(x => x.code == (!rif ? ele_info.info_code.err_history_schema_ifields : ele_info.info_code.err_history_schema_ifields_rif)
        && x.value.ToString().ToLower() == col.ToLower()) == 0)
        result.Add(ele_info.info_err(!rif ? ele_info.info_code.err_history_schema : ele_info.info_code.err_history_schema_rif
          , "la tabella storico " + (rif ? "di riferimento" : "") + " '" + table + "' non possiede il campo '" + col + "' nella sua struttura"));

    return result;
  }

  // riepilogo situazione per l'aggiornamento schema e dati del database con l'indice schema specificato
  void init_align(string index_schema, type_check type, string dbname_conn = "") {

    // carico il db da aggiornare
    db_schema db = dbname_conn != "" ? conn_db(dbname_conn) : conn_db_user();
    if (!db.exist_schema) throw new Exception("non è caricato lo schema xml relativo al database!");
    if (!db.exist_meta) throw new Exception("non è caricato il meta xml relativo al database!");

    // lo schema di riferimento
    db_xml db_ref = conn_schema(index_schema);
    try {
      if (!db_ref.exist_meta) throw new Exception("non è caricato il meta xml relativo al database di riferimento!");
      // check align schema - data
      List<ele_db> eles = new List<ele_db>();
      List<action_db> actions = new List<action_db>();
      if (type == type_check.sch) {
        // tabelle 
        foreach (string table in filter_objs(db.tables(), db.meta_doc.test_filter()))
          ele_db.update_info_tables(eles, table, check_schema_table(db, table, type, actions, db_ref), db.meta_doc.type_table(table));
        foreach (string table in filter_objs(db_ref.schema.tables_name(), db.meta_doc.test_filter()))
          if (!db.exist_table(table))
            ele_db.update_info_tables(eles, table, ele_info.info_err(ele_info.info_code.err_no_table_db, "la tabella '" + table + "' specificata nello schema xml di riferimento, non esiste nel database"
              , action_db.add_add_table(actions, table, db_ref.ver)), db_ref.meta_doc.type_table(table));

        // funzioni
        foreach (string func in filter_objs(db.functions(), db.meta_doc.test_filter()))
          ele_db.update_infos(eles, func, check_function(db, db_ref.schema, func, actions, type), ele_db.ele_type.function);
        foreach (string func in filter_objs(db_ref.schema.functions_name(), db.meta_doc.test_filter()))
          if (!db.exist_function(func))
            ele_db.update_infos(eles, func, ele_info.info_err(ele_info.info_code.err_no_func_into_db, "la funzione '" + func + "' specificata nello schema xml di riferimento, non esiste nel database"
              , action_db.add_add_fnc(actions, func, db_ref.ver)), ele_db.ele_type.function);

        // sp
        foreach (string sp in filter_objs(db.store_procedures(), db.meta_doc.test_filter()))
          ele_db.update_infos(eles, sp, check_sp(db, db_ref.schema, sp, actions, type), ele_db.ele_type.stored_procedure);
        foreach (string sp in filter_objs(db_ref.schema.sps_name(), db.meta_doc.test_filter()))
          if (!db.exist_procedure(sp))
            ele_db.update_infos(eles, sp, ele_info.info_err(ele_info.info_code.err_no_sp_into_db, "la store procedure '" + sp + "' specificata nello schema xml di riferimento, non esiste nel database")
              , ele_db.ele_type.stored_procedure);
      }
      // data
      else {
        // tabelle mancanti
        foreach (string table in db_ref.schema.tables_name())
          if (!db.exist_table(table))
            ele_db.update_info_tables(eles, table, ele_info.info_err(ele_info.info_code.err_no_table_db, "la tabella '" + table + "' presente nel db da importare, non esiste nel database")
              , db_ref.meta_doc.type_table(table));

        // check dati
        Dictionary<string, string> tmp_tables = new Dictionary<string, string>() { { "tmp", "__TMP_ALIGN" }, { "folders", "__TMP_ALIGN_FOLDERS" }, { "files", "__TMP_ALIGN_FILES" } };
        check_data_tables(db, db_ref, eles, actions, tmp_tables);
        if (db.exist_table(tmp_tables["folders"])) db.drop_table(tmp_tables["folders"]);
        if (db.exist_table(tmp_tables["files"])) db.drop_table(tmp_tables["files"]);
      }

      set_html(db, db_ref, type, eles, actions);
    } finally { db_ref.close_conn(); }
  }

  // torna la lista di tabelle linkate presenti nella chiave univa della tabella
  List<string> linked_key_tables(string table, db_schema db, List<string> result = null) {
    result = result == null ? new List<string>() : result;
    db.meta_doc.meta_tbl(table).links.ForEach(l => { result.Add(l.table); });
    return result;
  }

  // check allineamento dati tabelle
  void check_data_tables(db_schema db, db_xml db_ref, List<ele_db> eles, List<action_db> actions
    , Dictionary<string, string> tmp_tables) {
    string rep_folder = System.IO.Path.Combine(tmpFolder(), System.IO.Path.GetRandomFileName());
    List<string> ck_tables = new List<string>();
    foreach (string table in filter_objs(db.tables(), db.meta_doc.test_filter())
      .Where(x => db.meta_doc.type_table(x) == meta_doc.table_type.data))
      check_data_table(table, db, db_ref, eles, actions, ck_tables, rep_folder, tmp_tables);
  }

  // dai risultati in seguito all'analisi della struttura tabelle si effettua il check dei dati per l'allineamento
  void check_data_table(string table, db_schema db, db_xml db_ref, List<ele_db> eles, List<action_db> actions, List<string> ck_tables, string folder_rps
    , Dictionary<string, string> tmp_tables) {

    if (ck_tables.Contains(table.ToLower())) return;

    ele_db el = ele_db.check_add_table(eles, table, db.meta_doc.type_table(table));

    try {

      // align_data
      if (!db.meta_doc.test_without_linked())
        foreach (string tbl in linked_key_tables(table, db))
          if (table.ToLower() != tbl.ToLower()) check_data_table(tbl, db, db_ref, eles, actions, ck_tables, folder_rps, tmp_tables);

      ck_tables.Add(table.ToLower());

      // check struct
      ele_db.update_info_tables(eles, table, check_schema_table(db, table, type_check.data, actions, db_ref), db.meta_doc.type_table(table));

      // filtro eventi
      List<ele_info> check = new List<ele_info>(el.infos);
      foreach (ele_info e in check) e.clear_actions(actions);

      List<ele_info.info_code> err_codes = new List<ele_info.info_code>() { ele_info.info_code.err_no_table_rif , ele_info.info_code.err_no_table_rif
        , ele_info.info_code.err_no_field, ele_info.info_code.err_diff_unique_index, ele_info.info_code.err_no_unique_index, ele_info.info_code.err_no_service_field };
      // , ele_info.info_code.err_no_unique_index_rif
      IEnumerable<ele_info> errs = check.Where(x => err_codes.IndexOf(x.code) >= 0);

      List<ele_info.info_code> war_codes = new List<ele_info.info_code>() { ele_info.info_code.err_no_primary_index, ele_info.info_code.err_history_schema_ifields_rif
        , ele_info.info_code.err_primary_index, ele_info.info_code.err_no_table_history, ele_info.info_code.err_no_table_history_rif
        , ele_info.info_code.err_history_no_service_field , ele_info.info_code.err_history_no_service_field_rif, ele_info.info_code.err_history_schema_ifields };
      IEnumerable<ele_info> warns = check.Where(x => war_codes.IndexOf(x.code) >= 0);

      bool check_del_table = warns.Count(x => x.code == ele_info.info_code.err_no_table_history_rif
        || x.code == ele_info.info_code.err_history_no_service_field_rif || x.code == ele_info.info_code.err_history_schema_ifields_rif) == 0;

      List<ele_info> infos = new List<ele_info>();
      if (errs.Count() > 0) {
        foreach (ele_info ei in errs) ei.text = ei.text.Length > 3 && ei.text.Substring(ei.text.Length - 3, 3) == "(*)" ? ei.text : ei.text + "(*)";
        infos.Add(new ele_info(ele_info.info_code.err_cant_align, ele_info.info_type.warning, "non è possibile allineare i dati della tabella causa gli avvisi contrassegnati con il simbolo (*)"));
      } else {

        string w_codes = warns.Count() > 0 ? string.Join(",", warns.Select(x => x.code.ToString())) : "";
        foreach (ele_info ei in warns) ei.text = ei.text + "(-)";

        // vedo quanti record ci sono da aggiungere e quanti da aggiornare
        string err;
        long to_add = 0, to_upd = 0;
        if (!check_align_data(el.name, db, db_ref, infos, check_del_table, folder_rps, tmp_tables, out to_add, out to_upd, out err))
          infos.Add(new ele_info(ele_info.info_code.err_cant_align, ele_info.info_type.error, "non è stato possibile effettuare il check di allineamento dati: " + err));
        else if (to_add > 0 || to_upd > 0) {
          infos.Add(new ele_info(ele_info.info_code.i_can_align, ele_info.info_type.info
            , "è possibile allineare i dati della tabella" + (warns.Count() > 0 ? " con delle limitazioni (contrassegnate con il simbolo (-))" : "")
            + (to_add > 0 ? " - ci sono " + to_add.ToString() + " righe da aggiungere" : "")
            + (to_upd > 0 ? " - ci sono " + to_upd.ToString() + " righe da aggiornare" : "")
            , action_db.add_align_data_table(actions, table, w_codes, db_ref.schema.path, ids_path(folder_rps, table))));
        } else
          infos.Add(new ele_info(ele_info.info_code.i_count, ele_info.info_type.info, "non ci sono dati da allineare"));
      }

      ele_db.update_info_tables(eles, table, infos, db.meta_doc.type_table(table));
    } catch (Exception ex) {
      el.infos.Add(new ele_info(ele_info.info_code.err_cant_align, ele_info.info_type.error, "non è stato possibile effettuare il check dei dati: " + ex.Message));
    }
  }

  protected string ids_path(string folder, string table) { return System.IO.Path.Combine(folder, "ids_" + table.ToUpper() + ".xml"); }
  protected string list_path(string folder, string table) { return System.IO.Path.Combine(folder, "list_" + table.ToUpper() + ".xml"); }
  protected string schemas_path(string folder) { return System.IO.Path.Combine(folder, "db_schema.xml"); }

  string table_url(string table) { return getPageRef("table_view", "tbl=" + table + "&dbname=" + query_param("dbname")); }
  string table_url(string table, string db_name) { return getPageRef("table_view", "tbl=" + table + "&dbname=" + db_name); }

  #endregion

  protected void Page_Init(object sender, EventArgs e) {
  }

  protected void Page_Load(object sender, EventArgs e) {
    string msg = "";
    bool log = false;

    // esecuzione azione - azioni
    try {
      string id_action = doaction.Value;
      if (IsPostBack && id_action != "") {
        db_schema db = query_param("dbname") != "" ? conn_db(query_param("dbname"), true)
            : conn_db_user(true, true);
        doaction.Value = "";

        xmlDoc actions = xmlDoc.doc_from_xml(xml_actions.InnerText);
        foreach (XmlNode action in id_action.Split(',').Select(i => int.Parse(i)).OrderBy(i => i)
          .Select(id => actions.node("/root/actions/action[@id='" + id.ToString() + "']")))
          exec_action(db, action);

        close_dbs();
      }
    } catch (Exception ex) { msg = logErr("esecuzione azioni: " + ex.Message); log = true; }

    // check tabelle
    try {
      if (classPage.pageName == "tables_check") init_check(type_check.check);
      else if (classPage.pageName == "tables_upload") {
        string index_schema = "";
        if (query_param("idfl") != "") {
          xmlDoc doc = new xmlDoc(System.IO.Path.Combine(cfg_var("backupsFolder"), cfg_var("fsIndex")));
          index_schema = classPage.extract_dbpck(System.IO.Path.Combine(cfg_var("backupsFolder")
              , doc.get_value("/root/files/file[@idfile=" + query_int("idfl").ToString() + "]", "name")));
        } else if (query_param("dbname") != "" && query_param("tover") != "")
          index_schema = schema_path_fromconn(query_param("dbname"), true, false, false, query_param("tover"));

        init_align(index_schema, (type_check)Enum.Parse(typeof(type_check), query_param("type")), query_param("dbname"));
      } else if (classPage.pageName == "tables_list") init_list();
      else throw new Exception("pagina '" + classPage.pageName + "' non gestita.");
    } catch (Exception ex) { msg += logErr("check tabelle: " + ex.Message); }

    // alert
    if (msg != "") {
      if (!log) classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore - " + msg, "Attenzione!"));
      classPage.regScript(classPage.scriptStartAlert("Si è verificato un errore - " + msg + "<br>", "Esecuzione Azioni"));
    }
  }
}
