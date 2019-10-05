using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
using mlib;

namespace molinafy {
  public class wb_doc : wb_base {

    public enum tp_ele { title_doc, des_doc };

    public event EventHandler SavedElement;

    protected bool _loading = false;
    protected doc _doc = null;
    public doc doc { get { return _doc; } }

    public class wb_element {
      HtmlAgilityPack.HtmlNode _node = null;

      public tp_ele tp { get { return (tp_ele)Enum.Parse(typeof(tp_ele), _node.Attributes["tp_ele"].Value); } }
      public HtmlAgilityPack.HtmlNode node { get { return _node; } }
      
      public wb_element (string html) {
        HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
        hd.LoadHtml(html);
        _node = hd.DocumentNode.FirstChild;
      }

    }

    public void changed_element (string html_ele) {

      wb_element we = new wb_element(html_ele);

      // aggiorno il doc
      if (we.tp == tp_ele.title_doc) _doc.title = we.node.InnerText;
      else if (we.tp == tp_ele.des_doc) _doc.des = we.node.InnerHtml;

      // aggiorno il db
      save_doc(we.tp);
    }

    public void add_paragraph (string html_ele) {

      wb_element we = new wb_element(html_ele);

      if (we.tp == tp_ele.des_doc || we.tp == tp_ele.title_doc) { 
      }
    }

    public void check_changed_doc () {
      string html_ele = (string)this.Document.InvokeScript("check_changed_focused");
      if (!string.IsNullOrEmpty(html_ele)) changed_element(html_ele);
    }

    public wb_doc (int id_doc) {
      this.ObjectForScripting = new script_manager(this);
      load_doc(id_doc);
    }

    #region html doc

    protected void load_doc (int id_doc) {
      app._main.add_info("opening document " + id_doc.ToString() + "...");
      _loading = true;

      // load doc
      _doc = new doc(id_doc);

      // build html
      StringBuilder html = new StringBuilder();
      add_element(html, tp_ele.title_doc, _doc.title);
      add_element(html, tp_ele.des_doc, _doc.des);

      load_html(app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-doc").value)
        , new Dictionary<string, object>() { { "html", html.ToString() } }));
    }

    protected void add_element (StringBuilder html, tp_ele tp, string txt) {
      if (tp == tp_ele.title_doc) html.Append(html_element("h3", tp.ToString(), txt, tp.ToString(), 100, true));
      else if (tp == tp_ele.des_doc) html.Append(html_element("p", tp.ToString(), txt, tp.ToString(), 250));
    }

    protected string html_element (string html_ele, string tp_ele, string txt, string class_name, int maxlength = -1, bool not_empty = false) {
      txt = txt.Replace("\r\n", "</br>").Replace("\n", "</br>").Replace("\r", "</br>");
      return string.Format(@"<{0} tp_ele='{1}' class='{3}' contenteditable='true' selonfocus='true' {5}
         onkeydown='return ele_keydown(this, event)' {4}>{2}</{0}>"
        , html_ele, tp_ele, txt, class_name, maxlength > 0 ? "maxlength='" + maxlength.ToString() + "'" : "", not_empty ? "not_empty='true'" : "");
    }

    public void save_doc (tp_ele tp) {
      if (tp == tp_ele.title_doc || tp == tp_ele.des_doc)
        _doc.upd_testata(app._user.id_user);

      EventHandler handler = SavedElement;
      if (null != handler) handler(this, EventArgs.Empty);
    }

    #endregion

    #region eventi wb

    protected override void OnDocumentCompleted (WebBrowserDocumentCompletedEventArgs e) {
      base.OnDocumentCompleted(e);
      if (_loading) {
        _loading = false;
        app._main.add_info("...document " + _doc.id.ToString() + " opened");
      }
    }

    #endregion
  }

  [ComVisible(true)]
  public class script_manager {
    protected wb_doc _doc;
    public wb_doc doc { get { return _doc; } set { _doc = value; } }
    public script_manager (wb_doc doc) { _doc = doc; }

    public void changed_doc (string html_ele) { _doc.changed_element(html_ele); }
    public void add_paragraph (string html_ele) { _doc.add_paragraph(html_ele); }
  }
}
