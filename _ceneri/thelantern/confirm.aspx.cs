﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using mlib.tools;

public partial class confirm : tl_page {
  protected void Page_Load (object sender, EventArgs e) {
    try {
      DataRow dr = db_conn.first_row(@"select CONVERT(varchar(100), DecryptByKey(enc_nome)) as nome, CONVERT(varchar(100), DecryptByKey(enc_email)) as email
          from utenti where isnull(activated, 0) = 3 and activate_key = '" + qry_val("akey") + "';", open_key: true);
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      db_conn.exec(string.Format(@"update utenti set activated = 1, dt_activate = getdate(), tmp_key = null, activate_key = null
      where activate_key = '{0}';", qry_val("akey")));

      lbl_user.Value = dr["nome"].ToString();
      txt_title.InnerText = string.Format("Bravo {0}, sei iscritto al the Lantern!", dr["nome"]);
      txt_body.InnerText = string.Format("Per entrare metti il tuo utente '{0}' e la password giusta!", dr["nome"]);
      c_down.Visible = true;
    } catch (Exception ex) {
      log.log_err(ex);
      txt_title.InnerText = "Siamo spiacenti...";
      txt_body.InnerHtml = "Ma non siamo riusciti a confermare la tua registrazione!<br><br>" + ex.Message;
    }
  }
}