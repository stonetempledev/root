function sel_onfocus(el) { $(el).select(); }
function do_post_back(action, args) {
    $("#__action").val(action ? action : ""); $("#__args").val(args ? args : "");
    $("form[mainform=true]").submit();
}
function is_url(str) {
    var pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
  '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.?)+[a-z]{2,}|' + // domain name
  '((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
  '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
  '(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
  '(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
    return pattern.test(str);
}