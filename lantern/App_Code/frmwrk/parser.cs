using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using deeper.frmwrk;
using deeper.lib;
using deeper.db;

namespace deeper.frmwrk
{
  public class parser
  {
    lib_page _page = null;
    System.Xml.XPath.XPathNavigator _eval = null;

    public parser(lib_page page) {
      _page = page;
      _eval = new System.Xml.XPath.XPathDocument(new System.IO.StringReader("<r/>")).CreateNavigator();
    }

    public string get_string(string text, string cname = "", Dictionary<string, string> fields = null, System.Xml.XmlNode row = null
      , System.Data.DataRow dr = null, System.Web.UI.WebControls.GridViewRow container = null, db_schema db = null) {
      return parse_text(text, cname, fields, row, dr, container, false, db);
    }

    public object invoke(string text, string cname = "", Dictionary<string, string> fields = null
          , System.Xml.XmlNode row = null, System.Data.DataRow dr = null, System.Web.UI.WebControls.GridViewRow container = null) {
      return invoke_method(parse_text(text, cname, fields, row, dr, container, true));
    }

    protected object invoke_method(string text, db_schema db = null) {

      try {
      // {@method='<method name>',<type argument>:<value argument>}
      if (text.IndexOf("{@method") >= 0) {
        int startPar = text.IndexOf("{@method='") + 10;
        int endPar = text.IndexOf("}", startPar);
        if (_page == null)
          throw new Exception("non è possibile parsificare la parola chiave " + text.Substring(startPar, endPar - startPar));
        string pars = text.Substring(startPar - 1, (endPar - startPar) + 1);

        System.Reflection.MethodInfo mi = null;
        List<object> lstpar = new List<object>();
        foreach (string item in pars.Split(',')) {
          if (mi == null) {
            mi = _page.funcs.GetType().GetMethod(item.Substring(1, item.Length - 2));
            if (mi == null)
              throw new Exception("il metodo '" + item.Substring(1, item.Length - 2) + "' non è stato trovato!");
            continue;
          }

          string type = item.IndexOf(':') > 0 ? item.Substring(0, item.IndexOf(':')) : "string";
          string value = item.IndexOf(':') > 0 ? item.Substring(item.IndexOf(':') + 1, item.Length - item.IndexOf(':') - 1) : item;
          lstpar.Add(type == "string" ? (object)value : type == "int" ? (object)int.Parse(value) : null);
        }

          _page.funcs.set_active_db(db);

        return lstpar.Count == 0 ? mi.Invoke(_page.funcs, null) : mi.Invoke(_page.funcs, lstpar.ToArray());
      }

      return null;
      } finally { if (_page != null) _page.funcs.set_active_db(null); }
    }

    public double eval_double(string expr) {
      try {
      return (double)_eval.Evaluate(string.Format("number({0})", expr));
      } catch (Exception ex) { _page.logErr(expr); throw ex; }
    }

    public bool eval_bool(string expr) {
      try {
      return (bool)_eval.Evaluate(string.Format("boolean({0})", expr));
      } catch (Exception ex) { _page.logErr(expr); throw ex; }
    }

    protected string parse_text(string text, string cname = "", Dictionary<string, string> fields = null
        , System.Xml.XmlNode row = null, System.Data.DataRow dr = null, System.Web.UI.WebControls.GridViewRow container = null
      , bool exclude_method = false, db_schema db = null) {

      try {

        string result = text;
        if (text == "" || text == null) return result;

        // {@cond='<SCRIPT NAME>'}
        while (result.IndexOf("{@cond") >= 0) {
          int startPar = result.IndexOf("{@cond='") + 8;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          result = result.Replace("{@cond='" + par + "'}", _page.code_cond_id(cname, par));
        }

        // {@text_qry='<ID UNIVOCO QUERY>'}
        while (result.IndexOf("{@text_qry") >= 0) {
          int startPar = result.IndexOf("{@text_qry='") + 12;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          result = result.Replace("{@text_qry='" + par + "'}", _page.classPage.qry_text(par, cname));
        }

        // {@res_qry='<ID UNIVOCO QUERY>'}
        while (result.IndexOf("{@res_qry") >= 0) {
          int startPar = result.IndexOf("{@res_qry='") + 11;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          System.Data.DataTable dt = _page.classPage.dt_from_id(par, cname, "", fields, dr, true);
          result = result.Replace("{@res_qry='" + par + "'}", dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value ? dt.Rows[0][0].ToString() : "");
        }

        // {@siteurl}
        while (result.IndexOf("{@siteurl}") >= 0) {
          if (_page == null)
            throw new Exception("non è specificata la pagina web per la parsificazione dell'espressione '" + text + "'");

          result = result.Replace("{@siteurl}", _page.getRootUrl());
        }

        // {@approot}
        while (result.IndexOf("{@approot}") >= 0)
          result = result.Replace("{@approot}", _page.approot);

        // {@ctrlname}
        while (result.IndexOf("{@ctrlname}") >= 0) {
          if (cname == "")
            throw new Exception("non è stato specificato il nome del controllo per la parsificazione dell'espressione '" + text + "'");

          result = result.Replace("{@ctrlname}", cname);
        }

        // {@usr_type}
        while (result.IndexOf("{@usr_type}") >= 0) { result = result.Replace("{@usr_type}", _page.userTypeLogged); }

        // {@usr_childs}
        while (result.IndexOf("{@usr_childs}") >= 0) { result = result.Replace("{@usr_childs}", _page.userChilds.ToString()); }

        // {@pagename}
        while (result.IndexOf("{@pagename}") >= 0) {
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave {@activepage}");

          result = result.Replace("{@pagename}", _page.pageName);
        }

        // {@activepage}
        while (result.IndexOf("{@activepage}") >= 0) {
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave {@activepage}");

          result = result.Replace("{@activepage}", _page.getCurrentUrl());
        }

        // {@currurl}
        while (result.IndexOf("{@currurl}") >= 0) {
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave {@currurl}");

          result = result.Replace("{@currurl}", _page.getCurrentUrl(true));
        }

        // {@currurlargs='<LIST ARGUMENTS>'}
        while (result.IndexOf("{@currurlargs") >= 0) {
          int startPar = result.IndexOf("{@currurlargs='") + 15;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave {@currurlargs='...'}");

          string par = result.Substring(startPar, endPar - startPar);

          result = result.Replace("{@currurlargs='" + par + "'}", strings.combineurl(_page.getCurrentUrl(true), par));
        }

        // {@qrypar_def='<QUERY STRING PARAMETER>'}
        while (result.IndexOf("{@qrypar_def") >= 0) {
          int startPar = result.IndexOf("{@qrypar_def='") + 14;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          string value = _page.query_param(par);
          result = result.Replace("{@qrypar_def='" + par + "'}", !string.IsNullOrEmpty(value) ? value : "''");
        }

        // {@qrypar='<QUERY STRING PARAMETER>'}
        while (result.IndexOf("{@qrypar") >= 0) {
          int startPar = result.IndexOf("{@qrypar='") + 10;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          string value = _page.query_param(par);
          result = result.Replace("{@qrypar='" + par + "'}", value);
        }

        // {@valpar='<QUERY STRING PARAMETER>'}
        while (result.IndexOf("{@valpar") >= 0) {
          int startPar = result.IndexOf("{@valpar='") + 10;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          string value = _page.query_param(par);
          result = result.Replace("{@valpar='" + par + "'}", string.IsNullOrEmpty(value) ? "0" : "1");
        }

        // {@qrykey='<QUERY STRING PARAMETER>'}
        while (result.IndexOf("{@qrykey") >= 0) {
          int startPar = result.IndexOf("{@qrykey='") + 10;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          result = result.Replace("{@qrykey='" + par + "'}", _page.keystr(par) != "" ? _page.key(par).ToString() : "0");          
        }

        // {@qryfields='<TABLE NAME>'}
        while (result.IndexOf("{@qryfields") >= 0) {
          int startPar = result.IndexOf("{@qryfields='") + 13;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null || cname == "")
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);

          deeper.db.db_schema dbl = _page.conn_db_user();
          if (!db.exist_schema)
            throw new Exception("il database '" + dbl.name + "' non ha associato nessuno schema xml!");

          string value = "";
          foreach (string col_name in dbl.schema.fields_name(par))
            value += (value != "" ? ", " : "") + "[" + col_name + "]";

          if (value == "")
            throw new Exception("il parametro '" + par + "' della stringa url dev'essere valorizzato!");
          result = result.Replace("{@qryfields='" + par + "'}", value);
        }
        // {@var='<NAME VAR>'}
        while (result.IndexOf("{@var") >= 0) {
          int startPar = result.IndexOf("{@var='") + 7;
          int endPar = result.IndexOf("'}", startPar);

          string par = result.Substring(startPar, endPar - startPar);
          result = result.Replace("{@var='" + par + "'}", _page.cfg_var(par));
        }

        // {@attr='<ATTRIBUTE NAME XML NODE>'}
        while (result.IndexOf("{@attr") >= 0) {
          int startPar = result.IndexOf("{@attr='") + 8;
          int endPar = result.IndexOf("'}", startPar);
          string attr = result.Substring(startPar, endPar - startPar);

          if (row == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string value = "";
          if (row.Attributes[attr] != null)
            value = row.Attributes[attr].Value;

          result = result.Replace("{@attr='" + attr + "'}", value);
        }

        // {@node='<NODE NAME XML NODE>'}
        while (result.IndexOf("{@node") >= 0) {
          int startPar = result.IndexOf("{@node='") + 8;
          int endPar = result.IndexOf("'}", startPar);
          if (row == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));
          string node = result.Substring(startPar, endPar - startPar);

          string value = "";
          if (row.SelectSingleNode(node) != null)
            value = row.SelectSingleNode(node).InnerText;

          result = result.Replace("{@node='" + node + "'}", value);
        }

        // {@pagerefargs='<PAGE NAME>','<LIST ARGUMENTS>'}
        while (result.IndexOf("{@pagerefargs") >= 0) {
          int startPar = result.IndexOf("{@pagerefargs=") + 14;
          int endPar = result.IndexOf("}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string contents = result.Substring(startPar, endPar - startPar);

          string value = "";
          {
            bool first = true;
            string[] pars = contents.Split(',');
            foreach (string par in pars) {
              string arg = par.Substring(1, par.Length - 2);
              if (first) {
                value = _page.getPageRef(arg);
                first = false;
              } else
                value = strings.combineurl(value, arg);
            }
          }

          result = result.Replace("{@pagerefargs=" + contents + "}", value);
        }

        // {@pageref='<PAGE NAME>'}
        while (result.IndexOf("{@pageref") >= 0) {
          int startPar = result.IndexOf("{@pageref='") + 11;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          string value = _page.getPageRef(par);

          result = result.Replace("{@pageref='" + par + "'}", value);
        }

        // {@qrypar='<QUERY STRING PARAMETER>'}
        while (result.IndexOf("{@qrypar") >= 0) {
          int startPar = result.IndexOf("{@qrypar='") + 10;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);
          string value = _page.query_param(par);
          result = result.Replace("{@qrypar='" + par + "'}", value);
        }

        // {@siteurl='<SITE NAME>'}
        while (result.IndexOf("{@siteurl='") >= 0) {
          int startPar = result.IndexOf("{@siteurl='") + 11;
          int endPar = result.IndexOf("'}", startPar);

          string par = result.Substring(startPar, endPar - startPar);
          result = result.Replace("{@siteurl='" + par + "'}"
            , _page.cfg_value("/root/websites/website[@name='" + par + "']", "url"));
        }

        // {@imageurl='<IMAGE NAME>'}
        while (result.IndexOf("{@imageurl='") >= 0) {
          int startPar = result.IndexOf("{@imageurl='") + 12;
          int endPar = result.IndexOf("'}", startPar);

          string par = result.Substring(startPar, endPar - startPar);
          string value = get_string(_page.cfg_value("/root/images/image[@name='" + par + "']", "url"), cname, fields, row, dr);
          result = result.Replace("{@imageurl='" + par + "'}", value);
        }

        // {@pagetitle=''}
        while (result.IndexOf("{@pagetitle='") >= 0) {
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave {@pagetitle=''}");

          int startPar = result.IndexOf("{@pagetitle='") + 13, endPar = result.IndexOf("'}", startPar);
          string par = result.Substring(startPar, endPar - startPar);

          result = result.Replace("{@pagetitle='" + par + "'}"
              , _page.classPage.titlePage(_page.cfg_node("/root/pages/page[@name='" + par + "']"), fields, row, dr));
        }

        // {@desPage}
        while (result.IndexOf("{@pagedes='") >= 0) {
          if (_page == null) throw new Exception("non è possibile parsificare la parola chiave {@pagedes=''}");

          int startPar = result.IndexOf("{@pagedes='") + 11, endPar = result.IndexOf("'}", startPar);
          string par = result.Substring(startPar, endPar - startPar)
            , des = _page.classPage.desPage(_page.cfg_node("/root/pages/page[@name='" + par + "']"), fields, row, dr);

          result = result.Replace("{@pagedes='" + par + "'}", des != "" ? des : "&nbsp;");
        }

        // {@field='<FIELD NAME>'}, {@field='<FORM NAME.FIELD NAME>'}
        while (result.IndexOf("{@field") >= 0) {
          int startPar = result.IndexOf("{@field='") + 9;
          int endPar = result.IndexOf("'}", startPar);
          string src = result.Substring(startPar, endPar - startPar);
          string field = src.IndexOf('.') >= 0 ? src.Substring(src.IndexOf('.') + 1) : src;
          string formname = src.IndexOf('.') >= 0 ? src.Substring(0, src.IndexOf('.')) : "";

          string value = null;
          if (container != null) value = System.Web.UI.DataBinder.Eval(container.DataItem, field).ToString();
          if (value == null && (_page != null && (formname != "" || cname != ""))
            && _page.classPage.existControl(formname != "" ? formname : cname) && !_page.classPage.control(formname != "" ? formname : cname).fieldValue(field, out value)) value = null;
          if (value == null && (fields != null && fields.ContainsKey(field))) value = fields[field];
          if (value == null && (dr != null && dr[field] != null)) value = dr[field].ToString();

          if (value != null) result = result.Replace("{@field='" + src + "'}", value);
          else throw new Exception("non è stato possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));
        }

        // {@datefld='<FIELD NAME>','<FORMAT>'}
        while (result.IndexOf("{@datefld") >= 0) {
          int startPar = result.IndexOf("{@datefld=") + 10;
          int endPar = result.IndexOf("}", startPar);
          string field = result.Substring(startPar, endPar - startPar).Split(',')[0];
          field = field.Substring(1, field.Length - 2);
          string format = result.Substring(startPar, endPar - startPar).Split(',')[1];
          format = format.Substring(1, format.Length - 2);
          _page.classPage.formatDates(format, out format);

          // valore della form o dei campi passati da codice
          if (_page != null && fields == null) {
            if (cname == "")
              throw new Exception("l'espressione '" + text + "' non può essere valutata senza specificare il controllo d'appartenenza.");

            string value = null;
            if (container != null) value = System.Web.UI.DataBinder.Eval(container.DataItem, field).ToString();
            if (value == null && (_page != null && cname != "")
              && _page.classPage.existControl(cname) && !_page.classPage.control(cname).fieldValue(field, out value)) value = null;
            if (value == null && (dr != null && dr[field] != null)) value = dr[field].ToString();
            if (value == null && (fields != null && fields.ContainsKey(field))) value = fields[field];

            if (!string.IsNullOrEmpty(value)) value = DateTime.Parse(value).ToString(format);

            result = result.Replace("{@datefld=" + result.Substring(startPar, endPar - startPar) + "}", value);
          } else if (fields != null) {
            if (!fields.ContainsKey(field))
              throw new Exception("non è stato specificato il campo '" + field + "' richiesto dalla espressione '" + text + "'");

            string value = fields[field];

            if (value != "")
              value = DateTime.Parse(value).ToString(format);

            result = result.Replace("{@datefld=" + result.Substring(startPar, endPar - startPar) + "}", value);
          } else
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));
        }

        // {@qry????='<FIELD NAME>'}
        foreach (string type in (new string[10] { "Text", "EqText", "Void", "Numb", "Euro", "Migl", "Real", "Flag", "Date", "EqDate" })) {
          while (result.IndexOf("{@qry" + type) >= 0) {
            int startPar = result.IndexOf("{@qry" + type + "='") + (7 + type.Length);
            string flds = result.Substring(startPar, result.IndexOf("'}", startPar) - startPar);

            // valore dalla form o dei campi passati da codice
            string value = null;
            foreach (string f in flds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {

              if (_page != null && cname != "" 
                && _page.classPage.existControl(cname) && !_page.classPage.control(cname).fieldValue(f, out value, type == "Text" || type == "EqText" || type == "Void")) value = null;
              if (value == null && container != null) value = System.Web.UI.DataBinder.Eval(container.DataItem, f).ToString();
              if (value == null && (dr != null && dr.Table.Columns.Contains(f))) value = dr[f].ToString();
              if (value == null && (fields != null && fields.ContainsKey(f))) value = fields[f];

              if (value != null) break;
            }

            // parse valore da inserire nella query
            if (value == "" || value == null) value = type == "Void" ? "''" : (type == "EqDate" || type == "EqText" ? "is NULL" : "NULL");
            else {
              if (type == "Flag") value = value.ToLower() == "false" || value.ToLower() == "falso" || value.ToLower() == "0" ? "0" : "1";
              else if (type == "Text" || type == "EqText" || type == "Void") value = (type == "EqText" ? " = " : "") + "'" + value.Replace("'", "''") + "'";
              else if (type == "Date" || type == "EqDate") value = (type == "EqDate" ? " = " : "") + "'"
                + DateTime.Parse(value).ToString(_page != null && _page.conn_db_user() != null
                  ? _page.conn_db_user().dateFormatToQuery : "") + "'";
              else if (type == "Euro" || type == "Migl") value = page_ctrl.euroToDouble(value)
                  .ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
              else if (type == "Real") value = double.Parse(value)
                  .ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
            }

            result = result.Replace("{@qry" + type + "='" + flds + "'}", value);
          }
        }

        // function

        // {@property='<PROPERTY NAME PAGE>'}
        while (result.IndexOf("{@property") >= 0) {
          int startPar = result.IndexOf("{@property='") + 12;
          int endPar = result.IndexOf("'}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));

          string par = result.Substring(startPar, endPar - startPar);

          System.Reflection.PropertyInfo pi = _page.GetType().GetProperty(par);
          if (pi == null)
            throw new Exception("la proprietà '" + par + "' non esiste!");

          result = result.Replace("{@property='" + par + "'}", pi.GetValue(_page, null).ToString());
        }

        // {@method='<method name>',<type argument>:<value argument>}
        while (!exclude_method && result.IndexOf("{@method") >= 0) {
          int startPar = result.IndexOf("{@method='") + 10;
          int endPar = result.IndexOf("}", startPar);
          if (_page == null)
            throw new Exception("non è possibile parsificare la parola chiave " + result.Substring(startPar, endPar - startPar));
          string pars = result.Substring(startPar - 1, (endPar - startPar) + 1);

          result = result.Replace("{@method=" + pars + "}", invoke_method(result, db).ToString());
        }

        return result;
      } catch (Exception ex) { _page.logErr("parseExpression: '" + text + "'"); throw ex; }
    }
  }
}
