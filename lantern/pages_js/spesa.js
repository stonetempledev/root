function onchangequantita(ctrl) { updtotale($(ctrl).attr("form_name")); }

function onchangeprezzo(ctrl) { updtotale($(ctrl).attr("form_name")); }

function updtotale(frm) {
    var tot = combo_ctrl(frm, "totale");
    try {
        var prezzo = parse_input(frm, "prezzo");
        if (prezzo == null) tot.value = "";
        else {
            var qta = parse_input(frm, "quantita");
            tot.val((qta != null && qta != 0 ? qta * prezzo : prezzo)
                .toString().replace(".", ","));
            formatEuro(tot);
        }

    } catch (e) { tot.value = ""; }
}