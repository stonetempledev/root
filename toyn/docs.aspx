<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="docs.aspx.cs"
  Inherits="_docs" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <link href="docs.css" rel="stylesheet" type="text/css" />
  <link href="js/codemirror-5.49.2/lib/codemirror.css" rel="stylesheet" type="text/css" />
  <link href="js/codemirror-5.49.2/addon/hint/show-hint.css" rel="stylesheet" type="text/css" />
  <script src="js/codemirror-5.49.2/lib/codemirror.js" type="text/javascript"></script>
  <script src="js/codemirror-5.49.2/lib/formatting.js" type="text/javascript"></script>
  <script src="js/codemirror-5.49.2/addon/fold/xml-fold.js"></script>
  <script src="js/codemirror-5.49.2/addon/hint/xml-hint.js"></script>
  <script src="js/codemirror-5.49.2/addon/hint/show-hint.js"></script>
  <script src="js/codemirror-5.49.2/addon/edit/matchtags.js"></script>
  <script src="js/codemirror-5.49.2/addon/edit/closetag.js"></script>
  <script src="js/codemirror-5.49.2/xml_elements.js" type="text/javascript"></script>
  <script src="js/jquery.context.js" type="text/javascript"></script>
  <style>
    .CodeMirror
    {
      height: calc(100vh - 60px);
      margin: 3px;
    }
  </style>
  <script language="javascript">
    var _editor = null, _sc = null, _back_cmd = null, _vs = null;

    var tags = {
      "!top": ["element"],
      "!attrs": {},
      element: {
        attrs: { title: null, code: null, ref: null },
        children: ["element", "title", "text", "account", "value", "link", "list", "attivita", "par"]
      },
      par: {
        attrs: { title: null },
        children: ["title", "text"]
      },
      title: {
        attrs: { ref: null },
        children: null
      },
      text: {
        attrs: { title: null, style: ["underline", "bold"] },
        children: null
      }, list: {
        attrs: { title: null, style: ["inline"], closed: ["true"] },
        children: ["element", "title", "text", "link", "account", "value", "list", "attivita", "par"]
      }, account: {
        attrs: { title: null, email: null, user: null, password: null, notes: null },
        children: null
      }, value: {
        attrs: { title: null, notes: null },
        children: null
      }, link: {
        attrs: { title: null, ref: null, notes: null },
        children: null
      }, attivita: {
        attrs: { title: null, priorita: ["bassa", "normale", "alta"], stato: ["la prossima", "da iniziare", "in corso", "sospesa", "fatta"] },
        children: ["element", "title", "text", "link", "account", "value", "list", "attivita", "par"]
      }, code: {
        attrs: { title: null, notes: null, height: null },
        children: null
      }
    };

    $(document).ready(function () {

      // submenu
      $('.dropdown-menu a.dropdown-toggle').on('click', function (e) {
        $(this).closest('[menu=true]').find('.dropdown-submenu .dropdown-menu').hide();
        $(this).next('.dropdown-menu').show();
        return false;
      });

      // view
      if ($("#contents_doc").length) {

        _vs = get_param("vs");

        // sc
        if ($("#scroll_pos").val()) window.setTimeout(function () { $(window).scrollTop(parseInt($("#scroll_pos").val())); }, 500);

        // context menu
        init_context();
      }

      // xml
      if ($("#contents_xml").length) {
        status_txt("caricamento elementi...");

        _sc = get_param("sc"); _back_cmd = get_param("back_cmd");

        window.setTimeout(function () {

          _editor = CodeMirror.fromTextArea(doc_xml, {
            mode: 'xml', lineNumbers: false, lineWrapping: true, readOnly: false,
            autofocus: true, matchTags: { bothTags: true },
            autoCloseTags: true, hintOptions: { schemaInfo: tags },
            extraKeys: { "'<'": complete_after,
              "'/'": complete_afterlt,
              "' '": complete_intag,
              "'='": complete_intag,
              "Ctrl-Space": "autocomplete",
              "Alt-F": format_doc,
              "Ctrl-S": save_doc,
              "Alt-V": torna_vista,
              "Alt-S": save_doc_vista
            }
          });

          window.setTimeout(function () {
            format_editor(null, 0, true);
            end_status_to();
          }, 100);

          _editor.on("beforeChange", function (cm, change) {
            if (change.origin === "paste") {
              var new_txt = check_paste_xml(change.text), pos_cursor = _editor.getCursor()
                , spos = _editor.getScrollInfo();
              if (new_txt == null) {
                change.cancel(); return;
              }
              if (new_txt.trim().startsWith("<")) {
                change.update(null, null, new_txt.replace(/\r/g, '').split("\n"));
                window.setTimeout(function () {
                  format_editor(spos, pos_cursor);
                }, 100);
              }
            }

          });

          _editor.on("copy", function (cm, e) {
            try {
              var tm = _editor.getAllMarks();
              if (tm.length) {
                //var textContent = _editor.getRange(tm[0].find().from, tm[1].find().to)
                _editor.setSelection(tm[0].find().from, tm[1].find().to);
              }
              //e.preventDefault();

            } catch (e) { alert(e.message); }
          });

          // sub commands
          set_sub_cmds([{ fnc: "back_element()", title: "Vai alla vista (ALT+V)..." }
            , { fnc: "save_element()", title: "Salva (CTRL+S)..." }
            , { fnc: "save_element(true)", title: "Salva e torna alla vista (ALT+S)..."}]);
        }, 100);
      }
      // doc
      else {
        // sub commands
        var cmds = [];
        cmds.push({ fnc: "mod_xml()", title: "Modifica XML..." });
        if ($("#there_stored").val() == "1") {
          if (_vs == "1") cmds.push({ fnc: "view_stored(false)", title: "Nascondi storicizzati..." });
          else cmds.push({ fnc: "view_stored()", title: "Vedi elementi storicizzati..." });
        }
        set_sub_cmds(cmds);
      }
    });

    function init_context() {

      // tutti gli elementi
      var sel = "*[element_id][type_element!='attivita']:not([init_context])";
      if (!is_mobile()) {
        $(sel).contextmenu('#vw_menu', menu_base, pre_menu_base);
      } else { $(sel).dblclick(function ($e) { show_context($e, "#vw_menu", menu_base, pre_menu_base, 5); }); }
      $(sel).attr("init_context", "true");

      // attivita
      var sel = "*[element_id][type_element='attivita']:not([init_context])", sel_click = sel + " [clickable=true]";
      if (!is_mobile()) {
        $(sel_click).click(function () {
          var e = $(this).closest('[element_id]');
          change_stato_attivita(e.attr("element_id"), e.attr("stato"), e.attr("in_list") == "true");
        });
        $(sel).contextmenu('#vw_menu_attivita', menu_attivita, pre_menu_attivita);
      } else {
        $(sel).dblclick(function ($e) { show_context($e, "#vw_menu_attivita", menu_attivita, pre_menu_attivita, 5); });
      }
      $(sel).attr("init_context", "true");

      if (is_mobile()) { $(document).click(function () { $("[menu=true],[sub_menu=true]").hide(); }); }

    }

    function check_menu_cut(el, menu) {
      var ids = $("#cache_ids").val(), type = el.closest('[element_id]').attr("type_element");

      if (ids) {
        menu.find("[tp-menu=div-incolla],[tp-menu=incolla]").show();
        menu.find("[value='azzera']").text("togli dalla copia i " + ids.split(',').length + " oggetti...");
        if (type != "element" && type != "list" && type != "attivita")
          menu.find("[value='incolla_dentro']").hide();
        else menu.find("[value='incolla_dentro']").show();
      }
      else menu.find("[tp-menu=div-incolla],[tp-menu=incolla]").hide();

      if (type != "title") menu.find("[menu-value='copia_fine'],[menu-value='taglia_fine']").hide();
      else menu.find("[menu-value='copia_fine'],[menu-value='taglia_fine']").show();
    }

    function menu_cut(clicked, selected) {
      try {
        var tp = selected_val(selected), id = clicked.closest('[element_id]').attr("element_id");
        if (tp == "copia" || tp == "taglia") {
          var result = post_data({ "action": tp == "copia" ? "copy" : "cut", "id": id });
          if (result) {
            if (result.des_result == "ok") {
              $("#cache_ids").val(result.contents);
            } else show_danger("Attenzione!", result.message);
          }
        } else if (tp == "sposta_su" || tp == "sposta_fondo" || tp == "sposta_alto") {
          var result = post_data({ "action": tp == "sposta_alto" ? "move_first" : (tp == "sposta_su" ? "move_up" : "move_end"), "id": id });
          if (result) {
            if (result.des_result == "ok") {
              if (result.contents == "1") {
                reload_contents(result.html_element);
                reload_menu(result.menu_html);
              }
            } else show_danger("Attenzione!", result.message);
          }
        } else if (tp == "copia_fine" || tp == "taglia_fine") {
          status_txt("copia elementi in corso...")
          window.setTimeout(function () {
            var result = post_data({ "action": tp == "copia_fine" ? "copy_end" : "cut_end", "id": id });
            if (result) {
              if (result.des_result == "ok") {
                $("#cache_ids").val(result.contents);
              } else show_danger("Attenzione!", result.message);
            }
            end_status_to(300);
          }, 200);

        } else if (tp == "azzera") {
          var result = post_data({ "action": "reset_cache_ids" });
          if (result) {
            if (result.des_result == "ok") {
              $("#cache_ids").val("");
            } else show_danger("Attenzione!", result.message);
          }
        } else if (tp == "incolla_dopo" || tp == "incolla_prima" || tp == "incolla_dentro") {
          var result = post_data({ "action": tp == "incolla_dopo" ? "paste_after"
            : (tp == "incolla_prima" ? "paste_before" : "paste_inside"), "id": id
          });
          if (result) {
            if (result.des_result == "ok") {
              $("#cache_ids").val(result.contents);
              reload_contents(result.html_element);
              reload_menu(result.menu_html);
            } else show_danger("Attenzione!", result.message);
          }
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function menu_base(clicked, selected) {

      var tp = selected_val(selected), id = clicked.closest('[element_id]').attr("element_id");
      if (tp == "aggiorna") {
        edit_element(id); return;
      }
      else if (tp == "elimina") {
        var sc = $(window).scrollTop();
        remove_element(id);
        window.setTimeout(function () { $(window).scrollTop(sc); }, 200);
        return;
      } if (tp == "storicizza") {
        store_element(id, true); return;
      } if (tp == "destoricizza") {
        store_element(id, false); return;
      } else if (tp == "modifica") {
        window.location.href = set_param("vs", get_param("vs"), set_param("back_cmd", get_param("cmd")
            , set_param("sc", $(window).scrollTop(), $("#url_xml_clean").val() + "+id%3a" + id)));
        return;
      }
      menu_cut(clicked, selected);
    }

    function check_menu_storicizza(el, menu) {
      var stored = el.closest('[element_id]').attr("stored")
        , parent_stored = el.closest('[element_id]').attr("parent_stored");

      if (parent_stored == "true") {
        menu.find("[menu-value='storicizza'],[menu-value='destoricizza']").hide();
        return;
      }
      else {
        menu.find("[menu-value='storicizza'],[menu-value='destoricizza']").show();
      }

      // storicizza
      if (stored == "true") {
        var item = menu.find("[menu-value='storicizza']");
        item.attr("menu-value", "destoricizza").attr("title", "toglilo dallo scatolone");
        item.find("i").removeClass().addClass("fas fa-box-open");
      }
      else {
        var item = menu.find("[menu-value='destoricizza']");
        item.attr("menu-value", "storicizza").attr("title", "mettilo nello scatolone");
        item.find("i").removeClass().addClass("fas fa-archive");
      }

    }

    function pre_menu_base(el, menu) {
      var title = el.closest('[element_id]').attr("title_element");
      menu.find(".dropdown-item").show();

      // title
      if (title) menu.find("[title_row=true]").text(title.toUpperCase());
      else menu.find("[title_row=true]").text("Menù");

      // storicizza
      check_menu_storicizza(el, menu);

      // copia e incolla
      check_menu_cut(el, menu);
    }

    function pre_menu_attivita(el, menu) {
      var id = el.closest('[element_id]').attr("element_id"), stato = el.closest('[element_id]').attr("stato")
          , priorita = el.closest('[element_id]').attr("priorita");
      priorita = priorita == "" ? "normale" : priorita;
      stato = stato == "" ? "da iniziare" : stato;
      menu.find(".dropdown-item").show();

      // stato, priorita
      menu.find("[value='set,stato," + stato + "']").hide();
      menu.find("[value='set,priorita," + priorita + "']").hide();

      // storicizza
      check_menu_storicizza(el, menu);

      // copia e incolla
      check_menu_cut(el, menu);
    }

    function menu_attivita(clicked, selected) {
      var tp = selected_val(selected), id = clicked.closest('[element_id]').attr("element_id")
          , tps = tp.split(','), stato = clicked.closest('[element_id]').attr("stato")
          , priorita = clicked.closest('[element_id]').attr("priorita")
          , in_list = clicked.closest('[element_id]').attr("in_list");

      if (tps.length == 3 && tps[0] == "set" && tps[1] == "stato") {
        change_stato_attivita(id, stato, in_list == "true", tps[2]);
        return;
      } else if (tps.length == 3 && tps[0] == "set" && tps[1] == "priorita") {
        change_priorita_attivita(id, priorita, in_list == "true", tps[2]);
        return;
      }
      menu_base(clicked, selected);
    }

    function complete_after(cm, pred) {
      var cur = cm.getCursor();
      if (!pred || pred()) setTimeout(function () {
        if (!cm.state.completionActive)
          cm.showHint({ completeSingle: false });
      }, 100);
      return CodeMirror.Pass;
    }

    function complete_afterlt(cm) {
      return complete_after(cm, function () {
        var cur = cm.getCursor();
        return cm.getRange(CodeMirror.Pos(cur.line, cur.ch - 1), cur) == "<";
      });
    }

    function complete_intag(cm) {
      return complete_after(cm, function () {
        var tok = cm.getTokenAt(cm.getCursor());
        if (tok.type == "string" && (!/['"]/.test(tok.string.charAt(tok.string.length - 1)) || tok.string.length == 1)) return false;
        var inner = CodeMirror.innerMode(cm.getMode(), tok.state).state;
        return inner.tagName;
      });
    }

    function edit_element(id) {
      var e = $("[element_id=" + id + "]"), type_element = e.attr("type_element"), des_element = e.attr("title_element");

      if (type_element == "account") {
        show_dyn_dlg({ title: "Aggiorna " + des_element
          , rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]").text() }
            , { id: "notes", label: "Note Particolari", valore: e.find("[tp-val=notes]").text() }
            , { id: "user_pass", label: "Utente/Password", valore: e.find("[tp-val=user_pass]").text() }
            , { id: "email", label: "Email", valore: e.find("[tp-val=email]").text()}]
          , on_ok: function () { update_account(id, val_dyn("title"), val_dyn("notes"), val_dyn("user_pass"), val_dyn("email")); }
        });
      } else if (type_element == "attivita") {
        show_dyn_dlg({ title: "Aggiorna " + des_element
          , rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]:first").text()}]
          , on_ok: function () { update_attivita(id, val_dyn("title")); }
        });
      } else if (type_element == "code") {
        show_dyn_dlg({ title: "Aggiorna " + des_element
          , rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]").text() }
            , { id: "notes", label: "Note Particolari", valore: e.find("[tp-val=notes]").text()}]
          , on_ok: function () { update_code(id, val_dyn("title"), val_dyn("notes")); }
        });
      } else if (type_element == "element") {
        show_dyn_dlg({ title: "Aggiorna " + des_element
          , rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]:first").text() }
            , { id: "code", label: "Codice univoco", valore: e.find("[tp-val=code]:first").text() }
            , { id: "type", label: "Tipo elemento", valore: e.find("[tp-val=type]:first").text() }
            , { id: "ref", label: "Web Link/Comando", valore: e.find("[tp-val=ref]:first").text() }
            , { id: "notes", label: "Note Particolari", valore: e.find("[tp-val=notes]:first").text()}]
          , on_ok: function () { update_element(id, val_dyn("title"), val_dyn("code"), val_dyn("type"), val_dyn("ref"), val_dyn("notes")); }
        });
      } else if (type_element == "link") {
        show_dyn_dlg({ title: "Aggiorna " + des_element
          , rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]").text() }
            , { id: "ref", label: "Web Link/Comando", valore: e.find("[tp-val=ref]").text() }
            , { id: "notes", label: "Note Particolari", valore: e.find("[tp-val=notes]").text()}]
          , on_ok: function () { update_link(id, val_dyn("title"), val_dyn("ref"), val_dyn("notes")); }
        });
      } else if (type_element == "list") {
        show_dyn_dlg({ title: "Aggiorna " + des_element
          , rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]:first").text() }
            , { id: "notes", label: "Note Particolari", valore: e.find("[tp-val=notes]:first").text()}]
          , on_ok: function () { update_list(id, val_dyn("title"), val_dyn("notes")); }
        });
      } else if (type_element == "par") {
        show_dyn_dlg({ title: "Aggiorna " + des_element
          , rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]:first").text() }
            , { id: "ref", label: "Web Link/Comando", valore: e.find("[tp-val=ref]:first").text()}]
          , on_ok: function () { update_par(id, val_dyn("title"), val_dyn("ref")); }
        });
      } else if (type_element == "text") {
        show_dyn_dlg({ title: "Aggiorna " + des_element, rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]").text() }
          , { id: "stile", label: "Stile", valore: e.attr("styles"), type: "select"
            , values: [{ title: "Nessuno", value: "" }, { title: "Sottolineato", value: "underline" }
            , { title: "Grassetto", value: "bold" }, { title: "Corsivo", value: "italic"}]
          }, { id: "content", label: "Testo", valore: e.find("[tp-val=content]").text(), type: "textarea"}]
          , on_ok: function () { update_text(id, val_dyn("title"), val_dyn("stile"), val_dyn("content")); }
        });
      } else if (type_element == "title") {
        show_dyn_dlg({ title: "Aggiorna " + des_element, rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]").text() }
          , { id: "ref", label: "Web Link/Comando", valore: e.find("[tp-val=ref]").text()}]
          , on_ok: function () { update_title(id, val_dyn("title"), val_dyn("ref")); }
        });
      } else if (type_element == "value") {
        show_dyn_dlg({ title: "Aggiorna " + des_element, rows: [{ id: "title", label: "Titolo", valore: e.find("[tp-val=title]").text() }
          , { id: "notes", label: "Note Particolari", valore: e.find("[tp-val=notes]").text() }
          , { id: "content", label: "Valore", valore: e.find("[tp-val=content]").text()}]
          , on_ok: function () { update_value(id, val_dyn("title"), val_dyn("notes"), val_dyn("content")); }
        });
      }
    }

    function update_account(id, title, notes, up, email) {
      var e = $("[element_id=" + id + "]");
      var res = post_action({ "action": "update_account", "id": id, "title": title, "notes": notes, "user_pass": up, "email": email
        , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });

      if (res) reload_html_element(id, res.html_element, false, false, false);
    }

    function update_attivita(id, title) {
      var e = $("[element_id=" + id + "]");
      var res = post_action({ "action": "update_attivita", "id": id, "title": title
        , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });

      if (res) reload_html_element(id, res.html_element, false, false, false);
    }

    function update_code(id, title, notes) {
      var e = $("[element_id=" + id + "]");
      var res = post_action({ "action": "update_code", "id": id, "title": title, "notes": notes
        , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });

      if (res) reload_html_element(id, res.html_element, false, false, false);
    }

    function update_element(id, title, code, type, ref, notes) {
      var e = $("[element_id=" + id + "]"), me = $("[menu_id=" + id + "]")
        , res = post_action({ "action": "update_element", "id": id, "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored")
          , "no_opacity": e.attr("no_opacity"), "title": title, "code": code, "type": type, "ref": ref, "notes": notes
        });
      if (res) {
        reload_html_element(id, res.html_element, false, false, false);
        if (me.length > 0) { p = me.prev(); me.remove(); p.after(res.menu_html); }
      }
    }

    function update_link(id, title, ref, notes) {
      var e = $("[element_id=" + id + "]");
      var res = post_action({ "action": "update_link", "id": id, "title": title, "ref": ref, "notes": notes
          , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });
      if (res) reload_html_element(id, res.html_element, false, false, false);
    }

    function update_list(id, title, notes) {
      var e = $("[element_id=" + id + "]"), me = $("[menu_id=" + id + "]");
      var res = post_action({ "action": "update_list", "id": id, "title": title, "notes": notes
          , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });
      if (res) {
        reload_html_element(id, res.html_element, false, false, false);
        if (me.length > 0) { p = me.prev(); me.remove(); p.after(res.menu_html); }
      }
    }

    function update_par(id, title, ref) {
      var e = $("[element_id=" + id + "]");
      var res = post_action({ "action": "update_par", "id": id, "title": title, "ref": ref
          , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });
      if (res) {
        var mep = $("[menu_id=" + id + "]").length > 0 ? $("[menu_id=" + id + "]").prev() : null;
        reload_html_element(id, res.html_element, true, true, false);
        if (mep != null) mep.after(res.menu_html);
      }
    }

    function update_text(id, title, stile, content) {
      var e = $("[element_id=" + id + "]");
      var res = post_action({ "action": "update_text", "id": id, "title": title, "stile": stile, "content": content
        , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });
      if (res) reload_html_element(id, res.html_element, false, false, false);
    }

    function update_title(id, title, ref) {
      var e = $("[element_id=" + id + "]"), me = $("[menu_id=" + id + "]");
      var res = post_action({ "action": "update_title", "id": id, "title": title, "ref": ref
          , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });
      if (res) {
        reload_html_element(id, res.html_element, false, false, false);
        if (me.length > 0) { p = me.prev(); me.remove(); p.after(res.menu_html); }
      }
    }

    function update_value(id, title, notes, content) {
      var e = $("[element_id=" + id + "]");
      var res = post_action({ "action": "update_value", "id": id, "title": title, "notes": notes, "content": content
          , "in_list": e.attr("in_list"), "parent_stored": e.attr("parent_stored"), "no_opacity": e.attr("no_opacity")
      });
      if (res) reload_html_element(id, res.html_element, false, false, false);
    }

    function remove_element(id) {
      show_warning_yn("Attenzione!", "Sei sicuro di voler eliminare l'elemento?"
        , function () { remove_element2(id) });
    }

    function remove_element2(id) {
      try {
        var result = post_data({ "action": "remove_element", "id": id });
        if (result) {
          if (result.des_result == "ok") {
            delete_element(id);
            $("#cache_ids").val(result.vars["cache_ids"]);
          } else show_danger("Attenzione!", result.message);
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function delete_element(id, del_childs, del_menu, del_contenitor) {
      if (check_bool(del_menu, true)) {
        $("[menu_id=" + id + "]").remove();
        if (check_bool(del_childs, true)) $("[menu_childs_id=" + id + "]").remove();
      }
      $("[element_id=" + id + "]").remove();

      if (check_bool(del_contenitor, true)) $("[contenitor_id=" + id + "]").remove();
      if (check_bool(del_childs, true)) $("[childs_element_id=" + id + "]").remove();
    }

    function store_element(id, store) {
      try {
        var e = $("[element_id=" + id + "]");
        var result = post_data({ "action": store ? "store_element" : "unstore_element", "id": id, "in_list": e.attr("in_list") == "true"
          , "parent_stored": e.attr("parent_stored") == "true", "no_opacity": e.attr("no_opacity") == "true"
        });
        if (result) {
          if (result.des_result == "ok") {
            reload_html_element(id, result.html_element);
            reload_menu(result.menu_html);
          } else show_danger("Attenzione!", result.message);
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function reload_html_element(id, html, childs, del_menu, del_contenitor) {
      var p = $("[element_id=" + id + "]").prev(), c = $("[contenitor_id=" + id + "]").length > 0
        ? (check_bool(del_contenitor, true) ? $("[contenitor_id=" + id + "]").prev() : $("[contenitor_id=" + id + "]")) : null;

      if (c != null && check_bool(del_contenitor) == false)
        $("[element_id=" + id + "]").after("<div tmp='" + id + "'></div>");

      delete_element(id, childs, del_menu, del_contenitor);
      if (c != null) {
        if (check_bool(del_contenitor) == false) {
          $("[tmp=" + id + "]").after(html); $("[tmp=" + id + "]").remove();
        } else c.after(html);
      } else p.after(html);
      init_context();
    }

    function reload_contents(html) { $("#contents_doc").html(html); window.setTimeout(function () { init_context(); }, 500); }

    function reload_menu(html_menu) { $("#menu").html(html_menu); }

    function change_stato_attivita(id, stato_now, in_list, stato_new) {
      try {
        var result = post_data({ "action": "change_stato_attivita", "stato_now": stato_now
          , "stato_new": stato_new ? stato_new : "", "id": id, "in_list": in_list
        });
        if (result) {
          if (result.des_result == "ok") {
            var p = $("[element_id=" + id + "]").prev();
            $("[element_id=" + id + "]").remove();
            p.after(result.html_element);
            init_context();
          } else show_danger("Attenzione!", result.message);
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function change_priorita_attivita(id, priorita_now, in_list, priorita_new) {
      try {
        var result = post_data({ "action": "change_priorita_attivita", "priorita_now": priorita_now
          , "priorita_new": priorita_new ? priorita_new : "", "id": id, "in_list": in_list
        });
        if (result) {
          if (result.des_result == "ok") {
            var p = $("[element_id=" + id + "]").prev();
            $("[element_id=" + id + "]").remove();
            p.after(result.html_element);
            init_context();
          } else show_danger("Attenzione!", result.message);
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function save_doc_vista() { save_element(true); }

    function torna_vista() { back_element(); }

    function save_doc() { save_element(); }

    function format_doc() {
      var pos_cursor = _editor.getCursor()
        , spos = _editor.getScrollInfo();
      format_editor(spos, pos_cursor);
    }

    function format_editor(spos, pos_cursor, focus) {
      var tot_lines = _editor.lineCount();
      _editor.autoFormatRange({ line: 0, ch: 0 }, { line: tot_lines });
      if (spos != null) _editor.scrollIntoView(spos);
      if (pos_cursor != null) _editor.setCursor(pos_cursor);
      if (focus) _editor.focus();
    }

    function check_paste_xml(text_xml) {
      try {
        var result = post_data({ "action": "check_paste_xml", "text_xml": text_xml, "doc_xml": _editor.getValue(), "key_page": $("#key_page").val() });
        if (result) {
          if (result.des_result == "ok") {
            return result.contents;
          } else show_danger("Attenzione!", "i dati contenuti nell'xml non sono corrretti"
             + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
      return null;
    }

    function go_to_id(id) {
      try {
        var tot = _editor.lineCount();
        for (var i = 0; i < tot; i++) {
          var f = _editor.getLine(i).search(" _id=\"" + id + ":");
          if (f >= 0) {
            _editor.focus();
            _editor.setCursor(i);
          }
        }
      }
      catch (e) { }
    }

    function mod_xml() { window.location.href = set_param("vs", get_param("vs"), $("#url_xml").val()); return false; }

    function view_stored(sh) {
      var url_page = window.location.href;
      if (_sc) url_page = set_param("sc", _sc, url_page);
      url_page = set_param("vs", sh == null || sh ? "1" : "", url_page);
      window.location.href = url_page;
    }

    function url_view() {
      var url_page = _back_cmd ? set_param("cmd", _back_cmd, get_page()) : $("#url_view").val();
      window.location.href = set_param("vs", get_param("vs"), (_sc ? set_param("sc", _sc, url_page) : url_page));
    }

    function save_element(to_doc) {
      try {
        status_txt("salvataggio in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "save_element", "element_id": $("#id_element").val()
          , "max_level": $("#max_lvl").val(), "parent_id": $("#parent_id").val()
          , "first_order": $("#first_order").val(), "last_order": $("#last_order").val()
          , "xml": _editor.getValue(), "xml_bck": $("#doc_xml_bck").val(), "key_page": $("#key_page").val()
          });
          if (result) {
            if (result.des_result == "ok") {
              if (to_doc) window.setTimeout(function () { url_view(); }, 2000);
              else {
                var pos_cursor = _editor.getCursor(), spos = _editor.getScrollInfo();
                if (result.doc_xml != "<elements />" && result.doc_xml != "<elements/>") {
                  var doc = result.doc_xml.substring(10, result.doc_xml.length - 11);
                  _editor.getDoc().setValue(doc);
                  $("#doc_xml_bck").val(doc);
                } else $("#doc_xml_bck").val("");

                $("#menu").html(result.menu_html);

                $("#first_order").val(result.vars["first_order"]);
                $("#last_order").val(result.vars["last_order"]);

                window.setTimeout(function () {
                  format_editor(spos, pos_cursor);
                  status_txt("documento salvato con successo");
                  end_status_to(2000);
                }, 100);
              }
            } else { show_warning("Salvataggio elemento", "Stai più attento, il formato XML non è corretto!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_danger("Attenzione!", e.message); end_status(); }
      return false;
    }

    function back_element() {
      try { url_view(); } catch (e) { show_danger("Attenzione!", e.message); } return false;
    }

    function show_childs(id) {
      $("[childs_element_id=" + id + "]").show();
      $("[element_id=" + id + "]").css('color', '');
      $("[open_id=" + id + "]").hide();
    }

    var _code = null;
    function focus_code(id) {
      _code = $("[code_id=" + id + "]").val();
    }

    function blur_code(id) {
      var code_2 = $("[code_id=" + id + "]").val();
      if (_code != code_2) { _code = code_2; save_code(id, _code); }
    }

    function save_code(id, txt) {
      try {
        $("[code_id=" + id + "]").css("border-color", "lightgreen").css("box-shadow", "0 0 2px lightgreen");

        window.setTimeout(function () {

          var result = post_data({ "action": "save_code", "code": txt, "id": id });
          if (result) {
            if (result.des_result == "ok")
              $("[code_id=" + id + "]").css("border-color", "").css("box-shadow", "");
            else {
              $("[code_id=" + id + "]").css("border-color", "tomato").css("box-shadow", "0 0 2px tomato");
              show_danger("Attenzione!", "cè stato un problema nel salvataggio del codice sorgente!"
                + (result.message ? ": " + result.message : "") + "!");
            }
          }
        }, 200);

      } catch (e) { show_danger("Attenzione!", e.message); }
    }

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='url_xml' type='hidden' runat='server' />
  <input id='url_xml_clean' type='hidden' runat='server' />
  <input id='url_view' type='hidden' runat='server' />
  <input id='id_element' type='hidden' runat='server' />
  <input id='parent_id' type='hidden' runat='server' />
  <input id='first_order' type='hidden' runat='server' />
  <input id='last_order' type='hidden' runat='server' />
  <input id='max_lvl' type='hidden' runat='server' />
  <input id='key_page' type='hidden' runat='server' />
  <input id='cache_ids' type='hidden' runat='server' />
  <input id='scroll_pos' type='hidden' runat='server' />
  <input id='there_stored' type='hidden' runat='server' />
  <div class="container-fluid">
    <div class="row">
      <!-- sidebar menu -->
      <nav sidebar-tp='menu' class='d-none' sidebar-init='show'>
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
      </nav>
      <!-- view -->
      <div id="contents" sidebar-tp='' style='padding: 0px;' runat='server'>
        <!-- doc -->
        <div id="contents_doc" style='padding: 2px;' runat='server'>
        </div>
      </div>
      <!-- xml -->
      <div id="contents_xml" sidebar-tp='' style='padding: 0px;' runat='server'>
        <!-- doc -->
        <textarea id='doc_xml_bck' runat='server' style='display: none;'></textarea>
        <textarea id='doc_xml' runat='server'></textarea>
      </div>
      <!-- sidebar icon -->
      <div sidebar-tp='sh' onclick='sh_side_menu()'>
        <i sidebar-tp='icon'></i>
      </div>
    </div>
  </div>
  <!-- menu generico -->
  <ul id="vw_menu" menu='true' class="dropdown-menu">
    <li>
      <h4 title_row='true' class="dropdown-header" style='color: blue; background-color: whitesmoke;'>
        Menù</h4>
    </li>
    <li class="dropdown-item icon-item"><span class='icon-menu' title='modifica...' menu-value='aggiorna'>
      <i class="fas fa-pen text-info"></i></span><span class='icon-menu text-danger' title='cancella elemento'
        menu-value='elimina'><i class="fas fa-trash-alt"></i></span><span class='icon-menu'
          title='mettilo nello scatolone' menu-value='storicizza'><i class="fas fa-archive">
          </i></span><span class='icon-menu' title='modifica xml...' menu-value='modifica'><i
            class="fas fa-file-code"></i></span></li>
    <li class="dropdown-item icon-item"><span class='icon-menu' title='sposta su' menu-value='sposta_su'>
      <i class="fas fa-arrow-up"></i></span><span class='icon-menu' title='mettilo in cima'
        menu-value='sposta_alto'><i class="fas fa-angle-double-up"></i></span><span class='icon-menu'
          title='mettilo in fondo' menu-value='sposta_fondo'><i class="fas fa-angle-double-down">
          </i></span></li>
    <li class="dropdown-item icon-item"><span class='icon-menu' title='copia elemento'
      menu-value='copia'><i class='fas fa-copy'></i></span><span class='icon-menu text-warning'
        title='copia fino in fondo' menu-value='copia_fine'><i class='fas fa-copy'></i>
      </span><span class='icon-menu' title='taglia elemento' menu-value='taglia'><i class='fas fa-cut'>
      </i></span><span class='icon-menu text-warning' title='taglia fino in fondo' menu-value='taglia_fine'>
        <i class='fas fa-cut'></i></span></li>
    <!-- incolla -->
    <li tp-menu='div-incolla'>
      <div class="dropdown-divider">
      </div>
    </li>
    <li tp-menu='incolla' class="dropdown-submenu"><a class="dropdown-item dropdown-toggle"
      style='color: #17202A; font-weight: bold;' href="#">Incolla</a>
      <ul class="dropdown-menu" sub_menu='true'>
        <li><span class="dropdown-item" value='azzera'>togli dalla copia i 5 oggetti copiati...</span></li>
        <li><span class="dropdown-item" value='incolla_prima'>incolla prima...</span></li>
        <li><span class="dropdown-item" value='incolla_dopo'>incolla dopo...</span></li>
        <li><span class="dropdown-item" value='incolla_dentro'>incolla dentro...</span></li>
      </ul>
    </li>
  </ul>
  <!-- menu attività-->
  <ul id="vw_menu_attivita" menu='true' class="dropdown-menu">
    <li>
      <h4 class="dropdown-header" style='color: blue; background-color: whitesmoke;'>
        ATTIVITÀ</h4>
    </li>
    <li class="dropdown-item icon-item"><span class='icon-menu' title='modifica...' menu-value='aggiorna'>
      <i class="fas fa-pen text-info"></i></span><span class='icon-menu text-danger' title='cancella elemento'
        menu-value='elimina'><i class="fas fa-trash-alt"></i></span><span class='icon-menu'
          title='mettilo nello scatolone' menu-value='storicizza'><i class="fas fa-archive">
          </i></span><span class='icon-menu' title='modifica xml...' menu-value='modifica'><i
            class="fas fa-file-code"></i></span></li>
    <li class="dropdown-item icon-item"><span class='icon-menu' title='sposta su' menu-value='sposta_su'>
      <i class="fas fa-arrow-up"></i></span><span class='icon-menu' title='mettilo in cima'
        menu-value='sposta_alto'><i class="fas fa-angle-double-up"></i></span><span class='icon-menu'
          title='mettilo in fondo' menu-value='sposta_fondo'><i class="fas fa-angle-double-down">
          </i></span></li>
    <li class="dropdown-item icon-item"><span class='icon-menu' title='copia elemento'
      menu-value='copia'><i class='fas fa-copy'></i></span><span class='icon-menu' title='taglia elemento'
        menu-value='taglia'><i class='fas fa-cut'></i></span></li>
    <!-- incolla -->
    <li tp-menu='div-incolla'>
      <div class="dropdown-divider">
      </div>
    </li>
    <li tp-menu='incolla' class="dropdown-submenu"><a class="dropdown-item dropdown-toggle"
      style='color: #17202A; font-weight: bold;' href="#">Incolla</a>
      <ul class="dropdown-menu" sub_menu='true'>
        <li><span class="dropdown-item" value='azzera'>togli 5 oggetti copiati...</span></li>
        <li><span class="dropdown-item" value='incolla_prima'>incolla prima...</span></li>
        <li><span class="dropdown-item" value='incolla_dopo'>incolla dopo...</span></li>
        <li><span class="dropdown-item" value='incolla_dentro'>incolla dentro...</span></li>
      </ul>
    </li>
    <li>
      <div class="dropdown-divider">
      </div>
    </li>
    <!-- stato attivita -->
    <li class="dropdown-submenu"><a class="dropdown-item dropdown-toggle" style='color: #17202A;
      font-weight: bold;' href="#">Stato attività</a>
      <ul class="dropdown-menu" sub_menu='true'>
        <li><span class="dropdown-item" value="set,stato,da iniziare">da iniziare...</span></li>
        <li><span class="dropdown-item" value="set,stato,la prossima">la prossima...</span></li>
        <li><span class="dropdown-item" value="set,stato,in corso">in corso...</span></li>
        <li><span class="dropdown-item" value="set,stato,sospesa">sospesa...</span></li>
        <li><span class="dropdown-item" value="set,stato,fatta">fatta...</span></li></ul>
    </li>
    <div class="dropdown-divider">
    </div>
    <!-- priorita -->
    <li class="dropdown-submenu"><a class="dropdown-item dropdown-toggle" style='color: #17202A;
      font-weight: bold;' href="#">Priorità attività</a>
      <ul class="dropdown-menu" sub_menu='true'>
        <li><span class="dropdown-item" value="set,priorita,bassa">bassa...</span></li>
        <li><span class="dropdown-item" value="set,priorita,normale">normale...</span></li>
        <li><span class="dropdown-item" value="set,priorita,alta">alta...</span></li>
      </ul>
    </li>
  </ul>
</asp:Content>
