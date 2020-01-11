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
  <script language="javascript" charset="UTF-8">
    $(document).ready(function () {

    });

    function del_backup(fn) {
      try {
        status_txt("eliminazione backup in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "del_backup", "fn": fn });
          if (result) {
            if (result.des_result == "ok") {
              $("a[row-data='" + fn + "']").remove();
              status_txt("backup eliminato con successo");
              end_status_to(2000);
            } else { show_alert("Eliminazione backup", "Ci sono stati dei problemi!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_alert("Attenzione!", e.message); end_status(); }
    }

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class="container-fluid">
    <div id='gen_backups' runat='server' class="row">
      <div class='col-12'>
        <h2 style='margin-bottom: 45px;'>
          Generazione Backup</h2>
      </div>
      <div class='col-3 var-name'>
        type backup</div>
      <div class='col-9 var-value'>
        <input id="val_type" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        prefix filename</div>
      <div class='col-9 var-value'>
        <input id="prefix_filename" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        file format</div>
      <div class='col-9 var-value'>
        <input id="val_file_format" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-user</div>
      <div class='col-9 var-value'>
        <input id="val_net_user" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-pwd</div>
      <div class='col-9 var-value'>
        <input id="val_net_pwd" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-folder</div>
      <div class='col-9 var-value'>
        <input id="val_net_folder" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        sql-command</div>
      <div class='col-9 var-value'>
        <textarea id="sql_command" style='width:100%;' rows='5' runat="server"></textarea></div>
      <div id='result_bck' runat='server' class='col-12' style='margin-top:30px;'>
      </div>
      <div class='col-12' style='margin-top:50px;margin-bottom:50px;'>
        <asp:Button class='btn btn-primary float-right' runat='server' OnClick='gen_backup' Text='GENERA BACKUP'></asp:Button>
      </div>
    </div>
    <div id='res_backups' runat='server' class="row">
      <div class='col-12'>
        <h2 style='margin-bottom: 45px;'>
          Ripristino Backup</h2>
      </div>
      <div class='col-3 var-name'>
        type backup</div>
      <div class='col-9 var-value'>
        <input id="res_val_type" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-user</div>
      <div class='col-9 var-value'>
        <input id="res_val_net_user" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-pwd</div>
      <div class='col-9 var-value'>
        <input id="res_val_net_pwd" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-folder</div>
      <div class='col-9 var-value'>
        <input id="res_val_net_folder" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        sql-command</div>
      <div class='col-9 var-value'>
        <textarea id="res_sql_command" style='width:100%;' rows='5' runat="server"></textarea></div>
      <div id='result_res' runat='server' class='col-12' style='margin-top:30px;'>
      </div>
      <div class='col-12' style='margin-top:50px;margin-bottom:50px;'>
        <asp:Button class='btn btn-primary float-right' runat='server' OnClick='res_backup' Text='RIPRISTINA BACKUP'></asp:Button>
      </div>
    </div>
    <div id='del_backups' runat='server' class="row">
      <div class='col-12'>
        <h2 style='margin-bottom: 45px;'>
          Eliminazione Backup</h2>
      </div>
      <div class='col-3 var-name'>
        type backup</div>
      <div class='col-9 var-value'>
        <input id="del_val_type" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-user</div>
      <div class='col-9 var-value'>
        <input id="del_val_net_user" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-pwd</div>
      <div class='col-9 var-value'>
        <input id="del_val_net_pwd" type='text' runat="server" /></div>
      <div class='col-3 var-name'>
        net-folder</div>
      <div class='col-9 var-value'>
        <input id="del_val_net_folder" type='text' runat="server" /></div>
      <div id='result_del' runat='server' class='col-12' style='margin-top:30px;'>
      </div>
      <div class='col-12' style='margin-top:50px;margin-bottom:50px;'>
        <asp:Button class='btn btn-primary float-right' runat='server' OnClick='del_backup' Text='ELIMINA BACKUP'></asp:Button>
      </div>
    </div>
    <div id='view_backups' runat='server' class="row">
      <div class='col-12'>
        <h2 style='margin-bottom: 45px;'>
          Elenco Backups</h2>
      </div>
      <div id='res_view' runat='server' class='col-12'>
      </div>
      <div id='result_view' runat='server' class='col-12' style='margin-top:30px;'>
      </div>
    </div>
  </div>
</asp:Content>
