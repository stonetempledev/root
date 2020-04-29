using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using toyn;
using mlib.tools;
using mlib.xml;

public partial class login : tl_page {

  protected void Page_Load (object sender, EventArgs e) {
    lbl_alert.Visible = lbl_ok.Visible = false;
  }

  protected void Go_Click (object sender, EventArgs e) {
    try {
      if (user_mail.Value == "") err_msg("devi scrivere la email!");
      else if (user_name.Value == "") err_msg("qual'è il tuo nomignolo?");
      else if (!strings.is_alpha(user_name.Value)) err_msg("il tuo nomignolo contiene almeno un carattere sbagliato!");
      else if (user_pass.Value == "") err_msg("devi scrivere la password!");
      else if (user_pass.Value.Length < 8) err_msg("la password dev'essere almeno di 8 caratteri!");
      else if (user_pass.Value.Contains(' ')) err_msg("la password ha uno spazio e non va bene!");
      else if (user_pass2.Value == "") err_msg("devi confermare la password!");
      else if (user_pass.Value != user_pass2.Value) err_msg("la conferma della password è andata male!");
      else {
        // check nomignolo
        DataRow dr = db_conn.first_row(@"select count(*) as cc from utenti 
        where isnull(activated, 0) in (1, 2, 3) and nome = '" + user_name.Value + "';");
        if ((int)dr["cc"] > 0) { err_msg("c'è già uno che si chiama " + user_name.Value + "!"); return; }

        // registrazione
        string tkey = cry.rnd_str(32);
        int id_utente = int.Parse(db_conn.exec(string.Format(@"insert into utenti (nome, email, pwd, dt_ins, tmp_key, activate_key, activated)
         values ('{0}', '{1}', '{2}', getdate(), '{3}', '{4}', 2);"
          , user_name.Value, user_mail.Value, cry.encode_tobase64(user_pass.Value), tkey, cry.rnd_str(32)), true));

        // salvo il documento di benvenuto
        elements el = new elements();
        List<element> els = el.load_xml(@"<element title=""Benvenuto ##user##!""> 
         <text style=""bold"">Ciao ##user##, benvenuto nel toyn!</text>
         <text>Ci sono un sacco di funzionalità utili per salvare i tuoi appunti, prendere note, seguire le tue attività.</text>
         <text>Avrai anche la possibilità di organizzare i tuoi documenti, le tue foto, i tuoi contatti e tutta una serie di informazioni che ti saranno utili.</text>
         <title ref=""{cmdurl='view cmds'}"">view cmds</title>
         <text>Come prima cosa per cominciare comincia con il visualizzare l'elenco dei comandi a disposizione, poi piano piano col tempo sarai in grado di capire come funziona il toyn!</text></element>"
          .Replace("##user##", user_name.Value));
        el.save_elements(els);

        Response.Redirect(string.Format("iscritto.aspx?tkey={0}", tkey));
      }
    } catch (Exception ex) { log.log_err(ex); err_msg(ex.Message); }
  }

  protected void ok_msg (string txt) {
    lbl_ok.Visible = true; lbl_ok.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }

  protected void err_msg (string txt) {
    lbl_alert.Visible = true; lbl_alert.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }
}