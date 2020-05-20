<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="def_objects.aspx.cs"
  Inherits="_def_objects" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script type="text/javascript" language="javascript">

    $(document).ready(function () {
      // rebuild-pbjects
      if ($("#action").val() == "rebuild-objects") {
        if ($("[tp=action]").length > 0)
          start_rebuild($("[tp=action]:first").attr("order"), "progress");
      }
    });

    // start rebulid action
    function start_rebuild(order, sub_action) {
      var el = $("[tp=action][order=" + order + "]"), action = el.attr("action"), object_type = el.attr("object-type");
      var res = post_action({ "action": action, "sub-action": sub_action, "object-type": object_type
        , "order": el.attr("order"), "title": el.attr("title") });
      if (res) {
        var p = el.prev(); el.remove(); p.after(res.html_element);
        window.setTimeout(function () { end_rebuild(order, sub_action); }, 300);
      }
    }

    // end rebulid action
    function end_rebuild(order, sub_action) {
      if (sub_action == "progress") {
        start_rebuild(order, "do");
      }
      else if (sub_action == "do") {
        var next_order = parseInt(order) + 1;
        if ($("[tp=action][order=" + next_order.toString() + "]").length > 0) {
          window.setTimeout(function () { start_rebuild(next_order.toString(), "progress"); }, 300);
        }
      }
    }

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='action' type='hidden' runat='server' value='' />
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
          <h4 id='page_des' runat='server'>
            descrizione
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
