<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="home.aspx.cs"
  Inherits="_home" ClientIDMode="Static" %>

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
        <div class='d-block border-bottom pt-1 pb-1 mt-1 mb-3 d-flex justify-content-between'>
          <div>
            <h1 id='main_title' runat='server' class='light-blue-text'>
              Temple Of Your Notes</h1>
            <h2>
              <small id='sub_title' runat='server'>tutti i tuoi contenuti ben organizzati</small></h2>
          </div>
        </div>
      </div>
      <!-- sidebar icon -->
      <div sidebar-tp='sh' onclick='sh_side_menu()'>
        <i sidebar-tp='icon'></i>
      </div>
    </div>
  </div>
</asp:Content>
