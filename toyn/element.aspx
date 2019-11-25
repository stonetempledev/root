<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="element.aspx.cs"
  Inherits="_element" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <link href="element.css" rel="stylesheet" type="text/css" />
  <link href="js/codemirror-5.49.2/lib/codemirror.css" rel="stylesheet" type="text/css" />
  <script src="js/codemirror-5.49.2/lib/codemirror.js" type="text/javascript"></script>
  <script src="js/codemirror-5.49.2/lib/formatting.js" type="text/javascript"></script>
  <script src="js/codemirror-5.49.2/addon/fold/xml-fold.js"></script>
  <script src="js/codemirror-5.49.2/addon/edit/matchtags.js"></script>
  <script src="js/codemirror-5.49.2/xml.js" type="text/javascript"></script>
  <style>
    .CodeMirror
    {
      height: calc(100vh - 120px);
      margin: 5px;
    }
  </style>
  <script language="javascript" charset="UTF-8">
    var _editor = null;
    $(document).ready(function () {
      _editor = CodeMirror.fromTextArea(doc_xml, {
        mode: 'xml', lineNumbers: true, lineWrapping: true, readOnly: false,
        autofocus: true, matchTags: { bothTags: true }, extraKeys: { "Ctrl-J": "toMatchingTag" }
      });
      var totalLines = _editor.lineCount();
      _editor.autoFormatRange({ line: 0, ch: 0 }, { line: totalLines });
      _editor.focus();
      _editor.setCursor(0);

      _editor.on("beforeChange", function (cm, change) {
        if (change.origin === "paste") {
          var new_txt = check_paste_xml(change.text), pos_cursor = _editor.getCursor()
            , spos = _editor.getScrollInfo();
          if (new_txt == null) { change.cancel(); return; }
          if (new_txt.trim().startsWith("<")) {
            change.update(null, null, new_txt.replace(/\r/g, '').split("\n"));
            window.setTimeout(function () {
              var tot_lines = _editor.lineCount();
              _editor.autoFormatRange({ line: 0, ch: 0 }, { line: tot_lines });
              _editor.scrollIntoView(spos);
              _editor.setCursor(pos_cursor);
            }, 200);
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

    });

    function check_paste_xml(text_xml) {
      try {
        var result = post_data({ "action": "check_paste_xml", "text_xml": text_xml });
        if (result) {
          if (result.des_result == "ok") {
            return result.contents;
          } else status_err("il formato xml non è corretto"
             + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_alert("Attenzione!", e.message); }
      return null;
    }

    function mod_xml() { window.location.href = $("#url_xml").val(); return false; }

    function save_element() {
      try {
        var result = post_data({ "action": "save_element", "element_id": $("#id_element").val(), "xml": _editor.getValue() });
        if (result) {
          if (result.des_result == "ok") {
            status_text("documento salvato con successo");
            window.setTimeout(function () { window.location.href = $("#url_view").val(); }, 2000);
          } else status_err("si è verificato un errore nel salvataggio del documento"
             + (result.message ? ": " + result.message : "") + "!");
        }
      } catch (e) { show_alert("Attenzione!", e.message); }
      return false;
    }

    function back_element() {
      try {
        window.location.href = $("#url_view").val();
      } catch (e) { show_alert("Attenzione!", e.message); }
      return false;
    }

    function status_text(txt) {
      $("#lbl_status").show().text(txt);
      window.setTimeout(function () { $("#lbl_status").hide(); }, 4000);
    }

    function status_err(txt) {
      $("#lbl_status_err").show().text(txt);
      window.setTimeout(function () { $("#lbl_status_err").hide(); }, 5000);
    }


  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='url_xml' type='hidden' runat='server' />
  <input id='url_view' type='hidden' runat='server' />
  <input id='id_element' type='hidden' runat='server' />
  <div class="container-fluid">
    <div class="row">
      <!-- menu -->
      <nav class="d-none d-md-block col-md-3 sidebar">
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
      </nav>
      <!-- view -->
      <div id="contents" class="col-md-9 ml-sm-auto" style='padding: 0px;' runat='server'>
        <!-- view bar -->
        <div class='col-md-9 ml-sm-auto' style='height: 45px; display: block; position: fixed;
          padding: 5px; background-color: lightgray;'>
          <button class='btn btn-outline-light btn-sm' style='float: right;' onclick='return mod_xml()'>
            <img src="images/xml-24.png" /></button>
        </div>
        <!-- doc -->
        <div id="contents_doc" style='padding: 5px; padding-top: 40px;' runat='server'>
        </div>
      </div>
      <!-- xml -->
      <div id="contents_xml" class="col-md-9 ml-sm-auto" style='padding: 0px;' runat='server'>
        <!-- xml bar -->
        <div style='height: 45px; display: block; padding: 5px; background-color: lightgray;'>
          <button class='btn btn-outline-light btn-sm float-right' style='margin-right: 5px;'
            onclick='return save_element()'>
            <img src="images/upload-24.png" /></button>
          <button class='btn btn-outline-light btn-sm float-right' style='margin-right: 5px;'
            onclick='return back_element()'>
            <img src="images/file-24.png" /></button>
          <span id='lbl_status' class="h6 text-dark float-right" style='margin: 4px; display: none;'>
          </span><span id='lbl_status_err' class="badge badge-danger float-right" style='white-space: normal;
            padding: 5px; margin: 4px; display: none;'></span>
        </div>
        <!-- doc -->
        <textarea id='doc_xml' runat='server'></textarea>
      </div>
    </div>
</asp:Content>
