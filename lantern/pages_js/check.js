function click_spese(e) { click_riep(e, _url_regitrazioni); }
function click_speseq(e) { click_riep(e, _url_spese + "&codes=" + encodeURIComponent(_codes_spese)); }
function click_speses(e) { click_riep(e, _url_spese + "&not_codes=" + encodeURIComponent(_codes_svaghi)); }
function click_bollette(e) { click_riep(e, _url_bollette); }
function click_rifornimenti(e) { click_riep(e, _url_rifornimenti); }
function click_manutenzioni(e) { click_riep(e, _url_manutenzioni); }

function click_saldi(e) { click_riep(e, _url_cc + "&idcc=" + _cc_id); }
function click_spese_cc(e) { click_riep(e, _url_cc + "&neg=1&idcc=" + _cc_id); }
function click_entrate_cc(e) { click_riep(e, _url_cc + "&pos=1&idcc=" + _cc_id); }

function click_riep(e, url) {
  var to = e.dataPoint.x, from = null;
  if (e.dataPointIndex >= e.dataSeries.dataPoints.length - 1) { from = new Date(e.dataPoint.x); from.setDate(from.getDate() - 6); }
  else { from = new Date(e.dataSeries.dataPoints[e.dataPointIndex + 1].x); from.setDate(from.getDate() + 1); }
  window.open(url + "&from=" + from.format('yyyymmdd') + "&to=" + to.format('yyyymmdd'), '_blank')
}
