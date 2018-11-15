$(document).ready(function() {
    $(document).keydown(function (event) {
        var btns = $("a[shortkeys]");
        for (var i = 0; i < btns.length; i++)
            shortKey(event, btns[i]);
    });
});

// tooltips
$(function () {
    $(".item-tbl").tooltip({
        items: "input",
        content: function () {
            if ($(this).attr("show_title") == "true" && $(this).attr("type") != "checkbox") {
            var t = $(this).attr("title"), v = $(this).val();
            return t ? (v ? "<b>" + t + "</b>" : t) + (v ? "<br/><br/>" + v : "") : (v ? v : "");
            }
        }, tooltipClass: "tooltip-grid"
    });
    $(".header-tbl").tooltip({
        content: function () { return $(this).attr("title"); }
        , tooltipClass: "tooltip-grid"
    });
    $(".arrows").tooltip({
        content: function () {
            var filter = filter_text_field(this);
            return filter != "" ? "<b>filtri impostati</b><br/><br/>" + filter : "imposta filtri e ordinamento";
        }, tooltipClass: "tooltip-grid"
    });
});

function filter_text_field(ctrl) {
    var tbl = $(ctrl).parents('table[grid_name]'), gr_name = tbl.attr("grid_name"), textarea = $("#Xml" + tbl.attr("id"));
    if (textarea.length > 0) {
        var doc = new domDocument();
        if (doc.loadXml(textarea.val()) && doc.selNode("/root/filter/field[@name='" + $(ctrl).attr("field_name") + "'][@user_filter='true']"))
            return doc.selNode("/root/filter/field[@name='" + $(ctrl).attr("field_name") + "'][@user_filter='true']").text();
    }

    return "";
}

function shortKey(event, element) {
    var combine = element.getAttribute("shortkeys").split("+");
    for (var i = 0; i < combine.length; i++) {
        var code = strings.trim(combine[i]).toUpperCase();
        if ((code == "CTRL" && event.ctrlKey) || (code == "SHIFT" && event.shiftKey)
         || (code == "ALT" && event.altKey)
         || (String.fromCharCode(event.keyCode).toUpperCase() == code))
            continue;

        return;
    }

    element.click();
}

function resetFilter(gridId) {
    try {
        var textarea = $("#Xml" + gridId);
        if (textarea.length > 0) {
            var docXml = new domDocument();
            if (docXml.loadXml(textarea.val())) {
                docXml.rootNode().setAttributeText("reset", "true");
                textarea.val(docXml.getXml())
            }
        }

        document.forms[0].submit();
    } catch (e) { }
}

function changed_filter(gridid, filterName) { try { postBack(); } catch (e) { } }

function field_no_alias(field_alias) { return field_alias.indexOf('.') >= 0 ? field_alias.substr(field_alias.indexOf('.') + 1) : field_alias; }

function alias_field(field_alias) { return field_alias.indexOf('.') >= 0 ? field_alias.substring(0, field_alias.indexOf('.')) : ""; }

function update_filter(gridid, field, value, type, doPostBack) {

    try {

        if (doPostBack == null) doPostBack = true;
        if (type == null) type = "textbox";

        // aggiorno l'xml
        var textarea = $("#Xml" + gridid);
        if (textarea.length > 0) {
            var docXml = new domDocument();
            if (docXml.loadXml(textarea.val())) {
                var filters = docXml.selNode("/root/filter"), filter = filters.selNode("field[@name='" + field + "'][@user_filter='true']");
                if (value) {
                    if (filter == null) {
                        filter = filters.addNode("field", value);
                        filter.setAttributeText("user_filter", "true");
                        filter.setAttributeText("name", field);
                    } else filter.setText(value);
                }
                else if (filter != null) filters.removeChild(filter);

                docXml.rootNode().setAttributeText("page", "0");
                textarea.val(docXml.getXml());
                if (doPostBack) postBack();
            }
        }

    } catch (e) { }
}

function upd_active_page(gridid, activePage, doPostBack) {

    try {

        if (doPostBack == null) doPostBack = true;

        // aggiorno l'xml
        var textarea = $("#Xml" + gridid);
        if (textarea.length > 0) {
            var docXml = new domDocument();
            if (docXml.loadXml(textarea.val())) {
                docXml.rootNode().setAttributeText("page", activePage);
                textarea.val(docXml.getXml());
                if (doPostBack) postBack();
            }
        }

    } catch (e) { }
}

function updateGridSort(gridid, field, doPostBack) {

    try {

        if (doPostBack == null) doPostBack = true;

        // aggiorno l'xml
        var textarea = $("#Xml" + gridid);
        if (textarea.length > 0) {
            var docXml = new domDocument();
            if (docXml.loadXml(textarea.val())) {
                var sorts = docXml.selNode("/root/sort");
                var sort = sorts.selNode("field[@name='" + field + "']");
                if (sort == null) {
                    sort = sorts.addNode("field");
                    sort.setAttributeText("name", field);
                    sort.setAttributeText("direction", "ASC");
                }
                else {
                    var direction = sort.getAttributeText("direction");
                    if (direction == "ASC") sort.setAttributeText("direction", "DESC");
                    else if (direction == "DESC") sorts.removeChild(sort);
                }
                
                textarea.val(docXml.getXml());
                if (doPostBack) postBack();
            }
        }

    } catch (e) { }
}

function grid_contextmenu() {
    if (event.srcElement.getAttribute("colgrid") != null)
        return;

    event.cancelBubble = true;
    event.returnValue = false;
    return false;
}

function col_contextmenu(gridid, field) {
    var filterval = "", docXml = new domDocument();
    if (docXml.loadXml($("#Xml" + gridid).val())) {
        var node = docXml.selNode("/root/filter/field[@name='" + field + "'][@user_filter='true']");
        if (node != null) filterval = node.text();
    }

    metro_filter(filterval, "Imposta il filtro", function () { update_filter(gridid, field, text_alert()); }, true);
    event.cancelBubble = true;
    event.returnValue = false;
    return false;
}

function metro_filter(content, title, func, yesno) {
    metro_alert("<input id='metroalerttxt' style='width:100%;' type='text' value=\"" + content + "\" onkeydown=\"if(event.keyCode == 13) { alert_si(); return false; } else if(event.keyCode == 27) { alert_no(); return false; }\"/>"
        , title, null, func, yesno, false, 450, 175);
    $("#metroalerttxt").focus(function () { this.select(); });
}

function requestRowGrid(gridName, url, keys, queries, action, demandid) {

    try {

        // domandina
        if (demandid != null && demandid != "") {

            var textarea = document.getElementById(demandid);
            if (textarea == null)
                throw new Error("non è stato trovato il testo del messaggio di conferma!");

            metro_alert(innerText(textarea), "Attenzione!", null, function () { requestRowGrid(gridName, url, keys, queries, action); }, true);

            return;
        }

        // request doc
        var doc = new domDocument();
        if (!doc.loadXml("<request schema='page.request' action='ctrl_request'><pars/></request>"))
            throw new Error("problemi nell'inizializzazione del documento xml di richiesta.");

        doc.selNode("/request/pars").addNode("par", (queries != "" ? "execQueries" : "execActionItem")).setAttributeText("name", "type");
        doc.selNode("/request/pars").addNode("par", gridName).setAttributeText("name", "ctrlname");
        doc.selNode("/request/pars").addNode("par", keys).setAttributeText("name", "keys");
        if (queries != "") doc.selNode("/request/pars").addNode("par", queries).setAttributeText("name", "queries");
        if (action != "") doc.selNode("/request/pars").addNode("par", action).setAttributeText("name", "action");

        // send request
        var response = doc.http_request(url, false);
        if (response.selNode("/response").getAttributeText("result") == "error") {
            metro_alert("Ci sono stati dei problemi nel completare l'operazione"
                + (response.selNode("/response/err") != null ? " sull'elemento: " + response.selNode("/response/err").text() : "."), "Attenzione!");
            return;
        }

        metro_alert("aggiornamento avvenuto con successo!", "Messaggio", null, function () { postBack() });
    }
    catch (e) {
        metro_alert("Non è stato possibile completare l'operazione."
            + (e.message != "" ? "Errore: " + e.message : ""));
    }
}

function requestDelRowGrid(gridName, url, keys, primarykey, force, demandid) {

    try {

        // domandina
        if (demandid != null && demandid != "") {

            var textarea = document.getElementById(demandid);
            if (textarea == null)
                throw new Error("non è stato trovato il testo del messaggio di conferma!");

            metro_alert(innerText(textarea), "Attenzione!", null, function () {
                requestDelRowGrid(gridName, url, keys, primarykey, force);
            }, true);

            return;
        }

        // request doc
        var doc = new domDocument();
        if (!doc.loadXml("<request schema='page.request' action='ctrl_request'><pars/></request>"))
            throw new Error("problemi nell'inizializzazione del documento xml di richiesta.");

        doc.selNode("/request/pars").addNode("par", "delRecord").setAttributeText("name", "type");
        doc.selNode("/request/pars").addNode("par", gridName).setAttributeText("name", "ctrlname");
        doc.selNode("/request/pars").addNode("par", keys).setAttributeText("name", "keys");
        doc.selNode("/request/pars").addNode("par", primarykey).setAttributeText("name", "primarykey");
        doc.selNode("/request/pars").addNode("par", force ? "true" : "false").setAttributeText("name", "force");

        // send request
        var response = doc.http_request(url, false);
        if (response.selNode("/response").getAttributeText("result") == "error") {
            metro_alert("Ci sono stati dei problemi nel completare l'operazione"
                + (response.selNode("/response/err") != null ? " sull'elemento: " + response.selNode("/response/err").text() : "."), "Attenzione!");
            return;
        }

        // elementi collegati
        if (response.selNode("/response/related_records[.='true']")) {
            metro_alert("ci sono degli elementi collegati, devi decidere cosa farne...", "Messaggio"
             , null, function () { window.location = response.selNode("/response/url_delpage").text(); });
            return;
        }

        metro_alert("aggiornamento avvenuto con successo!", "Messaggio", null, function () { postBack() });
    }
    catch (e) {
        metro_alert("Non è stato possibile completare l'operazione."
            + (e.message != "" ? "Errore: " + e.message : ""));
    }
}

function request_grid(gridName, action, demandid) {

    try {

        // domandina
        if (demandid != null && demandid != "") {

            var textarea = document.getElementById(demandid);
            if (textarea == null)
                throw new Error("non è stato trovato il testo del messaggio di conferma!");

            metro_alert(innerText(textarea), "Attenzione!", null, function () {
                request_grid(gridName, action);
            }, true);

            return;
        }

        // request doc
        var doc = new domDocument();
        if (!doc.loadXml("<request schema='page.request' action='ctrl_request'><pars/></request>"))
            throw new Error("problemi nell'inizializzazione del documento xml di richiesta.");

        doc.selNode("/request/pars").addNode("par", "execAction").setAttributeText("name", "type");
        doc.selNode("/request/pars").addNode("par", gridName).setAttributeText("name", "ctrlname");
        doc.selNode("/request/pars").addNode("par", action).setAttributeText("name", "action");

        // send request
        var response = doc.http_request(window.location.href, false);
        if (response.selNode("/response").getAttributeText("result") == "error") {
            metro_alert("Ci sono stati dei problemi nel completare l'operazione"
                + (response.selNode("/response/err") != null ? " sull'elemento: " + response.selNode("/response/err").text() : "."), "Attenzione!");
            return;
        }

        metro_alert("aggiornamento avvenuto con successo!", "Messaggio", null, function () { postBack() });
    }
    catch (e) {
        metro_alert("Non è stato possibile effettuare l'aggiornamento."
            + (e.message != "" ? "Errore: " + e.message : ""));
    }
}

function swipe_grid(lr, grid_name) {
    var tbl = $("table[grid_name=" + grid_name + "]"), n_pages = parseInt(tbl.attr("n_pages")), textarea = $("#Xml" + tbl.attr("id"));
    var page = -1;
    if (textarea.length > 0) {
        var docXml = new domDocument();
        if (docXml.loadXml(textarea.val())) page = parseInt(docXml.rootNode().getAttributeText("page"));
    }
    if(lr == "right") { if (page > 0) upd_active_page(tbl.attr("id"), page - 1); }
    else { if (page < n_pages - 1) upd_active_page(tbl.attr("id"), page + 1); }    
}

function init_grid(page, grid_name) {
    $("table[grid_name=" + grid_name + "]").swipe({
        swipeRight: function (event, direction, distance, duration, fingerCount, fingerData) { swipe_grid("right", grid_name); }
		, swipeLeft: function (event, direction, distance, duration, fingerCount, fingerData) { swipe_grid("left", grid_name); }
    });
    add_unload_fnc("unload_grid('" + page + "', '" + grid_name + "')");
    upd_fields_arrows(grid_name); 
}

function unload_grid(page, grid_name) {
    var ukey = $("table[grid_name=" + grid_name + "]").attr("unload_key");
    var gridid = $("table[grid_name=" + grid_name + "]").attr("id");
    if (ukey) add_unload_key(ukey, "active_filter", $("#Xml" + gridid).val(), false);
}

function upd_fields_arrows(grid_name) {
    $("table[grid_name=" + grid_name + "]").find(".arrows").css("border"
        , function () { return filter_text_field(this) != "" ? "1px solid #2086bf" : "1px solid lightgray"; }); 
}