using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using deeper.db;

namespace deeper.frmwrk.ctrls
{
    public class section_ctrl : page_ctrl
    {
        public section_ctrl(page_cls page, XmlNode defNode, bool render = true) :
            base(page, defNode, render)
        { }

        public override void add()
        {
            base.add();

            try
            {
                HtmlControl parentElement = parentOnAdd();
                if (parentElement == null)
                    return;

                // div
                HtmlGenericControl section = new HtmlGenericControl("div");

                section.InnerHtml = _defNode.InnerText;

                parentElement.Controls.Add(section);
            }
            catch (Exception ex) { _cls.addError(ex); }
        }
    }
}
