using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace mlib {
  public class core {
    protected System.Xml.XPath.XPathNavigator _eval = new System.Xml.XPath.XPathDocument(new System.IO.StringReader("<r/>")).CreateNavigator();
    Dictionary<string, string> _cfg_keys = new Dictionary<string, string>();

    protected string _base_path = "";
    public string base_path { get { return _base_path; } }

    protected config _config = null;
    public config config { get { return _config; } }

    public core (string base_path) { _base_path = base_path; _config = new config(this); }

    #region configs

    public void reset_configs () { _cfg_keys.Clear(); _config.reset(); }

    public void load_config (xml_doc doc, string doc_key, string vars_key = "", bool page = false) {
      try {
        if (!_cfg_keys.Keys.Contains(doc_key)) _cfg_keys.Add(doc_key, "doc_path");
        _config.load_doc(doc_key, vars_key, doc, page);
      } catch (Exception ex) { _cfg_keys.Clear(); throw ex; }
    }

    public void load_page_config (xml_doc doc, string doc_key) { load_config(doc, doc_key, page: true); }

    public void reset_page_config () { _config.remove_for_page(); }

    public List<string> config_keys { get { return _cfg_keys.Keys.ToList(); } }

    public string app_setting (string name, bool throw_err = true) {
      if (throw_err && System.Configuration.ConfigurationManager.AppSettings[name] == null)
        throw new Exception("non c'è la variabile '" + name + "' nel config!");
      return parse(System.Configuration.ConfigurationManager.AppSettings[name] != null ?
        System.Configuration.ConfigurationManager.AppSettings[name].ToString() : "");
    }

    #endregion

    #region parse

    public string parse (string text, Dictionary<string, object> flds = null, DataRow dr = null) {

      try {

        int nstart = text != null ? text.IndexOf("{@") : -1; //string from = log.log_debugging && nstart >= 0 ? text : null;
        while (nstart >= 0) {
          int nend = text.IndexOf("}", nstart + 2);
          string cnt = text.Substring(nstart + 2, nend - nstart - 2);
          int nuguale = cnt.IndexOf("='");

          // chiavi secche
          if (nuguale < 0) {
            switch (cnt) {
              // {@basepath}
              case "basepath": text = text.Replace("{@basepath}", _base_path); break;
              // {@local-ip}
              case "local-ip": text = text.Replace("{@local-ip}", sys.local_ip()); break;
              // {@baseurl}
              case "baseurl": text = text.Replace("{@baseurl}", _base_path.Replace("\\", "/")); break;
              // {@machine-ip}
              case "machine-ip": text = text.Replace("{@machine-ip}", machine_ip()); break;
              // {@machine-name}
              case "machine-name": text = text.Replace("{@machine-name}", machine_name()); break;
              default: throw new Exception("chiave '" + cnt + "' inaspettata");
            }
          }
            // chiavi con singolo parametro
          else {
            string cmd = cnt.Substring(0, nuguale), par = cnt.Substring(nuguale + 2, cnt.Length - nuguale - 3), value = "";
            switch (cmd) {
              // {@field='<FIELD NAME>'}
              case "field": {
                value = get_val(par, flds, dr);
                break;
              }
              // {@null='<FIELD NAME>'}
              case "null": {
                value = get_val(par, flds, dr, "null");
                break;
              }
              // {@txtqry='<FIELD NAME>'}
              case "txtqry": {
                string val = get_val(par, flds, dr).ToString().Replace("'", "''").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                value = val != "" ? string.Format("'{0}'", val) : "NULL";
                break;
              }
              // {@txtqrycr='<FIELD NAME>'}
              case "txtqrycr": {
                string val = get_val(par, flds, dr).ToString().Replace("'", "''").Replace("\r\n", "' + char(13) + char(10) + '").Replace("\r", " ").Replace("\n", " ");
                value = val != "" ? string.Format("'{0}'", val) : "NULL";
                break;
              }
              // {@var='<name key>'}
              case "var": value = config.get_var(par).value; break;
              // {@setting='<name key>'}
              case "setting": value = app_setting(par); break;
              // {@date='<format string>'}
              case "date": value = DateTime.Now.ToString(par); break;
              default: throw new Exception("chiave con parametro '" + cmd + "' inaspettata!");
            }

            text = text.Replace("{@" + cmd + "='" + par + "'}", value);
          }
          nstart = text.IndexOf("{@");
        }

        //if (from != null) log.log_debug(string.Format("parsed '{0}' -> '{1}'", from, text));

        return text;
      } catch (Exception ex) { log.log_err(ex); throw ex; }
    }

    protected static string get_val (string par, Dictionary<string, object> flds = null, DataRow dr = null, string def = "") {
      if (flds == null && dr == null) throw new Exception("il campo '" + par + "' specificato nella query non è stato impostato!");
      if (flds != null && flds.ContainsKey(par)) return flds[par] != null ? flds[par].ToString() : def;
      else if (dr != null && dr.Table.Columns.Contains(par)) return dr[par] != DBNull.Value ? dr[par].ToString() : def;
      throw new Exception("il campo '" + par + "' specificato nella query non è stato impostato!");
    }

    public static string machine_name (bool lower = true) {
      try {
        return lower ? System.Environment.MachineName.ToLower() : System.Environment.MachineName;
      } catch { return ""; }
    }

    public static string machine_ip (string def = "") {
      try {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList) {
          if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            return ip.ToString();
        }
        return def;
      } catch { return def; }
    }

    public double eval_double (string expr) { try { return (double)_eval.Evaluate(string.Format("number({0})", expr)); } catch (Exception ex) { log.log_err(expr); throw ex; } }

    public bool eval_bool (string expr) { try { return (bool)_eval.Evaluate(string.Format("boolean({0})", expr)); } catch (Exception ex) { log.log_err(expr); throw ex; } }

    #endregion

    #region common

    public static string[] split (string lst) { return lst.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
    public static int[] split_ints (string lst) {
      return lst.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();
    }

    #endregion
  }
}
