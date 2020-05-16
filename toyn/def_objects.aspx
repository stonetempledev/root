<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="def_objects.aspx.cs"
  Inherits="_def_objects" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script type="text/javascript" language="javascript">

    $(document).ready(function () {

    });


  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class="container-fluid">
    <div class="row mb-4">
      <!-- sidebar menu -->
      <nav sidebar-tp='menu' class='d-none' sidebar-init='hide'>
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
      </nav>
      <!-- contenuti -->
      <div sidebar-tp='body'>
        <!-- title -->
        <div class='d-block border-bottom p-1 mt-1 mb-3'>
          <h1 id='page_title' runat='server' class='light-blue-text'>
            titolo</h1>
          <h4 id='page_des' runat='server'>descrizione
          </h4>
        </div>
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
