using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Web.UI.WebControls;
using deeper.frmwrk.ctrls;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.pages
{
  public class backup : deeper.frmwrk.page_cls
    {
        public backup(deeper.frmwrk.lib_page page, System.Xml.XmlNode pageNode)
            : base(page, pageNode)
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public override submitResponse submitFormAfterData(string name)
        {
            // aggiunta backup del database
            if (name == "form-backup" && _page.query_param("type") == "ins")
            {
                db_utilities.backupDb(this, ((form_ctrl)control("form-backup")).fieldValue("title"),
                    ((form_ctrl)control("form-backup")).fieldValue("des"), page.cfg_var("filesFolder"));

                return submitResponse.ok;
            }
            else if (name == "import-backup")
            {
                // upload pacchetto
                string pathtmp = System.IO.Path.Combine(_page.tmpFolder(), System.IO.Path.GetRandomFileName() + ".gz");
                form_control(name).upload_save("pckfile", pathtmp);

                // estrazione schema
                xmlDoc doc = new xmlDoc(extract_dbpck_index(pathtmp));

                // importazione del pacchetto
                string title = ((form_ctrl)control(name)).fieldValue("titolo");
                string notes = ((form_ctrl)control(name)).fieldValue("notes");
                db_utilities.backupPck(this, pathtmp, doc.root_value("name")
                    , title != "" ? title : doc.root_value("title", "importazione backup")
                    , DateTime.Parse(doc.root_value("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
                    , doc.root_value("ver"), notes != "" ? notes : doc.root_value("notes", "importazione backup da pacchetto esterno"));

                System.IO.File.Delete(pathtmp);

                return submitResponse.ok;
            }

            return submitResponse.notvalued;
        }
    }
}