using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using muzikaaa.Properties;

namespace muzikaaa
{
    public class process_icon : IDisposable
    {
		protected NotifyIcon _ni = null;
        public NotifyIcon ni { get { return _ni; } }
        protected player _player = null;
        public player form { get { return _player; } }

        public process_icon(player frm)
		{
			_ni = new NotifyIcon();
            _player = frm;
            _player.pi = this;
		}

		public void display()
		{
            _ni.MouseDoubleClick += new MouseEventHandler(ni_MouseDoubleClick);
            _ni.Icon = Resources.notify_icon;
            _ni.Text = "muzikaaa";
            _ni.Visible = true;

			// context menu.
            _ni.ContextMenuStrip = new context_menu(this).Create();
		}

        public void update_sposta() 
        { 
        }

		public void Dispose() { _ni.Dispose(); }

        void ni_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _player.play_next();
        }
    }
}
