﻿using System;
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
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using dlib.db;
using dlib.tools;
using dlib.xml;
using deepanotes;
using dlib;

public partial class _notes : tl_page {

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {

      notes ob = new notes();

      if (json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

          // task_state
          if (jr.action == "task_state") {
            List<free_label> fl = ob.load_free_labels();

            string folder_path;
            ob.update_task(jr.val_int("id"), out folder_path, fl, stato: jr.val_str("stato"));
            List<task_stato> stati = db_conn.dt_table(core.parse_query("menu-states")).Rows.Cast<DataRow>()
              .Select(r => new task_stato(db_provider.str_val(r["stato"]), 0, "", ""
                , db_provider.str_val(r["title_singolare"]))).ToList();
            res.html_element = parse_task(ob.load_task(jr.val_int("id")), folder_path, stati);
          }
            // set_filter_id
          else if (jr.action == "set_filter_id") {
            set_cache_var("active-task-filter", jr.val_str("filter_id"));
          }
            // remove_task
          else if (jr.action == "remove_task") {
            ob.remove_task(jr.val_int("id"));
          }
            // update_task
          else if (jr.action == "update_task") {
            List<free_label> fl = ob.load_free_labels();

            if (!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");

            string folder_path;
            ob.update_task(jr.val_int("id"), out folder_path, fl, title: jr.val_str("title"), assegna: jr.val_str("assegna")
              , priorita: jr.val_str("priorita"), stima: jr.val_str("stima"), tipo: jr.val_str("tipo"));
            List<task_stato> stati = db_conn.dt_table(core.parse_query("menu-states")).Rows.Cast<DataRow>()
              .Select(r => new task_stato(db_provider.str_val(r["stato"]), 0, "", ""
                , db_provider.str_val(r["title_singolare"]))).ToList();
            res.html_element = parse_task(ob.load_task(jr.val_int("id")), folder_path, stati);
          }
            // ren_task
          else if (jr.action == "ren_task") {
            if (!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");
            ob.ren_task(jr.val_int("id"), jr.val_str("title"));
          }
            // add_task
          else if (jr.action == "add_task") {
            if(!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");

            ob.add_task(jr.val_int("synch_folder_id"), jr.val_int("folder_id"), jr.val_str("stato")
              , jr.val_str("title"), jr.val_str("assegna"), jr.val_str("priorita"), jr.val_str("tipo"), jr.val_str("stima"));
          }
            // add_folder
          else if (jr.action == "add_folder") {
            if (!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");

            ob.add_folder(jr.val_int("synch_folder_id"), jr.val_int("folder_id"), jr.val_str("title"));
          }
            // ren_folder
          else if (jr.action == "ren_folder") {
            if (!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");
            ob.ren_folder(jr.val_int("synch_folder_id"), jr.val_int("folder_id"), jr.val_str("title"));
          }
            // del_folder
          else if (jr.action == "del_folder") {
            ob.del_folder(jr.val_int("synch_folder_id"), jr.val_int("folder_id"));
            res.contents = master.url_cmd("tasks");
          }
            // cut_element
          else if (jr.action == "cut_element") {
            int f_id = jr.val_int("element_id"), sf_id = jr.val_int("synch_folder_id");
            string tp = jr.val_str("tp_element");
            if (tp == "folder" || tp == "synch-folder") {
              if (sf_id > 0) {
                foreach (int id in ob.ids_childs_folders(sf_id)) {
                  add_element_cut(id, "f"); res.list.Add(id.ToString());
                }
              } else { add_element_cut(f_id, "f"); res.list.Add(f_id.ToString()); }
            } else if (tp == "task") add_element_cut(f_id, "t");
          }
            // paste_elements
          else if (jr.action == "paste_elements") {
            int f_id = jr.val_int("folder_id"), sf_id = jr.val_int("synch_folder_id");
            if (Session["elements_cut"] != null) {
              if (sf_id <= 0) sf_id = ob.get_synch_folder_id(f_id);
              string err = "", ids = "";
              foreach (string sid in Session["elements_cut"].ToString().Split(new char[] { ',' })) {
                string tp = sid.Substring(0, 1); int id = int.Parse(sid.Substring(1));
                try {
                  if (tp == "f") ob.move_folder(id, sf_id, f_id > 0 ? f_id : (int?)null);
                  else if (tp == "t") ob.move_task(id, sf_id, f_id > 0 ? f_id : (int?)null);
                } catch (Exception ex) {
                  //ids = ids + (ids != "" ? "," : "") + sid;
                  if (err == "") err = ex.Message;
                }
              }
              res.message = err;
              Session["elements_cut"] = ids == "" ? null : ids;
            }
          }
            // get_notes
          else if (jr.action == "get_details") {
            res.contents = ob.get_task_notes(jr.val_int("task_id"));
            string html_allegati = "";
            foreach (DataRow dr in ob.get_task_allegati(jr.val_int("task_id")).Rows) {
              html_allegati += core.parse_html_block("task-allegato", new string[,] { 
                { "http-path", db_provider.str_val(dr["http_path"]) }, { "file-name", db_provider.str_val(dr["file_name"]) }});
            }
            res.html_element = html_allegati != "" ? core.parse_html_block("task-allegati", new string[,] { { "html-allegati", html_allegati } }) : "";
          }
            // save_task_notes
          else if (jr.action == "save_task_notes") {
            ob.save_task_notes(jr.val_int("task_id"), jr.val_str("text"));
          }

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // tasks
      if (_cmd != null && _cmd.action == "view" && _cmd.obj == "tasks") {
        int? fi = qry_val("idt") != "" ? qry_int("idt") : (qry_val("id") != "" ? qry_int("id") : (int?)null)
          , sfi = qry_val("sft") != "" ? qry_int("sft") : (qry_val("sf") != "" ? qry_int("sf") : (int?)null);

        // filtro attivo
        List<task_filter> tfs = db_conn.dt_table(core.parse_query("filters-tasks")).Rows
          .Cast<DataRow>().Select(r => new task_filter(db_provider.int_val(r["task_filter_id"])
            , db_provider.str_val(r["filter_title"]), db_provider.str_val(r["filter_notes"])
            , db_provider.str_val(r["filter_def"]), db_provider.str_val(r["filter_class"]))).ToList();
        task_filter tf = tfs.FirstOrDefault(x => x.id == int.Parse(get_cache_var("active-task-filter", "1")));
        if (tf == null) tf = new task_filter(0, "elenco completo delle attività", "", "", "");

        ob.load_notes(fi, sfi, tf);
        menu.InnerHtml = parse_menu(ob.synch_folders, sfi.HasValue || fi.HasValue);
        folder_id.Value = qry_val("id");

        folder f = fi.HasValue ? ob.find_folder(fi.Value) : null;
        synch_folder sf = sfi.HasValue ? ob.find_synch_folder(sfi.Value) : null;
        string title = f != null ? f.folder_name : (sf != null ? sf.title : "");

        List<task_stato> stati = db_conn.dt_table(core.parse_query("menu-states")).Rows.Cast<DataRow>()
          .Select(r => new task_stato(db_provider.str_val(r["stato"]), 0, "", "", db_provider.str_val(r["title_singolare"]))).ToList();

        content.InnerHtml = "<div style='display:none'>virtuale</div>"
          + parse_tasks(ob, stati, tf, title, f != null ? ob.find_synch_folder(f.synch_folder_id).title + f.path : "", tfs);

        ClientScript.RegisterStartupScript(GetType(), "__task_states"
          , "var __task_priorita = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-priorita"))) + ";\r\n"
           + "var __task_stime = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-stime"))) + ";\r\n"
           + "var __task_tipi = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-tipi"))) + ";\r\n"
           + "var __task_assegna = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-assegna"))) + ";\r\n", true);

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected bool nome_valido(string nome) { return (new Regex("^[a-zA-Z0-9 ,ìèéùàò_]*$")).IsMatch(nome); }

  protected void add_element_cut(int id, string prefix) {
    if (Session["elements_cut"] == null) { Session["elements_cut"] = prefix + id.ToString(); return; }
    if (!Session["elements_cut"].ToString().Split(new char[] { ',' }).ToList().Contains(prefix + id.ToString())) {
      Session["elements_cut"] = Session["elements_cut"].ToString()
        + (Session["elements_cut"].ToString() != "" ? "," : "") + (prefix + id.ToString());
    }
  }

  protected string parse_tasks(notes n, List<task_stato> stati, task_filter tf = null, string title_folder = "", string path_folder = ""
    , List<task_filter> tfs = null) {

    StringBuilder sb = new StringBuilder();

    sb.Append(core.parse_html_block(title_folder == "" ? "title-attivita" : "title-attivita-folder"
      , new string[,] { { "title-folder", title_folder }, { "path-folder", path_folder }
      , { "filter-title", tf != null ? tf.title : "" }, { "filter-des", tf != null ? tf.notes + " ordinate per data decrescente" : "" }
      , { "conteggio", n.tasks != null && n.tasks.Count > 0 ? "per un totale di: " + n.tasks.Count.ToString() + " attività" 
          : "non è stata trovata nessuna attività" }
      , { "html-filters", tfs != null ? string.Join("", tfs.Select(x => 
        string.Format("<a class='dropdown-item {2}' href='javascript:change_filter({1})'>{0}</a>", x.title, x.id, x.class_css != "" ? "text-" + x.class_css : ""))) : "" }}));

    List<int> orders = n.tasks.Select(x => x.stato.order).Distinct().ToList();
    orders.Sort();
    foreach (int order in orders) {
      bool first = true;
      List<task> sub_tasks = n.tasks.Where(x => x.stato.order == order).OrderByDescending(xx => xx.dt_ref).ToList();
      foreach (task t in sub_tasks) {

        // title
        if (first) {
          string sub_title = sub_tasks.Count == 1 ? t.stato.title_singolare : t.stato.title_plurale;
          sb.Append(core.parse_html_block("open-title-sub-attivita", new string[,] { 
            { "count", sub_tasks.Count.ToString() }, { "cls", t.stato.cls }
            , { "title", sub_title != "" ? sub_title : (sub_tasks.Count == 1 ? "GENERICA" : "GENERICHE") } }));
          first = false;
        }

        // task
        string folder_path = "";
        if (t.file_id.HasValue) {
          folder f = t.folder_id.HasValue ? n.synch_folders.FirstOrDefault(x => x.id == t.synch_folder_id).get_folder(t.folder_id.Value) : null;
          folder_path = f != null ? f.path : "";
        } else {
          synch_folder sf = n.synch_folders.FirstOrDefault(x => x.id == t.synch_folder_id);
          folder f = sf.get_folder(t.folder_id.Value)
            , fp = f.parent_id.HasValue ? sf.get_folder(f.parent_id.Value) : null;
          folder_path = fp != null ? fp.path : "";
        }
        sb.Append(parse_task(t, n.find_synch_folder(t.synch_folder_id).title + folder_path, stati));
      }
      if (!first) sb.Append(core.parse_html_block("close-title-sub-attivita"));
    }

    return sb.ToString();
  }

  protected string des_date(DateTime dt) {
    int gg = (int)(DateTime.Now - dt).TotalDays;
    if (gg == 0) return "oggi";
    else if (gg == 1) return "ieri";
    else if (gg == 2) return "ieri l'altro";
    else if (gg >= 3 && gg <= 7) return gg.ToString() + " giorni fa";
    else if (gg > 7 && gg <= 14) return "una settimana fa";
    else if (gg > 14 && gg <= 21) return "due settimane fa";
    else if (gg > 21 && gg <= 28) return "tre settimane fa";
    else if (gg > 28 && gg <= 35) return "un mese fa";
    else if (gg > 35 && gg <= 60) return "due mesi fa";
    else if (gg > 60 && gg <= 90) return "tre mesi fa";
    else if (dt.Year == DateTime.Now.Year) return dt.ToString("dddd dd MMMM");
    else return dt.ToString("dddd dd MMMM yyyy");
  }

  protected string parse_task(task t, string folder_path, List<task_stato> stati) {
    if (t == null) return "";

    try {

      // dates
      string dins = t.dt_ins.HasValue ? des_date(t.dt_ins.Value) : ""
        , dupd = t.dt_upd.HasValue ? des_date(t.dt_upd.Value) : "";
      if (dupd != "") {
        if (dins == dupd ||
          (t.dt_ins.HasValue && t.dt_upd.HasValue && t.dt_ins.Value > t.dt_upd.Value)) dupd = "";
      }

      // classes
      bool cut = Session["elements_cut"] != null ? Session["elements_cut"].ToString()
        .Split(new char[] { ',' }).FirstOrDefault(x => x == "t" + t.id.ToString()) != null : false;

      // stati menu
      string ms = string.Join("", stati.Where(x => x.stato != t.stato.stato).Select(s =>
        core.parse_html_block("menu-state", new string[,] { { "task_id", t.id.ToString() }, { "assign_stato", s.stato }, { "title", s.title_singolare } })
        ));

      return core.parse_html_block("task", new string[,] { { "task_id", t.id.ToString() }, { "path", folder_path }
        , { "cls", t.stato.cls }, { "title", t.title }, { "val-assegna", t.user != null ? t.user : "" }
        , {"classes", cut ? "task-cut" : ""}
        , { "val-priorita", t.priorita != null ? t.priorita.priorita : "" }, { "val-stima", t.stima != null ? t.stima.stima : ""}
        , { "val-tipo", t.tipo != null ? t.tipo.tipo : "" }, { "menu-states", ms }
        , { "stato", !string.IsNullOrEmpty(t.stato.title_singolare) ? t.stato.title_singolare : "generica" }
        , { "assegnata", !string.IsNullOrEmpty(t.user) ? "assegnata a " + t.user : "&nbsp;" }
        , { "title_notes", (t.has_notes ? "vedi note" + (t.has_files ? " e allegati" : ""): "aggiungi note" + (t.has_files ? " o vedi allegati" : "")) + "..."}
        , { "data", (dins != "" ? "creata " + dins : "") + (dupd != "" ? (dins != "" ? " e " : "") + "aggiornata " + dupd : "") }
        , { "priorita", t.priorita != null && !string.IsNullOrEmpty(t.priorita.priorita) ? 
            parse_task_state(t.priorita.title_singolare, t.priorita.cls, "priorità di realizzazione") : ""}
        , { "stima", t.stima != null && !string.IsNullOrEmpty(t.stima.stima) ? 
            parse_task_state(t.stima.title_singolare, t.stima.cls, "stima approssimativa dei tempi di realizzazione") : ""}
        , { "tipo", t.tipo != null && !string.IsNullOrEmpty(t.tipo.tipo) ? 
            parse_task_state(t.tipo.title_singolare, t.tipo.cls, "tipo di attività") : ""}});
    } catch (Exception ex) {
      return core.parse_html_block("task-error", new string[,] { { "task_id", t.id.ToString() }, { "error", ex.Message }
        , { "source", ex.Source }, { "stack", ex.StackTrace }});
    }
  }

  protected string parse_task_state(string txt, string cls, string tooltip) {
    return core.parse_html_block("task-state", new string[,] { { "txt", txt }, { "cls", cls }, { "tooltip", tooltip } });
  }

  protected string parse_menu(List<synch_folder> sfs, bool open_home) {
    StringBuilder sb = new StringBuilder();
    foreach (synch_folder sf in sfs) {

      // tasks
      List<task> l = sf.tasks; int order = l.Count > 0 ? l.Min(x => x.stato.order) : -1
        , cc = order >= 0 ? l.Count(x => x.stato.order == order) : 0;
      string t_plurale = cc > 0 ? l.First(x => x.stato.order == order).stato.title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.stato.order == order).stato.title_singolare : ""
        , color = cc > 0 ? l.First(x => x.stato.order == order).stato.cls : "";

      t_plurale = t_plurale == "" ? "generiche" : t_plurale;
      t_singolare = t_singolare == "" ? "generica" : t_singolare;

      sb.Append(core.parse_html_block(block_level(0)
        , new string[,] { { "id", sf.id.ToString() }, { "tp", "synch-folder" }
          , { "url_open_home", open_home ? master.url_cmd("attivita") : "" }
          , { "url_synch_folder", master.url_cmd("attivita", pars: new string[,] { {"sf", sf.id.ToString() } }) }
          , { "title", sf.title }, { "childs", parse_folders_menu(sf.folders, 1) }
          , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita-synch"
            , new string[,] { { "class_spin", color }, { "synch_folder_id", sf.id.ToString() }
              , { "url_open_tasks", master.url_cmd("attivita", pars: new string[,] { { "sft", sf.id.ToString() } }) }
              , { "title", cc == 1 ? "una attività " + t_singolare : cc.ToString() + " attività " + t_plurale }
              , { "c_attivita", cc.ToString() }} ) : "" }}));
    }
    return string.Format("<ul class='nav flex-column'>{0}</ul>", sb.ToString());
  }

  protected string parse_folders_menu(List<folder> fs, int lvl) {
    StringBuilder sbb = new StringBuilder();

    foreach (folder f in fs.Where(x => !x.is_task)) {
      string url_open_folder = lvl >= 3 && f.folders.Where(x => x.task == null).Count() > 0
        ? master.url_cmd("attivita", pars: new string[,] { { "id", f.parent_id.ToString() } }) : "";

      List<int> cuts = Session["elements_cut"] != null ?
        Session["elements_cut"].ToString().Split(new char[] { ',' }).Where(y => y.StartsWith("f"))
        .Select(x => int.Parse(x.Substring(1))).ToList() : null;
      bool cut = cuts != null && cuts.Contains(f.folder_id);

      // tasks
      List<task> l = url_open_folder == "" ? f.tasks : f.sub_tasks;
      int order = l.Count > 0 ? l.Min(x => x.stato.order) : -1
        , cc = order >= 0 ? l.Count(x => x.stato.order == order) : 0;
      string t_plurale = cc > 0 ? l.First(x => x.stato.order == order).stato.title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.stato.order == order).stato.title_singolare : ""
        , color = cc > 0 ? l.First(x => x.stato.order == order).stato.cls : "";

      t_plurale = t_plurale == "" ? "generiche" : t_plurale;
      t_singolare = t_singolare == "" ? "generica" : t_singolare;

      sbb.Append(core.parse_html_block(block_level(lvl), new string[,] { { "id", f.folder_id.ToString() }, { "tp", "folder" }
        , { "class_cut", cut ? "voce-cut" : "" }, { "title", f.folder_name }, { "childs", parse_folders_menu(f.folders, lvl + 1) }, { "url_open_folder", url_open_folder }
        , { "url_folder", master.url_cmd("attivita", pars: new string[,] { { "id", f.folder_id.ToString() } }) }
        , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita"
          , new string[,] { { "class_spin", color }, { "folder_id", f.folder_id.ToString() }
            , { "url_open_tasks", master.url_cmd("attivita", pars: new string[,] { { "idt", f.folder_id.ToString() } }) }
            , { "title", cc == 1 ? "una attività " + t_singolare :  cc.ToString() + " attività " + t_plurale }
            , { "c_attivita", cc.ToString() }} ) : "" }}));
    }
    return sbb.Length > 0 ? string.Format("<ul>{0}</ul>", sbb.ToString()) : "";
  }

  protected string block_level(int lvl) {
    return "item-" + (lvl == 0 ? "synch-folder" : (lvl == 1 ? "secondo" : (lvl == 2 ? "terzo" : "quarto")));
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    //if (_cmd.action == "xml") this.master.set_status_txt("caricamento dati...");

  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

}
