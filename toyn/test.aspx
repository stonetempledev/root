<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="test.aspx.cs"
  Inherits="test" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    $(document).ready(function () {
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class='container'>
    <div class='row'>
      <div class="col">
        <div id='el_1' contenteditable class='h1'>Titulune</div>
        <div id='el_2' contenteditable>paragrafino</div>
        <div id='el_3' contenteditable class='h3'>Sotto titolo</div>
        <div id='el_4' contenteditable>paragrafino nel paragrafino</div>
      </div>
    </div>
  </div>
</asp:Content>
