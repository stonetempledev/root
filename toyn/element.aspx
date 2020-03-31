<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="element.aspx.cs"
  Inherits="_element" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <link href="element.css" rel="stylesheet" type="text/css" />
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
  <script src="js/jquery.context.min.js" type="text/javascript"></script>
  <style>
    .CodeMirror
    {
      height: calc(100vh - 72px);
      margin: 5px;
    }
  </style>
  <script language="javascript" charset="UTF-8">
    var _editor = null, _sc = null, _back_cmd = null;

    var tags = {
      "!top": ["element"],
      "!attrs": {},
      element: {
        attrs: { title: null, code: null, ref: null },
        children: ["element", "title", "text", "account", "value", "link", "list", "attivita"]
      },
      title: {
        attrs: { ref: null },
        children: null
      },
      text: {
        attrs: { title: null, style: ["underline", "bold"] },
        children: null
      }, list: {
        attrs: { title: null, style: ["inline"] },
        children: ["element", "title", "text", "link", "account", "value", "list", "attivita"]
      }, account: {
        attrs: { title: null, email: null, user: null, password: null, notes: null },
        children: null
      }, value: {
        attrs: { title: null, notes: null },
        children: null
      }, link: {
        attrs: { title: null, ref: null },
        children: null
      }, attivita: {
        attrs: { title: null, priorita: ["bassa", "normale", "alta"], stato: ["la prossima", "da iniziare", "in corso", "sospesa", "fatta"] },
        children: ["element", "title", "text", "link", "account", "value", "list", "attivita"]
      }
    };

    $(document).ready(function () {
      
      // view
      if ($("#contents_doc").length) {

        // sc
        if (get_param("sc")) window.setTimeout(function () { $(window).scrollTop(parseInt(get_param("sc"))); }, 500);

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
            extraKeys: { "'<'": completeAfter,
              "'/'": completeIfAfterLt,
              "' '": completeIfInTag,
              "'='": completeIfInTag,
              "Ctrl-Space": "autocomplete",
              "Alt-F": format_doc,
              "Ctrl-S": save_doc,
              "Alt-V": torna_vista,
              "Alt-S": save_doc_vista
            }
          });

          //editor.setOption("theme", theme);

          window.setTimeout(function () {
            var totalLines = _editor.lineCount();
            _editor.autoFormatRange({ line: 0, ch: 0 }, { line: totalLines });
            _editor.focus();
            _editor.setCursor(0);
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
          set_sub_cmds([{ fnc: "back_element()", title: "Vai alla vista (CTRL+B)..." }
            , { fnc: "save_element()", title: "Salva (CTRL+S)..." }
            , { fnc: "save_element(true)", title: "Salva e torna alla vista (ALT+S)..."}]);
        }, 100);
      }
      // doc
      else {
        // sub commands
        set_sub_cmds([{ fnc: "mod_xml()", title: "Modifica XML..."}]);
      }
    });

    function init_context(id) {
      // context menu
      $(id ? "[element_id=" + id + "]" : "*[element_id]").contextmenu('#vw_menu', function (clicked, selected) {
        var tp = selected.attr("value"), id = clicked.closest('[element_id]').attr("element_id");
        if (tp == "elimina") {
          var sc = $(window).scrollTop();
          remove_element(id);
          window.setTimeout(function () { $(window).scrollTop(sc); }, 200);
        } else if (tp == "modifica") {
          window.location.href = set_param("back_cmd", get_param("cmd")
              , set_param("sc", $(window).scrollTop(), $("#url_xml_clean").val() + "+id%3a" + id));
          return false;
        }
      });
    }

    function completeAfter(cm, pred) {
      var cur = cm.getCursor();
      if (!pred || pred()) setTimeout(function () {
        if (!cm.state.completionActive)
          cm.showHint({ completeSingle: false });
      }, 100);
      return CodeMirror.Pass;
    }

    function completeIfAfterLt(cm) {
      return completeAfter(cm, function () {
        var cur = cm.getCursor();
        return cm.getRange(CodeMirror.Pos(cur.line, cur.ch - 1), cur) == "<";
      });
    }

    function completeIfInTag(cm) {
      return completeAfter(cm, function () {
        var tok = cm.getTokenAt(cm.getCursor());
        if (tok.type == "string" && (!/['"]/.test(tok.string.charAt(tok.string.length - 1)) || tok.string.length == 1)) return false;
        var inner = CodeMirror.innerMode(cm.getMode(), tok.state).state;
        return inner.tagName;
      });
    }

    function remove_element(id) {
      try {
        var result = post_data({ "action": "remove_element", "id": id });
        if (result) {
          if (result.des_result == "ok") {
            $("[element_id=" + id + "]").remove();
            $("[childs_element_id=" + id + "]").remove();
            $("[contenitor_id=" + id + "]").remove();
          } else show_alert("Attenzione!", "si è verificato un problema" + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_alert("Attenzione!", e.message); }
    }

    function change_stato_attivita(id, stato_now, in_list) {
      try {
        var result = post_data({ "action": "change_stato_attivita", "stato_now": stato_now, "id": id, "in_list": in_list });
        if (result) {
          if (result.des_result == "ok") {
            var p = $("[element_id=" + id + "]").prev();
            $("[element_id=" + id + "]").remove();
            p.after(result.html_element);
            init_context(id);
          } else show_alert("Attenzione!", "si è verificato un problema" + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_alert("Attenzione!", e.message); }
    }

    function change_priorita_attivita(id, priorita_now, in_list) {
      try {
        var result = post_data({ "action": "change_priorita_attivita", "priorita_now": priorita_now, "id": id, "in_list": in_list });
        if (result) {
          if (result.des_result == "ok") {
            var p = $("[element_id=" + id + "]").prev();
            $("[element_id=" + id + "]").remove();
            p.after(result.html_element);
            init_context(id);
          } else show_alert("Attenzione!", "si è verificato un problema" + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_alert("Attenzione!", e.message); }
    }

    function save_doc_vista() { save_element(true); }

    function torna_vista() { back_element(); }

    function save_doc() { save_element(); }

    function format_doc() {
      var pos_cursor = _editor.getCursor()
        , spos = _editor.getScrollInfo();
      format_editor(spos, pos_cursor);
    }

    function format_editor(spos, pos_cursor) {
      var tot_lines = _editor.lineCount();
      _editor.autoFormatRange({ line: 0, ch: 0 }, { line: tot_lines });
      if (spos) _editor.scrollIntoView(spos);
      if (pos_cursor) _editor.setCursor(pos_cursor);
    }

    function check_paste_xml(text_xml) {
      try {
        var result = post_data({ "action": "check_paste_xml", "text_xml": text_xml, "doc_xml": _editor.getValue(), "key_page": $("#key_page").val() });
        if (result) {
          if (result.des_result == "ok") {
            return result.contents;
          } else show_alert("Attenzione!", "i dati contenuti nell'xml non sono corrretti"
             + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_alert("Attenzione!", e.message); }
      return null;
    }

    function got_to_id(id) {
      try {
        var tot = _editor.lineCount();
        for (var i = 0; i < tot; i++) {
          var f = _editor.getLine(i).search(" id=\"" + id + ":");
          if (f >= 0) {
            _editor.focus();
            _editor.setCursor(i);
          }
        }
      }
      catch (e) { }
    }

    function mod_xml() { window.location.href = $("#url_xml").val(); return false; }

    function url_view() {
      var url_page = _back_cmd ? set_param("cmd", _back_cmd, get_page()) : $("#url_view").val();
      window.location.href = _sc ? set_param("sc", _sc, url_page) : url_page; 
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
            } else { show_alert("Salvataggio elemento", "Stai più attento, il formato XML non è corretto!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_alert("Attenzione!", e.message); end_status(); }
      return false;
    }

    function back_element() {
      try { url_view(); } catch (e) { show_alert("Attenzione!", e.message); } return false;
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
  <div class="container-fluid">
    <div class="row">
      <!-- menu -->
      <nav class="d-none d-md-block col-md-3 sidebar">
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
      </nav>
      <!-- view -->
      <div id="contents" class="col-md-9 ml-sm-auto" style='padding: 0px;' runat='server'>
        <!-- doc -->
        <div id="contents_doc" style='padding: 5px;' runat='server'>
        </div>
      </div>
      <!-- xml -->
      <div id="contents_xml" class="col-md-9 ml-sm-auto" style='padding: 0px;' runat='server'>
        <!-- doc -->
        <textarea id='doc_xml_bck' runat='server' style='display: none;'></textarea>
        <textarea id='doc_xml' runat='server'></textarea>
      </div>
    </div>
  </div>
  <div id="vw_menu" class="dropdown-menu">
    <a class="dropdown-item" value='elimina' href="#">Elimina</a> <a class="dropdown-item"
      value='modifica' href="#">Modifica XML...</a>
  </div>
</asp:Content>
