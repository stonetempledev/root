<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="test.aspx.cs"
  Inherits="test" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <style>
    [contenteditable]
    {
      border-left: 2px solid rgba(255, 0, 0, .3);
      margin-left: 3px;
      outline: 0px solid transparent;
    }
  </style>
  <script language="javascript">

    /*
    URLS
    ! https://javascript.info/selection-range
    https://usefulangle.com/post/88/javascript-creating-simple-html-text-editor
    https://canjs.com/doc/guides/recipes/text-editor.html
    */

    $(document).ready(function () {
    });

    function par_key_down(el, ev) {
      try {
        // CTRL + SPACE
        if (ev.ctrlKey && ev.key == " ") {
          if (document.getSelection().rangeCount) {
            var range = document.getSelection().getRangeAt(0);
            // cursore
            if (range.collapsed) {
              // seleziona tutto il testo dell'elemento
              var el = $(range.startContainer).closest("[tp]");
              var range = new Range();
              range.setStart(el[0].firstChild, 0);
              range.setEnd(el[0].firstChild, el[0].firstChild.length);
              document.getSelection().removeAllRanges();
              document.getSelection().addRange(range);

            } else
            // selezione
            {
              // diventa un link
              var txt = range;
              range.deleteContents();

              var nn = document.createElement('span');
              $(nn).html("Testina di Vitello");
              $(nn).attr("tp", "txt");
              range.insertNode(nn);
            }
          }
        }
      } catch (e) { $("#msg").val("ERROR\n\n" + e.Message); }
    }

    function test_click() {
      try {
        var el = $("[tp='doc']");
        if (el.attr('contenteditable')) el.removeAttr('contenteditable');
        else el.attr('contenteditable', 'true');
      } catch (e) { alert(e.Message); }
    }

    function html_click() {
      try { var el = $("[tp='doc']"); $("#msg").val(el[0].outerHTML); } catch (e) { alert(e.Message); }
    }

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class='container'>
    <div class='row'>
      <div class="col">
        <!-- doc -->
        <div id="doc_el" tp='doc' contenteditable='true' style='padding: 2px;' onkeydown='par_key_down(this, event)'>
          <div tp='chap' class='h1'>
            Titolo</div>
          <div tp='par'>
            <span tp='txt'>prima</span> <a tp='url' href='http://www.google.com'>link</a> <span
              tp='txt'>dopo</span>
          </div>
        </div>
        <!-- panel -->
        <div class='mt-5'>
          <input type='button' class='btn btn-indigo btn-sm' onclick='test_click()' value='EDITABLE'>
          </input>
          <input type='button' class='btn btn-orange btn-sm' onclick='html_click()' value='HTML'>
          </input>
        </div>
        <textarea id='msg' style='width: 100%' rows="10"></textarea>
      </div>
    </div>
  </div>
</asp:Content>
