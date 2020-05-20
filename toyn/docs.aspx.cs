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
using toyn;

public partial class _docs : tl_page {

  protected int _max_level = 3;
  protected cmd _cmd = null;
  protected bool _view_stored = false;
  protected string _element_id = "";

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));
    _element_id = _cmd.obj == "element" ? _cmd.sub_cmd("id") : "";
    _view_stored = qry_val("vs") == "1";
    if (!json_request.there_request(this)) {

      // tolgo il parametro sc
      if (qry_val("sc") != "" && _cmd != null && _cmd.action == "view") {
        set_cache_var("_sc", qry_val("sc"));
        NameValueCollection pars = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        pars.Remove("sc");
        Response.Redirect(Request.Url.AbsolutePath + "?" + pars);
      }

    }

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {
      docs eb = new docs();

      if (json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {
          json_request jr = new json_request(this);

          // save_element
          if (jr.action == "save_element") {

            // carico l'xml e salvo tutto
            string element_id = jr.val_str("element_id"), key_page = jr.val_str("key_page");
            int parent_id = jr.val_int("parent_id"), max_level = jr.val_int("max_level");
            int? first_order = jr.val_int_null("first_order"), last_order = jr.val_int_null("last_order");
            List<element> els_bck = eb.load_xml(jr.val_str("xml_bck"), element_id != "" ? parent_id : (int?)null)
              , els = eb.load_xml(jr.val_str("xml"), element_id != "" ? parent_id : (int?)null);

            // parents
            bool parent_stored;
            List<element> pels = parents_elements(eb, element_id, out parent_stored);

            // salvataggio
            eb.check_and_del(els_bck, els);
            eb.save_elements(els, key_page, parent_id, first_order, last_order);
            check_cache(eb);
            eb.refresh_order_element(parent_id);

            // vedo se ci sono elementi figli
            int ml = element.max_level(els);
            foreach (element ec in element.find_elements(els, ml).Where(x => x.id > 0)) {
              DataRow dr = db_conn.first_row(core.parse_query("docs.has-childs"
                , new Dictionary<string, object>() { { "id_element", ec.id } }));
              ec.has_childs = db_provider.int_val(dr["has_childs"]) == 1;
              ec.has_child_elements = db_provider.int_val(dr["has_child_elements"]) == 1;
            }

            // xml
            res.doc_xml = element.get_doc(els, eb.load_types_elements()).xml;
            res.vars.Add("first_order", els.Count > 0 ? els[0].order.ToString() : "");
            res.vars.Add("last_order", els.Count > 0 ? els[els.Count - 1].order.ToString() : "");

            // menu
            res.menu_html = parse_menu_elements(els, true, max_level, parent_stored, true, element_id != "");

          } // save_code
          else if (jr.action == "save_code") {
            eb.set_attribute(element.type_element.code, jr.val_long("id"), "content", jr.val_str("code"));
          }
            // remove_element
          else if (jr.action == "remove_element") {
            // pulizia
            DataRow dr = db_conn.first_row(core.parse_query("docs.id-deleted"
              , new Dictionary<string, object>() { { "user_id", _user.id } }));
            int id_deleted = db_provider.int_val(dr["id_deleted"]);
            db_conn.exec(core.parse_query("docs.delete-element"
              , new Dictionary<string, object>() { { "id_element", jr.id }, { "user_id", _user.id }, { "id_deleted", id_deleted } }));
            eb.refresh_order_child(jr.id);
            res.add_var("cache_ids", check_cache(eb));

          }
            // store_element, unstore_element
          else if (jr.action == "store_element" || jr.action == "unstore_element") {
            bool in_list = jr.val_bool("in_list"), parent_stored = jr.val_bool("parent_stored")
              , no_opacity = jr.val_bool("no_opacity");

            if (jr.action == "store_element") eb.store_element(jr.id); else eb.unstore_element(jr.id);

            int max_lvl_els = 0, max_lvl = 0;
            List<element> els = eb.load_elements(out max_lvl_els, out max_lvl, _element_id != "" ? int.Parse(_element_id) : (int?)null, _max_level);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, in_list, parent_stored, no_opacity);
            res.menu_html = parse_menu_elements(els, false, max_lvl_els, view_stored: _view_stored, can_back: _element_id != "");

          } // change_stato_attivita
          else if (jr.action == "change_stato_attivita") {
            bool in_list = jr.val_bool("in_list");
            string stato = jr.val_str("stato_now"), stato_new = jr.val_str("stato_new")
              , new_stato = stato_new != "" ? stato_new : (stato == "" || stato == "da iniziare" ? "la prossima"
                : (stato == "la prossima" ? "in corso" : (stato == "in corso" ? "sospesa"
                  : (stato == "sospesa" ? "fatta" : (stato == "fatta" ? "da iniziare" : "fatta")))));

            eb.set_attribute(element.type_element.attivita, jr.id, "stato", new_stato);
            res.html_element = parse_element_base(eb.load_element(jr.id), in_list);

          } // change_priorita_attivita 
          else if (jr.action == "change_priorita_attivita") {
            bool in_list = jr.val_bool("in_list");
            string priorita = jr.val_str("priorita_now"), priorita_new = jr.val_str("priorita_new")
              , new_priorita = priorita_new != "" ? priorita_new
                : (priorita == "" || priorita == "normale" ? "alta" : (priorita == "alta" ? "bassa" : "normale"));

            eb.set_attribute(element.type_element.attivita, jr.id, "priorita", new_priorita);
            res.html_element = parse_element_base(eb.load_element(jr.id), in_list);

          } // check_paste_xml 
          else if (jr.action == "check_paste_xml") {
            StringBuilder text_xml = new StringBuilder();
            foreach (JValue j in jr.val_array("text_xml"))
              text_xml.AppendLine(j.Value.ToString());
            string key_page = jr.val_str("key_page"), doc_xml = jr.val_str("doc_xml"), paste_xml = text_xml.ToString();

            // elaboro l'xml
            if (!string.IsNullOrEmpty(paste_xml)) {

              // elenco ids da controllare
              List<string> ids = new List<string>();
              int ni = 0;
              while (true) {
                int fi = paste_xml.IndexOf(" _id=\"", ni);
                if (fi < 0) break;
                int start = fi + 6, end = paste_xml.IndexOf("\"", start);
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
                if (doc_xml.IndexOf("_id=\"" + id.ToString() + ":") >= 0)
                  ids_to_del.Add(val);
                else {
                  DataRow dr = db_conn.first_row(core.parse_query("docs.exists-element"
                  , new Dictionary<string, object>() { { "id_element", id } }));
                  if (kp != key_page && dr != null) ids_to_del.Add(val);
                  else if (dr == null) ids_to_del.Add(val);
                }
              }

              foreach (string id_del in ids_to_del)
                paste_xml = paste_xml.Replace("_id=\"" + id_del + "\"", "_from_id=\"" + id_del + "\"");
            }
            res.contents = paste_xml;
          }  // cut, copy
          else if (jr.action == "cut" || jr.action == "copy" || jr.action == "copy_end" || jr.action == "cut_end") {
            long id_cpy = jr.id;
            List<long> ids_cpy = jr.action == "copy_end" || jr.action == "cut_end" ? (eb.next_elements_to(id_cpy,
              new List<element.type_element>() { element.type_element.title, element.type_element.list, element.type_element.attivita, element.type_element.element }))
              : new List<long>() { id_cpy };

            string ids = get_cache_var("cache_ids"), ids2 = "";
            foreach (long id in ids_cpy) {
              List<long> cids = eb.get_child_elements_ids(id);
              List<long> pids = eb.get_parent_elements_ids(id);
              string code = id.ToString() + "-" + (jr.action == "cut" || jr.action == "cut_end" ? "ct" : "cp");
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
          else if (jr.action == "reset_cache_ids") {
            reset_cache_var("cache_ids");
          }
            // move_up
          else if (jr.action == "move_up") {
            DataTable dt = db_conn.dt_table(core.parse_query("docs.move-up"
              , new Dictionary<string, object>() { { "element_id", jr.id }
                , { "filter_stored_1", !_view_stored ? "and e.dt_stored is null" : "" }
                , { "filter_stored_2", !_view_stored ? "and e2.dt_stored is null" : "" } }));
            if (dt.Rows.Count == 2) {
              long id_1 = db_provider.long_val(dt.Rows[0]["elements_contents_id"])
                , id_2 = db_provider.long_val(dt.Rows[1]["elements_contents_id"]);
              int order_1 = db_provider.int_val(dt.Rows[0]["order"])
                , order_2 = db_provider.int_val(dt.Rows[1]["order"]);
              db_conn.exec(core.parse_query("docs.after-move-up"
                , new Dictionary<string, object>() { { "id_1", id_1 }, { "order_1", order_1 } 
                    , { "id_2", id_2 }, { "order_2", order_2 }}));
              res.contents = "1";

              int max_lvl_els = 0, max_lvl = 0;
              List<element> els = eb.load_elements(out max_lvl_els, out max_lvl
                , _element_id != "" ? int.Parse(_element_id) : (int?)null, _max_level);
              res.html_element = parse_elements_doc(els, view_stored: _view_stored);
              res.menu_html = parse_menu_elements(els, false, max_lvl_els, view_stored: _view_stored, can_back: _element_id != "");
            }
          }
            // move_end
          else if (jr.action == "move_end") {
            DataTable dt = db_conn.dt_table(core.parse_query("docs.move-end"
              , new Dictionary<string, object>() { { "element_id", jr.id } }));
            if (dt.Rows.Count == 2) {
              long element_id = db_provider.long_val(dt.Rows[0]["element_id"])
                , id_1 = db_provider.long_val(dt.Rows[0]["elements_contents_id"])
                , id_2 = db_provider.long_val(dt.Rows[1]["elements_contents_id"]);
              int order_1 = db_provider.int_val(dt.Rows[0]["order"])
                , order_2 = db_provider.int_val(dt.Rows[1]["order"]);
              db_conn.exec(core.parse_query("docs.after-move-end"
                , new Dictionary<string, object>() { { "element_id", element_id }, { "order_1", order_1 } 
                    , { "id_1", id_1 }, { "order_2", order_2 }}));
              res.contents = "1";

              int max_lvl_els = 0, max_lvl = 0;
              List<element> els = eb.load_elements(out max_lvl_els, out max_lvl
                , _element_id != "" ? int.Parse(_element_id) : (int?)null, _max_level);
              res.html_element = parse_elements_doc(els, view_stored: _view_stored);
              res.menu_html = parse_menu_elements(els, false, max_lvl_els, view_stored: _view_stored, can_back: _element_id != "");
            }
          }
            // move_first
          else if (jr.action == "move_first") {
            DataTable dt = db_conn.dt_table(core.parse_query("docs.move-first"
              , new Dictionary<string, object>() { { "element_id", jr.id } }));
            if (dt.Rows.Count == 2) {
              long element_id = db_provider.long_val(dt.Rows[0]["element_id"])
                , id_1 = db_provider.long_val(dt.Rows[0]["elements_contents_id"])
                , id_2 = db_provider.long_val(dt.Rows[1]["elements_contents_id"]);
              int order_1 = db_provider.int_val(dt.Rows[0]["order"])
                , order_2 = db_provider.int_val(dt.Rows[1]["order"]);
              db_conn.exec(core.parse_query("docs.after-move-first"
                , new Dictionary<string, object>() { { "element_id", element_id }, { "order_1", order_1 } 
                    , { "id_2", id_2 }, { "order_2", order_2 }}));
              res.contents = "1";

              int max_lvl_els = 0, max_lvl = 0;
              List<element> els = eb.load_elements(out max_lvl_els, out max_lvl
                , _element_id != "" ? int.Parse(_element_id) : (int?)null, _max_level);
              res.html_element = parse_elements_doc(els, view_stored: _view_stored);
              res.menu_html = parse_menu_elements(els, false, max_lvl_els, view_stored: _view_stored, can_back: _element_id != "");
            }
          }
            // paste_after
          else if (jr.action == "paste_after" || jr.action == "paste_before" || jr.action == "paste_inside") {
            db_conn.begin_trans();
            try {
              List<attribute.attribute_type> attrs = Enum.GetValues(typeof(attribute.attribute_type)).Cast<attribute.attribute_type>().ToList();
              long idref = jr.id;
              string ids = check_cache(eb, false);
              foreach (string i in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                long id = long.Parse(i.Split(new char[] { '-' })[0]);
                string tp = i.Split(new char[] { '-' })[1];

                // copy element
                if (tp == "cp") {
                  element elc = eb.load_element(id, true);
                  elc.reset_ids();
                  eb.save_element(elc, eb.load_types_elements());
                  id = elc.id;
                }

                // cut e copy position
                if (jr.action == "paste_after") {
                  db_conn.exec(core.parse_query("docs.paste-after"
                    , new Dictionary<string, object>() { { "ref_id", idref }, { "element_id", id } }));
                  idref = id;
                } else if (jr.action == "paste_before") {
                  db_conn.exec(core.parse_query("docs.paste-before"
                    , new Dictionary<string, object>() { { "ref_id", idref }, { "element_id", id } }));
                } else if (jr.action == "paste_inside") {
                  db_conn.exec(core.parse_query("docs.paste-inside"
                    , new Dictionary<string, object>() { { "ref_id", idref }, { "element_id", id } }));
                }
              }

              bool there_cut = false;
              foreach (string i in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                if (i.Split(new char[] { '-' })[1] == "ct") { there_cut = true; break; }
              }
              eb.refresh_order_child(idref, db_conn);
              if (there_cut) { reset_cache_var("cache_ids"); res.contents = ""; } else res.contents = ids;
              db_conn.commit();

              int max_lvl_els = 0, max_lvl = 0;
              List<element> els = eb.load_elements(out max_lvl_els, out max_lvl
                , _element_id != "" ? int.Parse(_element_id) : (int?)null, _max_level);
              res.html_element = parse_elements_doc(els, view_stored: _view_stored);
              res.menu_html = parse_menu_elements(els, false, max_lvl_els, view_stored: _view_stored, can_back: _element_id != "");

            } catch (Exception ex) { db_conn.rollback(); throw ex; }
          } // update_account
          else if (jr.action == "update_account") {
            string up = jr.val_str("user_pass"), user = "", pass = "";
            if (!string.IsNullOrEmpty(up)) {
              string[] upa = up.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
              user = upa[0]; pass = upa[1];
            }
            Dictionary<string, string> avalues = new Dictionary<string, string>() { 
              { "title", jr.val_str("title") }, { "notes", jr.val_str("notes") }
                , { "email", jr.val_str("email") }, { "user", user }, { "password", pass }};
            eb.save_element(eb.load_type_element(element.type_element.account, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList());
            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
          } // update_attivita
          else if (jr.action == "update_attivita") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { 
              { "title", jr.val_str("title") } };
            eb.save_element(eb.load_type_element(element.type_element.attivita, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList());
            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
          } // update_code  
          else if (jr.action == "update_code") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { 
              { "title", jr.val_str("title") }, { "notes", jr.val_str("notes") } };
            eb.save_element(eb.load_type_element(element.type_element.code, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList());
            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
          } // update_element
          else if (jr.action == "update_element") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { 
              { "title", jr.val_str("title") }, { "code", jr.val_str("code") }, { "type", jr.val_str("type") }
                , { "ref", jr.val_str("ref") }, { "notes", jr.val_str("notes") }};
            eb.save_element(eb.load_type_element(element.type_element.element, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList(), false);

            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored")
              , jr.val_bool("no_opacity"), true);
            if (_element_id != "" && el.id == long.Parse(_element_id)) el.back_element_id = el.parent_id;
            res.menu_html = parse_title_menu(el, ep, _max_level + 1, parent_stored: jr.val_bool("parent_stored"), can_back: _element_id != "");
          } // update_link
          else if (jr.action == "update_link") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { 
              { "title", jr.val_str("title") }, { "ref", jr.val_str("ref") }, { "notes", jr.val_str("notes") }};
            eb.save_element(eb.load_type_element(element.type_element.link, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList(), false);

            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
          } // update_list
          else if (jr.action == "update_list") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { 
              { "title", jr.val_str("title") }, { "notes", jr.val_str("notes") }};
            eb.save_element(eb.load_type_element(element.type_element.list, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList(), false);

            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
            if (_element_id != "" && el.id == long.Parse(_element_id)) el.back_element_id = el.parent_id;
            res.menu_html = parse_title_menu(el, ep, _max_level + 1, parent_stored: jr.val_bool("parent_stored"), can_back: _element_id != "");
          } // update_par
          else if (jr.action == "update_par") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { 
              { "title", jr.val_str("title") }, { "ref", jr.val_str("ref") }};
            eb.save_element(eb.load_type_element(element.type_element.par, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList(), false);

            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0, true);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
            if (_element_id != "" && el.id == long.Parse(_element_id)) el.back_element_id = el.parent_id;
            res.menu_html = parse_title_menu(el, ep, _max_level + 1, false, jr.val_bool("parent_stored"), can_back: _element_id != "");
          } // update_text
          else if (jr.action == "update_text") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { { "title", jr.val_str("title") }
              , { "style", jr.val_str("stile") }, { "content", jr.val_str("content") } };
            eb.save_element(eb.load_type_element(element.type_element.text, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList(), false);

            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
          } // update_title
          else if (jr.action == "update_title") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { { "content", jr.val_str("title") }
              , { "ref", jr.val_str("ref") } };
            eb.save_element(eb.load_type_element(element.type_element.title, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList(), false);

            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
            if (_element_id != "" && el.id == long.Parse(_element_id)) el.back_element_id = el.parent_id;
            res.menu_html = parse_title_menu(el, ep, _max_level + 1, parent_stored: jr.val_bool("parent_stored"), can_back: _element_id != "");
          } // update_value
          else if (jr.action == "update_value") {
            Dictionary<string, string> avalues = new Dictionary<string, string>() { { "title", jr.val_str("title") }
              , { "content", jr.val_str("content") }, { "notes", jr.val_str("notes") } };
            eb.save_element(eb.load_type_element(element.type_element.value, jr.id, attrs2: avalues)
              , eb.load_types_elements(), avalues.Keys.ToList(), false);

            List<element> els = eb.load_parents_elements(jr.id, _element_id != "" ? long.Parse(_element_id) : 0);
            element el = eb.find_element(jr.id, els), ep = el.parent_id > 0 ? eb.find_element(el.parent_id, els) : null;
            res.html_element = parse_edited_element(el, ep, jr.val_bool("in_list"), jr.val_bool("parent_stored"), jr.val_bool("no_opacity"), true);
          }

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);
        return;
      }

      // check cmd
      if (_cmd == null) return;
      if (_cmd.obj == "element" && _element_id == "") throw new Exception("devi specificare l'id dell'elemento!");

      // cache_ids, scroll_pos
      Dictionary<string, string> vars = get_cache_vars("cache_ids,_sc");
      if (vars.ContainsKey("cache_ids")) cache_ids.Value = vars["cache_ids"];
      if (vars.ContainsKey("_sc")) { scroll_pos.Value = vars["_sc"]; reset_cache_var("_sc"); }

      // view
      if (_cmd.action == "view" && (_cmd.obj == "elements" || _cmd.obj == "element")) {

        int max_lvl_els = 0, max_level = 0;

        // parents
        bool parent_stored;
        List<element> pels = parents_elements(eb, _element_id, out parent_stored);

        // document
        List<element> els = eb.load_elements(out max_lvl_els, out max_level, _element_id != "" ? int.Parse(_element_id) : (int?)null, _max_level);
        if (!_view_stored && els.FirstOrDefault(x => !x.dt_stored.HasValue) == null) _view_stored = true;
        contents_doc.InnerHtml = parse_elements_doc(els, parent_stored, _view_stored);
        contents.Attributes["sidebar-tp"] = "body";
        contents_xml.Visible = false;

        // client vars
        url_xml.Value = master.url_cmd("xml element" + (_element_id != "" ? " id:" + _element_id : "s"));
        url_xml_clean.Value = master.url_cmd("xml element");
        parent_id.Value = els.Count > 0 ? els[0].parent_id.ToString() : "";
        max_lvl.Value = max_lvl_els.ToString();
        there_stored.Value = element.find_stored(els) ? "1" : "";

        // menu
        menu.InnerHtml = parse_menu_elements(els, false, max_lvl_els, parent_stored, _view_stored, can_back: _element_id != "");
      }
        // xml
      else if (_cmd.action == "xml" && (_cmd.obj == "elements" || _cmd.obj == "element")) {

        string kp = strings.random_hex(4); int max_level_els = 0, max_level = 0;

        // parents
        bool parent_stored;
        List<element> pels = parents_elements(eb, _element_id, out parent_stored);

        // xml document
        List<element> els = eb.load_elements(out max_level_els, out max_level, _element_id != "" ? int.Parse(_element_id) : (int?)null, _max_level, key_page: kp);
        contents_xml.Visible = true;
        contents_xml.Attributes["sidebar-tp"] = "body";
        contents.Visible = false;
        doc_xml.Value = doc_xml_bck.Value = element.get_doc(els, eb.load_types_elements()).root_node.inner_xml;

        // client vars
        url_view.Value = master.url_cmd("view element" + (_element_id != "" ? " id:" + _element_id : "s"));
        id_element.Value = _element_id.ToString();
        parent_id.Value = els.Count > 0 ? els[0].parent_id.ToString() : "";
        first_order.Value = els.Count > 0 ? els[0].order.ToString() : "";
        last_order.Value = els.Count > 0 ? els[els.Count - 1].order.ToString() : "";
        max_lvl.Value = max_level_els.ToString();
        key_page.Value = kp;

        // menu
        menu.InnerHtml = parse_menu_elements(els, true, max_level_els, parent_stored, true, _element_id != "");

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected List<element> parents_elements(docs eb, string element_id, out bool parent_stored) {
    List<element> pels = element_id != "" ? eb.load_parents_elements(long.Parse(element_id)) : null;
    parent_stored = pels != null && pels.FirstOrDefault(x => x.dt_stored.HasValue) != null;
    return pels;
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    if (_cmd.action == "xml")
      this.master.set_status_txt("caricamento dati...");

  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

  #region cache

  protected string check_cache(docs eb, bool save = true) {
    string cache_ids = get_cache_var("cache_ids");
    if (string.IsNullOrEmpty(cache_ids)) return "";

    List<long> ids = eb.check_exists_elements(
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

  protected string parse_menu_elements(List<element> els, bool for_xml = false, int? max_level = null, bool parent_stored = false
    , bool view_stored = false, bool can_back = false) {
    StringBuilder sb = new StringBuilder();
    foreach (element el in els.Where(x => x.type == element.type_element.element || x.type == element.type_element.list
      || x.type == element.type_element.title || x.type == element.type_element.par || (can_back && x.type == element.type_element.attivita)))
      parse_menu(el, sb, el.level == 0, for_xml, max_level, parent_stored, view_stored, can_back);
    return sb.ToString();
  }

  protected void parse_menu(element e, StringBuilder sb, bool root = false, bool for_xml = false
    , int? max_level = null, bool parent_stored = false, bool view_stored = false, bool can_back = false) {

    if (e.id == 14) { int j = 0; }

    if (e.get_attribute_bool("closed")) return;
    if (!view_stored && e.dt_stored.HasValue) return;

    if (root) sb.AppendFormat("<ul menu_childs_id='" + e.id.ToString() + "' class='nav flex-column' style='padding:0px;margin-top:0px;' ><div tp='virtual'></div>\n");

    string reference = get_ref(e.get_attribute_string("ref"))
      , title = e.type == element.type_element.title && e.has_content ? e.content
      : (e.has_title ? e.title : (reference != "" ? reference : e.type != element.type_element.par ? "[senza titolo]" : ""));

    if (title != "") 
      parse_title_menu(e, title, sb
        , open_element_id: (max_level.HasValue && e.level == max_level && e.has_childs && !e.in_list ? e.id : 0)
        , back_element_id: e.back_element_id.HasValue && can_back ? e.back_element_id.Value : 0
        , for_xml: for_xml, parent_stored: parent_stored);

    // childs
    if (!max_level.HasValue || e.level < max_level) {
      bool first = true;
      foreach (element ec in e.childs) {
        if (!view_stored && ec.dt_stored.HasValue) continue;

        if (ec.type == element.type_element.title) {
          if (first && e.level >= 1 && e.type != element.type_element.par) { sb.Append("<ul menu_childs_id='" + e.id.ToString() + "'><div tp='virtual'></div>"); first = false; }
          parse_title_menu(ec, ec.has_content ? ec.content : "[senza titolo]", sb, for_xml: for_xml
            , into_par: e.type == element.type_element.par, parent_stored: parent_stored || e.dt_stored.HasValue);
        } else if (ec.type == element.type_element.element
          || ec.type == element.type_element.list || ec.type == element.type_element.par) {
          if (first && e.level >= 1 && e.type != element.type_element.par) { sb.Append("<ul menu_childs_id='" + e.id.ToString() + "'>"); first = false; }
          parse_menu(ec, sb, false, for_xml, max_level, parent_stored: parent_stored || e.dt_stored.HasValue, view_stored: view_stored, can_back: can_back);
        }
      }

      if (!first) sb.AppendFormat("{0}\n", !first ? "</ul>" : "");
      if (root) { sb.AppendFormat("</ul>\n"); }
    }
  }

  protected string parse_title_menu(element e, element ep, int? max_level = null, bool for_xml = false, bool parent_stored = false, bool can_back = false) {

    StringBuilder sb = new StringBuilder();

    if (e.type == element.type_element.element
        || e.type == element.type_element.list || e.type == element.type_element.par) {
      string reference = get_ref(e.get_attribute_string("ref"))
        , title = e.type == element.type_element.title && e.has_content ? e.content
          : (e.has_title ? e.title : (reference != "" ? reference : e.type != element.type_element.par ? "[senza titolo]" : ""));

      parse_title_menu(e, title, sb
        , open_element_id: (max_level.HasValue && e.level == max_level && e.has_childs && !e.in_list ? e.id : 0)
        , back_element_id: e.back_element_id.HasValue && can_back ? e.back_element_id.Value : 0
        , for_xml: for_xml, parent_stored: parent_stored);
    } else
      parse_title_menu(e, e.type == element.type_element.title ? (e.has_content ? e.content : "[senza titolo]")
        : (e.has_title ? e.title : "[senza titolo]"), sb, for_xml: for_xml
        , into_par: ep != null && ep.type == element.type_element.par, parent_stored: parent_stored || e.dt_stored.HasValue);

    return sb.ToString();
  }

  protected void parse_title_menu(element e, string title, StringBuilder sb, bool close = true, long open_element_id = 0
    , long back_element_id = 0, bool for_xml = false, bool into_par = false, bool parent_stored = false) {
    int level = !into_par ? e.level : e.level - 1;
    string ref_id = "ele_" + e.id.ToString(), reference = e.get_attribute_string("ref")
      , rf = !for_xml ? "#" + ref_id : "javascript:go_to_id(" + e.id.ToString() + ")";
    bool stored = parent_stored || e.dt_stored.HasValue;

    sb.AppendFormat(@"<li menu_id='{8}' style='{7}'><div style='display:block;{9}'>{6}<a {3} style='{4}' href='{1}'>{0}{5}</a></div>{2}"
      , (e.type == element.type_element.element ? "<img src='images/tr-gray-10.png' style='margin-right:3px;'>" : "") + title
      , rf, close ? "</li>" : "", e.level == 0 ? "class='h5'" : ""
      , level == 0 ? "color:steelblue;"
        : (level == 1 ? "color:skyblue;margin-top:10px;padding-top:5px;padding-left:3px;" : "font-size:95%;color:lightcyan;")
      , open_element_id > 0 ? "<a style='margin-left:15px;' href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + open_element_id.ToString()) + "\">"
        + "<img src='images/right-arrow-16.png' style='margin-bottom:4px;'></a>" : ""
      , back_element_id != 0 ? (back_element_id > 0 ? "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " element id:" + back_element_id.ToString())
         : "<a href=\"" + get_url_cmd((!for_xml ? "view" : "xml") + " elements"))
        + "\" style='margin-right:10px;'><img src='images/left-chevron-24.png' style='margin-left:3px;margin-bottom:4px;' width='20' height='20'></a>" : ""
        , "", e.id, stored ? "opacity:0.3;" : "");
  }

  #endregion

  #region document

  protected string parse_elements_doc(List<element> els, bool parent_stored = false, bool view_stored = false) {
    StringBuilder sb = new StringBuilder();
    sb.AppendFormat("<div virtual_id='0'></div>");
    els.ForEach(e => { parse_element_base(e, sb, parent_stored: parent_stored, view_stored: view_stored); });
    return sb.ToString();
  }

  protected string parse_edited_element(element el, element ep, bool in_list, bool parent_stored, bool no_opacity, bool no_contenitor = false) {
    if (ep != null) {
      StringBuilder sb = new StringBuilder();
      if (ep.type == element.type_element.element)
        parse_child_element(ep, el, sb, in_list, parent_stored, no_opacity, view_stored: _view_stored, no_contenitor: no_contenitor);
      else if (ep.type == element.type_element.list)
        parse_child_list(ep, el, sb, parent_stored, view_stored: _view_stored, no_contenitor: no_contenitor);
      else if (ep.type == element.type_element.attivita)
        parse_child_attivita(ep, el, sb, parent_stored, view_stored: _view_stored, no_contenitor: no_contenitor);
      else parse_element_base(el, sb, in_list, parent_stored, no_opacity, view_stored: _view_stored, no_contenitor: no_contenitor);
      return sb.ToString();
    }
    return parse_element(el, in_list, view_stored: _view_stored, no_contenitor: no_contenitor);
  }

  protected string parse_element(element e, bool in_list = false, bool view_stored = false, bool no_contenitor = false) {
    StringBuilder sb = new StringBuilder();
    parse_element_base(e, sb, in_list, view_stored: view_stored, no_contenitor: no_contenitor);
    return sb.ToString();
  }

  protected void parse_element(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false, bool view_stored = false) {
    if (e.type != element.type_element.element) throw new Exception("elemento '" + e.type + "' da parsificare inaspettato!");
    string dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";

    if ((e.has_title || e.get_attribute_string("ref") != "") && e.type == element.type_element.element)
      parse_type_element(e, sb, in_list, stored, no_opacity);

    if (!e.sham) {
      if (e.childs.Count > 0) {
        string cls = (_is_mobile ? "-mobile" : "");
        bool first = true;
        foreach (element ec in e.childs) {

          if (!view_stored && ec.dt_stored.HasValue) continue;

          if (first) {
            if (in_list) sb.AppendFormat(@"<ul childs_element_id='{0}' class='no-bullet{1}'>
              <div id='virtual_{0}'></div>", e.id, cls + (dt_stored != "" ? "-stored" : ""));
            else sb.AppendFormat(@"<div childs_element_id='{0}' class='childs-element{1}'>
              <div id='virtual_{0}'></div>", e.id, cls + (dt_stored != "" ? "-stored" : ""));
            first = false;
          }

          parse_child_element(e, ec, sb, in_list, stored, no_opacity, view_stored);
        }
        if (!first) { if (in_list) sb.Append("</ul>"); else sb.Append("</div>"); }
      }
    }
  }

  protected void parse_child_element(element e, element ec, StringBuilder sb, bool in_list, bool stored, bool no_opacity
    , bool view_stored = false, bool no_contenitor = false) {
    if (e.type != element.type_element.element) throw new Exception("elemento '" + e.type + "' da parsificare inaspettato!");

    string dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : ""
      , cls = _is_mobile ? "-mobile" : "";

    if (ec.type == element.type_element.list)
      parse_type_list(ec, sb, in_list, stored || dt_stored != "", no_opacity, view_stored: view_stored, no_contenitor: no_contenitor);
    else if (ec.type == element.type_element.attivita)
      parse_type_attivita(ec, sb, in_list, stored || dt_stored != "", no_opacity, view_stored: view_stored);
    else {
      if (in_list && !no_contenitor) sb.AppendFormat(@"<li {1} contenitor_id='{0}'>"
        , ec.id, (ec.type != element.type_element.element ? (ec.dt_stored.HasValue ? "class='nobullet'" : "class='bullet" + bullet_i(ec)) : "class='nobullet") + cls + "'");
      parse_element_base(ec, sb, in_list, stored || dt_stored != "", no_opacity, view_stored: view_stored, no_contenitor: no_contenitor);
      if (in_list && !no_contenitor) sb.AppendFormat(@"</li>");
    }
  }

  protected string parse_element_base(element e, bool in_list = false, bool parent_stored = false, bool no_contenitor = false) {
    StringBuilder sb = new StringBuilder();
    parse_element_base(e, sb, in_list, parent_stored, no_contenitor);
    return sb.ToString();
  }

  protected void parse_element_base(element e, StringBuilder sb, bool in_list = false, bool parent_stored = false
    , bool no_opacity = false, string add_style = "", bool view_stored = false, bool no_contenitor = false) {
    if (e == null || (!view_stored && e.dt_stored.HasValue)) return;
    if (e.type == element.type_element.title) parse_type_title(e, sb, in_list, parent_stored, no_opacity, add_style);
    else if (e.type == element.type_element.text) parse_type_text(e, sb, in_list, parent_stored, no_opacity, add_style);
    else if (e.type == element.type_element.element) parse_element(e, sb, in_list, parent_stored, no_opacity, view_stored);
    else if (e.type == element.type_element.list) parse_type_list(e, sb, in_list, parent_stored, no_opacity, view_stored, no_contenitor);
    else if (e.type == element.type_element.par) parse_type_par(e, sb, in_list, parent_stored, no_opacity, view_stored);
    else if (e.type == element.type_element.account) parse_type_account(e, sb, in_list, parent_stored, no_opacity);
    else if (e.type == element.type_element.value) parse_type_value(e, sb, in_list, parent_stored, no_opacity);
    else if (e.type == element.type_element.link) parse_type_link(e, sb, in_list, parent_stored, no_opacity);
    else if (e.type == element.type_element.attivita) parse_type_attivita(e, sb, in_list, parent_stored, no_opacity, view_stored);
    else if (e.type == element.type_element.code) parse_type_code(e, sb, in_list, parent_stored, no_opacity);
    else
      sb.AppendFormat("<h5 style='color:tomato'>nota bene: elemento '" + e.type + "' non gestito!</h5>");
  }

  protected void parse_type_list(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false, bool view_stored = false, bool no_contenitor = false) {
    if (e.type != element.type_element.list) throw new Exception("elemento tipo '" + e.type + "' inaspettato!");

    string ref_id = "ele_" + e.id.ToString(), a = string.Format("<a id='{0}' class='anchor'></a>", ref_id)
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";
    bool closed = e.get_attribute_bool("closed");

    // head
    {
      string tag = e.level <= 1 ? "h4" : "h5"
        , style = (e.level <= 1 ? "margin-bottom:5px;" : "margin-bottom:0px;") + (closed ? "color:lightgray;" : "")
        , html_notes = e.get_attribute_string("notes") != "" ? "<small title-val='...' tp-val='notes' class='ml-3'>" + e.get_attribute_string("notes") + "</small>" : "";

      // su - &#x25B2; giu - &#x25BC;
      string tr = closed ? string.Format("<span open_id='{0}' class='fas fa-chevron-circle-down'"
        + " style='display:inline-block;width:18px;height:18px;margin-left:5px;color:lightgray;cursor:pointer;"
        + "font-size:12pt;{1}' onclick='show_childs({0})'></span>", e.id
        , !_is_mobile ? "padding-bottom:2px;padding-left:2px;" : "padding-top:2px;padding-left:3px;") : "";

      string sym = dt_stored != "" ? "" : (e.level < 1 ? "<img src='images/sq-16.png' style='margin-right:3px;'></img>"
        : (e.level <= 2 ? "<img src='images/sq-14.png' style='margin-right:3px;'></img>"
          : "<img src='images/sq-12.png' style='margin-right:3px;'></img>"));

      if (!in_list) sb.AppendFormat(@"<{3} class='{3}-responsive' element_id='{1}' type_element='list' title_element=""{8}"" in_list='{14}' livello='{15}' no_opacity='{13}' parent_stored='{12}' stored='{10}' style='{9}{4}'>{11}{2}{7}{0}{5}{6}</{3}>"
        , (dt_stored != "" ? "&nbsp;&nbsp;" : "") + "<span tp-val='title'>" + e.title + "</span>", e.id, a, tag, style, tr, html_notes, sym, e.des
        , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : ""
        , dt_stored != "" ? "true" : "", label_stored(e, dt_stored), stored ? "true" : "", no_opacity ? "true" : "", in_list ? "true" : "", e.level);
      else {
        string li = !no_contenitor ? string.Format(@"<li class='{1}' contenitor_id='{0}' style='{2}overflow-wrap:break-word;'>"
          , e.id, dt_stored != "" ? "no-bullet" + (_is_mobile ? "-mobile" : "") : "square" + bullet_i(e) + (_is_mobile ? "-mobile" : "")
          , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "") : "";

        sb.AppendFormat(@"{5}<{3} class='{3}-responsive' element_id='{1}' type_element='list' title_element=""{8}"" in_list='{14}' livello='{15}' no_opacity='{13}' 
            parent_stored='{12}' stored='{10}' style='{4}'>{2}{11}{0}{6}{7}</{3}>"
          , (dt_stored != "" ? "&nbsp;&nbsp;" : "") + "<span tp-val='title'>" + e.title + "</span>", e.id, a, tag, style, li, tr, html_notes, e.des
          , !no_contenitor ? "</li>" : "", dt_stored != "" ? "true" : "", label_stored(e, dt_stored), stored ? "true" : "", no_opacity ? "true" : "", in_list ? "true" : "", e.level);
      }
    }

    // childs
    if (e.childs.Count > 0) {
      string cls = _is_mobile ? "-mobile" : "";
      bool first = true;
      foreach (element ec in e.childs) {
        if (!view_stored && ec.dt_stored.HasValue) continue;
        if (first) {
          sb.AppendFormat(@"<div childs_element_id='{0}' {2}><ul class='no-bullet{1}'><div id='virtual_{0}'></div>"
            , e.id, cls, closed ? "style='display:none'" : "");
          first = false;
        }
        parse_child_list(e, ec, sb, stored, view_stored);
      }
      if (!first) sb.AppendFormat(@"</ul></div>");
    }

  }

  protected void parse_child_list(element e, element ec, StringBuilder sb, bool stored, bool view_stored = false, bool no_contenitor = false) {
    if (e.type != element.type_element.list) throw new Exception("elemento tipo '" + e.type + "' inaspettato!");
    string cls = _is_mobile ? "-mobile" : ""
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";

    if (ec.type == element.type_element.list)
      parse_type_list(ec, sb, true, stored || dt_stored != "", view_stored: view_stored, no_contenitor: no_contenitor);
    else if (ec.type == element.type_element.attivita)
      parse_type_attivita(ec, sb, true, stored || dt_stored != "", view_stored: view_stored);
    else {
      bool inline = e.get_attribute_string("style") == "inline";
      string style = inline && (ec.type == element.type_element.account || ec.type == element.type_element.value
        || ec.type == element.type_element.link) ? (!_is_mobile ? "display:inline-block;margin-right:10px;" : "display:inline-block;margin-right:5px;") : "";
      bool opacity = e.dt_stored.HasValue || stored || ec.dt_stored.HasValue
        , stored_child = ec.dt_stored.HasValue;
      if (!no_contenitor) sb.AppendFormat(@"<li {1} style='{2}{3}overflow-wrap:break-word;' bullet='true' contenitor_id='{0}'>"
         , ec.id, stored_child ? "class='nobullet" + cls + "'" : ((ec.type != element.type_element.element ? "class='bullet" + bullet_i(ec) : "class='nobullet") + cls + "'")
         , style, opacity ? "opacity:0.4;" : "");
      parse_element_base(ec, sb, true, stored || dt_stored != "", opacity ? true : false, view_stored: view_stored);
      if (!no_contenitor) sb.AppendFormat(@"</li>");
    }
  }

  protected void parse_type_par(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false, bool view_stored = false) {
    if (e.type != element.type_element.par) throw new Exception("elemento " + e.type + " di tipo errato!");

    int level = !in_list ? e.level : 5;
    string ref_id = "ele_" + e.id.ToString(), title = e.has_title ? e.title : ""
      , reference = get_ref(e.get_attribute_string("ref"))
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));
    string a = string.Format("<a id='{0}' class='anchor'></a>", ref_id)
      , st_title = "style='" + (level == 1 ? "color:dimgray;margin-top:5px;padding-top:5px;margin-bottom:5px;padding-bottom:0px;"
      : (level >= 2 ? "color:dimgray;margin-bottom:5px;padding-bottom:0px;" : "color:dimgray;margin-bottom:5px;"))
      + (dt_stored != "" ? "margin-left:10px;" : "") + "'";
    string tag = level == 0 ? "h3" : (level == 1 ? "h3" : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5")))
      , border = in_list ? "bordered-small" : (e.level > 2 ? "bordered-small" : "bordered")
      , border_childs = border != "" ? border + "-childs" : "";
    if (title == "")
      sb.AppendFormat("<div element_id='{0}' type_element='par' title_element=\"{1}\" in_list='{6}' style='{3}' livello='{9}' no_opacity='{8}' parent_stored='{7}' stored='{4}' class='{2}'>{5}"
        , e.id, e.des, border, (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : ""
        , dt_stored != "" ? "true" : "", label_stored(e, dt_stored), in_list ? "true" : "", stored ? "true" : ""
        , no_opacity ? "true" : "", e.level);
    else {
      if (reference != "")
        sb.AppendFormat("<div element_id='{6}' type_element='par' title_element=\"{7}\" in_list='{12}' livello='{16}' no_opacity='{14}' parent_stored='{13}' stored='{10}' class='{8}' style='{9}overflow-wrap:break-word;'>{11}{5}<{0} class='{0}-responsive' {4}><a href='{2}' tp-val='title' {3}>{1}</a>{15}</{0}>"
          , tag, title, reference, !title_ref_cmd ? "target='blank'" : "", st_title, a, e.id, e.des, border
          , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : ""
          , dt_stored != "" ? "true" : "", label_stored(e, dt_stored), in_list ? "true" : "", stored ? "true" : "", no_opacity ? "true" : ""
          , "<span class='d-none' tp-val='ref'>" + e.get_attribute_string("ref") + "</span>", e.level);
      else
        sb.AppendFormat("<div element_id='{4}' type_element='par' title_element=\"{5}\" in_list='{10}' livello='{13}' no_opacity='{12}' parent_stored='{11}' stored='{8}' class='{6}' style='{7}overflow-wrap:break-word;'>{9}{3}<{0} class='{0}-responsive' {2}><span tp-val='title'>{1}</span></{0}>"
          , tag, title, st_title, a, e.id, e.des, border
          , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : ""
          , dt_stored != "" ? "true" : "", label_stored(e, dt_stored), in_list ? "true" : "", stored ? "true" : ""
          , no_opacity ? "true" : "", e.level);
    }

    // childs
    if (e.childs.Count > 0) {
      sb.AppendFormat("<div id='virtual_{0}'></div>", e.id);
      foreach (element ec in e.childs) {
        if (!view_stored && ec.dt_stored.HasValue) continue;
        parse_element_base(ec, sb, false, stored || dt_stored != "", stored || dt_stored != "" ? true : false
          , dt_stored != "" ? "margin-left:10px;" : "");
      }
    }
    sb.AppendFormat(@"</div>");

  }

  protected string image_attivita(string stato) {
    string img = stato == "la prossima" ? "circle-pr-16.png" : (stato == "in corso" ? "circle-ic-16.png" : (stato == "fatta" ? "circle-ft-16.png"
      : (stato == "sospesa" ? "circle-sp-16.png" : "circle-df-16.png")));
    return "images/" + img;
  }

  protected string label_stored(element e, string dt_stored, bool opacity = false, bool in_line = false) {
    return dt_stored != "" ?
      string.Format(@"<span style='{2}margin-top:5px;{3}font-style:italic;'>
        <small style='font-style:normal;margin-right:5px;'>&#9949;</small><small>{1} storicizzata{0}</small></span>"
      , dt_stored + (in_line ? ": " : ""), e.des, opacity ? "opacity:0.4;" : ""
      , in_line ? "display:inline;margin-right:5px;" : "display:block;", e.id) : "";
  }

  protected void parse_type_attivita(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false, bool view_stored = false) {
    if (e.type != element.type_element.attivita) throw new Exception("elemento " + e.type + " di tipo errato!");

    // head
    string title = e.has_title ? e.title : "[senza titolo]"
      , priorita = e.get_attribute_string("priorita"), stato = e.get_attribute_string("stato")
      , dt_upd = e.dt_upd.HasValue ? " - " + e.dt_upd.Value.ToString("dddd dd MMMM yyyy") : ""
      , dt_ins = e.dt_ins.HasValue ? " - " + e.dt_ins.Value.ToString("dddd dd MMMM yyyy") : ""
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";

    string cl = stato == "la prossima" ? (priorita == "alta" ? "danger" : "primary") : (stato == "fatta" ? "success" : (stato == "sospesa" ? "secondary"
      : (stato == "in corso" ? "warning" : (priorita == "alta" ? "danger" : (priorita == "bassa" ? "info" : "primary")))));

    sb.AppendFormat(@"<div element_id='{3}' type_element='attivita' title_element=""{9}"" stato='{6}' priorita='{7}' livello='{13}' no_opacity='{14}' parent_stored='{13}' stored='{11}' in_list='{8}' style='{10}margin-top:3px;margin-bottom:3px;'>{4}
     {12}<h4 style=""background: url('{0}') no-repeat 2px 6px;padding-left:22px;display:inline-block;margin:0px;{15}cursor:pointer;"">
     <span clickable='true' style=""white-space:normal;overflow-wrap:break-word;font-weight:normal;"" class='badge badge-{2}'><span tp-val='title'>{1}</span>{16}</span></h4>{5}</div>"
      , image_attivita(stato), title
      , cl, e.id, in_list ? "<li>" : "", in_list ? "</li>" : "", stato, priorita, in_list ? "true" : "false"
      , e.des, (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "", dt_stored != "" ? "true" : ""
      , label_stored(e, dt_stored), stored ? "true" : "", no_opacity ? "true" : ""
      , dt_stored != "" ? "margin-left:10px;" : ""
      , (stato == "la prossima" ? " - LA PROSSIMA" + dt_upd : (stato == "in corso" ? " - IN CORSO" + dt_upd
        : (stato == "sospesa" ? " - SOSPESA" + dt_upd : (stato == "fatta" ? " - FATTA" + dt_upd : dt_ins))))
        + (priorita == "alta" ? " - ALTA PRIORITÀ" : (priorita == "bassa" ? " - BASSA PRIORITÀ" : "")), e.level);

    // childs
    if (e.childs.Count > 0) {
      string cls = _is_mobile ? "-mobile" : "";
      bool first = true;
      foreach (element ec in e.childs) {
        if (!view_stored && ec.dt_stored.HasValue) continue;
        if (first) {
          sb.AppendFormat(@"<div childs_element_id='{0}'><ul class='no-bullet{1}'><div id='virtual_{0}'></div>", e.id, cls);
          first = false;
        }
        parse_child_attivita(e, ec, sb, stored, view_stored);
      }
      if (!first) sb.AppendFormat(@"</ul></div>");
    }
  }

  protected void parse_child_attivita(element e, element ec, StringBuilder sb, bool stored, bool view_stored = false, bool no_contenitor = false) {
    string dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : ""
      , cls = _is_mobile ? "-mobile" : "";
    bool opacity = stored || dt_stored != "" || ec.dt_stored.HasValue;

    if (ec.type == element.type_element.list)
      parse_type_list(ec, sb, true, stored || dt_stored != "", view_stored: view_stored, no_contenitor: no_contenitor);
    else if (ec.type == element.type_element.attivita)
      parse_type_attivita(ec, sb, true, stored || dt_stored != "", view_stored: view_stored);
    else {
      if (!no_contenitor) sb.AppendFormat(@"<li {1} style='{2}' contenitor_id='{0}'>"
        , ec.id, ec.dt_stored.HasValue ? "class='nobullet" + cls + "'" : ((ec.type != element.type_element.element ? "class='bullet" + bullet_i(ec) : "class='nobullet") + cls + "'")
        , opacity ? "opacity:0.4;" : "");
      parse_element_base(ec, sb, true, stored || dt_stored != "", opacity ? true : false, view_stored: view_stored);
      if (!no_contenitor) sb.AppendFormat(@"</li>");
    }
  }

  protected string bullet_i(element e) { return e.level <= 1 ? "" : (e.level == 2 ? "-2" : "-3"); }

  protected string image_bullet(element e) { return "images/circle-8" + bullet_i(e) + ".png"; }

  protected void parse_type_account(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false) {
    if (e.type != element.type_element.account) throw new Exception("elemento " + e.type + " di tipo errato!");

    string title = e.has_title ? e.title : "account", user = e.get_attribute_string("user")
      , pass = e.get_attribute_string("password"), notes = e.get_attribute_string("notes")
      , email = e.get_attribute_string("email")
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";

    sb.AppendFormat(@"<span element_id='{5}' type_element='account' title_element=""{6}"" in_list='{12}' livello='{14}' no_opacity='{11}' parent_stored='{10}' stored='{8}' style='{7}overflow-wrap:break-word;'>{9}{4}{13}{0}{1}{2}{3}</span>"
      , !string.IsNullOrEmpty(title) ? "<span class='lead' tp-val='title'>" + title + "</span>: " : ""
      , "<span tp-val='user_pass' class='mr-1'>" + user + (pass != "" ? "/" + pass : "") + "</span>"
      , "<span title-val='email' tp-val='email' class='mr-1'>" + email + "</span>"
      , "<span title-val='...' tp-val='notes' class='font-weight-lighter font-italic' style='color:darkgray;'>" + notes + "</span>"
      , !in_list && dt_stored == "" ? "<img src='" + image_bullet(e) + "' style='padding-left:3px;padding-right:3px;padding-bottom:3px;'></img>" : "", e.id, e.des
      , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "", dt_stored != "" ? "true" : ""
      , label_stored(e, dt_stored, true && !no_opacity), stored ? "true" : "", no_opacity ? "true" : "", in_list ? "true" : ""
      , dt_stored != "" ? "&nbsp;&nbsp;&nbsp;" : "", e.level);
  }

  protected void parse_type_value(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false) {
    if (e.type != element.type_element.value) throw new Exception("elemento " + e.type + " di tipo errato!");

    string title = e.title, content = e.get_attribute_string("content"), notes = e.get_attribute_string("notes")
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";

    sb.AppendFormat(@"<span element_id='{4}' type_element='value' title_element=""{5}"" in_list='{11}' 
        livello='{12}' no_opacity='{10}' parent_stored='{9}' stored='{7}' style='{6}overflow-wrap:break-word;'>{8}{3}{0}{1}{2}</span>"
      , !string.IsNullOrEmpty(title) ? "<span tp-val='title' class='font-weight-bold'>" + title + "</span>: " : ""
      , "<span tp-val='content'>" + content + "</span>", !string.IsNullOrEmpty(notes) ? "<span tp-val='notes' class='font-weight-lighter ml-2' title-val='...' style='color:darkgray;'>" + notes + "</span>" : ""
      , !in_list ? "<img src='" + image_bullet(e) + "' style='padding-left:3px;padding-right:3px;'></img>" : "", e.id, e.des
      , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "", dt_stored != "" ? "true" : ""
      , label_stored(e, dt_stored, false, true), stored ? "true" : "", no_opacity ? "true" : ""
      , in_list ? "true" : "", e.level);
  }

  protected void parse_type_code(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false) {
    if (e.type != element.type_element.code) throw new Exception("elemento " + e.type + " di tipo errato!");

    string title = e.title, notes = e.get_attribute_string("notes")
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";
    int height = e.get_attribute_int("height");

    // content
    //  & => &amp;
    //  > => &gt;
    //  < => &lt;
    string content = e.get_attribute_string("content").Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;");

    sb.AppendFormat(@"<div element_id='{4}' type_element='code' title_element=""{5}"" style='{7}' in_list='{12}' livello='{14}' no_opacity='{11}' parent_stored='{10}' stored='{8}'>{9}{3}{0}{2}
        <textarea code_id='{4}' style='color:green;{6}font-family:courier new;{13}' spellcheck='false' onfocus='focus_code({4})' onblur='blur_code({4})' wrap='off'>{1}</textarea></div>"
      , string.Format("<span class='font-weight-bold d-block' tp-val='title' style='{1}'>{0}</span>", title, dt_stored != "" ? "margin-left:10px;" : "")
      , content, string.Format("<span style='display:block;{1}' class='lead'><small tp-val='notes'>{0}</small></span>", notes, dt_stored != "" ? "margin-left:10px;" : "")
      , "", e.id, e.des, height <= 0 ? "height:200px;" : "height:" + height + "px;"
      , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "", dt_stored != "" ? "true" : ""
      , label_stored(e, dt_stored), stored ? "true" : "", no_opacity ? "true" : "", in_list ? "true" : ""
      , dt_stored != "" ? "margin-left:10px;width:calc(100% - 10px);" : "width:100%;", e.level);
  }

  protected void parse_type_link(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false) {
    if (e.type != element.type_element.link) throw new Exception("elemento " + e.type + " di tipo errato!");

    string title = e.title, reference = get_ref(e.get_attribute_string("ref")), notes = e.get_attribute_string("notes")
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : "";
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));

    sb.AppendFormat(@"<div element_id='{5}' type_element='link' title_element=""{6}"" in_list='{12}' livello='{15}' no_opacity='{11}' parent_stored='{10}' stored='{8}' 
      style='{7}overflow-wrap:break-word;'><span tp-val='title' class='d-none'>{13}</span><span tp-val='ref' class='d-none'>{14}</span>
        {9}<a href=""{0}"" style='overflow-wrap:break-word;' {3}>{4}{1}</a>{2}</div>"
      , reference, !string.IsNullOrEmpty(title) ? title : reference
      , notes != "" ? "<span class='font-weight-lighter ml-3' tp-val='notes' title-val='...' style='color:darkgray;'>" + notes + "</span>" : ""
      , !title_ref_cmd ? "target='blank'" : "", !in_list && dt_stored == "" ? "<img src='" + image_bullet(e) + "' style='padding-left:3px;padding-right:3px;padding-bottom:3px;'></img>" : "", e.id, e.des
      , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : ""
      , dt_stored != "" ? "true" : "", label_stored(e, dt_stored, in_line: true), stored ? "true" : ""
      , no_opacity ? "true" : "", in_list ? "true" : "", e.title, e.get_attribute_string("ref"), e.level);
  }

  protected void parse_type_title(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false, string add_style = "") {
    if (e.type != element.type_element.title) throw new Exception("elemento " + e.type + " di tipo errato!");

    int level = !in_list ? e.level : 5;
    string ref_id = "ele_" + e.id.ToString(), title = e.has_content ? e.content : "<titolo>"
      , reference = get_ref(e.get_attribute_string("ref"))
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : ""; ;
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));
    string a = string.Format("<a id='{0}' class='anchor'></a>", ref_id)
      , st = dt_stored != "" ? "style='color:dimgray;margin-bottom:5px;padding-bottom:0px;'" : ((level == 1 ? "style='color:dimgray;margin-top:15px;padding-top:15px;margin-bottom:5px;padding-bottom:0px;'"
      : (level >= 2 ? "style='color:dimgray;margin-bottom:5px;padding-bottom:0px;'" : "style='color:dimgray;margin-bottom:5px;'")));
    string tag = level == 0 ? "h3" : (level == 1 ? "h3" : (level == 2 ? "h4" : (level == 3 ? "h5" : "h5")));
    if (reference != "")
      sb.AppendFormat(@"<div element_id='{6}' type_element='title' title_element=""{7}"" stored='{10}' in_list='{11}' 
          parent_stored='{12}' livello='{16}' no_opacity='{13}' style='{8}overflow-wrap:break-word;{15}'>{9}{5}
          <{0} class='{0}-responsive' {4}>{14}
            <span tp-val='ref' class='d-none'>{17}</span><a href='{2}' tp-val='title' {3}>{1}</a></{0}></div>"
        , tag, title, reference, !title_ref_cmd ? "target='blank'" : "", st, a, e.id, e.des
        , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "", label_stored(e, dt_stored)
        , dt_stored != "" ? "true" : "", in_list ? "true" : "", stored ? "true" : "", no_opacity ? "true" : ""
        , dt_stored != "" ? "&nbsp;&nbsp;&nbsp;" : "", add_style, e.level, e.get_attribute_string("ref"));
    else
      sb.AppendFormat(@"<div element_id='{4}' type_element='title' title_element=""{5}"" stored='{8}' in_list='{9}' 
          parent_stored='{10}' livello='{14}' no_opacity='{11}' style='{6}overflow-wrap:break-word;{13}'>{7}{3}
            <{0} class='{0}-responsive' {2}><span tp-val='title' class='{12}'>{1}</span></{0}></div>"
        , tag, title, st, a, e.id, e.des
        , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "", label_stored(e, dt_stored)
        , dt_stored != "" ? "true" : "", in_list ? "true" : "", stored ? "true" : "", no_opacity ? "true" : ""
        , dt_stored != "" ? "ml-3" : "", add_style, e.level);
  }

  protected bool reference_cmd(string reference) { return reference.StartsWith("{cmdurl='"); }
  protected string get_ref(string ref_url) {
    return ref_url.StartsWith("{cmdurl='") ? _core.config.var_value_par("vars.router-cmd"
      , ref_url.Substring(9, ref_url.Length - 11)) : ref_url;
  }

  protected void parse_type_element(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false) {
    if (e.type != element.type_element.element) throw new Exception("elemento " + e.type + " di tipo errato!");

    int level = !in_list ? e.level : 5;
    string code = e.get_attribute_string("code"), type = e.get_attribute_string("type")
      , attr_ref = e.get_attribute_string("ref"), reference = get_ref(e.get_attribute_string("ref")), title = e.has_title ? e.title : (reference != "" ? reference : "[senza titolo]")
      , notes = e.get_attribute_string("notes")
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : ""; ;
    bool title_ref_cmd = reference_cmd(e.get_attribute_string("ref"));
    string a = string.Format("<a id='{0}' class='anchor'></a>", "ele_" + e.id.ToString())
      , st_title = "style='" + (level == 0 ? "margin-bottom:5px;padding-bottom:5px;padding-top:10px;background-color:white;color:steelblue;"
       : (level == 1 ? "background-color:whitesmoke;border-top:1pt solid lightgrey;margin-top:15px;padding-top:5px;margin-bottom:5px;padding-bottom:5px;"
       : "margin-bottom:5px;padding-bottom:0px;background-color:/whitesmoke;")) + (dt_stored != "" ? "margin-left:10px;" : "") + "'";
    string open = e.sham ? "<a style='margin-left:20px;' href=\"" + get_url_cmd("view element id:" + e.id.ToString()) + "\"><img src='images/right-arrow-24-black.png' style='margin-bottom:4px;'></a>" : "";

    string in_list_ct = (e.sham || in_list) ? string.Format(@"<small class='ml-3 grey-text font-weight-lighter' tp-val='code' title-val='code'>{0}</small>
      <small tp-val='type' class='ml-3 grey-text font-weight-lighter' title-val='type'>{1}</small>", code, type) : "";

    string sym = level < 1 ? "<img src='images/tr-16.png' style='margin-right:3px;'></img>"
      : (level <= 2 ? "<img src='images/tr-14.png' style='margin-right:3px;'></img>"
        : "<img src='images/tr-12.png' style='margin-right:3px;'></img>");

    string s_notes = level > 0 ? "<small class='ml-2 font-weight-light' tp-val='notes'>" + notes + "</small>" : ""
      , p_notes = level == 0 ? "<p class='ml-3' tp-val='notes'>" + notes + "</p>" : "";

    string tag = level == 0 ? "h1" : (level == 1 ? "h2" : (level == 2 ? "h3" : (level == 3 ? "h3" : "h4")));
    if (reference != "")
      sb.AppendFormat("<div element_id='{8}' type_element='element' title_element=\"{12}\" livello='{20}' no_opacity='{18}' parent_stored='{17}' stored='{14}' in_list='{16}' style='{13}overflow-wrap:break-word;{9}'>{15}{5}<{0} class='{0}-responsive' {4}><span class='d-none' tp-val='ref'>{19}</span><a href='{2}' {3}>{7}<span tp-val='title'>{1}</span></a>{6}{10}{11}</{0}>"
        , tag, title, reference, !title_ref_cmd ? "target='blank'" : "", st_title, a, in_list_ct, sym, e.id, level == 0 ? "border-top:1pt solid lightgray;margin-bottom:30px;" : ""
        , s_notes, open, e.des, (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : ""
        , dt_stored != "" ? "true" : "", label_stored(e, dt_stored), in_list ? "true" : ""
        , stored ? "true" : "", no_opacity ? "true" : "", attr_ref, e.level);
    else
      sb.AppendFormat("<div element_id='{7}' type_element='element' title_element=\"{10}\" livello='{17}' no_opacity='{16}' parent_stored='{15}' stored='{12}' in_list='{14}' style='{11}overflow-wrap:break-word;{8}'>{13}{3}<{0} class='{0}-responsive' {2}>{6}<span class='d-none' tp-val='ref'></span><span tp-val='title'>{1}</span>{5}{9}{4}</{0}>"
        , tag, title, st_title, a, open, in_list_ct, sym, e.id, level == 0 ? "border-top:1pt solid lightgray;margin-bottom:30px;" : ""
        , s_notes, e.des, (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : ""
        , dt_stored != "" ? "true" : "", label_stored(e, dt_stored)
        , in_list ? "true" : "", stored ? "true" : "", no_opacity ? "true" : "", e.level);

    if (!e.sham && !in_list) {
      sb.AppendFormat(@"<p class='lead ml-3' style='overflow-wrap:break-word;color:darkgray;padding:0px;margin:0px;margin-bottom:15px;'>
        <span title-val='code' tp-val='code'>{0}</span>
        <span title-val='type' tp-val='type'>{1}</span></p>", code, type);
    }
    sb.AppendFormat(p_notes + "</div>");
  }

  public enum text_styles { underline, bold, italic }
  protected text_styles[] get_styles(string styles) {
    if (string.IsNullOrEmpty(styles)) return new text_styles[] { };
    string ts = styles.Replace("][", ",").Replace("[", "").Replace("]", "");
    return ts.Split(new char[] { ',' }).Select(x => (text_styles)Enum.Parse(typeof(text_styles), x)).ToArray();
  }
  public bool is_style(string styles) { return !string.IsNullOrEmpty(styles); }

  protected void parse_type_text(element e, StringBuilder sb, bool in_list = false, bool stored = false, bool no_opacity = false, string add_style = "") {
    if (e.type != element.type_element.text) throw new Exception("elemento " + e.type + " di tipo errato!");

    int level = e.level;
    string content = e.get_attribute_string("content"), el_style = e.get_attribute_string("style")
      , dt_stored = e.dt_stored.HasValue ? " - " + e.dt_stored.Value.ToString("dddd dd MMMM yyyy") : ""
      , fs = level > 1 ? "" : "", style = (is_style(el_style) ? string.Join(""
        , get_styles(el_style).Select(s => s == text_styles.bold ? "font-weight:bold;"
          : (s == text_styles.italic ? "font-style:italic;" : (s == text_styles.underline ? "text-decoration: underline;" : "")))) : "") + fs;

    sb.AppendFormat(@"<p element_id='{4}' type_element='text' title_element=""{5}"" in_list='{9}' livello='{14}' no_opacity='{11}' 
        parent_stored='{10}' stored='{7}' styles='{15}' style='{2}{6}padding:0px;overflow-wrap:break-word;{1}{13}'>{8}{12}{3}{0}</p>"
      , "<span tp-val='content'>" + content + "</span>", style != "" ? style : "", !in_list ? "margin-bottom:10px;" : "margin-bottom:0px;"
      , e.has_title ? "<span class='font-weight-bold mr-2' tp-val='title'>" + e.title + "</span>" : "", e.id, e.des
      , (dt_stored != "" || stored) && !no_opacity ? "opacity: 0.4;" : "", dt_stored != "" ? "true" : ""
      , label_stored(e, dt_stored), in_list ? "true" : "", stored ? "true" : "", no_opacity ? "true" : ""
      , dt_stored != "" ? "&nbsp;&nbsp;&nbsp;" : "", add_style, e.level, el_style);
  }

  #endregion
}
