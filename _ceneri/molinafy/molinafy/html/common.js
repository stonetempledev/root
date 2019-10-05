function init_doc() {

  // @contenteditable, @selonfocus, @not_empty, salvataggio documento
  $('[contenteditable=true]').focus(function () {
    $(this).data("prev_cr", count_cr(this));
    $(this).data("prev_txt", clean_text(this));
    if ($(this).attr("selonfocus")) document.execCommand('selectAll', false, null);
  }).blur(function () {
    if (check_changed(this, true)) changed_doc($(this)[0].outerHTML);
  });

  // @maxlength
  $("[contenteditable='true'][maxlength]").on('keydown paste', function (event) {
    var ml = parseInt($(this).attr('maxlength'));
    if ($(this).text().length === ml && event.keyCode != 8) { event.preventDefault(); }
  });

}

function changed_doc(html_ele) { window.external.changed_doc(html_ele); }
function add_paragraph(html_ele) { window.external.add_paragraph(html_ele); }

function check_changed_focused() { var focused = $(':focus'); return check_changed(focused) ? $(focused)[0].outerHTML : ""; }

function check_changed(el, on_event) {
  if (!el || $(el).attr("contenteditable") != "true") return false;
  if (!clean_text(el) && $(el).attr("not_empty")) { $(el).text($(el).data("prev_txt")); if (on_event) event.preventDefault(); return false; }
  if ($(el).data("prev_txt") != clean_text(el)
    || $(this).data("prev_cr") != count_cr(el)) {
    $(el).data("prev_txt", clean_text(el));
    $(el).data("prev_cr", count_cr(el));
    return true;
  }
  return false;
}

function count_cr(el) { return ($(el).html().match(/<br>/g) || []).length; }

function clean_text(el) { return $.trim($(el).text()); }

function is_url(str) {
  var pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
  '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.?)+[a-z]{2,}|' + // domain name
  '((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
  '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
  '(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
  '(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
  return pattern.test(str);
}

function dump(obj) { var out = ''; for (var i in obj) out += i + ": " + obj[i] + "\n"; return out; }
