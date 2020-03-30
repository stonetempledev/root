using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class tl_master : System.Web.UI.MasterPage {

  public tl_master () {
  }

  public string cmd { get; set; }

  protected tl_page tlp { get { return ((tl_page)Parent); } }

  protected mlib.tools.config config { get { return tlp.core.config; } }

  public virtual void redirect_to (string page) { Response.Redirect(page); }

  public virtual string get_val (string id) { return ""; }
  public virtual void set_val (string id, string val) { }

  public string url_cmd (string cmd, string page = "") { return (page == "" ? config.get_var("vars.router-page").value : page) + "?cmd=" + HttpUtility.UrlEncode(cmd); }

  public void elab_cmd(string cmd) { Response.Redirect(url_cmd(cmd, config.get_var("vars.router-page").value)); }

  public void status_txt(string txt) {
    tlp.ClientScript.RegisterStartupScript(tlp.GetType(), "__status_txt"
      , "status_txt_ms(\"" + txt.Replace("\"", "'") + "\");", true);
  }

  public void err_txt(string txt) {
    tlp.ClientScript.RegisterStartupScript(tlp.GetType(), "__err_txt"
      , "err_txt(\"" + txt.Replace("\"", "'").Replace("\r", " ").Replace("\n", " ").Replace("\\", "/") + "\");", true);
  }
}