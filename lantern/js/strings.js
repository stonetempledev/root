function strings() {
    //    var privateVariable; // private member only available within the constructor 
    //    this.privilegedMethod = function () { // it can access private members 
    //        //.. 
    //    };
    //
}

// concatena due elementi del percorso
strings.concatenatePaths = function(path, path2)
{ 
    path = path.replace(/\\/g, "/");

    if(path2 == "")
        return path;

    if (path != "")
    {
        if(path.substring(path.length - 1, path.length) == "/")
            path = path + path2;
        else
            path = path + "/" + path2; 
    }    
    else
        path = path2;

    return path;
}

// Torna il percorso completo senza l'estensione
strings.removeExtensionFromPath = function(strPathFile) {
    var strResult = "";

    // tolgo l'estensione finale...
    if (strPathFile.substr(strPathFile.length - 4, 1) == ".")
        strResult = strPathFile.substr(0, strPathFile.length - 4);
    else if (strPathFile.substr(strPathFile.length - 5, 1) == ".")
        strResult = strPathFile.substr(0, strPathFile.length - 5);
    else
        strResult = strPathFile;

    return strResult;
}

strings.getExtensionFile = function (strPath) {
    var nIndex = strPath.lastIndexOf(".");

    if (nIndex < 0)
        return "";

    return strPath.substring(nIndex + 1, strPath.length).toLowerCase();
}

strings.getNameFile = function (strPath) {
    strPath = strPath.replace(/\\/g, "/");
    var nIndex = strPath.lastIndexOf("/");

    if (nIndex < 0)
        return strPath;

    return strPath.substring(nIndex + 1, strPath.length).toLowerCase();
}

strings.isAbsoluteURL = function (strURL) {
    var strTmp = strURL;

    strTmp = strTmp.substring(0, 7);
    strTmp = strTmp.toUpperCase();

    if (strTmp == "HTTP://")
        return true;

    return false;
}

strings.getURLFolderFromURL = function (strURL) {
    var strURLFolder = "";

    strURL = strURL.replace(/\\/g, "/");

    // mi becco l'ultima barra...
    var nIndex = strURL.lastIndexOf("/");
    if (nIndex >= 0) {
        strURLFolder = strURL.substring(0, nIndex);
    }
    else {
        strURLFolder = strURL;
    }

    return strURLFolder;
}

strings.getNameFromURL = function (strURL) {
    var strName = "";

    strURL = strURL.replace(/\\/g, "/");

    // mi becco l'ultima barra...
    var nIndex = strURL.lastIndexOf("/");
    if (nIndex >= 0) {
        strName = strURL.substring(nIndex + 1, strURL.length);
    }
    else {
        strName = strURL;
    }

    return strName;
}

// Ottenere il path relativo al folder del Garden Site
strings.getRelativePath = function (strPath, strBasePath) {
    var strRelativePath = strPath.substr(strBasePath.length, strPath.length - strBasePath.length);

    // levo l'eventuale '/' iniziale che fa casino...
    if (strRelativePath.substr(0, 1) == "/" ||
		strRelativePath.substr(0, 1) == "\\")
        strRelativePath = strRelativePath.substr(1, strRelativePath.length - 1);

    return strRelativePath;
}

strings.trim = function (s) {
    return strings.rtrim(strings.ltrim(s));
}

strings.ltrim = function (strOriginal) {
    var strTmp = strOriginal;
    while (strTmp.charAt(0) == " " ||
		strTmp.charAt(0) == "\t") {
        strTmp = strTmp.substring(1, strTmp.length);
    }

    return strTmp;
}

strings.rtrim = function (strOriginal) {
    var strTmp = strOriginal;
    while (strTmp.charAt(strTmp.length - 1) == " " ||
		strTmp.charAt(strTmp.length - 1) == "\t" ||
		strTmp.charAt(strTmp.length - 1) == "\r" ||
		strTmp.charAt(strTmp.length - 1) == "\n") {
        strTmp = strTmp.substring(0, strTmp.length - 1);
    }

    return strTmp;
}

