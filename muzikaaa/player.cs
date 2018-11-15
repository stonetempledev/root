using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using muzikaaa.classes;

namespace muzikaaa
{
    public partial class player : Form
    {
        bool _init_bass = false;

        // player state
        public enum player_state { refresh_stack, can_play };
        string _state = "";

        // songs
        List<song> _songs = null;
        int _isong = -1;

        // mixer
        int _stream = 0;
        long _end_stream;
        song _song = null;
        Single _speed = -8, _volume = 50;

        // screen
        int _width, _y, _active_main_info, _active_info, _next_active_info;
        Dictionary<string, display> _displays;
        Dictionary<string, string> _infos, _main_infos;
        string _main_state;

        // forms
        bool _active_cestoni = false;
        form_cestoni _cestoni = null;
        process_icon _pi = null;
        public process_icon pi { get { return _pi; } set { _pi = value; } }

        public player()
        {
            InitializeComponent();

            try
            {
                _displays = new Dictionary<string, display>();
                _infos = new Dictionary<string, string>();
                _main_infos = new Dictionary<string, string>();
            }
            catch (Exception ex) { show_exception(ex); }
        }

        protected void add_state(player_state state) { if (!exist_state(state)) _state += "[" + state.ToString() + "]"; }

        protected void remove_state(player_state state) { _state = _state.Replace("[" + state.ToString() + "]", ""); }

        protected bool exist_state(player_state state) { return _state.IndexOf("[" + state.ToString() + "]") >= 0; }

        private void player_Load(object sender, EventArgs e)
        {
            try
            {
                if (config.boolean("start_hide")) iconize();

                ShowInTaskbar = false;
                _width = Screen.PrimaryScreen.Bounds.Width - 25;

                // displays
                _y = 0;
                foreach (XmlNode node in program._config.displays())
                {
                    display dis = program._config.create_display(node, _width, _y);
                    _displays.Add(dis.name, dis);

                    dis.control.DoubleClick += new EventHandler(display_DoubleClick);
                    dis.control.DrawEmpty += new EventHandler(ctrl_DrawEmpty);
                    dis.btnClick += new EventHandler(dis_btnClick);

                    this.Controls.Add(dis.control);

                    _y += dis.height;
                }

                ClientSize = new Size(_width, _y);
                CenterToScreen();
                this.Top = 0;

                // forms
                _cestoni = new form_cestoni();
                _cestoni.FormClosed += new FormClosedEventHandler(_cestoni_FormClosed);

                // bass
                if (!(_init_bass = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)))
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());

                // carica le songs
                reload_songs();

                // timers                
                tmr_state.Interval = 500;
                tmr_state.Start();

                tmr_keys.Interval = 500;
                tmr_keys.Start();

                play_next();
            }
            catch (Exception ex) { view_error(ex); }
        }

        protected void reload_songs()
        {
            try
            {
                add_state(player_state.refresh_stack);

                view_text("caricamento songs", "state");

                _songs = program._config.get_songs();
                _isong = _isong < _songs.Count ? _isong : -1;
                if (_songs.Count > 0)
                {
                    add_state(player_state.can_play);
                    view_text("", "state");
                }
                else view_text("scegli un cestone!", "state");

                remove_state(player_state.refresh_stack);
            }
            catch (Exception ex) { view_error(ex); }

        }

        void dis_btnClick(object sender, EventArgs e) { elab_item(((matrix_item)sender).name); }

        protected void elab_item(string name)
        {
            if (name == "remove_info"
                && _active_info >= 0 && _infos.Count > 0 && _active_info < _infos.Count)
            {
                try
                {
                    Mp3Lib.Mp3File idv = new Mp3Lib.Mp3File(_song.path);
                    foreach (Id3Lib.Frames.FrameBase frm in idv.TagModel)
                    {
                        if (frm.FrameId == _infos.Keys.ElementAt(_active_info))
                        {
                            idv.TagModel.Remove(frm);
                            idv.Update();
                            break;
                        }
                    }
                }
                catch (Exception ex) { show_exception(ex); }
            }
            else if (name == "add_info") { }
            else if (name == "cestoni") cestoni();
            else if (name == "next") next();
            else if (name == "del") delete();
            else if (name == "icon") iconize();
            else if (name == "exit") Close();
            else if (name == "slower") set_speed(_speed - 3);
            else if (name == "faster") set_speed(_speed + 3);
            else if (name == "reset_speed") set_speed(0);
            else if (name == "up") set_volume(_volume + 10);
            else if (name == "down") set_volume(_volume - 10);
            else if (name == "reset") set_volume(50);
        }

        protected void set_speed(Single speed)
        {
            _speed = speed;
            if (_stream != 0) Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_TEMPO, _speed);
        }

        protected void set_volume(Single volume)
        {
            _volume = volume;
            if (_stream != 0) Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, _volume / 100);
        }

        void display_DoubleClick(object sender, EventArgs e)
        {
            if (exist_state(player_state.can_play) && ((Control)sender).Name == "main")
                next();
        }

        protected void view_error(Exception ex)
        {
            if (exist_display("main"))
                view_text("oRRoreee! - " + ex.Message);
            else show_exception(ex);
        }

        protected void view_text(string txt) { view_text(txt, "main"); }

        protected string get_text(string display)
        {
            return exist_display(display) ? get_display(display).get_text_first() : "";
        }
        protected void view_text(string txt, string display)
        {
            if (exist_display(display))
            {
                display dis = get_display(display);
                dis.set_location(0, new Point(0, dis.item_y(0)));
                dis.set_speed(0, item_speed.None);
                dis.set_text_first(txt);
            }
        }

        static public void show_exception(Exception ex)
        {
            MessageBox.Show(ex.Message + ", source: " + ex.Source + ", stack: " + ex.StackTrace, "oRRoreee!");
        }

        static public void show_exception(string txt)
        {
            MessageBox.Show(txt, "oRRoreee!");
        }

        public void play_next()
        {
            if (exist_state(player_state.can_play))
                next();
        }

        protected void close_song()
        {
            if (_stream != 0)
            {
                if (!Bass.BASS_StreamFree(_stream))
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                _song = null;
                _stream = 0;
            }
        }

        protected bool next() { return next(false); }

        protected bool next(bool from_delete)
        {
            try
            {
                if (!_init_bass || _songs == null || _songs.Count() == 0) return false;

                close_song();

                if (_songs.Count > 0)
                {
                    if (program._config.play_mode() == config.play_modes.random)
                        _song = _songs.ElementAt(new Random().Next(0, _songs.Count()));
                    else
                    {
                        _isong = _isong >= 0 ? (!from_delete ? (_isong < _songs.Count - 1 ? _isong + 1 : 0)
                          : (_isong < _songs.Count ? _isong : 0)) 
                          : (program._config.last_index_played() >= 0 ? program._config.last_index_played() : 0);
                        _song = _songs.ElementAt(_isong);
                        if (program._config.last_index_played() != _isong)
                        {
                            program._config.set_last_index_played(_isong);
                            program._config.save();
                        }
                    }
                }
                if ((_stream = Bass.BASS_StreamCreateFile(_song.path, 0, 0, BASSFlag.BASS_STREAM_DECODE)) == 0)
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());

                // the tempo channel 
                if ((_stream = BassFx.BASS_FX_TempoCreate(_stream, BASSFlag.BASS_FX_FREESOURCE)) == 0)
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());

                set_speed(_speed);
                set_volume(_volume);

                Bass.BASS_ChannelPlay(_stream, false);
                _end_stream = Bass.BASS_StreamGetFilePosition(_stream, BASSStreamFilePosition.BASS_FILEPOS_END);

                // infos
                _main_state = "";
                _infos.Clear();
                _main_infos.Clear();
                _active_main_info = 0; _next_active_info = -1;
                Mp3Lib.Mp3File idv = new Mp3Lib.Mp3File(_song.path);
                if (idv != null)
                {
                    foreach (Id3Lib.Frames.FrameBase frm in idv.TagModel)
                    {
                        if (frm.ToString() == "" || frm.FrameId == "PRIV") continue;

                        if (frm.FrameId == "TIT2" || frm.FrameId == "TALB"
                            || frm.FrameId == "TPE1" || frm.FrameId == "TPE2")
                        {
                            if (!_main_infos.ContainsKey(frm.FrameId)) _main_infos.Add(frm.FrameId, frm.ToString());
                        }
                        else if (!_infos.ContainsKey(frm.FrameId))
                            _infos.Add(frm.FrameId, frm.ToString());
                    }
                }
                if (!_main_infos.ContainsKey("TIT2")) _main_infos.Add("<FILE>", System.IO.Path.GetFileName(_song.path));
                if (!string.IsNullOrEmpty(_song.cestone)) _infos.Add("<CESTONE>", _song.cestone);

                string ni_text = "(" + _song.cestone + ") - " + (_main_infos.ContainsKey("TIT2") ? _main_infos["TIT2"] : _main_infos["<FILE>"]);
                _pi.ni.Text = ni_text.Length >= 64 ? ni_text.Substring(0, 63) : ni_text;

                // display infos
                _main_state = "started";

                if (exist_display("infos")) get_display("infos").clear_text();
                if (exist_display("main")) get_display("main").clear_text();

                ctrl_DrawEmpty(exist_display("infos") ? get_display("infos").control : null, new EventArgs());
                ctrl_DrawEmpty(exist_display("main") ? get_display("main").control : null, new EventArgs());

                return true;
            }
            catch (Exception ex) { view_error(ex); return false; }
        }

        void ctrl_DrawEmpty(object sender, EventArgs e)
        {
            if (_main_state == "started" && (sender != null && ((matrix_control)sender).Name == "main"))
            {
                display dis = get_display("main");
                int rdn = new Random().Next(0, 6);

                if (rdn <= 1)
                {
                    dis.set_location(0, new Point(0, dis.item_y(0)));
                    dis.set_direction(0, item_direction.Right);
                    dis.set_speed(0, item_speed.Middle);
                }
                else if (rdn == 2)
                {
                    dis.set_location(0, new Point(0, dis.item_y(0)));
                    dis.set_direction(0, item_direction.Down);
                    dis.set_speed(0, item_speed.Slow);
                }
                else if (rdn == 3)
                {
                    dis.set_location(0, new Point(dis.control.matrix_size()[0].Count, dis.item_y(0)));
                    dis.set_direction(0, item_direction.Right);
                    dis.set_speed(0, item_speed.Middle);
                }
                else if (rdn == 4)
                {
                    dis.set_location(0, new Point(0, dis.control.matrix_size().Count + 1));
                    dis.set_direction(0, item_direction.Up);
                    dis.set_speed(0, item_speed.Slow);
                }
                else if (rdn >= 5)
                {
                    dis.set_location(0, new Point(dis.control.matrix_size()[0].Count, dis.item_y(0)));
                    dis.set_direction(0, item_direction.Left);
                    dis.set_speed(0, item_speed.Middle);
                }

                dis.set_text_first(title_frame(_main_infos.Keys.ElementAt(_active_main_info)) + ": " + _main_infos.Values.ElementAt(_active_main_info));
                dis.control.start_move(200, new Random().Next(10, 20));
                _active_main_info = (_active_main_info < _main_infos.Count - 1) ? _active_main_info + 1 : 0;
            }
            else if (_main_state == "started" && (sender != null && ((matrix_control)sender).Name == "infos") && _infos.Count > 0)
            {
                display dis = get_display("infos");

                dis.set_location(0, new Point(0, dis.item_y(0)));
                dis.set_direction(0, item_direction.Down);
                dis.set_speed(0, item_speed.Slow);

                _active_info = _next_active_info > 0 ? _next_active_info : 0;
                dis.set_text_first(title_frame(_infos.Keys.ElementAt(_active_info)) + ": " + _infos.Values.ElementAt(_active_info));
                dis.control.start_move(150, new Random().Next(15, 30));
                _next_active_info = (_active_info < _infos.Count - 1) ? _active_info + 1 : 0;
            }
        }

        private bool exist_display(string name) { return _displays.ContainsKey(name); }
        private display get_display(string name) { return _displays[name]; }

        private void player_FormClosed(object sender, FormClosedEventArgs e)
        {
            close_song();
            Bass.BASS_Free();
        }

        private void tmr_state_Tick(object sender, EventArgs e)
        {
            try
            {
                if (exist_state(player_state.can_play) && (_stream != 0 && Bass.BASS_StreamGetFilePosition(_stream, BASSStreamFilePosition.BASS_FILEPOS_CURRENT) >= _end_stream))
                    next();
            }
            catch { }
        }

        protected string title_frame(string frame_id)
        {
            string des = Id3Lib.FrameDescription.GetDescription(frame_id);
            return frame_id == "<FILE>" ? "file"
              : (frame_id == "<CESTONE>" ? "cestone" : (des.IndexOf("/") > 0 ? des.Substring(0, des.IndexOf("/")).ToLower() : des.ToLower()));
        }

        private void player_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                ShowInTaskbar = false;
        }

        public void init_size()
        {
            if (_width > 0)
            {
                ClientSize = new Size(_width, _y);
                CenterToScreen();
                this.Top = 0;
            }
        }

        public void show_form()
        {
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            //TopMost = true;
            TopLevel = true;
            Show();
            //BringToFront();
            init_size();
        }

        public void iconize()
        {
            if (_active_cestoni) _cestoni.Close();
            WindowState = FormWindowState.Minimized;
            Hide();
        }

        public void delete()
        {
            try
            {
                if (_songs.Count >= 1 && _song != null)
                {
                    string path = _song.path;
                    close_song();
                    System.IO.File.Delete(path);
                    reload_songs();
                    next(true);
                }
            }
            catch (Exception ex) { view_error(ex); }
        }

        public void cestoni()
        {
            show_form();

            _active_cestoni = true;
            _cestoni.ShowDialog(this);
        }

        void _cestoni_FormClosed(object sender, FormClosedEventArgs e)
        {
            _active_cestoni = false;

            if (program._config.modified)
            {
                reload_songs();
                if (_song == null) next();
                program._config.save();
            }
        }

        private void tmr_keys_Tick(object sender, EventArgs e)
        {
            try
            {
                string msg = "";

                foreach (XmlNode btn in program._config.btn_keys())
                {
                    if (btn.Attributes["keys"].Value.Split(',').Length ==
                      btn.Attributes["keys"].Value.Split(',').Count(vkey => key_info.is_key_down((int)((Keys)Enum.Parse(typeof(Keys), vkey)))))
                    {
                        msg += string.Format(" [{0}]", btn.Attributes["name"].Value);
                        elab_item(btn.Attributes["name"].Value);
                    }
                }

                if (get_text("state") != msg) view_text(msg, "state");
            }
            catch { }
        }
    }
}
