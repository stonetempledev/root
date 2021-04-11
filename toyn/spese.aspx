<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="spese.aspx.cs"
    Inherits="_spese" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
    <link href="spese.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" language="javascript">

        $(document).ready(function () {

            // init grid
            var _val_cache = "";
            $('[fld]').on('focus', function () {
                _val_cache = $(this).text();
                if (!is_mobile()) $(this).selectText();
            });
            $('[fld]').on('blur', function () {
                if ($(this).text() != _val_cache) {
                    upload_spesa($(this).closest("[id-row]"));
                    _val_cache = null;
                }
            });
            $('[fld]').on('keydown', function (e) {
                if (e.keyCode == 13) {
                    var r = $(this).closest("[id-row]");
                    if (r.attr("err") == "true" || $(this).text() != _val_cache) {
                        upload_spesa(r);
                        _val_cache = null;
                    }
                    e.preventDefault();
                }
            });

            $('[for-fld]').on('click', function (e) {
                var fld = $(this).attr("for-fld"), r = $(this).closest("[id-row]");
                r.find("[fld=" + fld + "]").focus();
            });

            // sub commands
            set_sub_cmds([{ fnc: "add_spesa()", title: "Aggiungi spesa (ALT+A)..." }]);

        });

        $(document).on('keydown', function (e) {
            if (e.keyCode == 65 && e.altKey) {
                add_spesa();
            }
        });

        // !ocio!
        function add_spesa() { show_info("Test", "nuova spesa"); }

        // !ocio!
        function upload_spesa(r) {
            r.css("border", "1pt solid lightgreen").css("box-shadow", "-2px -2px 2px lightgreen");
            window.setTimeout(function () {
                //alert($(this).attr("fld") + "." + $(this).attr("tp-fld"))
                //alert(r.attr("id-row") + "." + r.attr("tp-row"))
                var result = post_data({ "action": "save_row" }, true);
                if (result) {
                    if (result.des_result == "ok") {
                        r.attr("err", "");
                        r.css("border-color", "").css("box-shadow", "");
                        r.css("border", "").css("box-shadow", "");
                    } else {
                        r.css("border", "1pt solid tomato").css("box-shadow", "-2px -2px 2px tomato");
                        r.attr("err", "true");
                        r.find('[tp=err]').remove();
                        r.append("<div tp='err' class='op-5 col-12 clr-alert'><span class='badge badge-pill badge-danger ws-n fw-n fs-110'>si &#233; verificato un errore durante l'aggiornamento della rava e della fava: " + result.message + "</span></div>");
                    }
                }
            }, 1000);
        }

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
                    <div class='d-none d-md-block'>
                        <h1 id='main_title' runat='server' class='light-blue-text'>Spese</h1>
                        <h3>
                            <small id='sub_title' runat='server'></small></h3>
                    </div>
                    <div class='d-block d-md-none'>
                        <h2 id='main_title_sm' runat='server' class='light-blue-text'>Spese</h2>
                        <h5>
                            <small id='sub_title_sm' runat='server'></small></h5>
                    </div>
                </div>
                <!-- spese -->
                <div id="grid_spese" runat="server" class="d-block">
                </div>
            </div>
            <!-- sidebar icon -->
            <div sidebar-tp='sh' onclick='sh_side_menu()'>
                <i sidebar-tp='icon'></i>
            </div>
        </div>
    </div>
</asp:Content>
