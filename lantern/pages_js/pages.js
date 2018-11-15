function check_http_ctrl(ctrl) {
    var val = $(ctrl).val().replace(/\\/g, '/');
    if (val && val.length > 0 && val.substring(val.length - 1, val.length) == '/') val = val.substring(0, val.length - 1);
    var prefixs = ["http://", "https://", "ftp://"], find = false;
    prefixs.forEach(function (el) { if (val.length >= el.length && val.substr(0, el.length).toLowerCase() == el.toLowerCase()) { find = true; return; } });
    $(ctrl).val((!find ? "http://" : "") + val);
}

