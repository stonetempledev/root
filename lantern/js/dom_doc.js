// modalita
var _use_actx = false;
try { var tmp = new ActiveXObject('Microsoft.XMLDOM'); _use_actx = true; } catch (e) { }
//function isdom() { return DOMParser == undefined; }
//function isie() { return navigator.appName == "Microsoft Internet Explorer" ? true : false; }


//////////////////////////////
// domDocument
//
function domDocument(xml) {

    // proprieta
    var _docObj = null;
    var _key = 1;
    var _urlDoc = "";
    var _domAPI = new xmlClass();
    var _me = this;
    var _keyDoc = "";

    // definizione proprietà
    this.domAPI = _domAPI;
    this.isDomDocument = true;

    this.loadXml = function(xmlText) {

        var result = true;

        _docObj = _domAPI.loadXml(xmlText);
        if (_docObj == null || _docObj == false)
            result = false;

        return result;
    }

    this.getKey = function() { return _keyDoc; }
    this.setKey = function(value) { _keyDoc = value; }

    this.setDocObj = function(doc) { _docObj = doc; }

    this.getUrlDoc = function() { return _urlDoc; }

    this.setUrlDoc = function(value) { _urlDoc = value; }

    this.getXml = function() {

        if (_docObj == null) return "";

        return _domAPI.getXmlOfDoc(_docObj);
    }

    this.rootNode = function() {
        if (_docObj == null) return null;

        return new domNode(_docObj.documentElement, _me);
    }

    this.getTextNode = function(querySel) {

        var node = _me.selNode(querySel);
        return node.text();
    }

    this.setTextNode = function(querySel, text) {

        var node = _me.selNode(querySel);
        node.setText(text);
    }

    this.addNode = function(nodeName, text) {

        var root = _me.rootNode();
        return root.addNode(nodeName, text);
    }

    this.selNode = function(querySel) {
        if (_docObj == null) return null;

        var node = _domAPI.selNode(_docObj.documentElement, querySel);
        if (node == null)
            return null;

        return new domNode(node, _me);
    }

    this.selNodes = function(querySel) {
        if (_docObj == null) return null;

        result = [];

        var array = _domAPI.selNodes(_docObj.documentElement, querySel);
        for (var i = 0; i < array.length; i++)
            result.push(new domNode(array[i], _me));

        return result;
    }

    this.http_request = function(page_url, asynch, fncStateChange) {

        var doc = _domAPI.http_request(_docObj, page_url, asynch, fncStateChange);
        if (doc == null)
            return null;

        var result = new domDocument();
        result.setDocObj(doc);

        return result;
    }

    this.saveDoc = function() {

        if (!page.request.saveDoc(_me, _urlDoc))
            return false;

        return true;
    }

    this.setKeys = function() {
        _me.setNodeKeys(_me.rootNode());
    }

    this.setNodeKeys = function(parent) {

        // aggiungo le chiavi di riconoscimento...
        var nodes = parent.selNodes("*");
        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i];
            if (node.getAttributeText("keyid") == "") {
                node.setAttributeText("keyid", _key.toString());
                if (_key < 0)
                    _key = 0;

                _key++;
            }

            _me.setNodeKeys(node);
        }
    }

    this.cleanKeys = function() {
        _cleanKeyElement(_me.rootNode());
    }

    this.cleanNodeKeys = function(parent) {
        _cleanKeyElement(parent);
    }

    function _cleanKeyElement(node) {

        node.setAttributeText("keyid");

        // tolgo le chiavi...	
        var nodes = node.selNodes("*");
        for (var i = 0; i < nodes.length; i++)
            _cleanKeyElement(nodes[i]);
    }

    this.findKeyId = function(id) {
        return _me.selNode("//*[@keyid='" + id + "']");
    }

    /////////////////////////////////////
    // xmlClass
    //
    // Classe che offre l'accesso ai nodi ed ai documenti xml
    function xmlClass() {

        var _xhttp = null;

        this.httpRequest = function() { return _xhttp; }

        this.loadXml = function(xmlText) {

            var result = null;
            try {

                if (_use_actx) { //IE
                    result = new ActiveXObject('Microsoft.XMLDOM');
                    var loadResult = result.loadXML(xmlText);
                    if (!loadResult)
                        throw new Error("It was not possible to load xml.");
                }
                else { // Firefox, Chrome, etc...
                    var parser = new DOMParser();

                    result = parser.parseFromString(xmlText, "text/xml");
                }
            } catch (e) { result = null; }

            return result;
        }

        this.getXmlOfDoc = function(doc) {

            var xmlText = "";
            if (_use_actx)  //IE
                xmlText = doc.xml;
            else {
                var serializer = new XMLSerializer();

                xmlText = serializer.serializeToString(doc);
            }

            return xmlText;
        }

        this.getXmlOfNode = function(node) {

            var xml = "";
            if (node.isDomNode != undefined)
                node = node.node();

            if (_use_actx)  //IE
                xml = node.xml;
            else {
                var serializer = new XMLSerializer();

                xml = serializer.serializeToString(node);
            }

            return xml;
        }

        this.http_request = function (doc, pageRequest, asynch, fncStateChange) {

            if (asynch == null) asynch = false;

            // http request
            if (_xhttp != null)
                _xhttp = null;

            if (_use_actx) // IE 
                _xhttp = new ActiveXObject("Microsoft.XMLHTTP");
            else if (window.XMLHttpRequest)  // firefox, chrome
                _xhttp = new XMLHttpRequest();

            // init http request
            _xhttp.open("POST", pageRequest, asynch);
            //_xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
            _xhttp.setRequestHeader("panel-client-request", "page.request");
            if (asynch) {
                if (fncStateChange != null)
                    _xhttp.onreadystatechange = fncStateChange;
                else
                    _xhttp.onreadystatechange = this.handleStateChange;
            }

            if (_use_actx) // IE 
                _xhttp.Send(doc);
            else
                _xhttp.send(doc);

            // get response
            if (asynch)
                return null;

            var response = this.loadXml(_xhttp.responseText);
            if (response == null)
                throw "non è stato possibile caricare la risposta dal documento '" + doc + "'";

            return response;
        }

        this.handleStateChange = function() {
            switch (_xhttp.readyState) {
                case 0: // UNINITIALIZED
                case 1: // LOADING
                case 2: // LOADED
                case 3: // INTERACTIVE
                    break;
                case 4: // COMPLETED                    
                    //handleResponse(xmlhttp.status, xmlhttp.responseText);
                    break;
                default: //alert("error");
            }
        }

        //this.loadUrl = function (urlDoc) {
        //    var result = null;
        //    try {
        //        // http request
        //        var xhttp = null;
        //        if (_use_actx) // IE 
        //            xhttp = new ActiveXObject("Microsoft.XMLHTTP");
        //        else if (window.XMLHttpRequest)  // firefox, chrome
        //            xhttp = new XMLHttpRequest();
        //        xhttp.open("GET", urlDoc, false);
        //        xhttp.send();
        //        if (xhttp.status == 404)
        //            result = null;
        //        else
        //            result = this.loadXml(xhttp.responseText);
        //    } catch (e) { result = null; }
        //    return result;
        //}

        this.ownerDocument = function(node) {

            //if (_use_actx)
            return node.ownerDocument;
            //else
            //    return node.document;
        }

        this.selNode = function(node, querySel) {

            if (node.isDomNode != undefined)
                node = node.node();

            var result = null;
            var doc = this.ownerDocument(node);
            if (_use_actx) {
                //if (typeof doc.setProperty != 'undefined')
                //  doc.setProperty('SelectionLanguage', 'XPath');

                result = node.selectSingleNode(querySel);

            } else if (document.implementation.hasFeature('XPath', '3.0')) {

                var resolver = doc.createNSResolver(doc.documentElement);
                var tmpRes = doc.evaluate(querySel, node, resolver, XPathResult.FIRST_ORDERED_NODE_TYPE, null);

                result = tmpRes.singleNodeValue;
            }

            return result;
        }

        this.selNodes = function(node, querySel) {

            var result = null;
            if (node.isDomNode != undefined)
                node = node.node();

            var doc = this.ownerDocument(node);
            if (_use_actx) {
                //if (typeof doc.setProperty != 'undefined')
                //      doc.setProperty('SelectionLanguage', 'XPath');

                var nodes = node.selectNodes(querySel);

                result = [];
                for (var tmp = null; tmp = nodes.nextNode(); )
                    result.push(tmp);

            } else if (document.implementation.hasFeature('XPath', '3.0')) {
                var resolver = doc.createNSResolver(node);
                var nodes = doc.evaluate(querySel, node, resolver, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
                result = [];
                var count = nodes.snapshotLength;
                for (var i = 0; i < count; i++) {
                    result.push(nodes.snapshotItem(i));
                }
            } else {
                result = [];
            }

            return result;
        }

        this.textNode = function(node) {

            if (_use_actx)
                return node.text;

            return node.textContent;
        }

        this.setTextNode = function(node, text) {
            if (text == null) text = "";

            if (_use_actx)
                node.text = text;
            else
                node.textContent = text;
        }

        this.addNode = function(node, nodeName, text) {

            var doc = this.ownerDocument(node);
            var newNode = node.appendChild(doc.createElement(nodeName));

            if (text != null)
                this.setTextNode(newNode, text);

            return newNode;
        }

        this.appendChild = function(parent, child) {

            return parent.appendChild(child.cloneNode(true));
        }

        this.removeChild = function(parent, child) {

            parent.removeChild(child);
        }

        this.setAttributeNode = function(node, attribute, text) {

            if (_use_actx) {
                var attrObj = node.attributes.getNamedItem(attribute);
                if (text != null) {
                    if (attrObj == null) {
                        attrObj = node.ownerDocument.createAttribute(attribute);
                        node.attributes.setNamedItem(attrObj);
                    }
                    attrObj.text = text;
                }
                else {
                    if (attrObj != null)
                        node.attributes.removeNamedItem(attribute);
                }
            }
            else {
                if (text != null)
                    node.setAttribute(attribute, text);
                else
                    node.removeAttribute(attribute);
            }
        }

        this.getAttributeNode = function(node, attribute) {

            if (_use_actx) {

                var attrObj = node.attributes.getNamedItem(attribute);
                if (attrObj != null)
                    return attrObj.text;
            }
            else {
                var result = node.getAttribute(attribute);
                if (result != null)
                    return result;
            }

            return "";
        }
    }

    if (xml != null) _me.loadXml(xml);
}

//domDocument.loadUrl = function (urlDoc) {
//    var doc = new domDocument();
//    if (!doc.loadUrlDoc(urlDoc))
//        return null;
//    return doc;
//}