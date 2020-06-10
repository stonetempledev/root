<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="notes.aspx.cs"
  Inherits="_notes" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <style>
    .voce
    {
      padding-top: 3px;
      padding-bottom: 3px;
    }
    .voce:after
    {
      content: " ";
      border-bottom: 1px solid darkslategray;
      position: absolute;
      left: 0px;
      z-index: 1;
      width: 1000px;
      margin-top: 3px;
      display: block;
    }
    .primo *
    {
      color: steelblue;
    }
    .secondo *
    {
      color: skyblue;
    }
    .terzo *
    {
      color: lightblue;
    }
    .quarto *
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

    function open_attivita(id_folder) { window.location = set_param("sf", "", set_param("id", id_folder, window.location.href)); }

    function open_attivita_synch(synch_folder_id) { window.location = set_param("id", "", set_param("sf", synch_folder_id, window.location.href)); }

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
