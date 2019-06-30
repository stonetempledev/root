<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="checkdb.aspx.cs"
  Inherits="_checkdb" ClientIDMode="Static" MaintainScrollPositionOnPostback="true" %>

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

    function exec_script(id, script) { do_post_back("exec_script", "id:" + id + ";script:" + script); }
    function exec_scripts(ids, scripts) { do_post_back("exec_scripts", "ids:" + ids + ";scripts:" + scripts); }
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div id='div_contents' runat='server' class="container-fluid">
  </div>
</asp:Content>
