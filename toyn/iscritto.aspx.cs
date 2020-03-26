using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using mlib.tools;

public partial class login : tl_page {

  protected void Page_Load (object sender, EventArgs e) {
    try {
      DataRow dr = db_conn.first_row(@"select nome, email, activate_key 
        from utenti where isnull(activated, 0) = 2 and tmp_key = '" + qry_val("tkey") + "';");
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      db_conn.exec(string.Format(@"update utenti set activated = 3 where tmp_key = '{0}';", qry_val("tkey")));

      send_mail(dr["email"].ToString(), "conferma iscrizione the Lantern",
        string.Format("<h3>Ciao {0}!</h3><p><a href='{1}confirm.aspx?akey={2}'>Clicca qui per confermare la tua iscrizione!</a></p>"
        , dr["nome"], base_url, dr["activate_key"]));

      txt_title.InnerText = string.Format("Benvenuto {0} in the Lantern!", dr["nome"]);
      txt_body.InnerText = "Ora vai nella tua mail e conferma la tua iscrizione!";
    } catch (Exception ex) {
      log.log_err(ex);
      txt_title.InnerText = "Siamo spiacenti...";
      txt_body.InnerText = "Ma non siamo riusciti ad inviarti la mail di conferma della tua iscrizione all'indirizzo."
        + "<br><br>" + ex.Message;
    }

  }
}