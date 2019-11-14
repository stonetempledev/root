using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;
using mlib;
using mlib.tools;

public partial class test : tl_page {

  protected void Page_Load (object sender, EventArgs e) {
  }

  protected void btn_email_click (object sender, EventArgs e) {
    try {
      add_class(email, "show");

      send_mail(mail_ad.Value, "mail test il Lantern!", "the body of the mail");

      show_result(result_email, "MAIL INVIATA CON SUCCESSO!");
    } catch (Exception ex) { show_result(result_email, "<b>" + ex.Message + "</b>", true); }
  }

  protected void show_result (HtmlControl alert, string html, bool err = false) {
    string cls_ok = "alert-success", cls_err = "alert-danger";
    result_email.Visible = true;
    remove_class(result_email, !err ? cls_err : cls_ok); add_class(result_email, !err ? cls_ok : cls_err);
    result_email.InnerHtml = html;
  }
}