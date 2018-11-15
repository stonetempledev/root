
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace muzikaaa
{
    public enum item_direction : int { None = -1, Up = 0, Down = 2, Left = 3, Right = 4 }

    public enum item_speed : int
    {
        None = -1, Slow = 0, Idle = 1,
        Fast = 2, Middle = 4, Letter = 8, Double = 16
    }

    public enum led_style : int { Round = 0, Square = 1 }

    public struct led_online
    {
        public int _line_no;
        public string _led_on;

        public led_online(int line_no, string led_on)
        {
            _line_no = line_no;
            _led_on = led_on;
        }
    }

    public struct matrix_symbol
    {
        public bool[,] _led_on_matrix;
        public string _des;
        public uint _code;

        public matrix_symbol(uint p_uiCode, string p_sDescription, bool[,] p_bLedOnMatrix)
        {
            _code = p_uiCode;
            _des = p_sDescription;
            _led_on_matrix = p_bLedOnMatrix;
        }
    }

    public struct matrix_symbol_font
    {
        public string _name;
        public List<matrix_symbol> _symbols;

        public matrix_symbol_font(string name, List<matrix_symbol> symbols)
        {
            _name = name;
            _symbols = symbols;
        }
    }

    public struct matrix_symbol_collection
    {
        public List<matrix_symbol_font> _fonts;

        public matrix_symbol_collection(List<matrix_symbol_font> fonts)
        {
            _fonts = fonts;
        }
    }

    public class led_property
    {
        public Point _location;
        public int _radius;

        public led_property()
        {
            _location.X = 0;
            _location.Y = 0;
            _radius = 0;
        }
    }

    public class matrix_item
    {
        public enum type_item { text = 0, button }

        int _id;
        bool[,] _master_led_on;
        Point _curr_location;
        item_direction _direction;
        item_speed _speed;
        string _text, _name;
        type_item _tp;

        public matrix_item(int id, type_item tp, string name, bool[,] master_led_on, string text, Point init_location, item_direction dir, item_speed speed)
        {
            _id = id;
            _tp = tp;
            _name = name;
            _master_led_on = master_led_on;
            _direction = dir;
            _speed = speed;
            _curr_location = init_location;
            _text = text;
        }

        public int GetId() { return _id; }

        public int GetLineOffset() { return _curr_location.Y; }
        public int GetRowOffset() { return _curr_location.X; }
        public bool[,] GetMasterLedOn() { return _master_led_on; }
        public string GetText() { return _text; }
        public type_item type { get { return _tp; } }
        public string name { get { return _name; } }

        public void SetLedOnArray(bool[,] master_ledon, string text) { _master_led_on = master_ledon; _text = text; }
        public Point location { get { return _curr_location; } set { _curr_location = value; } }
        public void SetDirection(item_direction p_dDirection) { _direction = p_dDirection; }
        public void SetSpeed(item_speed p_sSpeed) { _speed = p_sSpeed; }

        /// <summary>
        /// Move the item in the led matrix according to its properties
        /// </summary>
        /// <param name="tick_count">Clock tick count</param>
        /// <param name="p_iNbCtrlLedLine">Number of lines in the matrix</param>
        /// <param name="p_iNbCtrLedRow">Number of rows in the matrix</param>
        public void MoveItem(uint tick_count, int p_iNbCtrlLedLine, int p_iNbCtrLedRow)
        {
            int iOffset = 0;
            switch (_speed)
            {
                case item_speed.Slow: if ((tick_count % 2) == 0) iOffset = 1; break;
                case item_speed.Idle: iOffset = 1; break;
                case item_speed.Fast: iOffset = 2; break;
                case item_speed.Middle: iOffset = 4; break;
                case item_speed.Letter: iOffset = 8; break;
                case item_speed.Double: iOffset = 16; break;
                default: break;
            }

            // Any movement ?
            if (iOffset == 0) return;

            // Applies the offest according to the item direction 
            switch (_direction)
            {
                case item_direction.Up: _curr_location.Y -= iOffset; break;
                case item_direction.Down: _curr_location.Y += iOffset; break;
                case item_direction.Left: _curr_location.X -= iOffset; break;
                case item_direction.Right: _curr_location.X += iOffset; break;
                default: break;
            }

            // Exit by right
            if (_curr_location.X >= p_iNbCtrLedRow) _curr_location.X = -_master_led_on.GetLength(1);
            // Exit by left
            else if (_curr_location.X < -_master_led_on.GetLength(1))
                _curr_location.X = p_iNbCtrLedRow - 1;

            // Exit by bottom
            if (_curr_location.Y >= p_iNbCtrlLedLine) _curr_location.Y = -_master_led_on.GetLength(0);
            // Exit by top
            else if (_curr_location.Y < -_master_led_on.GetLength(0))
                _curr_location.Y = p_iNbCtrlLedLine - 1;
        }

    }

    /// <summary>
    /// Definition of the led matrix control
    /// </summary>
    public partial class matrix_control : Control
    {
        int _nlines, _nrows, _cell_size;
        double _coeff;
        SolidBrush _on_brush, _off_brush;
        led_style _led_style;
        List<List<led_property>> _led_property_list;
        matrix_symbol_collection _font_collection;
        List<matrix_item> _item_list;
        Timer _timer;
        uint _tick_count; int _wait_ticks;
        bool _can_raise_empty;

        public event EventHandler DrawEmpty;

        #region constuctor

        public matrix_control(string symbol_file)
        {
            // Double bufferisation
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint, true);

            // Size
            _nlines = 16;
            _nrows = 64;
            _coeff = 0.67;

            // Color
            _on_brush = new SolidBrush(Color.Red);
            _off_brush = new SolidBrush(Color.DarkGray);

            // Style
            _led_style = led_style.Round;

            // Led collection
            _item_list = new List<matrix_item>();
            _led_property_list = new List<List<led_property>>();

            // Timer
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 70;
            _timer.Tick += new EventHandler(timer_Tick);
            _tick_count = 0;

            // Id count
            _can_raise_empty = false;

            // Place the leds on the control
            matrix_size(_nlines, _nrows);

            font_from_file(symbol_file);
        }

        #endregion

        #region properties

        [Category("Matrix size"), Description("Rows"), DefaultValue(64)]
        public int led_rows
        {
            get { return _nrows; }
            set
            {
                _nrows = value;
                matrix_size(_nlines, _nrows);
                Invalidate();
            }
        }

        [Category("Matrix size"), Description("Lines"), DefaultValue(2)]
        public int led_lines
        {
            get { return _nlines; }
            set
            {
                _nlines = value;
                matrix_size(_nlines, _nrows);
                Invalidate();
            }
        }

        [Category("Matrix size"), Description("Size Led"), DefaultValue(5)]
        public int led_size
        {
            get { return _cell_size; }
            set
            {
                matrix_size(this.Height / value, this.Width / value);
                Invalidate();
            }
        }

        [Category("Matrix size"), Description("Size coeff"), DefaultValue(0.66)]
        public double size_coeff
        {
            get { return _coeff; }
            set
            {
                if ((value < 0) || (value < 1)) Invalidate();
                _coeff = value;
                pos_and_size();
                Invalidate();
            }
        }

        [Category("Led Color"), Description("On"), DefaultValue("192,45,0")]
        public Color led_on_color
        {
            get { return _on_brush.Color; }
            set
            {
                _on_brush.Color = value;
                Invalidate();
                Refresh();
            }
        }

        [Category("Led Color"), Description("Off"), DefaultValue("192,111,105")]
        public Color led_off_color
        {
            get { return _off_brush.Color; }
            set
            {
                _off_brush.Color = value;
                Invalidate();
                Refresh();
            }
        }

        #endregion

        #region modificators

        public led_style get_led_style() { return _led_style; }

        public void set_led_style(led_style ls)
        {
            _led_style = ls;
            this.Refresh();
        }

        public void matrix_size(int lines, int rows)
        {
            _led_property_list.Clear();

            for (int iline = 0; iline < lines; iline++)
            {
                List<led_property> line_prop = new List<led_property>();
                for (int i = 0; i < rows; i++)
                    line_prop.Add(new led_property());

                _led_property_list.Add(line_prop);
            }

            pos_and_size();
        }

        public List<List<led_property>> matrix_size() { return _led_property_list; }

        private void pos_and_size()
        {
            int size_from_width = this.Width / (_led_property_list[0].Count);
            int size_from_height = this.Height / (_led_property_list.Count);
            _cell_size = (size_from_height > size_from_width) ? size_from_width : size_from_height;

            int width_margin = (this.Width - _led_property_list[0].Count * _cell_size) / 2;
            int height_margin = (this.Height - _led_property_list.Count * _cell_size) / 2;
            int y_linepos = height_margin + _cell_size / 2;

            for (int iline = 0; iline < _led_property_list.Count; iline++)
            {
                int x_rowpos = width_margin + _cell_size / 2;

                for (int irow = 0; irow < _led_property_list[0].Count; irow++)
                {
                    _led_property_list[iline][irow]._location.X = x_rowpos;
                    _led_property_list[iline][irow]._location.Y = y_linepos;
                    _led_property_list[iline][irow]._radius = (int)(_cell_size * _coeff / 2);

                    x_rowpos += _cell_size;
                }

                y_linepos += _cell_size;
            }

            this.Refresh();
        }

        #endregion

        #region items

        protected matrix_item find_item(int id) { return _item_list.Find(x => x.GetId() == id); }

        public void add_text_item(int id, string txt, Point location, item_direction dir, item_speed speed)
        { _item_list.Add(new matrix_item(id, matrix_item.type_item.text, "", ledon_from_string(txt), txt, location, dir, speed)); }

        public void add_btn_item(int id, string name, string txt, Point location, item_direction dir, item_speed speed)
        { _item_list.Add(new matrix_item(id, matrix_item.type_item.button, name, ledon_from_string(txt), txt, location, dir, speed)); }

        public int get_new_pos_x(int to_i)
        {
            int result = 0;

            for (int i = 0; i < to_i; i++)
                result += _item_list[i].GetMasterLedOn().GetUpperBound(1);

            return result;
        }

        public void set_item_location(int id, Point location)
        {
            find_item(id).location = location;
            this.Refresh();
        }

        public void set_item_direction(int id, item_direction p_dDirection)
        { find_item(id).SetDirection(p_dDirection); }

        public void set_item_speed(int id, item_speed p_sSpeed)
        { find_item(id).SetSpeed(p_sSpeed); }

        public string item_text(int id)
        { return find_item(id).GetText(); }

        public void set_item_text(int id, string txt)
        {
            find_item(id).SetLedOnArray(ledon_from_string(txt), txt);

            this.Refresh();
        }

        public List<matrix_item> items { get { return _item_list; } }

        private void move_items(uint tick_count)
        {
            foreach (matrix_item lmiItem in _item_list)
                lmiItem.MoveItem(tick_count, _led_property_list.Count, _led_property_list[0].Count);

            this.Refresh();
        }

        public void start_move(int ms)
        { start_move(ms, 0); }

        public void start_move(int ms, int wait_ticks)
        {
            _wait_ticks = wait_ticks;
            _timer.Interval = ms;
            _timer.Start();
        }

        public void stop_move()
        {
            _timer.Stop();
        }

        #endregion

        #region Symbols

        private bool[,] ledon_from_char(char p_cSourceChar)
        {
            bool[,] bExclaim = new bool[4, 4];

            // Look in the font collection
            foreach (matrix_symbol lmsSymbol in _font_collection._fonts[0]._symbols)
                if (p_cSourceChar == lmsSymbol._code)
                    return lmsSymbol._led_on_matrix;

            // Unfound symbol
            bExclaim[0, 0] = true;
            bExclaim[0, 2] = true;
            bExclaim[1, 0] = true;
            bExclaim[1, 2] = true;
            bExclaim[3, 0] = true;
            bExclaim[3, 2] = true;
            return bExclaim;
        }

        public bool[,] ledon_from_string(string src_string)
        {
            int max_height_char = 0;
            int nb_led_used = 0;
            int led_count = 0;
            List<bool[,]> led_on_char = new List<bool[,]>();

            //Get the list of the ledOn array symbols form the string's char

            // Loop on the char
            for (int iIdxChar = 0; iIdxChar < src_string.Length; iIdxChar++)
            {
                // Get the corresponding array
                led_on_char.Add(ledon_from_char(src_string[iIdxChar]));

                // Memorize the maximum height size of the chars
                if (led_on_char[led_on_char.Count - 1].GetLength(0) > max_height_char)
                    max_height_char = led_on_char[led_on_char.Count - 1].GetLength(0);

                // Updates the nb of led used by the all chars
                nb_led_used += led_on_char[led_on_char.Count - 1].GetLength(1);
            }

            // Allocation
            bool[,] ret_ledon = new bool[max_height_char, nb_led_used];

            // Loop on the char arrays
            foreach (bool[,] curr_ledon in led_on_char)
            {
                for (int iIdxLine = 0; iIdxLine < curr_ledon.GetLength(0); iIdxLine++)
                    for (int iIdxRow = 0; iIdxRow < curr_ledon.GetLength(1); iIdxRow++)
                        ret_ledon[iIdxLine, led_count + iIdxRow] = curr_ledon[iIdxLine, iIdxRow];

                led_count += curr_ledon.GetLength(1);
            }

            return ret_ledon;
        }

        public void font_from_resource(string resname)
        { font_from_reader(new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resname))); }

        public void font_from_file(string filename) { font_from_reader(new XmlTextReader(filename)); }

        private void font_from_reader(XmlTextReader reader_xml)
        {
            List<matrix_symbol_font> lstFontList = new List<matrix_symbol_font>();

            // Init the reader
            reader_xml.WhitespaceHandling = WhitespaceHandling.None;

            // While there is somthing to read
            while (reader_xml.Read())
            {
                List<matrix_symbol> lstSymbolList = new List<matrix_symbol>();

                // End of font ?
                if (reader_xml.Name != "LedMatrixSymbolFont")
                    continue;

                // New font
                string sFontName = reader_xml.GetAttribute("FontName");

                // Identify the symbols of the font
                while (reader_xml.Read())
                {
                    List<led_online> lstLedOnLine = new List<led_online>();

                    // End of sybmols ?
                    if (reader_xml.Name != "LedMatrixSymbol") break;

                    // New symbol
                    uint uiSmbCode = Convert.ToUInt32(reader_xml.GetAttribute("SymbolCode"));
                    string des = reader_xml.GetAttribute("Description");

                    // Identify the Led matrix of the symbol
                    while (reader_xml.Read())
                    {
                        if (reader_xml.Name != "LedLine") break;

                        lstLedOnLine.Add(new led_online(Convert.ToInt32(reader_xml.GetAttribute("LineNumber"))
                            , reader_xml.GetAttribute("LineLedOn")));

                    }

                    lstSymbolList.Add(new matrix_symbol(uiSmbCode, des, led_online_to_led_onmatrix(lstLedOnLine)));

                }// Loop on symbol processing

                lstFontList.Add(new matrix_symbol_font(sFontName, lstSymbolList));

            }// Loop on font processing

            // Valid font collection ?
            if (lstFontList.Count == 0 || lstFontList[0]._symbols.Count == 0) return;

            _font_collection = new matrix_symbol_collection(lstFontList);
        }

        /// <summary>
        /// Convert a xml ledOn lines to a ledOn bool array
        /// </summary>
        /// <param name="p_lstLedOnLine">List of led led on line to convert</param>
        /// <returns>LedOn line array</returns>
        private bool[,] led_online_to_led_onmatrix(List<led_online> p_lstLedOnLine)
        {
            int iMaxLineSize = 0;
            int iMaxLineNb = 0;
            bool[,] bReturnLedOnMatrix;

            // Get the size of the matrix
            foreach (led_online lolLine in p_lstLedOnLine)
            {
                if (lolLine._led_on.Length > iMaxLineSize) iMaxLineSize = lolLine._led_on.Length;

                if (lolLine._line_no > iMaxLineNb) iMaxLineNb = lolLine._line_no;
            }

            // Creation of the return matrix
            bReturnLedOnMatrix = new bool[iMaxLineNb + 1, iMaxLineSize];

            // Build the matix
            foreach (led_online lolLine in p_lstLedOnLine)
            {
                for (int iIdxChar = 0; iIdxChar < lolLine._led_on.Length; iIdxChar++)
                    if (lolLine._led_on[iIdxChar] == '#') bReturnLedOnMatrix[lolLine._line_no, iIdxChar] = true;
            }

            return bReturnLedOnMatrix;
        }

        #endregion

        #region Events

        void timer_Tick(object sender, EventArgs e)
        {
            if (_wait_ticks > 0) { _wait_ticks--; Refresh(); return; }

            move_items(_tick_count);
            _tick_count++;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            pos_and_size();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics gfx = pe.Graphics;

            // Calling the base class OnPaint
            base.OnPaint(pe);

            // Antialiasing
            gfx.SmoothingMode = SmoothingMode.AntiAlias;

            // Return if non consistant led
            if (_led_property_list[0][0]._radius == 0) return;

            bool raise_event = true;

            for (int iline = 0; iline < _led_property_list.Count; iline++)
            {
                for (int irow = 0; irow < _led_property_list[0].Count; irow++)
                {
                    bool led_on = false;

                    foreach (matrix_item itm in _item_list)
                    {
                        int line_item = iline - itm.GetLineOffset();
                        int row_item = irow - itm.GetRowOffset();

                        if ((line_item < 0) || (line_item >= itm.GetMasterLedOn().GetLength(0)) ||
                           (row_item < 0) || (row_item >= itm.GetMasterLedOn().GetLength(1))) continue;

                        led_on |= itm.GetMasterLedOn()[line_item, row_item];
                    }

                    // State of the led
                    if (led_on == true)
                    {
                        raise_event = false;
                        _can_raise_empty = true;

                        if (_led_style == led_style.Square) pe.Graphics.FillRectangle(_on_brush,
                            _led_property_list[iline][irow]._location.X - _led_property_list[iline][irow]._radius,
                            _led_property_list[iline][irow]._location.Y - _led_property_list[iline][irow]._radius,
                            _led_property_list[iline][irow]._radius << 1, _led_property_list[iline][irow]._radius << 1);
                        else pe.Graphics.FillPie(_on_brush,
                              _led_property_list[iline][irow]._location.X - _led_property_list[iline][irow]._radius,
                              _led_property_list[iline][irow]._location.Y - _led_property_list[iline][irow]._radius,
                              _led_property_list[iline][irow]._radius << 1, _led_property_list[iline][irow]._radius << 1, 0, 360);
                    }
                    else
                    {
                        if (_led_style == led_style.Square) pe.Graphics.FillRectangle(_off_brush,
                                _led_property_list[iline][irow]._location.X - _led_property_list[iline][irow]._radius,
                                _led_property_list[iline][irow]._location.Y - _led_property_list[iline][irow]._radius,
                                _led_property_list[iline][irow]._radius << 1, _led_property_list[iline][irow]._radius << 1);
                        else pe.Graphics.FillPie(_off_brush,
                                _led_property_list[iline][irow]._location.X - _led_property_list[iline][irow]._radius,
                                _led_property_list[iline][irow]._location.Y - _led_property_list[iline][irow]._radius,
                                _led_property_list[iline][irow]._radius << 1, _led_property_list[iline][irow]._radius << 1, 0, 360);
                    }
                }
            }

            if (raise_event && _can_raise_empty) { DrawEmpty(this, new EventArgs()); _can_raise_empty = false; }
        }

        #endregion
    }
}

// 1300