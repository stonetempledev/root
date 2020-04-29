<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="home.aspx.cs"
  Inherits="_home" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script type="text/javascript" language="javascript">

    $(document).ready(function () {

    });

    // PARTE PRINCIPALE 

    function modify_hp_title() {
      show_dyn_dlg({ title: "Titolo Home Page", rows: [{ id: "title", icon: "heading", label: "Titolo", valore: $("#main_title").text()}]
        , on_ok: function () {
          var result = post_action({ "action": "set_title_hp", "new_title": val_dyn("title") });
          if (result) $("#main_title").text(result.contents);
        }
      });
    }

    // MACRO SEZIONE

    function add_macro_sec() {
      show_dyn_dlg({ title: "Nuova Macro Sezione", rows: [{ id: "title", icon: "heading", label: "Titolo" }, { id: "des", label: "Note Particolari"}]
        , on_ok: function () {
          if (val_dyn("title")) {
            var result = post_action({ "action": "add_macro_sezione", "title": val_dyn("title"), "notes": val_dyn("des") });
            if (result) $("#macro_sezioni").append(result.html_element);
          }
        }
      });
    }

    function modify_ms_title(id) {
      var ms_el = $("[id-ms=" + id + "]");
      show_dyn_dlg({ title: "Aggiorna Macro Sezione", rows: [{ id: "title", icon: "heading", label: "Titolo", valore: $(ms_el).find("[tp-val='title']").text() }
        , { id: "notes", label: "Note Particolari", valore: $(ms_el).find("[tp-val='notes']").text()}]
        , on_ok: function () {
          if (val_dyn("title")) {
            var result = post_action({ "action": "upd_macro_sezione", "id": id, "title": val_dyn("title"), "notes": val_dyn("notes") });
            if (result) $("[id-ms=" + id + "] [tp=ms-title]").html(result.html_element);
          }
        }
      });
    }

    function empty_ms(id) {
      show_warning_yn("Attenzione!", "Sei sicuro di voler svuotare la macro sezione?", function () {
        if (empty_macro_sezione(id)) { var el = $("[id-ms=" + id + "] [tp=ms-body]").empty(); }
      });
    }

    function empty_macro_sezione(id) { var result = post_action({ "action": "empty_macro_sezione", "id": id }); return result ? true : false; }

    function move_ms(where, id) {
      var result = post_action({ "action": "move_ms", "id": id, "where": where });
      if (result) $("#macro_sezioni").html(result.html_element);
    }

    function del_ms(id) {
      show_warning_yn("Attenzione!", "Sei sicuro di voler cancellare tutta la macro sezione?", function () {
        if (del_macro_sezione(id)) $("[id-ms=" + id + "]").remove();
      });
    }

    function del_macro_sezione(id) { var result = post_action({ "action": "del_macro_sezione", "id": id }); return result ? true : false; }

    //  SEZIONE

    function add_sec(id, after) {
      var el = check_bool(after) ? $("[id-s=" + id + "]") : $("[id-ms=" + id + "]");
      show_dyn_dlg({ title: "Nuova Sezione", rows: [{ id: "title", icon: "heading", label: "Titolo" }
        , { id: "notes", label: "Note Particolari" }
        , { id: "type", label: "Tipo", type: "select", values: [{ title: "Note Libere", value: "notes_free"}]}]
        , on_ok: function () {
          if (val_dyn("title")) {
            var result = post_action({ "action": check_bool(after) ? "add_sezione_after" : "add_sezione", "id": id, "title": val_dyn("title"), "notes": val_dyn("notes"), "type": val_dyn("type") });
            if (result) {
              if (check_bool(after)) el.after(result.html_element);
              else el.find("[tp=ms-body]").append(result.html_element);
            }
          }
        }
      });
    }

    function modify_s(id) {
      var s_el = $("[id-s=" + id + "]");
      show_dyn_dlg({ title: "Aggiorna Sezione", rows: [
        { id: "title", icon: "heading", label: "Titolo", valore: s_el.find("[tp-val='title']").text() }
        , { id: "notes", label: "Note Particolari", valore: s_el.find("[tp-val='notes']").text() }
        , { id: "cols", label: "Larghezza", valore: s_el.attr("val-cols"), type: 'select'
            , values: [{ title: "tutta la pagina", value: "12" }, { title: "met\u00E0 pagina", value: "6" }
              , { title: "un terzo di pagina", value: "4" }, { title: "un quarto di pagina", value: "3"}]
        }
        , { id: "rows", label: "Righe Casella di Testo", valore: s_el.attr("val-rows")}]
        , on_ok: function () {
          if (val_dyn("title")) {
            var result = post_action({ "action": "upd_sezione", "id": id, "title": val_dyn("title")
              , "notes": val_dyn("notes"), "cols": val_dyn("cols"), "rows": val_dyn("rows")
            });
            if (result) { var s_el = $("[id-s=" + id + "]"), p = s_el.prev(); s_el.remove(); p.after(result.html_element); }
          }
        }
      });
    }

    function del_s(id) {
      show_warning_yn("Attenzione!", "Sei sicuro di voler cancellare la sezione?", function () {
        if (del_sezione(id)) $("[id-s=" + id + "]").remove();
      });
    }

    function del_sezione(id) { var result = post_action({ "action": "del_sezione", "id": id }); return result ? true : false; }

    function move_s(where, id) {
      var ms_el = $("[id-s=" + id + "]").closest("[id-ms]");
      var result = post_action({ "action": "move_s", "id": id, "id-ms": ms_el.attr("id-ms"), "where": where });
      if (result) { var p = ms_el.prev(); ms_el.remove(); p.after(result.html_element); }
    }

    function change_emphasis_s(id) {
      var s_el = $("[id-s=" + id + "]");
      var result = post_action({ "action": "change_emphasis_s", "id": id, "from-emphasis": s_el.attr("val-emphasis") });
      if (result) { var p = s_el.prev(); s_el.remove(); p.after(result.html_element); }
    }

    // NOTES FREE

    var _notes = null;
    function focus_notes(id) {
      _notes = $("[id-s=" + id + "] [tp-val=text]").val();
    }

    function blur_notes(id) {
      var notes_2 = $("[id-s=" + id + "] [tp-val=text]").val();
      if (_notes != notes_2) { _notes = null; save_notes(id, notes_2); }
    }

    function save_notes(id, txt) {
      try {
        $("[id-s=" + id + "] [tp-val=text]").css("border-color", "lightgreen").css("box-shadow", "0 0 2px lightgreen");
        window.setTimeout(function () {
          if (post_action({ "action": "save_notes", "id": id, "text": txt })) 
            $("[id-s=" + id + "] [tp-val=text]").css("border-color", "").css("box-shadow", "");
          else 
            $("[id-s=" + id + "] [tp-val=text]").css("border-color", "tomato").css("box-shadow", "0 0 2px tomato");
        }, 200);

      } catch (e) { show_danger("Attenzione!", e.message); }
    }


  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class="container-fluid">
    <!-- title -->
    <div class="row mb-4">
      <div class="col">
        <div class='border-bottom p-1 mt-1 d-flex justify-content-between'>
          <h1 id='main_title' runat='server' class='light-blue-text'>
            ...</h1>
          <div class="dropdown no-arrow mr-1 mt-1">
            <a class="dropdown-toggle" href="#" role="button" data-toggle="dropdown" aria-haspopup="true"
              aria-expanded="false"><i class="fas fa-ellipsis-v"></i></a>
            <div class="dropdown-menu dropdown-menu-right shadow">
              <div class="dropdown-header">
                Home Page</div>
              <div class="dropdown-item icon-item">
                <span class='icon-menu' title='modifica titolo pagina...' onclick='modify_hp_title()'>
                  <i class="fas fa-pen text-info"></i></span><span class='icon-menu text-success' title='aggiungi macro sezione...'
                    onclick='add_macro_sec()'><i class="fas fa-plus"></i></span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- macro sezioni -->
    <div id="macro_sezioni" runat="server">
    </div>
  </div>
</asp:Content>
