using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace muzikaaa.classes
{
    public class display_row_element
    {
        public static int _id_el = 0;
        int _id;
        string _name, _type, _text;

        public display_row_element(string type)
            :this(type, "", "")
        {
        }

        public display_row_element(string type, string name, string text)
        {
            _id = _id_el++;
            _name = name;
            _type = type;
            _text = text;
        }

        public int id { get { return _id; } }
        public string name { get { return _name; } }
        public string type { get { return _type; } }
        public string text { get { return _text; } }
    }

    public class display_row
    {
        string _name, _align;
        List<display_row_element> _elements = null;

        public display_row()
            :this("", "", null)
        {
        }

        public display_row(string name, string align, IEnumerable<display_row_element> elements)
        {
            _name = name;
            _align = align;
            _elements = elements != null ? new List<display_row_element>(elements) : new List<display_row_element>();
        }

        public string name { get { return _name; } }
        public string align { get { return _align; } }
        public List<display_row_element> elements { get { return _elements; } }
    }

    public class display
    {
        string _name;
        int _led_size; 
        double _coeff;
        string _back_color, _on_color, _off_color;
        matrix_control _ctrl = null;
        List<display_row> _rows = null;

        public event EventHandler btnClick;

        public display(string name, int width, int y, int led_size, double coeff, string back_color
            , string on_color, string off_color, IEnumerable<display_row> rows)
        {
            _name = name;
            _led_size = led_size;
            _coeff = coeff;
            _back_color = back_color;
            _on_color = on_color;
            _off_color = off_color;
            _ctrl = new matrix_control(config.setting("symbol_path"));
            _ctrl.MouseMove += new MouseEventHandler(_ctrl_MouseMove);
            _ctrl.Click += new EventHandler(_ctrl_Click);

            // rows
            _rows = new List<display_row>(rows);
            if (_rows.Count == 0) _rows.Add(new display_row("", "", new List<display_row_element> { new display_row_element("text") }));
            for(int i = 0; i < _rows.Count; i++)
            {
                display_row row = _rows[i];

                if (row.elements.Count == 0) row.elements.Add(new display_row_element("text"));

                for(int ir = 0; ir < row.elements.Count; ir++)
                {
                    display_row_element el = row.elements[ir];
                    int points = _ctrl.ledon_from_string(el.text).GetUpperBound(1);
                    int x = row.align != "" && row.align == "right"
                        ? (width / _led_size) - (points + _ctrl.get_new_pos_x(ir)) : _ctrl.get_new_pos_x(ir);

                    if (el.type == "text") _ctrl.add_text_item(el.id, el.text, new Point(x, item_y(i)), item_direction.None, item_speed.None);
                    else _ctrl.add_btn_item(el.id, el.name, el.text, new Point(x, item_y(i)), item_direction.None, item_speed.None);
                }
            }

            _ctrl.Name = _name;
            _ctrl.Location = new Point(0, y);
            _ctrl.Size = new Size(width, this.height);
            _ctrl.led_size = _led_size;

            _ctrl.set_led_style(led_style.Square);
            _ctrl.BackColor = color_from_string(_back_color);
            _ctrl.led_on_color = color_from_string(_on_color);
            _ctrl.led_off_color = color_from_string(_off_color);
            _ctrl.size_coeff = _coeff;
        }

        void _ctrl_Click(object sender, EventArgs e)
        {
            matrix_item i = get_btn(((MouseEventArgs)e).X, ((MouseEventArgs)e).Y);
            if(i != null)
                btnClick(i, new EventArgs());
        }

        void _ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            matrix_item i = get_btn(e.X, e.Y);
            if (i != null) System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
        }

        public matrix_item get_btn(int X, int Y)
        {
            int mX = X / _led_size, mY = Y / _led_size;
            foreach (matrix_item i in _ctrl.items)
            {
                int width = i.GetMasterLedOn().GetUpperBound(1); // *_led_size;
                int height = i.GetMasterLedOn().GetUpperBound(0); // *_led_size;
                if (mX > i.location.X && mX < i.location.X + width
                    && mY > i.location.Y && mY < i.location.Y + height)
                {
                    if (i.type == matrix_item.type_item.button) return i;
                }
            }

            return null;
        }

        public string name { get { return _name; } }
        public int led_size { get { return _led_size; } }
        public int led_rows { get { return _rows.Count * 10; } }
        public matrix_control control { get { return _ctrl; } }
        public List<display_row> rows { get { return _rows; } }
        public int height { get { return _led_size * led_rows; } }
        public double coeff { get { return _coeff; } }
        public string back_color { get { return _back_color; } }
        public string on_color { get { return _on_color; } }
        public string off_color { get { return _off_color; } }

        //public void set_text(string name, string text)
        //{
        //    try
        //    { set_text(_rows.Find(row => row.name != "" && row.name == name).id, text); }
        //    catch { }
        //}

        public Color color_from_string(string color)
        {
            return color.Substring(0, 1) != "#" ? Color.FromName(color)
                : Color.FromArgb(int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));
        }

        public void set_text(int id_row, string text)
        {
            try { _ctrl.set_item_text(id_row, text); }
            catch { }
        }

        public void set_text_first(string text)
        {
            try { _ctrl.set_item_text(_rows[0].elements[0].id, text); }
            catch { }
        }

        public string get_text_first() {
          return _ctrl.item_text(_rows[0].elements[0].id); 
        }

      public void set_location(int irow, Point pt)
        {
            try
            {
                if (_rows.Count == 0 || _rows.Count <= irow) return;

                _ctrl.set_item_location(_ctrl.items[irow].GetId(), pt);
            }
            catch { }
        }

        public void set_direction(int irow, item_direction dir)
        {
            try
            {
                if (_rows.Count == 0 || _rows.Count <= irow) return;

                _ctrl.set_item_direction(_ctrl.items[irow].GetId(), dir);
            }
            catch { }
        }

        public void set_speed(int irow, item_speed speed)
        {
            try
            {
                if (_rows.Count == 0 || _rows.Count <= irow) return;

                _ctrl.set_item_speed(_ctrl.items[irow].GetId(), speed);
            }
            catch { }
        }

        public void clear_text()
        {
            try
            {
                foreach (display_row row in _rows)
                    foreach(display_row_element el in row.elements)
                        if (_ctrl.item_text(el.id) != "") _ctrl.set_item_text(el.id, "");
            }
            catch { }
        }

        public int item_y(int i_item) { return (i_item * 10) + 1; }
    }
}
