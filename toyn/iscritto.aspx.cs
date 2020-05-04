﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using mlib.db;
using mlib.tools;
using mlib.tiles;

public partial class login : tl_page {

  protected void Page_Load (object sender, EventArgs e) {
    try {
      DataRow dr = db_conn.first_row(@"select user_id, nome, email, activate_key 
        from users where isnull(activated, 0) = 2 and tmp_key = '" + qry_val("tkey") + "';");
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      this.user = new user(db_provider.int_val(dr["user_id"]), db_provider.str_val(dr["nome"])
        , db_provider.str_val(dr["email"]), mlib.tiles.user.type_user.normal);

      db_conn.exec(string.Format(@"update users set activated = 3 where tmp_key = '{0}';", qry_val("tkey")));

      string upass = get_cache_var("tmp_password");
      set_cache_var("tmp_password", "");

      send_mail(dr["email"].ToString(), "conferma iscrizione al toyn",
        string.Format(@"<h2>Ciao {0}!</h2><p>Ti sei appena iscritto al <a href='{1}'>TOYN</a>!</p>
          <p><i>Se ti sei già dimenticato la password eccola qua: {3}</i></p>
          <h3><a href='{1}confirm.aspx?akey={2}'>Clicca qui per confermare la tua iscrizione!</a></h3>"
        , dr["nome"], core.base_url, dr["activate_key"], upass));

      txt_title.InnerText = string.Format("Ciao {0}!", dr["nome"]);
      txt_body.InnerText = "Adesso vai nella tua mail e conferma la tua iscrizione...";
    } catch (Exception ex) {
      log.log_err(ex);
      txt_title.InnerText = "Peccato...";
      txt_body.InnerText = "Ma non è stato possibile inviarti la mail di conferma all'indirizzo."
        + "<br><br>" + ex.Message;
    }

  }
}