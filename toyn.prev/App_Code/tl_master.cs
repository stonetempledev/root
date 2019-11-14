using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class tl_master : System.Web.UI.MasterPage {

  public tl_master () {
  }

  protected tl_page tlp { get { return ((tl_page)Parent); } }

  protected mlib.tools.config config { get { return tlp.core.config; } }

  public virtual void redirect_to (string page) { Response.Redirect(page); }

  public virtual string get_val (string id) { return ""; }
  public virtual void set_val (string id, string val) { }

  public string url_cmd (string cmd, string page = "") { return (page == "" ? config.get_var("vars.router-page").value : page) + "?cmd=" + HttpUtility.UrlEncode(cmd); }

}