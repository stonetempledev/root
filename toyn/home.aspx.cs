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

public partial class _home : tl_page {

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {

      sections hpb = new sections();

      if (json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

          // PARTE PRINCIPALE

          // set_title_hp
          if (jr.action == "set_title_hp") {
            string new_title = hpb.set_main_title(jr.val_str("new_title"));
            res.contents = !string.IsNullOrEmpty(new_title) ? new_title : "la " + user.name + " home page";
          }
            // MACRO SEZIONE

            // add_macro_sezione
          else if (jr.action == "add_macro_sezione") {
            string title = jr.val_str("title"), notes = jr.val_str("notes");
            string id = db_conn.exec(core.parse_query("sections.add-macro-section"
              , new string[,] { { "title", title }, { "notes", notes }, { "id_utente", user.id_str } }), true);
            res.html_element = parse_macro_section(hpb, int.Parse(id));
          }
            // upd_macro_sezione
          else if (jr.action == "upd_macro_sezione") {
            string id = jr.val_str("id"), title = jr.val_str("title"), notes = jr.val_str("notes");
            db_conn.exec(core.parse_query("sections.upd-macro-section"
              , new string[,] { { "title", title }, { "notes", notes }, { "id", id } }));
            res.html_element = parse_macro_section(hpb, int.Parse(id), true);
          }
            // del_macro_sezione
          else if (jr.action == "del_macro_sezione") {
            db_conn.exec(core.parse_query("sections.del-macro-section", new string[,] { { "id", jr.val_str("id") } }));
          }
            // move_ms
          else if (jr.action == "move_ms") {
            if (jr.val_str("where") == "su")
              db_conn.exec(core.parse_query("sections.up-macro-section", new string[,] { { "id", jr.val_str("id") } }));
            else if (jr.val_str("where") == "in_cima")
              db_conn.exec(core.parse_query("sections.first-macro-section", new string[,] { { "id", jr.val_str("id") } }));
            else if (jr.val_str("where") == "in_fondo")
              db_conn.exec(core.parse_query("sections.last-macro-section", new string[,] { { "id", jr.val_str("id") } }));
            else throw new Exception("l'azione '" + jr.val_str("where") + "' non è gestita!");
            res.html_element = parse_macro_sections(hpb.load_macros_sections());
          }
            // empty_macro_sezione
          else if (jr.action == "empty_macro_sezione") {
            db_conn.exec(core.parse_query("sections.empty-macro-section", new string[,] { { "id", jr.val_str("id") } }));
          }

            // SEZIONE

            // add_sezione
          else if (jr.action == "add_sezione") {
            string id = db_conn.exec(core.parse_query("sections.add-section"
              , new string[,] { { "id", jr.val_str("id") }, { "title", jr.val_str("title") }, { "type", jr.val_str("type") }
                , { "notes", jr.val_str("notes") }, { "id_utente", user.id_str } }), true);
            res.html_element = parse_section(hpb.load_section(int.Parse(id)));
          }
            // add_sezione_after
          else if (jr.action == "add_sezione_after") {
            string id = db_conn.exec(core.parse_query("sections.add-section-after"
              , new string[,] { { "id", jr.val_str("id") }, { "title", jr.val_str("title") }, { "type", jr.val_str("type") }
                , { "notes", jr.val_str("notes") }, { "id_utente", user.id_str } }), true);
            res.html_element = parse_section(hpb.load_section(int.Parse(id)));
          }
            // upd_sezione
          else if (jr.action == "upd_sezione") {
            string id = jr.val_str("id");
            section s = hpb.load_section(int.Parse(id));
            s.title = jr.val_str("title");
            s.notes = jr.val_str("notes");
            s.cols = jr.val_int("cols");
            s.set_attribute_int("rows", jr.val_int("rows", 4));
            hpb.update_section(s);
            res.html_element = parse_section(s);
          }
            // del_sezione
          else if (jr.action == "del_sezione") {
            db_conn.exec(core.parse_query("sections.del-section", new string[,] { { "id", jr.val_str("id") } }));
          }
            // save_notes
          else if (jr.action == "save_notes") {
            section s = hpb.load_section(jr.val_int("id"));
            s.set_attribute_text("text", jr.val_str("text"));
            hpb.update_section(s);
          }
            // move_s
          else if (jr.action == "move_s") {
            if (jr.val_str("where") == "su")
              db_conn.exec(core.parse_query("sections.up-section", new string[,] { { "id", jr.val_str("id") } }));
            else if (jr.val_str("where") == "in_cima")
              db_conn.exec(core.parse_query("sections.first-section", new string[,] { { "id", jr.val_str("id") } }));
            else if (jr.val_str("where") == "in_fondo")
              db_conn.exec(core.parse_query("sections.last-section", new string[,] { { "id", jr.val_str("id") } }));
            else throw new Exception("l'azione '" + jr.val_str("where") + "' non è gestita!");
            res.html_element = parse_macro_section(hpb, jr.val_int("id-ms"));
          }
            // change_emphasis_s
          else if (jr.action == "change_emphasis_s") {
            string emphasis = jr.val_str("from-emphasis");
            List<emphasis> l = hpb.load_emphasies();
            emphasis em = l.FirstOrDefault(x => x.style == emphasis);
            if (em == null) em = l[0];
            else {
              em = l.FirstOrDefault(x => x.order == em.order + 1);
              if (em == null) em = l[0];
            }

            db_conn.exec(core.parse_query("sections.upd-section-emphasis"
              , new string[,] { { "emphasis", em.style }, { "id", jr.val_str("id") } }));
            res.html_element = parse_section(hpb.load_section(jr.val_int("id")));
          }

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // check cmd
      if (_cmd == null) return;

      // view
      if (_cmd.action == "view" && _cmd.obj == "home-page") {

        page_sections ps = hpb.load_home_page();
        main_title.InnerText = ps.title != "" ? ps.title : "la " + user.name + " home page";
        macro_sezioni.InnerHtml = parse_macro_sections(ps.macro_sections);

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    //if (_cmd.action == "xml") this.master.set_status_txt("caricamento dati...");

  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

  #region macro sezione

  protected string parse_macro_sections(List<macro_section> mss) {
    StringBuilder sb = new StringBuilder("<div tp='to-append'></div>");
    mss.ForEach(x => { sb.Append(parse_macro_section(x)); });
    return sb.ToString();
  }

  protected string parse_macro_section(macro_section ms, bool only_title = false) {
    if (!only_title) {
      StringBuilder sb = new StringBuilder();
      foreach (section s in ms.sections)
        sb.Append(parse_section(s));
      return core.parse_html_block("macro-section"
        , new Dictionary<string, object>() { { "ms", ms }, { "html-sections", sb.ToString() } });
    }
    return core.parse_html_block("title-macro-section", new Dictionary<string, object>() { { "ms", ms } });
  }

  protected string parse_macro_section(sections hp, int id, bool only_title = false) {
    return parse_macro_section(hp.load_macro_section(id), only_title);
  }

  #endregion

  #region sezione

  protected string parse_section(section s, bool only_title = false) {
    return core.parse_html_block(!only_title ? "section" : "title-section"
      , new Dictionary<string, object>() { { "s", s } });
  }

  #endregion

}
