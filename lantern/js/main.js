var _check_changed = false

// salvataggio impostazioni client
var _unload_fncs = null, _request = null;
window.onbeforeunload = function() 
{
    try{ 
        if(_unload_fncs != null){ _unload_fncs.forEach(function (expr) { eval(expr); } ); }

        if(_request != null && _request.length > 0) { 
            var doc = new domDocument();
            if (!doc.loadXml("<request schema='page.request' action='save_unload_keys'></request>"))
                throw new Error("problemi nell'inizializzazione del documento xml di richiesta.");

            _request.forEach(function (key) {
                var pars_node = doc.selNode("/request").addNode("pars");
                pars_node.addNode("par", key["keys"]).setAttributeText("name", "keys");
                pars_node.addNode("par", key["var"]).setAttributeText("name", "var");
                pars_node.addNode("par", key["val"]).setAttributeText("name", "val");
                pars_node.addNode("par", key["onsession"] ? "true" : "false").setAttributeText("name", "onsession");
            });

            doc.http_request(window.location.href, false);        

            _request = null;
        }        
    
    }catch(e){}
}

function add_unload_fnc(expr) { if(_unload_fncs == null) _unload_fncs = []; _unload_fncs.push(expr); };
function add_unload_key(keys, name_var, val, onsession) { 
    if(_request == null) _request = []; 
    _request.push({"keys": keys, "var": name_var, "val": val, "onsession": onsession != null ? onsession : true}); 
};

// page_flags
function set_page_flag(text) { if(!check_page_flag(text) && ctrl_flags().length > 0) ctrl_flags().val(ctrl_flags().val() + "[" + text + "]"); }
function check_page_flag(text) { return ctrl_flags().length > 0 && ctrl_flags().val().indexOf("[" + text + "]") >= 0; }
function ctrl_flags() { return $("input[ctrl-id=_page_flags]"); }

function isie() { return navigator.appName == "Microsoft Internet Explorer" ? true : false; }

function refresh_page() { window.setTimeout(function(){ set_page_flag("refreshed"); postBack(); }, 200); }

// torna il valore del parametro del query string
function querystring(key, default_) {
    var regex = new RegExp("[\\?&]" + key.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]") + "=([^&#]*)");
    var qs = regex.exec(window.location.href);
    return qs == null ? (default_ == null ? "" : default_) : qs[1];
}

function setparam(uri, key, value) {
    var re = new RegExp("([?|&])" + key + "=.*?(&|#|$)", "i");
    var separator = uri.indexOf('?') != -1 ? "&" : "?";
    return uri.match(re) ? (uri.replace(re, '$1' + key + "=" + value + '$2'))
        : (uri + separator + key + "=" + value);
}

function innerText(element, text) {
    if (element == null || typeof (element) == "undefined") return "";

    if(text == null) return typeof (element.textContent) != "undefined" ? element.textContent : element.innerText;

    if (typeof (element.textContent) != "undefined") element.textContent = text;
    else element.innerText = text;
}


function text_alert() { return $("#metroalerttxt").val(); }

function metro_alert(content, tit, refonclose, evalonclose, yesno, icon, wd, hg) {
    try{
        if ($('#dlg-metro').length) $('#dlg-metro').remove();

        var yes_icon = !(icon != null && icon == false);
        $(document.body).append("<div id='dlg-metro' style='display:none;padding:5px'>"
            + (yes_icon ? " <span class='" + (icon == null ? "mif-checkmark" : icon) + " mif-2x' style='margin-right:15px'></span>" : "") + content + "</div>");

        var fnc_onclose = (refonclose != null && refonclose != "") || (evalonclose != null) ? function (result) {
            if (evalonclose != null) evalonclose(result);
            if (refonclose != null && refonclose != "") window.location = refonclose;
        } : null;

        var btns = yesno == null || !yesno ? [{
			        text: "Ok",
			        click: function() {
				        $( '#dlg-metro' ).dialog( "close" );
                        if(fnc_onclose != null) fnc_onclose("ok");
			        }
		        }] : 
                [{
			        text: "Si",
			        click: function() {
				        $( '#dlg-metro' ).dialog( "close" );
                        if(fnc_onclose != null) fnc_onclose("yes");
			        }
		        },{
			        text: "No",
			        click: function() {
				        $( '#dlg-metro' ).dialog( "close" );                        
			        }
		        }];

        $( "#dlg-metro" ).dialog({ autoOpen: false, modal: true, width: wd ? wd : 450, height: hg ? hg : 350
            , title: tit != null ? tit : "Messaggio", buttons: btns, resizable: true, draggable: false
            , closeOnEscape: false, dialogClass: 'no-close'
        });

        $("#dlg-metro").dialog("open");

    } catch (e) { if($("#dlg-metro").length) $("#dlg-metro").hide(); alert("errore in metro_alert(): " + e.message); }
}

function alert_si(){ $('#dlg-metro').dialog('option', 'buttons')[0].click(); }

function alert_no(){ $('#dlg-metro').dialog('option', 'buttons')[1].click(); }

function newMenu ()  { return new domDocument("<menu/>"); }

function addMenuItem(docitm, title, code)
{
    var item = docitm.isDomDocument == undefined ? item.addNode("item")
     : (docitm.isDomDocument == true) ? docitm.rootNode().addNode("item") : null;

    item.setAttributeText("title", title);
    item.setAttributeText("code", code);
    
    return item;
}

// metroMenu
//
// gestione menu di contesto legato agli elementi html del documento
// doc: documento xml contenente gli elementi da parsificare
//  <menu>
//   <item title=''>
//    <item title='' code=''/>
//   </item>
//  </menu>
function metroMenu(selDelegate, doc) {

    try {

        // creo il div per il messaggio
        if ($("#menu-panel").length == 0) {
            var div = document.createElement("div");
            div.id = "menu-panel";
            document.body.appendChild(div);
        }

        // costruisco l'html
        var html = "";//htmlFromMenuItem(doc);

        // lo visualizzo
        $(document).contextmenu({
            delegate: selDelegate,
		    preventContextMenuForPopup: true,
		    preventSelect: true,
		    taphold: true,
            menu: "#menu-panel-main",
            position: function (event, ui) {
                return { my: "left top", at: "left bottom", of: ui.target };
            },
        })

    } catch (e) {
        if ($("#menu-panel").length > 0)
            $("#menu-panel")[0].style.display = "none";

        alert("errore in metroMenu(): " + e.message);
    }
}

function postBack(cmd, val) {

    try {

        var form = document.forms[0];
        if(cmd != null) form.__EVENTTARGET.value = cmd;
        if(val != null) form.__EVENTARGUMENT.value = val;
        form.submit();

    } catch (e) { }
}

function request_form(ctrl, form_name, action, demandid) {

    try {

        if($(ctrl).attr("clicked_submit") == "true") return true;

        // domandina
        if (demandid != null && demandid != "") {

            var textarea = document.getElementById(demandid);
            if (textarea == null)
                throw new Error("non è stato trovato il testo del messaggio di conferma!");

            metro_alert(innerText(textarea), "Attenzione!", null, function() { request_form(ctrl, form_name, action); }, true);

            return false;
        }

        $(ctrl).attr("clicked_submit", "true");
        $(ctrl).click();

        return false;
    }
    catch (e) {
        metro_alert("Non è stato possibile cancellare il record."
            + (e.message != "" ? "Errore: " + e.message : ""));

        return false;
    }
}

function getVar(name) { return innerText(document.getElementById("var" + name)); }

function navToPrev() {
    try {
        if(_check_changed)
            metro_alert("Sono state apportate delle modifiche.<br/><br/>Vuoi uscire ugualmente?"
            , "Attenzione", null, function() { /*if(check_page_flag('refreshed') && window.opener) window.opener.refresh_page();*/ window.close(); }, true); 
        else { if(querystring("child") == "1") { /*if(check_page_flag('refreshed') && window.opener) window.opener.refresh_page();*/ window.close(); } else window.location = getVar("urlToPrev"); }        
    }
    catch (e) { metro_alert("L'operazione non è andata a buon fine." + (e.message != "" ? "Errore: " + e.message : "")); }
}

function reloadPage() {

    try { window.location = window.location; }
    catch (e) { metro_alert("L'operazione non è andata a buon fine."
        + (e.message != "" ? "Errore: " + e.message : "")); }
}



