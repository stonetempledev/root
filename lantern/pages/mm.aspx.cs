using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Web.UI.HtmlControls;

public partial class _mm : deeper.frmwrk.lib_page
{
  protected void Page_Load(object sender, EventArgs e) {
    XmlDocument doc = new XmlDocument();
    doc.Load(path_doc());
    classPage.xml_topage("xml_doc", doc.OuterXml);
    Title = doc.DocumentElement.Attributes["title"].Value;
  }

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // httprequest
    if (Request.Headers["panel-client-request"] != null) {
      XmlDocument out_xml = new XmlDocument();
      out_xml.LoadXml("<response result='ok'/>");
      try {
        XmlDocument input = new XmlDocument();
        input.Load(Request.InputStream);

        string action = input.DocumentElement.Attributes["action"].Value;
        if (action == "save") {
          // ripulisco il documento
          string name_file = input.DocumentElement.Attributes["name_file"].Value;
          input.DocumentElement.Attributes.Remove(input.DocumentElement.Attributes["name_file"]);
          input.DocumentElement.Attributes.Remove(input.DocumentElement.Attributes["action"]);
          input.SelectNodes("/mm/struct//node").Cast<XmlNode>().ToList()
              .ForEach(nd => { nd.Attributes.Remove(nd.Attributes["vis_id"]); });

          // salvataggio
          input.Save(path_doc());
        }
      }
      catch (Exception ex) {
        out_xml.DocumentElement.Attributes["result"].Value = "error";
        out_xml.DocumentElement.AppendChild(out_xml.CreateElement("err")).InnerText = ex.Message;
      }

      out_xml.Save(Response.OutputStream);
      Response.End();
      return;
    }
  }

  /// <summary>
  /// Dato il nome del documento, si ottiene il percorso completo da usare per l'apertura 
  /// </summary>
  protected string path_doc() {
    System.Data.DataTable dt = classPage.dt_from_id("path_file");
    return System.IO.Path.Combine(dt.Rows[0]["rel_path"].ToString().Replace("/", "\\"), dt.Rows[0]["file_name"].ToString());
  }

  ///// <summary>
  ///// Aggiunta di un documento xml al body
  ///// </summary>
  //public void xml_to_main(string id, string xml) {
  //  HtmlTextArea xmlArea = new HtmlTextArea();
  //  xmlArea.ID = id;
  //  xmlArea.Value = xml;
  //  xmlArea.Style.Add("visibility", "hidden");
  //  xmlArea.Style.Add("display", "none");
  //  main_form.Controls.AddAt(0, xmlArea);
  //}
}

