////////////////////////////
// domNode
//
function domNode(node, doc) {

    if (doc.isDomDocument == undefined)
        throw new Error("domNode: documento in ingresso errato");

    var _node = node;
    var _doc = doc;
    var _me = this;

    // proprietà    
    this.isDomNode = true;

    // metodi

    this.node = function () { return _node; }

    this.parentNode = function () {
        if (_node == null) return null;

        var parent = _node.parentNode;
        if (parent == null)
            return null;

        return new domNode(parent, _doc);
    }

    this.getXml = function () {
        if (_node == null) return "";

        return _doc.domAPI.getXmlOfNode(_node);
    }

    this.nodeName = function() {
        if (_node == null) return "";

        return _node.nodeName;
    }

    this.nodeNameWithoutPrefix = function() {

        var result = _me.nodeName();
        if (_me.nodeName().indexOf(":") > 0)
            result = _me.nodeName().substring(_me.nodeName().indexOf(":") + 1, _me.nodeName().length);

        return result;
    }

    this.prefix = function() {
        if (_node == null) return "";

        if (_node.prefix != "")
            return _node.prefix + ":";

        return "";
    }

    this.namespaceURI = function() {
        if (_node == null) return "";

        return _node.namespaceURI;
    }

    this.getOwnerDoc = function () {
        return _doc;
    }

    this.text = function() {
        return _doc.domAPI.textNode(_node);
    }

    this.setText = function(text) {
        _doc.domAPI.setTextNode(_node, text);
    }

    this.addNode = function(nodeName, text) {
        var node = _doc.domAPI.addNode(_node, nodeName, text);
        return new domNode(node, _doc);
    }

    this.setAttributeText = function(attribute, text) {
        _doc.domAPI.setAttributeNode(_node, attribute, text);
    }

    this.getAttributeText = function(attribute) {
        return _doc.domAPI.getAttributeNode(_node, attribute);
    }

    this.selNode = function(querySel) {
        var node = _doc.domAPI.selNode(_node, querySel);
        if(node == null)
            return null;

        return new domNode(node, _doc);
    }

    this.existNode = function (querySel) {
        var node = _doc.domAPI.selNode(_node, querySel);
        if (node == null)
            return false;

        return true;
    }

    this.selNodes = function (querySel) {
        result = [];
        var array = _doc.domAPI.selNodes(_node, querySel);
        for (var i = 0; i < array.length; i++)
            result.push(new domNode(array[i], _doc));

        return result;
    }

    this.appendChild = function (node) {
        var newNode = _doc.domAPI.appendChild(_node, node.node());
        if (newNode == null)
            return null;

        return new domNode(newNode, _doc);
    }

    this.removeChild = function (node) {
        _doc.domAPI.removeChild(_node, node.node());
    }

    this.getKeyId = function () {
        return _me.getAttributeText("keyid");
    }

    this.findKeyId = function (id) {
        return _me.selNode("//*[@keyid='" + id + "']");
    }
}

