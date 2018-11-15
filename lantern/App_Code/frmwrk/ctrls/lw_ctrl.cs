using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Xml;
using deeper.db;

namespace deeper.frmwrk.ctrls
{
  public class lw_ctrl : page_ctrl
  {
    public lw_ctrl(page_cls page, XmlNode defNode, bool render = true) :
      base(page, defNode, render) { }

    public override Dictionary<string, string> key_fields() {
      return string.IsNullOrEmpty(rootAttr("key")) ? null : key_fields(rootAttr("key"));
    }

    protected string htmlSectionWithKeys(string name) {
      string keys;
      return applySectionKeys(name, htmlSection(name, out keys), keys);
    }

    protected string htmlSectionWithKeys(string name, out string ifCond) {
      ifCond = ""; string keys;
      return applySectionKeys(name, htmlSection(name, out ifCond, out keys), keys);
    }

    public override void add() {
      base.add();

      try {
        HtmlControl parentElement = parentOnAdd();
        if (parentElement == null)
          return;

        // html open list
        string secOpen = rootAttr("sec-open");
        if (secOpen != "") _cls.addHtmlSection(_cls.page.parse(htmlSectionWithKeys(secOpen)), parentElement);

        // rows
        {
          // sezioni html
          System.Collections.Hashtable htmlRows = new System.Collections.Hashtable();
          string htmlCloseRow = "", htmlSubRow = "", codeCondSubRow = "";
          {
            // sezioni riga
            foreach (string section in rootAttr("sec-rows").Split(',')) {
              string codeCond = "";
              string html = htmlSectionWithKeys(section, out codeCond);
              if (codeCond != "") codeCond = _cls.page.code_cond_id(_name, codeCond);
              htmlRows.Add(section, new string[] { html, codeCond });
            }

            string secCloseRow = rootAttr("sec-close-row");
            if (secCloseRow != "")
              htmlCloseRow = htmlSectionWithKeys(secCloseRow);

            string secSubRow = rootAttr("sec-sub-row");
            if (secSubRow != "") {
              string ifCond = "";
              htmlSubRow = htmlSectionWithKeys(secSubRow, out ifCond);
              if (ifCond != "")
                codeCondSubRow = _cls.page.code_cond_id(_name, ifCond);
            }
          }

          // rows
          if (rootAttr("script") != "") {
            if (rootAttr("break-key") != "")
              throw new Exception("attenzione l'opzione @breakKeysQuery nel caso di una selezione di nodi xml non è supportata!");

            // xml nodes
            int count = -1;
            object listNodes = listFromXmlScript(rootAttr("script"), out count);
            for (int i = 0; i < count; i++) {
              XmlNode row = indexNode(listNodes, i);

              // ciclo sezioni configurate
              foreach (System.Collections.DictionaryEntry secRow in htmlRows) {
                string codeCond = ((string[])secRow.Value)[1];
                if ((codeCond != "" && bool.Parse(_cls.page.parse(codeCond, _name, row))) || codeCond == "") {
                  _cls.addHtmlSection(_cls.page.parse(((string[])secRow.Value)[0], _name, row), parentElement);
                  break;
                }
              }
            }
          }
          else if (rootAttr("select") != "") {
            System.Data.DataTable dt = _cls.page.classPage.dt_from_id(rootAttr("select"), _name);

            // break key
            List<string> breakKeys = rootExistAttr("break-key") ?
              rootAttr("break-key").Split(',').Select(x => x.Trim()).ToList() : null;

            // righe
            if (dt != null) {
              int i = 0;
              Dictionary<string, string> key = null;
              foreach (System.Data.DataRow dr in dt.Rows) {
                // key
                bool breakRow = true;
                if (breakKeys != null) {
                  // init
                  if (key == null) {
                    key = new Dictionary<string, string>();
                    foreach (string keyField in breakKeys)
                      key.Add(keyField, dr[keyField] != null && dr[keyField] != DBNull.Value ? dr[keyField].ToString() : "");
                  }
                  else {
                    // rottura chiave?
                    bool breakTmp = false;
                    foreach (string keyField in breakKeys) {
                      string value = dr[keyField] != null && dr[keyField] != DBNull.Value ? dr[keyField].ToString() : "";
                      if (key[keyField] != value) {
                        breakTmp = true;
                        key[keyField] = value;
                      }
                    }

                    if (!breakTmp)
                      breakRow = false;
                  }
                }

                // html row
                if (breakRow) {
                  // html close row
                  if (i > 0 && htmlCloseRow != "")
                    _cls.addHtmlSection(_cls.page.parse(htmlCloseRow, _name, dr), parentElement);

                  // html begin row
                  foreach (System.Collections.DictionaryEntry secRow in htmlRows) {
                    string codeCond = ((string[])secRow.Value)[1];
                    if ((codeCond != "" && bool.Parse(_cls.page.parse(codeCond, _name, dr))) || codeCond == "") {
                      _cls.addHtmlSection(_cls.page.parse(((string[])secRow.Value)[0], _name, dr), parentElement);
                      break;
                    }
                  }
                }

                // html sub row
                if (htmlSubRow != "" && !(codeCondSubRow != "" && bool.Parse(_cls.page.parse(codeCondSubRow, _name, dr)) == false)) 
                  _cls.addHtmlSection(_cls.page.parse(htmlSubRow, _name, dr), parentElement);
 
                i++;
              }

              // close row
              if (i > 0 && htmlCloseRow != "") _cls.addHtmlSection(_cls.page.parse(htmlCloseRow, _name, dt.Rows[i - 1]), parentElement);
            }
          }
        }

        // html close list
        if (rootAttr("sec-close") != "") _cls.addHtmlSection(_cls.page.parse(htmlSectionWithKeys(rootAttr("sec-close"))), parentElement);

        // client script
        if (rootAttr("client-script") != "") _cls.regScript(_cls.clientScript(rootAttr("client-script")));
      }
      catch (Exception ex) { _cls.addError(ex); }
    }
  }
}