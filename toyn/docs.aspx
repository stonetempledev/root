<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="docs.aspx.cs"
  Inherits="_docs" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <link href="docs.css" rel="stylesheet" type="text/css" />
  <script language="javascript">

    $(document).ready(function () {
    });

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class="container-fluid">
    <div class="row">
      <!-- sidebar menu -->
      <nav sidebar-tp='menu' class='d-none' sidebar-init='show'>
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
      </nav>
      <!-- view -->
      <div id="contents" sidebar-tp='body' style='padding: 0px;' runat='server'>
      </div>
      <!--  -->
      <!-- sidebar icon -->
      <div sidebar-tp='sh' onclick='sh_side_menu()'>
        <i sidebar-tp='icon'></i>
      </div>
    </div>
  </div>
</asp:Content>
