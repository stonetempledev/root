<%@ Page Language="C#" AutoEventWireup="true" CodeFile="tables.aspx.cs" Inherits="_tables" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="ISO-8859-1">
    <title></title>
    <link type="text/css" rel="stylesheet" href="sys.css" />
    <script language="javascript">
        function do_actions(all) {
            var value = "";
            var ctrl = $("input[id_action]");
            for (var i = 0; i < ctrl.length; i++)
                value += (ctrl[i].checked || (all != null && all == true))
                    ? ((value != "" ? "," : "") + $(ctrl[i]).attr("id_action")) : "";

            input_ctrl("clid", "doaction").val(value);

            postBack();
        }
    </script>
</head>
<body>
    <form mainform='true' runat="server">
    <asp:TextBox ID="logtype" clientid='logtype' CssClass="input-hidden" runat="server" />
    <input type="hidden" id="doaction" clid="doaction" runat="server" />
    <textarea id="xml_actions" runat="server" style="display: none"></textarea>
    <div id="contents" runat="server">
    </div>
    </form>
</body>
</html>
