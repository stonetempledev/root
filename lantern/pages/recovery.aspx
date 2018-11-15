<%@ Page Language="C#" AutoEventWireup="true" CodeFile="recovery.aspx.cs" Inherits="_recovery" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="ISO-8859-1">
    <title>Untitled Page</title>
    <link type="text/css" rel="stylesheet" href="sys.css" />
    <script language="javascript">
        function selectedFile(ctrl, idtxt) {
            var p = $(ctrl).val();
            p = p.replace(/^.*(\\|\/|\:)/, '');
            $("input[id=" + idtxt + "]").val(p);
        }
    </script>
</head>
<body>
    <form mainform='true' runat="server">
    <h2>
        Databases</h2>
    <div>
        <asp:TextBox ID="logtype" clientid='logtype' CssClass="input-hidden" runat="server" />
        <!-- utente -->
        <div class="rcvry-sec">
            <span class='rcvry-sec-title'>utente/i</span> <span class='rcvry-sec-subtitle'>user id:</span><span
                id='uid' runat="server" />
            <span class='rcvry-sec-subtitle'>, user type:</span><span id='utype' runat="server" />
            <span class='rcvry-sec-subtitle'>, user conn.:</span><span id='uconn' runat="server" />            
            <span class='rcvry-sec-subtitle'>, visibilità utenti:</span><span id='uvis' runat="server" />
            <br />
            <asp:Button ID="user_logout" Text="log out" CssClass='rcvry-btn' OnClick="logout_onclick"
                runat="server" />
        </div>
        <!-- connessione -->
        <div class="rcvry-sec">
            <span class='rcvry-sec-title'>connessione</span> <span>codice: </span>
            <asp:DropDownList ID="dbs" AutoPostBack="True" runat="server" />
            <br />
            <br />
            <span class='rcvry-sec-subtitle'>descrizione:</span><span id='conn_des' runat="server" /><br />
            <span class='rcvry-sec-subtitle'>stato connessione:</span><span id='conn_state' runat="server" /><br />
            <span class='rcvry-sec-subtitle'>ver:</span><span id='conn_ver' runat="server" /><br />
            <span class='rcvry-sec-subtitle'>last ver:</span><span id='last_ver' runat="server" /><br />
            <span class='rcvry-sec-subtitle'>check utenti:</span><span id='check_utenti' runat="server" /><br />
            <a id='view_tables' runat="server" class='btn-section'>view tables</a><a id='view_meta' runat="server"
                class='btn-section'>view meta</a><a id='view_schema' runat="server" class='btn-section'>view
                    schema</a>
            <asp:Button ID="gen_schema" Text="exp. schema" CssClass='rcvry-btn' OnClick="expschema_onclick"
                runat="server" />
            <asp:Button ID="integrita_schema" Text="check integrita" ToolTip="controllo allineamento struttura db con lo schema registrato"
                CssClass='rcvry-btn' OnClick="integrita_onclick" runat="server" />
            <asp:Button ID="set_utenti" Text="reset utenti" ToolTip="copia delle impostazioni utenti dal database di base"
                CssClass='rcvry-btn' OnClick="reset_users_onclick" runat="server" />
            <asp:Button ID="upgrade_schema" Text="upgrade db schema" ToolTip="aggiornamento della struttura del database all'ultima versione disponibile"
                CssClass='rcvry-btn' OnClick="upgrade_onclick" runat="server" />
            <asp:Button ID="init_infos" Text="init infos" CssClass='rcvry-btn' OnClick="init_infos_onclick"
                runat="server" />
            <asp:Button ID="exp_db" Text="export data" CssClass='rcvry-btn' OnClick="exp_db_onclick"
                runat="server" />
            <asp:Button ID="exp_all" Text="export" CssClass='rcvry-btn' OnClick="exp_all_onclick"
                runat="server" />
        </div>
        <!-- infos -->
        <div class="rcvry-sec">
            <span id='title_infos' runat='server' class='rcvry-sec-title'>history infos</span>
            <div id="infos" runat="server">
            </div>
        </div>
        <!-- backups -->
        <div class="rcvry-sec" id="mount_row" runat="server">
            <span class='rcvry-sec-title'>backups</span>
            <asp:HiddenField ID="bcksGroup" runat="server" />
            <asp:DropDownList ID="bcks" AutoPostBack="True" runat="server" />
            <br />
            <asp:Button ID="btnExpSchema" Text="exp. schema" CssClass="rcvry-btn" OnClick="expSchema"
                runat="server" />
            <asp:Button ID="btnExpPck" Text="exp. pck" CssClass="rcvry-btn" OnClick="expBackup"
                runat="server" />
            <asp:Button ID="btnMount" Text="ripristina" CssClass='rcvry-btn' OnClick="mountDb"
                runat="server" />
            <asp:Button ID="btnMountData" Text="ripristina dati" CssClass='rcvry-btn' OnClick="mountData"
                runat="server" />
        </div>
        <!-- scripts -->
        <div class="rcvry-sec" id="scripts_row" runat="server">
            <span class='rcvry-sec-title'>scripts</span>
            <asp:HiddenField ID="scriptsGroup" runat="server" />
            <asp:DropDownList ID="scripts" AutoPostBack="True" runat="server" OnSelectedIndexChanged="scripts_onsel" />
            <br />
            <br />
            <span class='rcvry-sec-subtitle'>descrizione:</span><span id='script_des' runat="server" />
            <br />
            <br />
            <asp:Button ID="btnExecScript" Text="esegui" CssClass='rcvry-btn' OnClick="exec_script"
                runat="server" />
            <asp:Button ID="btnViewScript" Text="view" CssClass='rcvry-btn' OnClick="view_script"
                runat="server" />
        </div>
        <!-- pocket -->
        <div class="rcvry-sec" id="pocket_row" runat="server">
            <span class='rcvry-sec-title'>pacchetto - schema xml</span> <span>carica: </span>
            <input id="fileUpload" type='file' style='display: none;' runat="server" onchange="selectedFile(this, 'pathUpload')" />
            <input id="pathUpload" type="text" readonly="readonly" />
            <input id="btnUpload" type="button" title='seleziona pacchetto gz o indice xml' value='sfoglia...'
                onclick="$('input[id=fileUpload]').click()" />
            <asp:Button ID="import_pocket" Text="importa" OnClick="importDb" ToolTip="importa intero pacchetto gz (schema + dati) o indice xml (solo schema)"
                runat="server" CssClass="rcvry-btn-little" /><br />
            <br />
            <span>cartella di destinazione: </span>
            <input id="pathFolder" type="text" runat="server" style="margin-left: 42px;" /><asp:Button
                ID="extractPocket" Text="spacchetta" OnClick="extractPck" runat="server" CssClass="rcvry-btn-little" /><br />
            <br />
            <span>cartella pocket: </span>
            <input id="pathFolderGen" type="text" runat="server" style="margin-left: 42px;" /><asp:Button
                ID="genPocket" Text="compatta" OnClick="genPck" runat="server" CssClass="rcvry-btn-little" />
        </div>
    </div>
    </form>
</body>
</html>
