﻿<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="router.aspx.cs"
  Inherits="_router" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <style>
    li
    {
      word-wrap: break-word;
    }
  </style>
  <script language="javascript">
    $(document).ready(function () {
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class="container-fluid">
    <div class="row">
      <!-- contenuti -->
      <div id='div_contents' runat='server' class="col-12">
      </div>
    </div>
  </div>
</asp:Content>
