using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;
using System.IO;
using mlib;
using mlib.tools;
using mlib.db;
using mlib.xml;
using mlib.tiles;

namespace toyn {
  public class element_content {
    public enum element_content_type { element, title, text, value, account }

    public int content_id { get; set; }
    public element element { get; set; }
    public element_content_type content_type { get; set; }
    public child child { get; set; }

    public element element_child { get { return this.child as element; } }
    public title title { get { return this.child as title; } }
    public text text { get { return this.child as text; } }
    public value value { get { return this.child as value; } }
    public account account { get { return this.child as account; } }

    protected Dictionary<int, object> _childs = null;

    public element_content(element e, element child, int content_id = 0) {
      this.element = e; this.content_id = content_id; this.child = child; child.element_content = this; this.content_type = element_content_type.element;
    }

    public element_content(element e, title child, int content_id = 0) {
      this.element = e; this.content_id = content_id; this.child = child; child.element_content = this; this.content_type = element_content_type.title;
    }

    public element_content(element e, text child, int content_id = 0) {
      this.element = e; this.content_id = content_id; this.child = child; child.element_content = this; this.content_type = element_content_type.text;
    }

    public element_content(element e, value child, int content_id = 0) {
      this.element = e; this.content_id = content_id; this.child = child; child.element_content = this; this.content_type = element_content_type.value;
    }

    public element_content(element e, account child, int content_id = 0) {
      this.element = e; this.content_id = content_id; this.child = child; child.element_content = this; this.content_type = element_content_type.account;
    }
  }
}