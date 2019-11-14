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

    public account(element el, int account_id, string account_user, string account_password, string account_notes = "") :base(el, account_id) {
      this.account_user = account_user; this.account_password = account_password; this.account_notes = account_notes;
    }

    public account(element el, int account_id, string user_password, string account_notes = ""): base(el, account_id) {
      this.account_user = user_password.Contains('/') ? user_password.Split(new char[] { '/' })[0] : user_password;
      this.account_password = user_password.Contains('/') ? user_password.Split(new char[] { '/' })[1] : "";
      this.account_notes = account_notes;
    }
  }
}
