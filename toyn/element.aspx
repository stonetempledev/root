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
  <script src="js/codemirror-5.49.2/xml.js" type="text/javascript"></script>
  <style>
    .CodeMirror
    {
      height: calc(100vh - 72px);
      margin: 5px;
    }
  </style>
  <script language="javascript" charset="UTF-8">
    var _editor = null;
    $(document).ready(function () {

      // xml
      if ($("#contents_xml").length) {
        status_txt("caricamento elementi...");

        window.setTimeout(function () {

          _editor = CodeMirror.fromTextArea(doc_xml, {
            mode: 'xml', lineNumbers: true, lineWrapping: true, readOnly: false,
            autofocus: true, matchTags: { bothTags: true },
            autoCloseTags: true, extraKeys: { "Ctrl-Space": "autocomplete" }
          });

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
              if (new_txt == null) { change.cancel(); return; }
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
          set_sub_cmds([{ fnc: "back_element()", title: "Vai alla vista..." }
            , { fnc: "save_element()", title: "Salva..." }
            , { fnc: "save_element(true)", title: "Salva e torna alla vista..."}]);
        }, 100);
      }
      // doc
      else {
        // sub commands
        set_sub_cmds([{ fnc: "mod_xml()", title: "Modifica XML..."}]);
      }
    });

    function format_editor(spos, pos_cursor) {
      var tot_lines = _editor.lineCount();
      _editor.autoFormatRange({ line: 0, ch: 0 }, { line: tot_lines });
      if (spos) _editor.scrollIntoView(spos);
      if (pos_cursor) _editor.setCursor(pos_cursor);
    }

    function check_paste_xml(text_xml) {
      try {
        var result = post_data({ "action": "check_paste_xml", "text_xml": text_xml });
        if (result) {
          if (result.des_result == "ok") {
            return result.contents;
          } else show_alert("Attenzione!", "il formato xml non è corretto"
             + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_alert("Attenzione!", e.message); }
      return null;
    }

    function mod_xml() { window.location.href = $("#url_xml").val(); return false; }

    function save_element(to_doc) {
      try {
        status_txt("salvataggio in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "save_element", "element_id": $("#id_element").val()
          , "max_level": $("#max_lvl").val(), "parent_id": $("#parent_id").val(), "xml": _editor.getValue(), "xml_bck": $("#doc_xml_bck").val()
          });
          if (result) {
            if (result.des_result == "ok") {
              if (to_doc) window.setTimeout(function () { window.location.href = $("#url_view").val(); }, 2000);
              else {
                var pos_cursor = _editor.getCursor(), spos = _editor.getScrollInfo();
                _editor.getDoc().setValue(result.doc_xml.substring(10, result.doc_xml.length - 11));
                $("#menu").html(result.menu_html);
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
      try {
        window.location.href = $("#url_view").val();
      } catch (e) { show_alert("Attenzione!", e.message); }
      return false;
    }

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='url_xml' type='hidden' runat='server' />
  <input id='url_view' type='hidden' runat='server' />
  <input id='id_element' type='hidden' runat='server' />
  <input id='parent_id' type='hidden' runat='server' />
  <input id='max_lvl' type='hidden' runat='server' />
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
</asp:Content>
