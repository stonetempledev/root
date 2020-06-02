<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="attivita.aspx.cs"
  Inherits="_attivita" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <style>
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
      color: lightcyan;
    }
    .quarto *
    {
      color: whitesmoke;
    }
  </style>
  <script type="text/javascript" language="javascript">

    $(document).ready(function () {

    });


  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
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
