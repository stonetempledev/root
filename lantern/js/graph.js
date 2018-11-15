var fonts = "sans-serif";

$(document).ready(function () {
  CanvasJS.addCultureInfo("it", { decimalSeparator: ",", digitGroupSeparator: ".",
    days: ["domenica", "lunedi", "martedi", "mercoledi", "giovedi", "venerdi", "sabato"],
    shortDays: ["dom", "lun", "mar", "mer", "gio", "ven", "sab"],
    months: ["Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno", "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre"],
    shortMonths: ["Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic"]
  });

  init_charts();
});

function init_charts() {
  $("div[grname]").each(function (index) {
    var name = $(this).attr("grname"), doc = new domDocument($("#data_" + name).val());
    render_graph($(this).attr("id"), doc.selNode("/root/datas"));
  });
}

function render_graph(id, datas) {
  var fmt_val = datas.getAttributeText("format-value"), dts = [];
  datas.selNodes("data").forEach(function (data) {
    var pts = [];
    data.selNodes("row").forEach(function (row) {
      if (row.selNode("y").text() != "{null}") pts.push({ x: new Date(row.selNode("x").text()), y: parseFloat(row.selNode("y").text()) })
    });
    dts.push({ dataPoints: pts, showInLegend: true, legendText: data.getAttributeText("lg-grid") != "" ? data.getAttributeText("lg-grid") : null, axisYType: data.getAttributeText("axis-type"), color: data.getAttributeText("color-line"), yValueFormatString: fmt_val
      , toolTipContent: data.getAttributeText("tp-grid"), type: data.getAttributeText("type"), fillOpacity: data.getAttributeText("fill-opacity")
      , indexLabelFontColor: "black", indexLabelFontWeight: "bold", indexLabelFontSize: 16, markerSize: data.getAttributeText("marker-size")
      , lineThickness: data.getAttributeText("line-thikness"), xValueFormatString: "DDDD DD MMMM YYYY"
      , click: window[data.getAttributeText("onclick")]
    });
  });
  render_days(id, datas.getAttributeText("interval"), fmt_val, dts);
}


function render_days(id_chart, tp_interval, format_value, dts) {
  var chart = new CanvasJS.Chart(id_chart,
  { culture: "it", theme: "theme1"
    , axisX: { labelFontSize: 15, labelFontFamily: fonts, lineColor: "lightgrey", lineThickness: 1, tickThickness: 0
      , valueFormatString: "DD MMM YYYY", interval: tp_interval ? 1 : null, intervalType: tp_interval, labelAngle: 50
    }
    , axisY: { labelFontSize: 14, labelFontFamily: fonts, lineColor: "lightgrey", lineThickness: 1, tickThickness: 1
      , gridThickness: 1, gridColor: "lightgrey", valueFormatString: format_value // "\u20AC #,##0.##"
    }
    , axisY2: { labelFontSize: 14, labelFontFamily: fonts, lineColor: "lightgrey", lineThickness: 1, tickThickness: 1
      , gridThickness: 0, gridColor: "lightgrey", valueFormatString: format_value // "\u20AC #,##0.##"
    }, data: dts
    , legend: {
      cursor: "pointer",
      itemclick: function (e) {
        if (typeof (e.dataSeries.visible) === "undefined" || e.dataSeries.visible) e.dataSeries.visible = false;
        else e.dataSeries.visible = true;
        chart.render();
      }
    }

  });

  chart.render();
}

function i_x(id_chart, val) {
  if (val == null) {
    var i = $("#" + id_chart).attr("i_x");
    return i ? i : 0;
  }

  $("#" + id_chart).attr("i_x", val.toString());
}

function scroll_right(id_chart) { var i = i_x(id_chart); i++; i_x(id_chart, i); get_chart(id_chart, i); }

function scroll_left(id_chart) { var i = i_x(id_chart); i--; i_x(id_chart, i); get_chart(id_chart, i); }

function get_chart(id_chart, i_x) {
  try {
    // request doc
    var doc = new domDocument();
    if (!doc.loadXml("<request schema='page.request' action='ctrl_request'><pars/></request>"))
      throw new Error("problemi nell'inizializzazione del documento xml di richiesta.");

    doc.selNode("/request/pars").addNode("par", "graph.scroll").setAttributeText("name", "type");
    doc.selNode("/request/pars").addNode("par", $("#" + id_chart).attr('grname')).setAttributeText("name", "ctrlname");
    doc.selNode("/request/pars").addNode("par", i_x.toString()).setAttributeText("name", "i_x");

    // send request
    var response = doc.http_request(window.location.href, false);
    if (response.selNode("/response") != null && response.selNode("/response").getAttributeText("result") == "error")
      throw new Error(response.selNode("/response/err") != null ? response.selNode("/response/err").text() : "");

    render_graph(id_chart, response.selNode("/datas"));

  } catch (e) { metro_alert("si è verificato un errore: " + e.message); }
}
