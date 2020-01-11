using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mlib;
using mlib.xml;

namespace toyn {
  public class account : child {
    public string account_user { get; set; }
    public string account_password { get; set; }
    public string account_notes { get; set; }

    public account(element el) : base(el, content_type.account) { }

    public account(element el, int account_id, string account_user, string account_password, string account_notes = "")
      : base(el, content_type.account, account_id) {
      this.account_user = account_user; this.account_password = account_password; this.account_notes = account_notes;
    }

    public account(element el, int account_id, string user_password, string account_notes = "")
      : base(el, content_type.account, account_id) {
      this.account_user = user_password.Contains('/') ? user_password.Split(new char[] { '/' })[0] : user_password;
      this.account_password = user_password.Contains('/') ? user_password.Split(new char[] { '/' })[1] : "";
      this.account_notes = account_notes;
    }

    public override void add_xml_node(xml_node el) {
      xml_node nd = el.add_node("account");
      nd.set_attrs(new string[,] { { "user", this.account_user }
        , { "password", this.account_password }, { "notes", this.account_notes }, { "id", this.id.ToString() } });
    }

    public static account load_xml(element el, xml_node nd) {
      account a = new account(el); a.load_xml_node(el, nd); return a;
    }

    public override void load_xml_node(element el, xml_node nd) {
      this.element = el;
      this.id = nd.get_int("id", 0);
      this.account_user = nd.get_attr("user");
      this.account_password = nd.get_attr("password");
      this.account_notes = nd.get_attr("notes");
    }
  }
}
