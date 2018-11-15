using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace deeper.lib
{
    public class strings
    {
        static public bool isInt32(string s)
        {
            try
            {
                Int32.Parse(s);
            }
            catch { return false; }

            return true;
        }

        static public bool isNumeric(string s)
        {
            try
            {
                float.Parse(s);
            }
            catch { return false; }

            return true;
        }

        static public int parseInt(string s, int def = 0)
        {
            int result = def;

            try { result = Int32.Parse(s); }
            catch { result = def; }

            return result;
        }

        static public string fileName(string pathFile, bool withExt = true)
        {
            try
            {
                pathFile = pathFile.Replace("\\", "/");
                if (pathFile.LastIndexOf("/") > 0)
                    pathFile = pathFile.Substring(pathFile.LastIndexOf("/") + 1, pathFile.Length - pathFile.LastIndexOf("/") - 1);

                if (!withExt && pathFile.LastIndexOf(".") > 0)
                    pathFile = pathFile.Substring(0, pathFile.LastIndexOf("."));
            }
            catch (Exception e) { string msg = e.Message; pathFile = ""; }

            return pathFile;
        }

        static public string pathWithSlash(string path, bool urlSlash = true)
        {
            if (path == "" || path.Substring(path.Length - 1, 1) == "\\" ||
                path.Substring(path.Length - 1, 1) == "/")
                return path;

            if (!urlSlash)
                return path + "/";

            return path + "\\";
        }

        static public Boolean isAbsoluteUrl(string url)
        {
            string url2 = url.Replace("\\", "/");
            if ((url2.Length >= 2 && url2.Substring(1, 1).ToLower() == ":")
                || (url2.Length >= 2 && url2.Substring(0, 2).ToLower() == "//")
                || (url2.Length >= 7 && url2.Substring(0, 7).ToLower() == "http://")
                || (url2.Length >= 8 && url2.Substring(0, 8).ToLower() == "https://")
                || (url2.Length >= 7 && url2.Substring(0, 7).ToLower() == "file://")
                || (url2.Length >= 6 && url2.Substring(0, 6).ToLower() == "ftp://")
                || (url2.Length >= 7 && url2.Substring(0, 7).ToLower() == "sftp://"))
            {
                return true;
            }

            return false;
        }

        static public string combineurl(string url, string parsurl)
        { return parsurl != "" ? url + (url.IndexOf("?") > 0 ? "&" : "?") + parsurl : url; }

        static public string combineurl(string url, List<string> parsurl)
        {
            foreach (string pars in parsurl) 
                url = combineurl(url, pars);
            
            return url;
        }

        static public bool like(string str, string wildcard)
        {
            return new Regex("^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(str);
        }

        static public Regex reg_like(string wildcard)
        {
            return new Regex("^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        static public bool findString(List<string> values, string value)
        {
            foreach (string tmp in values)
            {
                if (tmp.ToUpper() == value.ToUpper())
                    return true;
            }

            return false;
        }

        static public bool containTag(string tags, string tag)
        {
            string[] list = tags.Split(',');
            foreach (string item in list)
                if (item.Trim().ToLower() == tag.ToLower())
                    return true;
            return false;
        }

        static public string rel_path(string base_path, string path_file) {
          if (!isAbsoluteUrl(base_path) || !isAbsoluteUrl(path_file)) throw new Exception("entrambi i percorsi devono essere assoluti!");

          return (path_file.Length >= path_file.Length && base_path.ToLower().Replace("/", "\\") == path_file.Substring(0, base_path.Length).ToLower().Replace("/", "\\") ?
            path_file.Substring(base_path.Length, path_file.Length - base_path.Length) : path_file);
        }
    }
}



