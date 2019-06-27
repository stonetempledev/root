﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using mlib.tools;

public partial class reimposta : tl_page {
  protected string _nome, _mail;
  protected void Page_Load (object sender, EventArgs e) {
    try {
      lbl_alert.Visible = lbl_ok.Visible = false;
      DataRow dr = db_conn.first_row(@"select CONVERT(varchar(100), DecryptByKey(enc_nome)) as nome, CONVERT(varchar(100), DecryptByKey(enc_email)) as email
          from utenti where activated = 1 and activate_key = '" + qry_val("akey") + "';", open_key: true);
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      _nome = dr["nome"].ToString(); _mail = dr["email"].ToString();

      txt_title.InnerText = string.Format("Reimposta la password {0}!", dr["nome"]);
    } catch (Exception ex) {
      log.log_err(ex);
    }
  }

  protected void Go_Click (object sender, EventArgs e) {
    try {
      if (user_pass.Value == "") err_msg("devi scrivere la password!");
      else if (user_pass.Value.Length < 8) err_msg("la password dev'essere almeno di 8 caratteri!");
      else if (user_pass2.Value == "") err_msg("devi confermare la password!");
      else if (user_pass.Value != user_pass2.Value) err_msg("la conferma della password è andata male!");
      else {

        // ri-registrazione
        string tkey = mlib.tools.cry.rnd_str(32);
        db_conn.exec(string.Format(@"insert into log_azioni_utenti (id_utente, id_azione, dt_ins)
         select u.id_utente, au.id_azione, getdate() as dt_ins
         from utenti u join azioni_utenti au on au.azione = 'repassword'
         where activated = 1 and CONVERT(varchar(100), DecryptByKey(enc_nome)) = '{0}'", _nome), open_key: true);
        db_conn.exec(string.Format(@"update utenti set pwd = '{0}', dt_upd = getdate()
          where activated = 1 and CONVERT(varchar(100), DecryptByKey(enc_nome)) = '{1}';"
          , mlib.tools.cry.encode_tobase64(user_pass.Value), _nome), open_key: true);

        Response.Redirect(string.Format("reiscritto.aspx?akey={0}", qry_val("akey")));
      }
    } catch (Exception ex) { mlib.tools.log.log_err(ex); err_msg(ex.Message); }
  }

  protected void ok_msg (string txt) {
    lbl_ok.Visible = true; lbl_ok.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }

  protected void err_msg (string txt) {
    lbl_alert.Visible = true; lbl_alert.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }
}