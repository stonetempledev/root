using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.Security;
using mlib.db;
using mlib.tools;
using mlib.tiles;

public partial class _default : tl_master {

  protected void Page_Init (object sender, EventArgs e) {

    try {
      // check user
      string u_name = ""; int u_id = -1;
      FormsIdentity id = (FormsIdentity)tlp.User.Identity;
      FormsAuthenticationTicket ticket = id.Ticket;
      u_name = ticket.Name.Split(new char[] { '|' })[0];
      u_id = int.Parse(ticket.Name.Split(new char[] { '|' })[1]);

      // check utente
      string u_email = ""; user.type_user u_tp = user.type_user.none;
      if (cry.encode_tobase64(u_name) == tlp.core.app_setting("ad-usr")) {
        u_email = cry.decrypt(tlp.core.app_setting("ad-mail"), "kokko32!");
        u_id = 0; u_tp = user.type_user.admin;
      } else {
        DataRow dr = tlp.db_conn.first_row(@"select CONVERT(varchar(100), DecryptByKey(enc_nome)) as nome, CONVERT(varchar(100), DecryptByKey(enc_email)) as email, isnull(activated, 0) as activated 
        from utenti where CONVERT(varchar(100), DecryptByKey(enc_nome)) = '" + tlp.user + "';", open_key: true);
        if (dr == null || Convert.ToInt16(dr["activated"]) != 1) { tlp.log_out("login.aspx"); return; }
        u_email = dr["email"].ToString(); u_tp = user.type_user.normal;
      }

      tlp.set_user(u_id, u_name, u_email, u_tp);

      // command
      txt_cmd.Value = tlp.qry_val("cmd");

    } catch { }
  }

  protected void Page_Load (object sender, EventArgs e) {
  }

  protected void Cmd_Click (object sender, EventArgs e) { elab_cmd(config.get_var("vars.router-page").value); }

  public void elab_cmd(string page) { Response.Redirect(url_cmd(txt_cmd.Value, page)); }

  public override void redirect_to (string page) { Response.Redirect(page + "?cmd=" + HttpUtility.UrlEncode(txt_cmd.Value)); }

  public override string get_val (string id) {
    Control ctrl = FindControl(id); return ctrl != null ? (ctrl is HtmlInputText ? ((HtmlInputText)ctrl).Value
      : (ctrl is HtmlInputHidden ? ((HtmlInputHidden)ctrl).Value : "")) : "";
  }

  public override void set_val (string id, string val) {
    Control ctrl = FindControl(id); 
    if (ctrl != null && ctrl is HtmlInputText) ((HtmlInputText)ctrl).Value = val;
    else if (ctrl != null && ctrl is HtmlInputHidden) ((HtmlInputHidden)ctrl).Value = val;
  }
}
