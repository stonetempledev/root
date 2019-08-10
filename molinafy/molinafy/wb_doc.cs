using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HtmlAgilityPack;

namespace molinafy {
  public class wb_doc : wb_base {

    public enum tp_ele { title_doc, des_doc };

    public event EventHandler ChangedDoc;
    public event EventHandler SavedDoc;

    protected bool _loading = false, _changed = false;
    protected doc _doc = null;
    public doc doc { get { return _doc; } }

    public bool changed { get { return _changed; } }
    public void changed_doc () {
      _changed = true;
      EventHandler handler = ChangedDoc;
      if (null != handler) handler(this, EventArgs.Empty);
    }
    /*
    function save_doc() { return window.external.save_items($('html')[0].outerHTML); }
    public bool save_items (string h, bool show_err = true) {
      try {
        //System.IO.File.WriteAllText("c:\\tmp\\doc.html", h);
        classes.node.dal.save_items(_node_id, classes.item.parse_html(h));
        return true;
      } catch (Exception ex) { if (show_err) frm_popup.show_error(ex.Message); return false; }
    }

    public static List<item> parse_html (string html) {
      HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
      //htmlDoc.OptionFixNestedTags = true;
      doc.LoadHtml(html);
      List<item> items = new List<item>();
      if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
        throw new Exception("html document incorrect: " + doc.ParseErrors.ElementAt(0).Reason);
      if (doc.DocumentNode != null) {
        foreach (HtmlNode inode in doc.DocumentNode.SelectNodes("//*[@i-type]"))
          items.Add(parse_node(inode));
      }
      return items;
    }
     * */
    public void save_doc () {
      _changed = false;



      EventHandler handler = SavedDoc;
      if (null != handler) handler(this, EventArgs.Empty);
    }

    public wb_doc (int id_doc) {
      this.ObjectForScripting = new script_manager(this);
      load_doc(id_doc);
    }

    #region html doc

    protected void load_doc (int id_doc) {
      app._main.add_info("opening document " + id_doc.ToString() + "...");
      _loading = true;
      _doc = new doc(id_doc);

      StringBuilder html = new StringBuilder();

      add_element(html, tp_ele.title_doc, _doc.title);
      add_element(html, tp_ele.des_doc, _doc.des);

      load_html(app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-doc").value)
        , new Dictionary<string, object>() { { "html", html.ToString() } }));
    }

    protected void add_element (StringBuilder html, tp_ele tp, string txt) {
      if (tp == tp_ele.title_doc) html.Append(html_element("h3", tp.ToString(), txt, tp.ToString()));
      else if (tp == tp_ele.des_doc) html.Append(html_element("p", tp.ToString(), txt, tp.ToString()));
    }

    protected string html_element (string html_ele, string tp_ele, string txt, string class_name) {
      return string.Format("<{0} tp_ele='{1}' class='{3}' contenteditable='true' onkeydown='return ele_keydown(this, event)'>{2}</{0}>"
        , html_ele, tp_ele, txt, class_name);
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

    public void changed_doc () { _doc.changed_doc(); }
    public void save_doc () { _doc.save_doc(); }
  }
}
