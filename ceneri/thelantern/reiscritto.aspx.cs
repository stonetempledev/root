﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using mlib.tools;

public partial class reiscritto : tl_page {

  protected void Page_Load (object sender, EventArgs e) {
    try {
      DataRow dr = db_conn.first_row(@"select CONVERT(varchar(100), DecryptByKey(enc_nome)) as nome, CONVERT(varchar(100), DecryptByKey(enc_email)) as email
          , activate_key from utenti where activated = 1 and activate_key = '" + qry_val("akey") + "';", open_key: true);
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      db_conn.exec(string.Format(@"update utenti set activate_key = null 
        where activated = 1 and activate_key = '{0}';", qry_val("akey")));

      txt_title.InnerText = string.Format("Bravo {0} hai reimpostato la password in the Lantern!", dr["nome"]);
      txt_body.InnerHtml = "<a href='login.aspx?nm=" + dr["nome"].ToString() + "'>Ora puoi entrare con la tua nuova password!</a>";
    } catch (Exception ex) {
      log.log_err(ex);
      txt_title.InnerText = "Siamo spiacenti...";
      txt_body.InnerText = "Ma si è verificato un errore...<br><br>" + ex.Message;
    }

  }
}