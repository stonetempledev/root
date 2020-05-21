<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="backups.aspx.cs"
  Inherits="_backups" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <style>
    .var-name
    {
      border-bottom: 1pt solid lightgrey;
      padding: 5px;
      font-weight: bold;
    }
    .var-value
    {
      border-bottom: 1pt solid lightgrey;
      padding: 5px;
    }
    .var-value input
    {
      width: 100%;
    }
  </style>
  <script language="javascript">
    $(document).ready(function () {

    });

    function del_backup(fn) {
      show_warning_yn("Attenzione!", "Sei sicuro di voler cancellare il backup '" + fn + "'?"
      , function () { window.setTimeout(function () { del_backup2(fn); }, 500); });
    }

    function del_backup2(fn) {
      try {
        status_txt("eliminazione backup in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "del_backup", "fn": fn });
          if (result) {
            if (result.des_result == "ok") {
              $("a[row-data='" + fn + "']").remove();
              status_txt_ms("backup eliminato con successo!");
            } else { show_danger("Eliminazione backup", "Ci sono stati dei problemi!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_danger("Attenzione!", e.message); end_status(); }
    }

    function down_backup(fn) {
      try {
        status_txt("scarico backup in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "down_backup", "fn": fn });
          if (result) {
            if (result.des_result == "ok") {

              var link = document.createElement("a");
              link.download = result.url_name;
              link.href = result.url_file;
              link.click();

              status_txt_ms("backup scaricato con successo!");
            } else { show_danger("Scarico backup", "Ci sono stati dei problemi!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_danger("Attenzione!", e.message); end_status(); }
    }

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class="container-fluid" style='margin-top: 20px;'>
    <div id='gen_backups' runat='server' class="row">
      <div class='col-12'>
        <h2 style='margin-bottom: 45px;'>
          Generazione Backup</h2>
      </div>
      <div class='col-2 var-name lead'>
        type backup</div>
      <div class='col-10 var-value'>
        <input id="val_type" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        prefix filename</div>
      <div class='col-10 var-value'>
        <input id="prefix_filename" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        file format</div>
      <div class='col-10 var-value'>
        <input id="val_file_format" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        libs folders</div>
      <div class='col-10 var-value'>
        <input id="val_libs_folders" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        net-user</div>
      <div class='col-10 var-value'>
        <input id="val_net_user" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        net-pwd</div>
      <div class='col-10 var-value'>
        <input id="val_net_pwd" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        net-folder</div>
      <div class='col-10 var-value'>
        <input id="val_net_folder" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        notes</div>
      <div class='col-10 var-value'>
        <textarea id="notes_txt" class='form-control w-100' rows='5' maxlength="500" runat="server"></textarea></div>
      <div class='col-2 var-name lead'>
        sql-command</div>
      <div class='col-10 var-value'>
        <textarea id="sql_command" class='form-control w-100' rows='5' runat="server"></textarea></div>
      <div class='col-12' style='margin-top: 20px; margin-bottom: 50px;'>
        <button class='btn btn-primary btn-lg w-100' runat='server' onserverclick='gen_backup'>
          GENERA BACKUP</button>
      </div>
    </div>
    <div id='res_backups' runat='server' class="row">
      <div class='col-12'>
        <h2 style='margin-bottom: 45px;'>
          Ripristino Backup</h2>
      </div>
      <div class='col-2 var-name lead'>
        type backup</div>
      <div class='col-10 var-value'>
        <input id="res_val_type" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        net-user</div>
      <div class='col-10 var-value'>
        <input id="res_val_net_user" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        net-pwd</div>
      <div class='col-10 var-value'>
        <input id="res_val_net_pwd" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        net-folder</div>
      <div class='col-10 var-value'>
        <input id="res_val_net_folder" class='form-control' type='text' runat="server" /></div>
      <div class='col-2 var-name lead'>
        sql-command</div>
      <div class='col-10 var-value'>
        <textarea id="res_sql_command" class='form-control w-100' rows='5' runat="server"></textarea></div>
      <div class='col-12' style='margin-top: 20px;'>
        <button class='btn btn-lg btn-warning w-100' runat='server' onserverclick='res_backup'>
          RIPRISTINA BACKUP</button>
      </div>
    </div>
    <div id='view_backups' runat='server' class="row">
      <div class='col-12' style='margin-bottom: 45px;'>
        <h2>
          Elenco Backups<small> clicca su un backup per effettarne il ripristino</small></h2>
      </div>
      <div id='res_view' runat='server' class='col-12'>
      </div>
    </div>
  </div>
</asp:Content>
