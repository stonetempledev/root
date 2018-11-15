using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;

/// <summary>
/// Summary description for login
/// </summary>
namespace deeper.pages
{
    public class variables : deeper.frmwrk.page_cls
    {
        public variables(deeper.frmwrk.lib_page page, System.Xml.XmlNode pageNode)
            : base(page, pageNode)
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public override void onInit(object sender, EventArgs e, bool request = false, bool addControls = true)
        {
            base.onInit(sender, e, request);


            if (addControls) {
              System.Text.StringBuilder str = new System.Text.StringBuilder();

              str.Append("<html><head><meta charset=\"ISO-8859-1\"></head><body style='overflow:auto;'><div style=\"font-family:'Segoe UI Light', 'Segoe UI', 'Lucida Grande', Verdana, Arial, Helvetica, sans-serif;\">");

              // browser capabilities
              System.Web.HttpBrowserCapabilities browser = _page.Request.Browser;
              str.Append("<h3>Browser Capabilities</h3>"
                + "<b>Type :</b>" + browser.Type + "<br>"
                + "<b>Name :</b>" + browser.Browser + "<br>"
                + "<b>Version :</b>" + browser.Version + "<br>"
                + "<b>Major Version :</b>" + browser.MajorVersion + "<br>"
                + "<b>Minor Version :</b>" + browser.MinorVersion + "<br>"
                + "<b>Platform :</b>" + browser.Platform + "<br>"
                + "<b>Is Beta :</b>" + browser.Beta + "<br>"
                + "<b>Is Crawler :</b>" + browser.Crawler + "<br>"
                + "<b>Is AOL :</b>" + browser.AOL + "<br>"
                + "<b>Is Win16 :</b>" + browser.Win16 + "<br>"
                + "<b>Is Win32 :</b>" + browser.Win32 + "<br>"
                + "<b>Supports Frames :</b>" + browser.Frames + "<br>"
                + "<b>Supports Tables :</b>" + browser.Tables + "<br>"
                + "<b>Supports Cookies :</b>" + browser.Cookies + "<br>"
                + "<b>Supports VBScript :</b>" + browser.VBScript + "<br>"
                + "<b>Supports JavaScript :</b>" + browser.EcmaScriptVersion.ToString() + "<br>"
                + "<b>Supports Java Applets :</b>" + browser.JavaApplets + "<br>"
                + "<b>Supports ActiveX Controls :</b>" + browser.ActiveXControls + "<br>"
                + "<b>Supports JavaScript Version :</b>" + browser["JavaScriptVersion"] + "<br>");

              // server variables
              str.Append("<h3>Server Variables</h3>");
              System.Collections.Specialized.NameValueCollection coll = _page.Request.ServerVariables;
              String[] arr1 = coll.AllKeys;
              for (int loop1 = 0; loop1 < arr1.Length; loop1++) {
                str.Append("<b>Key: " + arr1[loop1] + "</b>:");
                String[] arr2 = coll.GetValues(arr1[loop1]);
                for (int loop2 = 0; loop2 < arr2.Length; loop2++) 
                  str.Append((loop2 > 0 ? ", " : "") + _page.Server.HtmlEncode(arr2[loop2]));
                str.Append("<br>");
              }

              // db factories
              str.Append("<h3>Db Factory classes</h3>");
              foreach (DataRow dr in DbProviderFactories.GetFactoryClasses().Rows) 
                str.Append(" - " + string.Join(", ", dr.Table.Columns.Cast<DataColumn>().Select(
                  col => dr[col.ColumnName] != DBNull.Value ? col.ColumnName + ": " + dr[col.ColumnName].ToString() : "")) + "<br/>");
              str.Append("<br>");

              str.Append("</div></body></html>");
              _page.Response.Write(str.ToString());
              _page.Response.End();
            }
        }
    }
}