using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace muzikaaa
{
    static class program
    {
        public static config _config = null; 

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _config = new config(config.setting("config_path"));

                player frm = new player();
                using (process_icon pi = new process_icon(frm))
                {
                    pi.display();

                    Application.Run(frm);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ooRRrooore"); }
        }
    }
}
