$(document).ready(function() {
    window.setTimeout(function(){ $("[field_name][form_name]:first").focus(); }, 150);
});

function form_name (ctrl) { return ctrl.attr("form_name"); }
function field_name (ctrl) { return ctrl.attr("field_name"); }
function input_ctrl(attr, value) { return $("input[" + attr + "=" + value + "]"); }
function combo_ctrl(form, field) { return $("input[form_name=" + form + "][field_name=" + field + "]"); }
function combo_ctrl_id(form, field) { return $("input[form_name_cmb=" + form + "][field_name_cmb=" + field + "]"); }
function btn_ctrl(form, name) { return $("input[form_name=" + form + "][btn_name=" + name + "]"); }
function combo_id(form, field) { return combo_ctrl_id(form, field).val(); }
function set_combo_id(form, field, id) { combo_ctrl_id(form, field).val(id); }
function reset_combo(form, field) { combo_ctrl(form, field).val(""); set_combo_id(form, field, ""); }

function show_combo(form, field) {

    var ctrl = $("input[form_name=" + form + "][field_name=" + field + "]");

    //if(ctrl.attr("opened") == "true") { ctrl.autocomplete("close"); return; }

    if(ctrl.attr("tolist") == "true" && ctrl.val() == ctrl.attr("title")) ctrl.val("");

    ctrl.autocomplete("option", "minLength", 0);
    ctrl.autocomplete("search", "");
    ctrl.autocomplete("option", "minLength", ctrl.autocomplete("option", "minLength"));

    ctrl.focus();

    return false;
}

function combo_onfocus(ctrl) { if($(ctrl).attr("tolist") == "false") _changed_val = $(ctrl).val(); }

function combo_onblur(form, field) {

    try {
        var ctrl = combo_ctrl(form, field), docXml = ctrl.attr("xmldoc"), txtarea = $("#" + docXml)
         , fld_des = txtarea.attr("fielddes"), onsel_des = ctrl.attr("onsel_desfield");

        if($(ctrl).attr("tolist") == "false")
        {
            if(_changed_val != $(ctrl).val()) _check_changed = true;
        var row = combo_row_byval(ctrl.val(), docXml, txtarea.attr("fieldval"));
        if (row == null) {
                if(ctrl.val() == "") { set_combo_id(form, field, ""); if (onsel_des && fld_des) combo_ctrl(form, onsel_des).val(""); }
            return;
        }

        if(combo_reloadkey(form, field)) return;

        set_combo_id(form, field, row.selNode(txtarea.attr("fieldid")).text());
        if (onsel_des && fld_des) combo_ctrl(form, onsel_des).val(row.selNode(fld_des).text());
        } else { $(ctrl).val($(ctrl).attr("title")); }
    } catch (e) { alert(e.message); }
}

var _last_url = null;
function combo_reloadkey(form, field)
{
    var ctrl = combo_ctrl(form, field), doc_xml = ctrl.attr("xmldoc"), txtarea = $("#" + doc_xml), reset_keys = ctrl.attr("onsel_resetkeys")
    , row = combo_row_byval(ctrl.val(), doc_xml, txtarea.attr("fieldval")), reload_key = ctrl.attr("onsel_setkey");

    var url = window.location.href;
    if (row != null && reload_key && ctrl.attr("key_value") != row.selNode(txtarea.attr("fieldid")).text()) 
        url = setparam(url, reload_key, row.selNode(txtarea.attr("fieldid")).text());

    if (reset_keys) reset_keys.split(",").forEach(function (item) { if(item) url = setparam(url, item, ""); });

    if((_last_url && url != _last_url) || (!_last_url && window.location.href != url)) { window.location = url; _last_url = url; return true; }

    return false;
}

function combo_row_byval(value, idDoc, fld_des) {

    value = value.trim().toLowerCase();    
    var xmlDoc = new domDocument($("#" + idDoc).val());
    var rows = xmlDoc.selNodes("/rows/row");
    for (var i = 0; i < rows.length; i++) {
        var row = rows[i];
        if (row.selNode(fld_des).text().trim().toLowerCase() == value)
            return row;
    }

    return null;
}

function list_ctrl(form, field) { return $("input[form_name=" + form + "][field_name=" + field + "]"); }

function list_values(form, field) { return list_ctrl(form, field).val(); }

function list_splitted(form, field) { return list_values(form, field).replace(/\[/g, "").split("]"); }

function listDoc(form, field) { return new domDocument($("#" + list_ctrl(form, field).attr("xmldoc")).val()); }

function listDocAttr(form, field, attr) { return $("#" + list_ctrl(form, field).attr("xmldoc")).attr(attr); }

function init_list(form, field) {

    var ctrl = $("div[form_name=" + form + "][field_name=" + field + "_des]");

    var html = "";
    var split = list_splitted(form, field);
    for (var i = 0; i < split.length; i++) {
        if (split[i] == "") continue;
        var text = listDoc(form, field).selNode("/rows/row[" + listDocAttr(form, field, "fieldid") + "=" + split[i] + "]/" + listDocAttr(form, field, "fieldval")).text();
        html += (html != "" ? ", " : "") + (ctrl.attr("can_remove") == "true" ? "<a href=\"javascript:removeFromList('" + form + "', '" + field + "', '" + split[i] + "')\" title=\"rimuovere l'elemento...\">"
            + text + "</a>" : "<span>" + text + "</span>");
    }
    ctrl.html(html);
}

function removeFromList(form, field, code) {
    var list = list_ctrl(form, field);
    while (list.val().indexOf("[" + code + "]") >= 0)
        list.val(list.val().replace("[" + code + "]", ""));
    init_list(form, field);
    _check_changed = true;
}

function add_tolist(form, combo, tolist) {
    var id = combo_id(form, combo);
    if (upd_list(form, tolist, id)) {
        combo_ctrl(form, combo).val("");
        set_combo_id(form, combo, "");
    }
    _check_changed = true;
}

function upd_list(form, tolist, id) {
    if (id != null && id != "" && list_values(form, tolist).indexOf("[" + id + "]") < 0) {
        list_ctrl(form, tolist).val(list_ctrl(form, tolist).val() + "[" + id + "]");
        init_list(form, tolist);
        return true;
    }

    return false;
}

function init_combo(form, field, tolist) {

    var ctrl = $("input[form_name=" + form + "][field_name=" + field + "]")
     , txtarea = $("#" + ctrl.attr("xmldoc")), fldId = txtarea.attr("fieldid")
     , fld_val = txtarea.attr("fieldval"), fld_des = txtarea.attr("fielddes")
     , onsel_des = ctrl.attr("onsel_desfield"), onsel = ctrl.attr("onsel_item");

    if(tolist){ 
        ctrl.val($(ctrl).attr("title"));
        $(ctrl).focus(function() { if($(ctrl).val() == $(ctrl).attr("title")) $(ctrl).val(""); });
    } 

    ctrl.autocomplete({
        minLength: 0
        , source: function (request, response) {
            // filtro elementi già selezionati
            var vals = tolist != "" ? list_values(form, tolist) : "";
            var arr_vals = vals != "" ? vals.substring(0, vals.length - 1).replace(/\[/g, '').split("]") : null; 
            var filter = "";
            if(arr_vals != null) arr_vals.forEach( function(id) { filter += (filter != "" ? " and " : "") + fldId + "!='" + id + "'"; });

            var doc = new domDocument(txtarea.val());
            var list = doc.selNodes("/rows/row" + (filter != "" ? "[" + filter + "]" : ""));            
            //var matcher = new RegExp("^" + $.ui.autocomplete.escapeRegex($(ctrl).val()), "i");            
            //var matcher = new RegExp($.ui.autocomplete.escapeRegex($(ctrl).val()), "i");
            var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
            response($.grep(list, function (item) {
                return matcher.test(item.selNode(fld_val).text());
            }));
        }//, focus: function (event, ui) { return false; }
        , select: function (event, ui) {
            ctrl.val(ui.item.selNode(fld_val).text());
            set_combo_id(form, field, ui.item.selNode(fldId).text());
            if (onsel_des && fld_des) 
                combo_ctrl(form, onsel_des).val(ui.item.selNode(fld_des).text());

            combo_reloadkey(form, field);

            if(tolist) add_tolist(form, field, tolist);
            if(onsel) eval(onsel)(ctrl);

            return false;
        }, close: function (event, ui) { /*ctrl.attr("opened", "");*/ }
        , open: function (event, ui) { /*ctrl.attr("opened", "true");*/ }
    }).data("ui-autocomplete")._renderItem = function (ul, item) {
	    var title = fld_des ? item.selNode(fld_des).text() : "";
	    return $("<li>").append("<a title=\"" + title + "\" class='combo-item'>" 
            + item.selNode(fld_val).text() + "</a>").appendTo(ul);
	};

}

function initEditor(form, field) {

    $("textarea[form_name=" + form + "][field_name=" + field + "]").htmlarea({
        toolbar: [
                    ["html"], ["bold", "italic", "underline"], 
                    ["justifyleft", "justifycenter", "justifyright"],
                    ["p", "h1", "h2", "h3", "h4", "h5", "h6"],
                ],
        //css: getVar("siteurl") + "libjs/jHtmlArea-0.8/jHtmlArea.Editor.css",
    });
}

function initEuro(form, field) {
    $("input[form_name=" + form + "][field_name=" + field + "]").blur(function () {
        formatEuro(this);
    });
}

function initMigliaia(form, field) {
    $("input[form_name=" + form + "][field_name=" + field + "]").blur(function () {
        formatMigliaia(this);
    });
}

function formatEuro(ctrl) {
    $(ctrl).parseNumber({ format: "€ #,###.00", locale: "it" });
    $(ctrl).formatNumber({ format: "€ #,###.00", locale: "it" });
}

function formatMigliaia(ctrl) {
    $(ctrl).parseNumber({ format: "#,###", locale: "it" });
    $(ctrl).formatNumber({ format: "#,###", locale: "it" });
}

function parse_input(form, field) {
    var ctrl = $("input[form_name=" + form + "][field_name=" + field + "]");
    return ctrl.val() == "" ? null : (ctrl.attr("field_type") == "int" ? parseInt(ctrl.val())
        : ctrl.attr("field_type") == "euro" ? jQuery.parseNumber($(ctrl).val(), { format: "€ #,###.00", locale: "it" }) 
        : ctrl.attr("field_type") == "migliaia" ? jQuery.parseNumber($(ctrl).val(), { format: "#,###", locale: "it" })
        : ctrl.attr("field_type") == "real" ? parseFloat(ctrl.val()) : null);
}

function initDate(form, field, value, format) {
    window.METRO_CURRENT_LOCALE = "it";
    $($("input[form_name=" + form + "][field_name=" + field + "]")[0].parentElement).datepicker(
        { preset: value != null && value != "" ? value : false, format: format, locale: "it"
            , inputVal: $("input[form_name_date=" + form + "][field_name_date=" + field + "]")
            , onSelect: "sel_calendar('" + form + "', '" + field + "')"
    });
}

function sel_calendar(form, field) { 
    var ctrl = $("input[form_name=" + form + "][field_name=" + field + "]");
    if(_changed_val != $(ctrl).val()) _check_changed = true;
}

function overCalendar(form, field) {
    var ctrl = $("input[form_name=" + form + "][field_name=" + field + "]");
    ctrl.attr("cal_showed", $(ctrl[0].parentElement).datepicker("showedCalendar"));
}

function clickCalendar(form, field) {
    var ctrl = $("input[form_name=" + form + "][field_name=" + field + "]");
    _changed_val = ctrl.val();
    $(ctrl[0].parentElement).datepicker(
        ctrl.attr("cal_showed") == "true" ? "hideCalendar" : "showCalendar");
    overCalendar(form, field);
}

function dateblur(element) {
    if(element.value != "") $(element.parentElement).datepicker("setDate"
        , new Date(Date.parse(element.value)));
    else $(element.parentElement).datepicker("resetDate");
    $(element.parentElement).datepicker("hideCalendar");    
    if(_changed_val != $(element).val()) _check_changed = true;
}

function datefocus(element) {
    _changed_val = $(element).val();
    $(element.parentElement).datepicker("hideCalendar");
    if($(element.parentElement).datepicker('inputVal') != ""){
        var dt = $(element.parentElement).datepicker('getDateObject');
        if (!isNaN(dt) && dt != null) { 
            element.value = dt.format('yyyy-mm-dd'); 
            $(element).select(); 
        }
    }
}

function onkeydowndate(event, element, form_name) {
    if ((event.keyCode == 38 || event.keyCode == 40) && element.value != ""){
        var dt = $(element.parentElement).datepicker('getDateObject');
        if (!isNaN(dt) && dt != null) {
            var newdt = new Date();
            newdt.setTime(event.keyCode == 38 ? dt.getTime() + (24 * 60 * 60 * 1000)
             : dt.getTime() - (24 * 60 * 60 * 1000));

            element.value = newdt.format('yyyy-mm-dd');
            $(element.parentElement).datepicker("setDate", newdt, false);
            $(element.parentElement).datepicker("hideCalendar");
        }
        return false;
    }
    else if(event.keyCode == 13){
        dateblur(element);
        if(submit(form_name)) return false;        
    }

    return true;
}

function submit(form)
{
    if($("input[btn_type=submit][form_name=" + form + "]").length > 0) {        
        window.setTimeout(function(){ $("input[btn_type=submit][form_name=" + form + "]:first").click() }, 100);
        return true;
    }
    return false;
}

function onkeydowninput(event, element, form_name) {
    if(event.keyCode == 13) {
        $(element).blur();
        if(submit(form_name)) return false;
    }

    return true;
}

function selFile(form, field) {
    var p = $("input[form_name=" + form + "][field_name=" + field + "][subname=upload-ctrl]").val();
    $("input[form_name=" + form + "][field_name=" + field + "][subname=input-ctrl]").val(p.replace(/^.*(\\|\/|\:)/, ''));
}

function click_button(el, type) {
    try{
        if(type != "action" && (querystring("child") == "1" && type == "exit")) { window.close(); return false; }
        else if (type == "submit" || (type == "action" && $(el).attr("checks") == "true")) { if(!check_combos($(el).attr("form_name"))) return false; }
    } catch(e) {}
    
    return true;
}

function check_combos(form_name)
{
    var valid = true;
    $("[field_type='combo']").each(function() { 
        if($(this).attr("tolist") == "false" && $(this).val()) 
        {
            var doc = $(this).attr("xmldoc"), txtarea = $("#" + doc);
            if (combo_row_byval($(this).val(), doc, txtarea.attr("fieldval")) == null) {
                metro_alert("Hai specificato un valore non presente nella lista!" + ($(this).attr("title") != "" ? "<br/><br/>" + $(this).attr("title") : ""), "Nota Bene");
                return valid = false;                    
            }
        }
    });

    return valid;
}

function request_combo(form, field)
{
    try {

        var ctrl = $("*[form_name=" + form + "][field_name=" + field + "]"), select = ctrl.attr("lst_sel");

        // request doc
        var doc = new domDocument();
        if (!doc.loadXml("<request schema='page.request' action='ctrl_request'><pars/></request>"))
            throw new Error("problemi nell'inizializzazione del documento xml di richiesta.");

        doc.selNode("/request/pars").addNode("par", form).setAttributeText("name", "ctrlname");
        doc.selNode("/request/pars").addNode("par", select).setAttributeText("name", "select");
        doc.selNode("/request/pars").addNode("par", "reload_combo").setAttributeText("name", "type");

        // send request
        var response = doc.http_request(window.location.href, false);
        if (response.selNode("/response[@result='error']") != null) {
            metro_alert("Ci sono stati dei problemi nell'aggiornare la lista - "
                + (response.selNode("/response/err") != null ? " sull'elemento: " + response.selNode("/response/err").text() : "."), "Attenzione!");
            return;
        }

        var txtarea = $("textarea[forcombos=" + select + "]");
        txtarea.val(response.getXml());
        var fldid = txtarea.attr("fieldid"), max = -1, rows = response.selNodes("/rows/row");
        for (var i = 0; i < rows.length; i++) {
            max = rows[i].selNode(fldid).text() != "" && parseInt(rows[i].selNode(fldid).text()) > max ?
                parseInt(rows[i].selNode(fldid).text()) : max;
        }

        if(max >= 0 && ctrl.attr("field_type") == "list") upd_list(form, field, max);
        else {
            var fld_val = txtarea.attr("fieldval"), fld_des = txtarea.attr("fielddes"), onsel_des = ctrl.attr("onsel_desfield");
            set_combo_id(form, field, max.toString());
            ctrl.val(response.selNode("/rows/row[" + fldid + "=" + max + "]/" + fld_val).text());
            if (onsel_des && fld_des) combo_ctrl(form, onsel_des)
                .val(response.selNode("/rows/row[" + fldid + "=" + max + "]/" + fld_des).text());
        }
    }
    catch (e) { metro_alert("Non è stato possibile completare l'operazione." + (e.message != "" ? "Errore: " + e.message : "")); }
}

function set_active_tab(tab_name, index) { $("#" + tab_name + "_activetab").val(index); }

function get_active_tab(tab_name) { return $("#" + tab_name + "_activetab").val(); }

function init_tab(page, tab_name) { add_unload_fnc("unload_tab('" + page + "', '" + tab_name + "')"); }

function unload_tab (page, tab_name) { 
    var ukey = $("input[tab_name=" + tab_name + "]").attr("unload_key");
    if(ukey) add_unload_key(ukey, "active_tab", get_active_tab(tab_name)); 
}

// check changed
var _changed_val = null;
function focus_input(ctrl) { _changed_val = $(ctrl).val(); }
function blur_input(ctrl) { if(_changed_val != $(ctrl).val()) _check_changed = true; }
