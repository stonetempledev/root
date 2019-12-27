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

    public int content_id { get; set; }
    public child child { get; set; }

    public element element_child { get { return this.child as element; } }
    public title title { get { return this.child as title; } }
    public text text { get { return this.child as text; } }
    public value value { get { return this.child as value; } }
    public account account { get { return this.child as account; } }

    protected Dictionary<int, object> _childs = null;

    public element_content(element child, int content_id = 0) {
      this.content_id = content_id; this.child = child; child.element_content = this; 
    }

    public element_content(title child, int content_id = 0) {
      this.content_id = content_id; this.child = child; child.element_content = this; 
    }

    public element_content(text child, int content_id = 0) {
      this.content_id = content_id; this.child = child; child.element_content = this; 
    }

    public element_content(value child, int content_id = 0) {
      this.content_id = content_id; this.child = child; child.element_content = this; 
    }

    public element_content(account child, int content_id = 0) {
     this.content_id = content_id; this.child = child; child.element_content = this; 
    }
  }
}