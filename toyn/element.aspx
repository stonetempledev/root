<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="element.aspx.cs"
  Inherits="_element" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <link href="element.css" rel="stylesheet" type="text/css" />
  <link href="js/codemirror-5.49.2/lib/codemirror.css" rel="stylesheet" type="text/css" />
  <script src="js/codemirror-5.49.2/lib/codemirror.js" type="text/javascript"></script>
  <script src="js/codemirror-5.49.2/lib/formatting.js" type="text/javascript"></script>
  <script src="js/codemirror-5.49.2/xml.js" type="text/javascript"></script>
  <script language="javascript">
    $(document).ready(function () {
    });

    function mod_xml() { window.location.href = $("#url_xml").val(); return false; }
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='url_xml' type='hidden' runat='server' />
  <input id='url_view' type='hidden' runat='server' />
  <div class="container-fluid">
    <div class="row">
      <!-- menu -->
      <nav class="d-none d-md-block col-md-3 sidebar">
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
      </nav>
      <!-- view -->
      <div id="contents" class="col-md-9 ml-sm-auto" style='padding: 0px;' runat='server'>
        <!-- view bar -->
        <div class='bg-secondary' style='display: block; padding: 5px;'>
          <button class='btn btn-outline-light btn-sm' onclick='return mod_xml()'>
            Modifica XML</button>
        </div>
        <div id="contents_doc" style='padding: 5px;' runat='server'>
        </div>
      </div>
      <!-- xml -->
      <div id="contents_xml" class="col-md-9 ml-sm-auto px-4" runat='server'>
        <textarea id='doc_xml' runat='server'></textarea>
      </div>
    </div>
</asp:Content>
