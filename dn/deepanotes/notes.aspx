<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="notes.aspx.cs"
  Inherits="_notes" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <style>
    .voce
    {
      padding-top: 3px;
      padding-bottom: 3px;
    }
    .voce-cut a
    {
      color: khaki !important;
    }
    .secondo:before
    {
      content: " ";
      border-bottom: 1px solid darkslategray;
      position: absolute;
      left: 0px;
      z-index: 1;
      width: 1000px;
      display: block;
    }
    .primo a
    {
      color: steelblue;
    }
    .secondo a
    {
      color: skyblue;
    }
    .terzo a
    {
      color: lightblue;
    }
    .quarto a
    {
      color: whitesmoke;
    }
  </style>
  <script type="text/javascript" language="javascript">

    $(document).ready(function () {
      if ($("#folder_id").val()) {
        window.setTimeout(function () {
          $("#menu").scrollTop($("[tp-item='folder'][item-id='" + $("#folder_id").val() + "']").position().top + 100);
        }, 300);
      }
    });

    function task_state(task_id, assign_stato) {
      try {
        $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");
        window.setTimeout(function () {
          try {
            var t = $("[task-id=" + task_id + "]"), res = post_action({ "action": "task_state", "id": task_id, "stato": assign_stato });
            if (res && res.html_element) {
              var p = t.prev(); t.remove(); p.after(res.html_element);
              t.css("border-color", "").css("box-shadow", "");
            } else t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");
          } catch (e) { show_danger("Attenzione!", e.message); }
        }, 1500);
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function modify_task(task_id) {
      try {
        var t = $("[task-id=" + task_id + "]");
        show_dyn_dlg({ title: "Aggiorna Attivit\u00E0", rows: [
        { id: "title", icon: "heading", label: "Titolo", valore: t.find("[tp-val='title']").text() }
          , { id: "assegna", label: "Assegna a", valore: t.attr("task-assegna"), type: 'select', values: __task_assegna }
          , { id: "priorita", label: "Priorit\u00E0", valore: t.attr("task-priorita"), type: 'select', values: __task_priorita }
          , { id: "tipo", label: "Tipo", valore: t.attr("task-tipo"), type: 'select', values: __task_tipi }
          , { id: "stima", label: "Stima", valore: t.attr("task-stima"), type: 'select', values: __task_stime}]
        , on_ok: function () {

          $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");

          window.setTimeout(function () {
            try {

              var t = $("[task-id=" + task_id + "]"), res = post_action({ "action": "update_task", "id": task_id, "title": val_dyn("title")
                , "assegna": val_dyn("assegna"), "priorita": val_dyn("priorita"), "tipo": val_dyn("tipo"), "stima": val_dyn("stima")
              });
              if (res && res.html_element) {
                var p = t.prev(); t.remove(); p.after(res.html_element);
                t.css("border-color", "").css("box-shadow", "");
              } else t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");

            } catch (e) { show_danger("Attenzione!", e.message); }
          }, 1500);


        }
        });
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function del_task(task_id) {
      show_warning_yn("Attenzione!", "Sei sicuro di voler eliminare l'attivit&aacute;?"
        , function () { remove_task(task_id) });
    }

    function remove_task(id) {
      try {
        var result = post_data({ "action": "remove_task", "id": id });
        if (result) {
          if (result.des_result == "ok") window.location.reload();
          else show_danger("Attenzione!", result.message);
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function add_attivita(stato) {
      try {
        show_dyn_dlg({ title: "Aggiungi Attivit\u00E0", rows: [
        { id: "title", icon: "heading", label: "Titolo", valore: "" }
          , { id: "assegna", label: "Assegna a", valore: "", type: 'select', values: __task_assegna }
          , { id: "priorita", label: "Priorit\u00E0", valore: "", type: 'select', values: __task_priorita }
          , { id: "tipo", label: "Tipo", valore: "", type: 'select', values: __task_tipi }
          , { id: "stima", label: "Stima", valore: "", type: 'select', values: __task_stime}]
        , on_ok: function () {

          window.setTimeout(function () {
            try {
              var res = post_action({ "action": "add_task", "folder_id": get_param("id"), "synch_folder_id": get_param("sf")
                , "stato": stato, "title": val_dyn("title"), "assegna": val_dyn("assegna"), "priorita": val_dyn("priorita"), "tipo": val_dyn("tipo"), "stima": val_dyn("stima")
              });
              if (res) window.location.reload();
            } catch (e) { show_danger("Attenzione!", e.message); }
          }, 1500);
        }
        });
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function del_folder(id, tp_id) {
      show_danger_yn("Attenzione!",
        get_param("sf") || tp_id == "synch-folder" ? "Sei sicuro di voler cancellare il contenuto della cartella?"
          : "Sei sicuro di voler cancellare la cartella e tutto il contenuto?"
        , function () { remove_folder(id, tp_id) });
    }

    function cut_folder(id, tp_id, el) {
      try {
        window.setTimeout(function () {
          try {
            var res = post_action({ "action": "cut_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
              , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf")
            });
            if (res) {
              $.each(res.list, function (index, value) {
                $("li[tp-item='folder'][item-id=" + value + "]").addClass("voce-cut");
              });
            }
          } catch (e) { show_danger("Attenzione!", e.message); }
        }, 500);
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function paste_folder(id, tp_id) {
      try {
        window.setTimeout(function () {
          try {
            var res = post_action({ "action": "paste_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
              , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf")
            });
            if (res) {
              if (res.message)
                show_danger("Nota Bene!", res.message, function () { window.location.reload(); });
              else window.location.reload();
            }
          } catch (e) { show_danger("Attenzione!", e.message); }
        }, 500);
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function remove_folder(id, tp_id) {
      try {
        window.setTimeout(function () {
          try {
            var res = post_action({ "action": "del_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
              , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf")
            });
            if (res) { if (tp_id) window.location.reload(); else window.location.href = res.contents; }
          } catch (e) { show_danger("Attenzione!", e.message); }
        }, 500);
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function ren_folder(name, id, tp_id) {
      try {
        show_dyn_dlg({ title: "Rinomina la Cartella", rows: [
        { id: "title", icon: "heading", label: "Titolo", valore: name ? name : $("[tp='name_folder']").text()}]
        , on_ok: function () {
          if (!val_dyn("title")) return;
          window.setTimeout(function () {
            try {
              var res = post_action({ "action": "ren_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
                , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf"), "title": val_dyn("title")
              });
              if (res) window.location.reload();
            } catch (e) { show_danger("Attenzione!", e.message); }
          }, 500);
        }
        });
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function add_folder(id, tp_id) {
      try {
        show_dyn_dlg({ title: "Aggiungi Cartella", rows: [
        { id: "title", icon: "heading", label: "Titolo", valore: ""}]
        , on_ok: function () {
          if (!val_dyn("title")) return;
          window.setTimeout(function () {
            try {
              var res = post_action({ "action": "add_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
                , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf"), "title": val_dyn("title")
              });
              if (res) window.location.reload();
            } catch (e) { show_danger("Attenzione!", e.message); }
          }, 500);
        }
        });
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function change_filter(filter_id) {
      try {
        var result = post_data({ "action": "set_filter_id", "filter_id": filter_id });
        if (result) {
          if (result.des_result == "ok") window.location.reload();
          else show_danger("Attenzione!", result.message);
        }
      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function open_notes(task_id) {
      var sec = $("[task-id='" + task_id + "'] [tp-item='section-notes']"), ta = $("[task-id='" + task_id + "'] [tp-item='txt-notes']")
       , tf = $("[task-id='" + task_id + "'] [tp-item='section-allegati']"), btn = $("[task-id='" + task_id + "'] [tp-item='btn-notes']");
      if (sec.attr("state") == "opened") {
        sec.hide(350); tf.hide(350); sec.attr("state", "");
        if (ta.val()) btn.text("vedi note..."); else btn.text("aggiungi note...");
      }
      else {
        // read notes
        if (!sec.attr("readed")) {
          try {
            $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");
            window.setTimeout(function () {
              try {
                var t = $("[task-id=" + task_id + "]"), ta = $("[task-id='" + task_id + "'] [tp-item='txt-notes']")
                  , btn = $("[task-id='" + task_id + "'] [tp-item='btn-notes']");
                var res = post_action({ "action": "get_details", "task_id": task_id });
                if (res) {
                  ta.val(res.contents); sec.attr("readed", "1");
                  sec.show(350); sec.attr("state", "opened");
                  if (res.html_element) { tf.html(res.html_element); tf.show(350); }
                  t.css("border-color", "").css("box-shadow", "");
                  btn.text("salva e nascondi le note...");
                }
                else t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");
              } catch (e) { show_danger("Attenzione!", e.message); }
            }, 200);
          } catch (e) { show_danger("Attenzione!", e.message); }
        }
        else { sec.show(350); sec.attr("state", "opened"); btn.text("salva e nascondi le note..."); }
      }
    }

    var _notes = null;
    function focus_task_notes(task_id) {
      _notes = $("[task-id='" + task_id + "'] [tp-item='txt-notes']").val();
    }

    function blur_task_notes(task_id) {
      var notes_2 = $("[task-id='" + task_id + "'] [tp-item='txt-notes']").val();
      if (_notes != notes_2) { _notes = null; save_task_notes(task_id, notes_2); }
    }

    function save_task_notes(task_id, txt) {
      try {
        $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");
        window.setTimeout(function () {
          var t = $("[task-id=" + task_id + "]"), ta = $("[task-id='" + task_id + "'] [tp-item='txt-notes']");
          if (post_action({ "action": "save_task_notes", "task_id": task_id, "text": txt }))
            t.css("border-color", "").css("box-shadow", "");
          else
            t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");
        }, 200);

      } catch (e) { show_danger("Attenzione!", e.message); }
    }

    function show_menu(el) { $(el).find("[tp='menu-item']").show(); }

    function hide_menu(el) { $(el).find("[tp='menu-item']").hide(); }


  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='folder_id' type='hidden' runat='server' />
  <div class="container-fluid">
    <div class="row mb-4">
      <!-- sidebar menu -->
      <nav sidebar-tp='menu' class='d-none' sidebar-init='show'>
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
        <!--<div id='sub_menu' class='sidebar-sticky-sub' runat='server'>
          <a href='javascript:add_folders()' style='color:gray;'>AGGIUNGI</a>
          <a href='javascript:cut_folders()' style='color:gray;'>TAGLIA</a>
          <a href='javascript:ren_folders()' style='color:gray;'>RINOMINA</a>
          <a href='javascript:del_folders()' style='color:gray;'>RIMUOVI</a>
        </div>-->
      </nav>
      <!-- contenuti -->
      <div sidebar-tp='body'>
        <!-- content -->
        <div id='content' runat='server'>
        </div>
      </div>
      <!-- sidebar icon -->
      <div sidebar-tp='sh' onclick='sh_side_menu()'>
        <i sidebar-tp='icon'></i>
      </div>
    </div>
  </div>
</asp:Content>
