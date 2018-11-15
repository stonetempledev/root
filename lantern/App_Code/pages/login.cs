using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using deeper.frmwrk.ctrls;

/// <summary>
/// Summary description for login
/// </summary>
namespace deeper.pages
{
  public class login : deeper.frmwrk.page_cls
  {
    public login(deeper.frmwrk.lib_page page, System.Xml.XmlNode pageNode)
      : base(page, pageNode) {
      _auth = false;
    }

    public override void onLoad(object sender, EventArgs e) {
      base.onLoad(sender, e);

      // pre cominciare resetto l'utente loggato
      if (!_page.IsPostBack)
        _page.resetLoggedUser();
    }

    public override bool action(string actionName, string formName, string keys = "", string noConfirm = "", string refurl = "") {
      if (base.action(actionName, formName, keys, noConfirm, refurl))
        return true;

      if (actionName == "login") {
        // utente, password inserita
        string user = input_ctrl(formName, "userName").Text
          , pwd = input_ctrl(formName, "userPwd").Text;

        // utente di emergenza segreto per la recovery
        if (_page.isRcvUser(user, pwd)) {
          _page.setLoggedUser(user, pwd);

          _page.redirect(_page.getPageRef("recovery"));

          return true;
        }

        // controllo dati
        if (user == "") {
          regScript(scriptStartAlert("Devi inserire un utente valido!", "Attenzione!"));
          return true;
        }

        // vedo se esiste l'utente inserito
        int id_user = 0;
        if (!_page.canLogin(user, pwd, out id_user)) {
          regScript(scriptStartAlert("L'utente '" + user + "' non è valido!", "Attenzione!"));
          return true;
        }

        // setto l'utente
        _page.setLoggedUser(user, pwd);

        reset_session_keys(id_user);

        _page.redirect(_page.Session["redirectAfterLogin"] != null ?
            _page.Session["redirectAfterLogin"].ToString() : _page.getPageRef(_page.cfg_var("homePageName")));

        return true;
      }

      return false;
    }
  }
}