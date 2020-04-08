using System;
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
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using mlib.db;
using mlib.tools;
using mlib.xml;
using mlib.tiles;
using toyn;

public partial class _element : tl_page {

  protected int _max_level = 3;
  protected cmd _cmd = null;
  protected elements _e = null;
  protected bool _post = false;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _e = new elements(db_conn, core, config, _user.id);
    if (Request.Headers["toyn-post"] == null) {
      _cmd = qry_val("cmd") != "" ? new cmd(qry_val("cmd")) : null;
      // tolgo il parametro sc
      if (qry_val("sc") != "" && _cmd != null && _cmd.action == "view") {
        set_cache_var("_sc", qry_val("sc"));
        NameValueCollection pars = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        pars.Remove("sc");
        Response.Redirect(Request.Url.AbsolutePath + "?" + pars);
      }
    } else _post = true;

    // elab requests & cmds
    try {

      if (_post) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          string json = String.Empty;
          Request.InputStream.Position = 0;
          using (var inputStream = new StreamReader(Request.InputStream)) {
            json = inputStream.ReadToEnd();
          }

          if (!string.IsNullOrEmpty(json)) {
            JObject jo = JObject.Parse(json);

            // save_element
            string act = jo["action"].ToString();
            if (act == "save_element") {

              // carico l'xml e salvo tutto
              string element_id = jo["element_id"].ToString(), key_page = jo["key_page"].ToString();
              int parent_id = jo["parent_id"].ToString() != "" ? Convert.ToInt32(jo["parent_id"].ToString()) : 0
                , max_level = Convert.ToInt32(jo["max_level"]);
              int? first_order = jo["first_order"].ToString() != "" ? int.Parse(jo["first_order"].ToString()) : (int?)null;
              int? last_order = jo["last_order"].ToString() != "" ? int.Parse(jo["last_order"].ToString()) : (int?)null;
              List<element> els_bck = _e.load_xml(jo["xml_bck"].ToString(), element_id != "" ? parent_id : (int?)null)
                , els = _e.load_xml(jo["xml"].ToString(), element_id != "" ? parent_id : (int?)null);

              // salvataggio
              _e.check_and_del(els_bck, els);
              _e.save_elements(els, key_page, parent_id, first_order, last_order);
              check_cache();
              _e.refresh_order_element(parent_id);

              // vedo se ci sono elementi figli
              int ml = element.max_level(els);
              foreach (element ec in element.find_elements(els, ml).Where(x => x.id > 0)) {
                DataRow dr = db_conn.first_row(core.parse(config.get_query("elements.has-childs").text
                  , new Dictionary<string, object>() { { "id_element", ec.id } }));
                ec.has_childs = db_provider.int_val(dr["has_childs"]) == 1;
                ec.has_child_elements = db_provider.int_val(dr["has_child_elements"]) == 1;
              }

              // xml
              res.doc_xml = element.get_doc(els).xml;
              res.vars.Add("first_order", els.Count > 0 ? els[0].order.ToString() : "");
              res.vars.Add("last_order", els.Count > 0 ? els[els.Count - 1].order.ToString() : "");

              // menu
              StringBuilder sb = new StringBuilder();
              parse_menu(element_id, els, sb, true, max_level);
              res.menu_html = sb.ToString();

            } // remove_element
            else if (act == "remove_element") {
              long id = long.Parse(jo["id"].ToString());

              // pulizia
              DataRow dr = db_conn.first_row(core.parse(config.get_query("elements.id-deleted").text, new Dictionary<string, object>() { { "id_utente", _user.id } }));
              int id_deleted = db_provider.int_val(dr["id_deleted"]);
              db_conn.exec(core.parse(config.get_query("elements.delete-element").text
                , new Dictionary<string, object>() { { "id_element", id }, { "id_utente", _user.id }, { "id_deleted", id_deleted } }));
              _e.refresh_order_child(id);
              res.add_var("cache_ids", check_cache());

            }
              // change_stato_attivita
            else if (act == "change_stato_attivita") {
              long id = long.Parse(jo["id"].ToString());
              bool in_list = bool.Parse(jo["in_list"].ToString());
              string stato = jo["stato_now"].ToString(), stato_new = jo["stato_new"].ToString()
                , new_stato = stato_new != "" ? stato_new : (stato == "" || stato == "da iniziare" ? "la prossima"
                  : (stato == "la prossima" ? "in corso" : (stato == "in corso" ? "sospesa"
                    : (stato == "sospesa" ? "fatta" : (stato == "fatta" ? "da iniziare" : "fatta")))));

              _e.set_attribute(new element(this.core) { type = element.type_element.attivita, id = id }
               , _e.load_attribute(element.type_element.attivita, "stato"), "'" + new_stato + "'");
              res.html_element = parse_element_base(_e.load_element(id), in_list);

            } // change_priorita_attivita 
            else if (act == "change_priorita_attivita") {
              long id = long.Parse(jo["id"].ToString());
              bool in_list = bool.Parse(jo["in_list"].ToString());
              string priorita = jo["priorita_now"].ToString(), priorita_new = jo["priorita_new"].ToString()
                , new_priorita = priorita_new != "" ? priorita_new
                  : (priorita == "" || priorita == "normale" ? "alta" : (priorita == "alta" ? "bassa" : "normale"));

              _e.set_attribute(new element(this.core) { type = element.type_element.attivita, id = id }
               , _e.load_attribute(element.type_element.attivita, "priorita"), "'" + new_priorita + "'");
              res.html_element = parse_element_base(_e.load_element(id), in_list);

            } // check_paste_xml 
            else if (act == "check_paste_xml") {
              StringBuilder text_xml = new StringBuilder();
              foreach (JValue j in jo["text_xml"] as JArray)
                text_xml.AppendLine(j.Value.ToString());
              string key_page = jo["key_page"].ToString(), doc_xml = jo["doc_xml"].ToString(), paste_xml = text_xml.ToString();

              // elaboro l'xml
              if (!string.IsNullOrEmpty(paste_xml)) {

                // elenco ids da controllare
                List<string> ids = new List<string>();
                int ni = 0;
                while (true) {
                  int fi = paste_xml.IndexOf(" id=\"", ni);
                  if (fi < 0) break;
                  int start = fi + 5, end = paste_xml.IndexOf("\"", start);
                  ni = start;
                  if (end < 0) break;
                  ids.Add(paste_xml.Substring(start, end - start));
                }

                // ids da cancellare
                List<string> ids_to_del = new List<string>();
                foreach (string val in ids) {
                  string kp = val.Split(new char[] { ':' })[1];
                  int id = int.Parse(val.Split(new char[] { ':' })[0]);

                  // se c'è già l'id
                  if (doc_xml.IndexOf("id=\"" + id.ToString() + ":") >= 0)
                    ids_to_del.Add(val);
                  else {
                    DataRow dr = db_conn.first_row(core.parse(config.get_query("elements.exists-element").text
                    , new Dictionary<string, object>() { { "id_element", id } }));
                    if (kp != key_page && dr != null) ids_to_del.Add(val);
                    else if (dr == null) ids_to_del.Add(val);
                  }
                }

                foreach (string id_del in ids_to_del)
                  paste_xml = paste_xml.Replace("id=\"" + id_del + "\"", "from_id=\"" + id_del + "\"");
              }
              res.contents = paste_xml;
            }  // cut, copy
            else if (act == "cut" || act == "copy" || act == "copy_end" || act == "cut_end") {
              long id_cpy = long.Parse(jo["id"].ToString());

              List<long> ids_cpy = act == "copy_end" || act == "cut_end" ? (_e.next_elements_to(id_cpy,
                new List<element.type_element>() { element.type_element.title, element.type_element.list, element.type_element.attivita, element.type_element.element }))
                : new List<long>() { id_cpy };

              string ids = get_cache_var("cache_ids"), ids2 = "";
              foreach (long id in ids_cpy) {
                List<long> cids = _e.get_child_elements_ids(id);
                List<long> pids = _e.get_parent_elements_ids(id);
                string code = id.ToString() + "-" + (act == "cut" || act == "cut_end" ? "ct" : "cp");
                foreach (string i in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                  string el = i.Split(new char[] { '-' })[0], tp = i.Split(new char[] { '-' })[1];
                  if (el == id.ToString() || cids.Contains(long.Parse(el))
                    || pids.Contains(long.Parse(el))) continue;
                  ids2 += (ids2 != "" ? "," : "") + i;
                }
                ids2 += (ids2 != "" ? "," : "") + code;
              }

              set_cache_var("cache_ids", ids2);
              res.contents = ids2;
            }
              // reset_cache_ids
            else if (act == "reset_cache_ids") {
              reset_cache_var("cache_ids");
            }
              // move_up
            else if (act == "move_up") {
              long id = long.Parse(jo["id"].ToString());
              DataTable dt = db_conn.dt_table(string.Format(@"select top 2 ec.* from elements_contents ec 
               join [elements] e on e.element_id = ec.child_element_id and isnull(e.deleted, 0) = 0
               where ec.child_element_id = {0} 
                or (ec.element_id = (select top 1 element_id from elements_contents where child_element_id = {0})
                  and [order] < (select top 1 ec2.[order] from elements_contents ec2 
                    join [elements] e2 on e2.element_id = ec2.child_element_id and isnull(e2.deleted, 0) = 0
                    where ec2.child_element_id = {0}))
               order by [order] desc", id));
              if (dt.Rows.Count == 2) {
                long id_1 = db_provider.long_val(dt.Rows[0]["elements_contents_id"])
                  , id_2 = db_provider.long_val(dt.Rows[1]["elements_contents_id"]);
                int order_1 = db_provider.int_val(dt.Rows[0]["order"])
                  , order_2 = db_provider.int_val(dt.Rows[1]["order"]);
                db_conn.exec(string.Format(@"update elements_contents set [order] = {1} where elements_contents_id = {0};
                  update elements_contents set [order] = {3} where elements_contents_id = {2};"
                  , id_1, order_2, id_2, order_1));
                res.contents = "1";
              }
            }
              // move_end
            else if (act == "move_end") {
              long id = long.Parse(jo["id"].ToString());
              DataTable dt = db_conn.dt_table(string.Format(@"select top 2 ec.* from elements_contents ec 
               join [elements] e on e.element_id = ec.child_element_id and isnull(e.deleted, 0) = 0
               where ec.child_element_id = {0}
                or (ec.element_id = (select top 1 element_id from elements_contents where child_element_id = {0})
                  and [order] = (select max(ec2.[order]) 
					          from elements_contents ec2 
					          join [elements] e2 on e2.element_id = ec2.child_element_id and isnull(e2.deleted, 0) = 0
					          where ec2.element_id = ec.element_id))
               order by [order]", id));
              if (dt.Rows.Count == 2) {
                long element_id = db_provider.long_val(dt.Rows[0]["element_id"])
                  , id_1 = db_provider.long_val(dt.Rows[0]["elements_contents_id"])
                  , id_2 = db_provider.long_val(dt.Rows[1]["elements_contents_id"]);
                int order_1 = db_provider.int_val(dt.Rows[0]["order"])
                  , order_2 = db_provider.int_val(dt.Rows[1]["order"]);
                db_conn.exec(string.Format(@"update elements_contents set [order] = [order] - 1 
                    where element_id = {0} and [order] > {1};
                  update elements_contents set [order] = {2} where elements_contents_id = {3};"
                  , element_id, order_1, order_2, id_1));
                res.contents = "1";
              }
            }
              // paste_after
            else if (act == "paste_after" || act == "paste_before" || act == "paste_inside") {
              db_conn.begin_trans();
              try {
                List<attribute.attribute_type> attrs = Enum.GetValues(typeof(attribute.attribute_type)).Cast<attribute.attribute_type>().ToList();
                long idref = long.Parse(jo["id"].ToString());
                string ids = check_cache(false);
                foreach (string i in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                  long id = long.Parse(i.Split(new char[] { '-' })[0]);
                  string tp = i.Split(new char[] { '-' })[1];

                  // copy element
                  if (tp == "cp") {
                    element elc = _e.load_element(id, true);
                    elc.reset_ids();
                    _e.save_element(elc);
                    id = elc.id;
                  }

                  // cut e copy position
                  if (act == "paste_after") {
                    db_conn.exec(string.Format(@"update elements_contents set [order] = [order] + 1
                        where element_id = (select top 1 element_id from elements_contents where child_element_id = {0})
                          and [order] > (select top 1 [order] from elements_contents where child_element_id = {0});
                      delete from elements_contents where child_element_id = {1};
                      insert into elements_contents (element_id, child_element_id, [order])
                       select top 1 element_id, {1} as child_element_id, [order] + 1 as [order]
                        from elements_contents where child_element_id = {0};", idref, id));
                    idref = id;
                  } else if (act == "paste_before") {
                    db_conn.exec(string.Format(@"update elements_contents set [order] = [order] + 1
                        where element_id = (select top 1 element_id from elements_contents where child_element_id = {0})
                          and [order] >= (select top 1 [order] from elements_contents where child_element_id = {0});
                      delete from elements_contents where child_element_id = {1};
                      insert into elements_contents (element_id, child_element_id, [order])
                       select top 1 element_id, {1} as child_element_id, [order] - 1 as [order]
                        from elements_contents where child_element_id = {0};", idref, id));
                  } else if (act == "paste_inside") {
                    db_conn.exec(string.Format(@"delete from elements_contents where child_element_id = {1};
                      insert into elements_contents (element_id, child_element_id, [order])
                       select t.element_id, {1} as child_element_id, t.new_order
                        from (select {0} as element_id, isnull(max(ec.[order]) + 1, 0) as new_order 
                         from elements_contents ec 
                         where ec.element_id = {0}
                           and not exists (select top 1 1 from elements 
                            where element_id = ec.child_element_id and isnull(deleted, 0) > 0)) t", idref, id));
                  }
                }

                bool there_cut = false;
                foreach (string i in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                  if (i.Split(new char[] { '-' })[1] == "ct") { there_cut = true; break; }
                }
                _e.refresh_order_child(idref, db_conn);
                if (there_cut) { reset_cache_var("cache_ids"); res.contents = ""; } else res.contents = ids;
                db_conn.commit();
              } catch (Exception ex) { db_conn.rollback(); throw ex; }
            }
          } else throw new Exception("nessun dato da elaborare!");

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        Response.Clear();
        Response.ContentType = "application/json";
        Response.Write(JsonConvert.SerializeObject(res));
        Response.Flush();
        Response.SuppressContent = true;
        HttpContext.Current.ApplicationInstance.CompleteRequest();

        return;
      }

      // check cmd
      if (_cmd == null) return;

      // cache_ids, scroll_pos
      Dictionary<string, string> vars = get_cache_vars("cache_ids,_sc");
      if (vars.ContainsKey("cache_ids")) cache_ids.Value = vars["cache_ids"];
      if (vars.ContainsKey("_sc")) { scroll_pos.Value = vars["_sc"]; reset_cache_var("_sc"); }

      // view
      if (_cmd.action == "view" && (_cmd.obj == "elements" || _cmd.obj == "element")) {

        string element_id = _cmd.obj == "element" ? _cmd.sub_cmd("id") : ""; int max_lvl_els = 0, max_level = 0;
        if (_cmd.obj == "element" && element_id == "") throw new Exception("devi specificare l'id dell'elemento!");

        List<element> els = _e.load_elements(out max_lvl_els, out max_level, element_id != "" ? int.Parse(element_id) : (int?)null, _max_level);

        // client vars
        url_xml.Value = master.url_cmd("xml element" + (element_id != "" ? " id:" + element_id : "s"));
        url_xml_clean.Value = master.url_cmd("xml element");
        max_lvl.Value = max_lvl_els.ToString();

        // document
        StringBuilder sb = new StringBuilder();
        parse_elements(els, sb);

        contents_doc.InnerHtml = sb.ToString();
        contents_xml.Visible = false;
        sb.Clear();

        // menu
        parse_menu(element_id, els, sb, false, max_lvl_els);
        menu.InnerHtml = sb.ToString();
      }
        // xml
      else if (_cmd.action == "xml" && (_cmd.obj == "elements" || _cmd.obj == "element")) {

        string element_id = _cmd.obj == "element" ? _cmd.sub_cmd("id") : "", kp = strings.random_hex(4); int max_level_els = 0, max_level = 0;
        if (_cmd.obj == "element" && element_id == "") throw new Exception("devi specificare l'id dell'elemento!");

        List<element> els = _e.load_elements(out max_level_els, out max_level, element_id != "" ? int.Parse(element_id) : (int?)null, _max_level, key_page: kp);

        // client vars
        url_view.Value = master.url_cmd("view element" + (element_id != "" ? " id:" + element_id : "s"));
        id_element.Value = element_id.ToString();
        parent_id.Value = els.Count > 0 ? els[0].parent_id.ToString() : "";
        first_order.Value = els.Count > 0 ? els[0].order.ToString() : "";
        last_order.Value = els.Count > 0 ? els[els.Count - 1].order.ToString() : "";
        max_lvl.Value = max_level_els.ToString();
        key_page.Value = kp;

        // xml document
        contents_xml.Visible = true;
        contents.Visible = false;
        doc_xml.Value = doc_xml_bck.Value = element.get_doc(els).root_node.inner_xml;

        // menu
        StringBuilder sb = new StringBuilder();
        parse_menu(element_id, els, sb, true, max_level_els);
        menu.InnerHtml = sb.ToString();
      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) {
      log.log_err(ex);
      if (!_post) master.err_txt(ex.Message);
    }
  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

  #region cache

  protected string check_cache(bool save = true) {
    string cache_ids = get_cache_var("cache_ids");
    if (string.IsNullOrEmpty(cache_ids)) return "";

    List<long> ids = _e.check_exists_elements(
      cache_ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(i => long.Parse(i.Split(new char[] { '-' })[0])).ToList());

    string ids2 = ""; bool modified = false;
    foreach (string i in cache_ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
      string el = i.Split(new char[] { '-' })[0], tp = i.Split(new char[] { '-' })[1];
      if (!ids.Contains(long.Parse(el))) { modified = true; continue; }
      ids2 += (ids2 != "" ? "," : "") + i;
    }
    if (modified && save) set_cache_var("cache_ids", ids2);
    return ids2;
  }

  #endregion

  #region menu

  protected void parse_menu(string active_element_id, List<element> els, StringBuilder sb, bool for_xml = false, int? max_level = null) {
    foreach (element el in els.Where(x => x.type == element.type_element.element
      || x.type == element.type_element.list || x.type == element.type_element.title))
      parse_menu(active_element_id, el, sb, true, for_xml, max_level);
  }

  protected void parse_menu(string active_element_id, element e, StringBuilder sb, bool root = false, bool for_xml = false, int? max_level = null) {
    if (e.get_attribute_bool("closed")) return;
    if (root) sb.AppendFormat("<ul menu_childs_id='" + e.id.ToString() + "' class='nav flex-column' style='padding:0px;margin-top:0px;' >\n");

    string reference = get_ref(e.get_attribute_string("ref"))
      , title = e.type == element.type_element.title && e.has_content ? e.content 
        : (e.has_title ? e.title : (reference != "" ? reference : "[senza titolo]"));
    if (title != "") {
      parse_title_menu(e, title, sb
        , open_element_id: (e.level == _max_level + 1 && e.has_childs && !e.in_list ? e.id : 0)
        , back_element_id: e.back_element_id.HasValue && active_element_id != "" ? e.back_element_id.Value : 0, for_xml: for_xml);
    }

    bool first = true;
    foreach (element ec in e.childs) {
      if (ec.type == element.type_element.title) {
        if (first && e.level >= 1) { sb.Append("<ul menu_childs_id='" + e.id.ToString() + "'>"); first = false; }
        parse_title_menu(ec, ec.has_content ? ec.content : "[senza titolo]", sb, for_xml: for_xml);
      } else if (ec.type == element.type_element.element || ec.type == element.type_element.list) {
        if (first && e.level >= 1) { sb.Append("<ul menu_childs_id='" + e.id.ToString() + "'>"); first = false; }
        parse_menu(active_element_id, ec, sb, false, for_xml, max_level);
      }
    }

    sb.AppendFormat("{0}\n", !first ? "</ul>" : "");
    if (root) { sb.AppendFormat("</ul>\n"); }
  }

  protected void parse_title_menu(element e, string title, StringBuilder sb, bool close = true
    , long open_element_id = 0, long back_element_id = 0, bool for_xml = false) {

    int level = e.level;
    string ref_id = "ele_" + e.id.ToString(), reference = e.get_attribute_string("ref")
      , rf = !for_xml ? "#" + ref_id : "javascript:got_to_id(" + e.id.ToString() + ")";

    sb.AppendFormat(@"<li menu_id='{8}' style='{7}'><div style='display:block;'>{6}<a {3} style='{4}' href='{1}'>{0}{5}</a></div>{2}"
      , (e.type == element.type_element.element ? "<img src='images/tr-gray-10.png' style='margin-right:3px;'>" : "") + title
      , rf, close ? "</li>" : "", e.level == 0 ? "class='h5'" : ""
      , level == 0 ? "color:steelblue;"
        : (level == 1 ? "color:skyblue;margin-top:10px;padding-top:5px;padding-left:3px;" : "font-size:95%;color:lightcyan;")
      , open_element_id > 0 ? "<a style='margin-left:15px;' href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + open_element_id.ToString()) + "\">"
        + "<img src='images/right-arrow-16.png' style='margin-bottom:4px;'></a>" : ""
      , back_element_id != 0 ? (back_element_id > 0 ? "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + back_element_id.ToString())
         : "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " elements"))
        + "\" style='margin-right:10px;'><img src='images/left-chevron-24.png' style='margin-left:3px;margin-bottom:4px;' width='20' height='20'></a>" : ""
      , "", e.id);
  }

  #endregion

  #region document

  protected void parse_elements(List<element> els, StringBuilder sb) {
    els.ForEach(e => { parse_element_base(e, sb); });
  }

  protected void parse_element(element e, StringBuilder sb, bool in_list = false) {
    
    if ((e.has_title || e.get_attribute_string("ref") != "") && e.type == element.type_element.element)
      parse_type_element(e, sb, in_list);

    if (!e.sham) {
      if (e.childs.Count > 0) {
        string cls = _is_mobile ? "-mobile" : "";
        if (in_list) sb.AppendFormat("<ul childs_element_id='{0}' class='no-bullet{1}'>", e.id, cls);
        else sb.AppendFormat("<div childs_element_id='{0}' class='childs-element{1}'>", e.id, cls);
        foreach (element ec in e.childs) {
          if (ec.type == element.type_element.list)
            parse_type_list(ec, sb, in_list);
          else if (ec.type == element.type_element.attivita)
            parse_type_attivita(ec, sb, in_list);
          else {
            if (in_list) sb.AppendFormat(@"<li {1} contenitor_id='{0}'>"
              , ec.id, (ec.type != element.type_element.element ? "class='bullet" + bullet_i(ec) : "class='nobullet") + cls + "'");
            parse_element_base(ec, sb, in_list);
            if (in_list) sb.AppendFormat(@"</li>");
          }
        }
        if (in_list) sb.Append("</ul>"); else sb.Append("</div>");
      }
    }
  }

  protected string parse_element_base(element e, bool in_list = false) {
    StringBuilder sb = new StringBuilder();
    parse_element_base(e, sb, in_list);
    return sb.ToString();
  }

  protected void parse_element_base(element e, StringBuilder sb, bool in_list = false) {
    if (e == null) return;
    if (e.type == element.type_element.title) parse_type_title(e, sb, in_list);
    else if (e.type == element.type_element.text) parse_type_text(e, sb, in_list);
    else if (e.type == element.type_element.element) parse_element(e, sb, in_list);
    else if (e.type == element.type_element.list) parse_type_list(e, sb, in_list);
    else if (e.type == element.type_element.account) parse_type_account(e, sb, in_list);
    else if (e.type == element.type_element.value) parse_type_value(e, sb, in_list);
    else if (e.type == element.type_element.link) parse_type_link(e, sb, in_list);
    else if (e.type == element.type_element.attivita) parse_type_attivita(e, sb, in_list);
  }



  protected void parse_type_list(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.list) throw new Exception("elemento di tipo errato!");

    string ref_id = "ele_" + e.id.ToString(), a = string.Format("<a id='{0}' class='anchor'></a>", ref_id);
    bool closed = e.get_attribute_bool("closed");

    // head
    if (e.has_title) {
      string tag = e.level <= 1 ? "h4" : "h5", style = (e.level <= 1 ? "margin-bottom:5px;" : "margin-bottom:0px;")
        + (closed ? "color:gray;" : ""), html_notes = e.get_attribute_string("notes") != "" ? "<small style='margin-left:10px;'>" + e.get_attribute_string("notes") + "</small>" : "";

      // su - &#x25B2; giu - &#x25BC;
      string tr = closed ? string.Format("<span open_id='{0}' style='border:1pt solid lightgray;border-radius:18px;"
        + "display:inline-block;width:18px;height:18px;margin-left:10px;color:lightgray;cursor:pointer;"
        + "font-size:9pt;{1}' onclick='show_childs({0})'>&#x25BC;</span>", e.id
        , !_is_mobile ? "padding-bottom:2px;padding-left:2px;" : "padding-top:2px;padding-left:3px;") : "";

      string sym = e.level < 1 ? "<img src='images/sq-16.png' style='margin-right:3px;'></img>"
        : (e.level <= 2 ? "<img src='images/sq-14.png' style='margin-right:3px;'></img>"
          : "<img src='images/sq-12.png' style='margin-right:3px;'></img>"); 
      
      if (!in_list) sb.AppendFormat(@"<{3} element_id='{1}' type_element='list' title_element=""{8}"" style='{4}'>{2}{7}{0}{5}{6}</{3}>"
        , e.title, e.id, a, tag, style, tr, html_notes, sym, e.des);
      else sb.AppendFormat(@"<li class='square{5}' contenitor_id='{1}' style='overflow-wrap:break-word;'><{3} element_id='{1}' type_element='list' title_element=""{8}"" style='{4}'>{2}{0}{6}{7}</{3}></li>"
        , e.title, e.id, a, tag, style, bullet_i(e) + (_is_mobile ? "-mobile" : ""), tr, html_notes, e.des);
    }

    // childs
    if (e.childs.Count > 0) {
      string cls = _is_mobile ? "-mobile" : "";
      sb.AppendFormat(@"<div childs_element_id='{0}' {2}><ul class='no-bullet{1}'><div id='virtual_{0}'></div>"
        , e.id, cls, closed ? "style='display:none'" : "");
      foreach (element ec in e.childs) {
        if (ec.type == element.type_element.list)
          parse_type_list(ec, sb, true);
        else if (ec.type == element.type_element.attivita)
          parse_type_attivita(ec, sb, true);
        else {
          bool inline = e.get_attribute_string("style") == "inline";
          string style = inline && (ec.type == element.type_element.account || ec.type == element.type_element.value
            || ec.type == element.type_element.link) ? (!_is_mobile ? "display:inline-block;margin-left:10px;" : "display:inline-block;margin-left:5px;") : "";
          sb.AppendFormat(@"<li {1} style='{2}overflow-wrap:break-word;' contenitor_id='{0}'>"
            , ec.id, (ec.type != element.type_element.element ? "class='bullet" + bullet_i(ec) : "class='nobullet") + cls + "'", style);
          parse_element_base(ec, sb, true);
          sb.AppendFormat(@"</li>");
        }
      }
      sb.AppendFormat(@"</ul></div>");
    }

  }

  protected string image_attivita(string stato) {
    string img = stato == "la prossima" ? "circle-pr-16.png" : (stato == "in corso" ? "circle-ic-16.png" : (stato == "fatta" ? "circle-ft-16.png"
      : (stato == "sospesa" ? "circle-sp-16.png" : "circle-df-16.png")));
    return "images/" + img;
  }

  protected void parse_type_attivita(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.attivita) throw new Exception("elemento di tipo errato!");

    // head
    string title = e.has_title ? e.title : "[senza titolo]"
      , priorita = e.get_attribute_string("priorita"), stato = e.get_attribute_string("stato")
      , dt_upd = e.dt_upd.HasValue ? " - " + e.dt_upd.Value.ToString("dddd dd MMMM yyyy") : ""
      , dt_ins = e.dt_ins.HasValue ? " - " + e.dt_ins.Value.ToString("dddd dd MMMM yyyy") : "";

    string cl = stato == "la prossima" ? "dark" : (stato == "fatta" ? "success" : (stato == "sospesa" ? "secondary"
      : (stato == "in corso" ? "warning" : (priorita == "alta" ? "danger" : (priorita == "bassa" ? "light" : "primary")))));

    sb.AppendFormat("<div element_id='{3}' type_element='attivita' title_element=\"{9}\" stato='{6}' priorita='{7}' in_list='{8}' style='margin-top:3px;margin-bottom:3px;'>{4}"
     + "<h4 style=\"background: url('{0}') no-repeat 2px 6px;padding-left:22px;display:inline-block;margin:0px;cursor:pointer;\">"
     + "<span clickable='true' style=\"white-space:normal;overflow-wrap:break-word;font-weight:normal;\" class='badge badge-{2}'>{1}</span></h4>{5}</div>"
     , image_attivita(stato), title + (stato == "la prossima" ? " - LA PROSSIMA" + dt_upd : (stato == "in corso" ? " - IN CORSO" + dt_upd
        : (stato == "sospesa" ? " - SOSPESA" + dt_upd : (stato == "fatta" ? " - FATTA" + dt_upd : dt_ins))))
        + (priorita == "alta" ? " - ALTA PRIORITÀ" : (priorita == "bassa" ? " - BASSA PRIORITÀ" : ""))
      , cl, e.id, in_list ? "<li>" : "", in_list ? "</li>" : "", stato, priorita, in_list ? "true" : "false", e.des);

    // childs
    if (e.childs.Count > 0) {
      string cls = _is_mobile ? "-mobile" : "";
      sb.AppendFormat(@"<div childs_element_id='{0}'><ul class='no-bullet{1}'><div id='virtual_{0}'></div>", e.id, cls);
      foreach (element ec in e.childs) {
        if (ec.type == element.type_element.list)
          parse_type_list(ec, sb, true);
        else if (ec.type == element.type_element.attivita)
          parse_type_attivita(ec, sb, true);
        else {
          sb.AppendFormat(@"<li {1} contenitor_id='{0}'>"
            , ec.id, (ec.type != element.type_element.element ? "class='bullet" + bullet_i(ec) : "class='nobullet") + cls + "'");
          parse_element_base(ec, sb, true);
          sb.AppendFormat(@"</li>");
        }
      }
      sb.AppendFormat(@"</ul></div>");
    }
  }

  protected string bullet_i(element e) { return e.level <= 1 ? "" : (e.level == 2 ? "-2" : "-3"); }

  protected string image_bullet(element e) { return "images/circle-8" + bullet_i(e) + ".png"; }

  protected void parse_type_account(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.account) throw new Exception("elemento di tipo errato!");

    string title = e.has_title ? e.title : "account", user = e.get_attribute_string("user")
      , pass = e.get_attribute_string("password"), notes = e.get_attribute_string("notes")
      , email = e.get_attribute_string("email");
    sb.AppendFormat(@"<span element_id='{5}' type_element='account' title_element=""{6}"" style='overflow-wrap:break-word;' class='lead'>{4}{0}{1}{2}{3}</span>"
      , !string.IsNullOrEmpty(title) ? "<b>" + title + ": </b>" : ""
      , user != "" || email != "" ? email + (email != "" && user != "" ? " - " : "") + user : "", pass != "" ? "/" + pass : ""
      , notes != "" ? "<span style='font-style:italic;margin-left:5px;color:darkgray;'>(" + notes + ")</span>" : ""
      , !in_list ? "<img src='" + image_bullet(e) + "' style='padding-left:3px;padding-right:3px;'></img>" : "", e.id, e.des);
  }

  protected void parse_type_value(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.value) throw new Exception("elemento di tipo errato!");

    string title = e.title, content = e.get_attribute_string("content"), notes = e.get_attribute_string("notes");

    sb.AppendFormat(@"<span element_id='{4}' type_element='value' title_element=""{5}"" style='overflow-wrap:break-word;' class='lead'>{3}{0}{1}{2}</span>"
      , !string.IsNullOrEmpty(title) ? "<b>" + title + ": </b>" : ""
      , content, notes != "" ? "<span style='font-style:italic;margin-left:5px;color:darkgray;'>(" + notes + ")</span>" : ""
      , !in_list ? "<img src='" + image_bullet(e) + "' style='padding-left:3px;padding-right:3px;'></img>" : "", e.id, e.des);
  }

  protected void parse_type_link(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.link) throw new Exception("elemento di tipo errato!");

    string title = e.title, reference = get_ref(e.get_attribute_string("ref")), notes = e.get_attribute_string("notes");
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));

    sb.AppendFormat("<div element_id='{5}' type_element='link' title_element=\"{6}\" style='overflow-wrap:break-word;'><a class='lead' href=\"{0}\" style='overflow-wrap:break-word;' {3}>{4}{1}</a>{2}</div>"
      , reference, !string.IsNullOrEmpty(title) ? title : reference
      , notes != "" ? "<span class='lead' style='font-style:italic;margin-left:5px;color:darkgray;'>(" + notes + ")</span>" : ""
      , !title_ref_cmd ? "target='blank'" : "", !in_list ? "<img src='" + image_bullet(e) + "' style='padding-left:3px;padding-right:3px;'></img>" : "", e.id, e.des);
  }

  protected void parse_type_title(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.title) throw new Exception("elemento di tipo errato!");

    int level = !in_list ? e.level : 5;
    string ref_id = "ele_" + e.id.ToString(), title = e.has_content ? e.content : "<titolo>"
      , reference = get_ref(e.get_attribute_string("ref"));
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));
    string a = string.Format("<a id='{0}' class='anchor'></a>", ref_id)
      , st = (level == 1 ? "style='color:dimgray;margin-top:15px;padding-top:15px;margin-bottom:5px;padding-bottom:0px;'"
      : (level >= 2 ? "style='color:dimgray;margin-bottom:5px;padding-bottom:0px;'" : "style='color:dimgray;margin-bottom:5px;'"));
    string tag = level == 0 ? "h3" : (level == 1 ? "h3" : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5")));
    if (reference != "")
      sb.AppendFormat("<div element_id='{6}' type_element='title' title_element=\"{7}\" style='overflow-wrap:break-word;'>{5}<{0} {4}><a href='{2}' {3}>{1}</a></{0}></div>"
        , tag, title, reference, !title_ref_cmd ? "target='blank'" : "", st, a, e.id, e.des);
    else
      sb.AppendFormat("<div element_id='{4}' type_element='title' title_element=\"{5}\" style='overflow-wrap:break-word;'>{3}<{0} {2}><span>{1}</span></{0}></div>"
        , tag, title, st, a, e.id, e.des);
  }

  protected bool reference_cmd(string reference) { return reference.StartsWith("{cmdurl='"); }
  //protected string value_ref(string reference) { return get_ref(reference); }
  protected string get_ref(string ref_url) {
    return ref_url.StartsWith("{cmdurl='") ? _core.config.var_value_par("vars.router-cmd"
      , ref_url.Substring(9, ref_url.Length - 11)) : ref_url;
  }

  protected void parse_type_element(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.element) throw new Exception("elemento di tipo errato!");

    int level = !in_list ? e.level : 5;
    string code = e.get_attribute_string("code"), type = e.get_attribute_string("type")
      , reference = get_ref(e.get_attribute_string("ref")), title = e.has_title ? e.title : (reference != "" ? reference : "[senza titolo]")
      , notes = e.get_attribute_string("notes");
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));
    string a = string.Format("<a id='{0}' class='anchor'></a>", "ele_" + e.id.ToString())
      , st = level == 0 ? "style='margin-bottom:5px;padding-bottom:5px;padding-top:10px;background-color:white;color:steelblue;'"
       : (level == 1 ? "style='background-color:whitesmoke;border-top:1pt solid lightgrey;margin-top:15px;padding-top:5px;margin-bottom:5px;padding-bottom:5px;'"
       : "style='margin-bottom:5px;padding-bottom:0px;background-color:/whitesmoke;'");
    string open = e.sham ? "<a style='margin-left:20px;' href=\"" + get_url_cmd("view element id:" + e.id.ToString()) + "\"><img src='images/right-arrow-24-black.png' style='margin-bottom:4px;'></a>" : "";

    string small = (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(type)) && (e.sham || in_list) ?
      string.Format("<small>{0}{1}</small>", !string.IsNullOrEmpty(code) ? " code: " + code : ""
        , !string.IsNullOrEmpty(type) ? " type: " + type : "") : "";

    string sym = level < 1 ? "<img src='images/tr-16.png' style='margin-right:3px;'></img>"
      : (level <= 2 ? "<img src='images/tr-14.png' style='margin-right:3px;'></img>"
        : "<img src='images/tr-12.png' style='margin-right:3px;'></img>");

    string tag = level == 0 ? "h1" : (level == 1 ? "h2" : (level == 2 ? "h3" : (level == 3 ? "h3" : "h4")));
    if (reference != "")
      sb.AppendFormat("<div element_id='{8}' type_element='element' title_element=\"{12}\" style='overflow-wrap:break-word;{9}'>{5}<{0} {4}><a href='{2}' {3}>{7}{1}</a>{6}{10}{11}</{0}>"
        , tag, title, reference, !title_ref_cmd ? "target='blank'" : "", st, a, small, sym, e.id, level == 0 ? "border-top:1pt solid lightgray;margin-bottom:30px;" : ""
        , notes != "" ? "<small>  " + notes + "</small>" : "", open, e.des);
    else
      sb.AppendFormat("<div element_id='{7}' type_element='element' title_element=\"{10}\" style='overflow-wrap:break-word;{8}'>{3}<{0} {2}>{6}<span>{1}</span>{5}{9}{4}</{0}>"
        , tag, title, st, a, open, small, sym, e.id, level == 0 ? "border-top:1pt solid lightgray;margin-bottom:30px;" : ""
        , notes != "" ? "<small>  " + notes + "</small>" : "", e.des);

    if ((!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(type)) && !e.sham && !in_list) {
      sb.AppendFormat("<p class='lead' style='overflow-wrap:break-word;font-size:95%;color:darkgray;padding:0px;margin:0px;margin-bottom:15px;'>{0}{2}{1}</p>"
        , !string.IsNullOrEmpty(code) ? string.Format("code: <span style='font-weight:bold;'>{0}</span>", code) : ""
        , !string.IsNullOrEmpty(type) ? string.Format("type: <span style='font-weight:bold;'>{0}</span>", type) : ""
        , !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(type) ? ", " : "");
    }
    sb.AppendFormat("</div>");
  }

  public enum text_styles { underline, bold }
  protected text_styles[] get_styles(string styles) {
    if (string.IsNullOrEmpty(styles)) return new text_styles[] { };
    string ts = styles.Replace("][", ",").Replace("[", "").Replace("]", "");
    return ts.Split(new char[] { ',' }).Select(x => (text_styles)Enum.Parse(typeof(text_styles), x)).ToArray();
  }
  public bool is_style(string styles) { return !string.IsNullOrEmpty(styles); }

  protected void parse_type_text(element e, StringBuilder sb, bool in_list = false) {
    if (e.type != element.type_element.text) throw new Exception("elemento di tipo errato!");

    int level = e.level;
    string content = e.get_attribute_string("content"), el_style = e.get_attribute_string("style");

    string fs = level > 1 ? "" : "", style = (is_style(el_style) ? string.Join(""
      , get_styles(el_style).Select(s => s == text_styles.bold ? "font-weight:bold;"
        : (s == text_styles.underline ? "font-style:italic;" : ""))) : "") + fs;
    sb.AppendFormat("<p element_id='{4}' type_element='text' title_element=\"{5}\" class='lead' style='{2}padding:0px;style='overflow-wrap:break-word;{1}'>{3}{0}</p>"
      , content, style != "" ? style : "", !in_list ? "margin-bottom:10px;" : "margin-bottom:0px;"
      , !string.IsNullOrEmpty(e.title) ? "<span class='h5' style='margin-right:10px;'>" + e.title + "</span>" : "", e.id, e.des);
  }

  #endregion
}