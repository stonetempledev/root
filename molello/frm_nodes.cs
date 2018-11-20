using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using mlib;
using mlib.tools;

namespace molello {
    public partial class frm_nodes : frm_base {
        public frm_nodes() {
            InitializeComponent();

        }

        private void lbl_title_MouseDown(object sender, MouseEventArgs e) { mouse_down(e); }
        private void lbl_title_MouseMove(object sender, MouseEventArgs e) { mouse_move(e); }
        private void lbl_title_MouseUp(object sender, MouseEventArgs e) { mouse_up(); }

        private void pb_close_MouseClick(object sender, MouseEventArgs e) { Close(); }

        private void frm_nodes_Load(object sender, EventArgs e) {
            try {
                lbl_title.Text = app._core.config.get_var("vars.title-app").value;

                wb_toolbar.ObjectForScripting = new script_manager(this);
                wb_menu.ObjectForScripting = new script_manager(this);
                wb_body.ObjectForScripting = new script_manager(this);

                load_top(@"<li><span>ROOT</span></li>");
                load_menu(@"<table style='width:100%'>
            <tr><td align=right><img title='aggiungi nodo...' onclick='window.external.add_node()' src='{@var='vars.path-images'}\add-24.png' style='cursor:pointer;'/></td></tr></table>");
                load_body("<span>BODY</span>");
            } catch (Exception ex) { frm_popup.show_error(ex.Message); }
        }

        public void test() { frm_popup.show_msg("OSTI!", "miii"); }

        protected void load_top(string html) {
            try {
                wb_toolbar.DocumentText = app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-top").value)
                  , new Dictionary<string, object>() { { "html", html } });
            } catch (Exception ex) { frm_popup.show_error(ex.Message); }
        }

        protected void load_menu(string html) {
            try {
                wb_menu.DocumentText = app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-menu").value)
                  , new Dictionary<string, object>() { { "html", html } });
            } catch (Exception ex) { frm_popup.show_error(ex.Message); }
        }

        protected void load_body(string html) {
            try {
                wb_body.DocumentText = app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-body").value)
                  , new Dictionary<string, object>() { { "html", html } });
            } catch (Exception ex) { frm_popup.show_error(ex.Message); }
        }

        private void lbl_title_DoubleClick(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
            else this.WindowState = FormWindowState.Normal;
        }

        public void add_node() {
            frm_popup.show_msg("AGGIUNTA NODO", "lavori in corso...");
        }
    }

    [ComVisible(true)]
    public class script_manager {
        frm_nodes _form;

        public script_manager(frm_nodes form) { _form = form; }
     
        public void add_node() { _form.add_node(); }
    }

}
