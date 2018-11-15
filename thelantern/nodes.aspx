<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="nodes.aspx.cs"
    Inherits="_nodes" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
    <link href="nodes.css?ver=2" rel="stylesheet" type="text/css" />
    <link href="js/metisMenu/metisMenu.css" rel="stylesheet" type="text/css" />
    <script src="js/metisMenu/metisMenu.js" type="text/javascript"></script>
    <script src="js/dragscroll.js?ver=7" type="text/javascript"></script>
    <style>
        li
        {
            word-wrap: break-word;
        }
    </style>
    <script language="javascript">
        var _scrolling_bar = false, _scrolling_from = -1;
        $(document).ready(function () {
            $('#mnu_nodes').metisMenu();
            $(window).on('resize', rsz_page);
            $("#tree_bar div").scrollLeft(10000);
            bind_drag($("#tree_bar"), $("#bread_bar"));
        });

        function rsz_page() {
            var screen = get_dev_size(); if (screen == "md" || screen == "lg" || screen == "xl") {
                if ($("#menu_bar").attr("tp-show") == "forced") sh_menu();
            }
        }

        function sh_menu() {
            if (!$("#menu_bar").attr("tp-show")) {
                $("#contents").hide();
                $("#menu_bar").removeClass("col-md-3 d-md-block d-none").addClass("col-12");
                $("#menu_bar").attr("tp-show", "forced");
                $("#btn-show-menu").attr("src", "images/right-arrow-32.png");
            } else {
                $("#contents").show();
                $("#menu_bar").removeClass("col-12").addClass("col-md-3 d-md-block d-none");
                $("#menu_bar").attr("tp-show", "");
                $("#btn-show-menu").attr("src", "images/left-arrow-32.png");
            }
        }

        function to_root() {
            if (!__dragscroll_move) {
                var to_end = $("#tree_bar div").scrollLeft() == 0;
                if (to_end) alert($("#tree_bar div").scrollLeft());
                $("#tree_bar div").animate({ scrollLeft: to_end ? 10000 : 0 }, 750, 'swing', function () { });
            }
        }

        function open_node(id) { if (!__dragscroll_move) window.location.href = $("#url_node").val() + encodeURI(id > 0 ? " id:" + id : ""); }

        
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
    <input id='url_node' type='hidden' runat="server"></input>
    <div class="container-fluid">
        <div class="row">
            <div class="col-btn topbar-btn">
                <img id='btn-show-menu' src='images/left-arrow-32.png' class='d-md-none' onclick='sh_menu()'
                    style='cursor: pointer;' />
            </div>
            <div id='tree_bar' class="col-bar topbar">
                <div id='bread_bar' class='bread'>
                    <ul id='path_nodes' runat='server'>
                    </ul>
                </div>
            </div>
        </div>
        <div class="row">
            <!-- menu -->
            <nav id='menu_bar' class="d-none d-md-block col-md-3 sidebar">
                   <div class="sidebar-nav">
                     <ul class="metismenu" id="mnu_nodes" runat='server'>
                     </ul>
                   </div>
                </nav>
            <!-- contenuti -->
            <div id='contents' runat='server' class="col-md-9 ml-sm-auto px-4">
            </div>
        </div>
    </div>
</asp:Content>
