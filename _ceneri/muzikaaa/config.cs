using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using muzikaaa.classes;

namespace muzikaaa
{
    public class config
    {
        public enum play_modes { random, linear }

        XmlDocument _doc = null;
        string _path = "";
        bool _modified = false;

        public config(string path)
        {
            _doc = new XmlDocument();
            _doc.Load(_path = path);
        }

        public void save() { if (_doc != null) _doc.Save(_path); _modified = false; }
        public bool modified { get { return _modified; } }

        public static string setting(string key)
        {
            try { return parse(System.Configuration.ConfigurationManager.AppSettings[key]); }
            catch (Exception ex) { player.show_exception("key: '" + key + "'"); throw ex; }
        }

        public static bool boolean(string key) { return bool.Parse(setting(key)); }

        protected static string parse(string value)
        {
            // {@app_folder}
            value = value.Replace("{@app_folder}", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));

            // {@setting:<nome_var>}
            while (value.IndexOf("{@setting") >= 0)
            {
                string var = par(value, "{@setting");
                value = value.Replace("{@setting:" + var + "}", setting(var));
            }

            return value;
        }

        protected static string par(string contents, string key)
        {
            int startPar = contents.IndexOf(key + ":") + key.Length + 1;
            return contents.Substring(startPar, contents.IndexOf("}", startPar) - startPar);
        }

        protected XmlNode node_pc() { return _doc.SelectSingleNode("/mzk/pcs/pc[@name='" + System.Environment.MachineName + "']"); }

        public play_modes play_mode()
        {
            return _doc.SelectSingleNode("/mzk").Attributes["play"] != null
                ? (play_modes)Enum.Parse(typeof(play_modes), _doc.SelectSingleNode("/mzk").Attributes["play"].Value) : play_modes.random;
        }

        public void set_last_index_played(int i)
        {
            if (_doc.SelectSingleNode("/mzk").Attributes["last_index"] == null)
                _doc.SelectSingleNode("/mzk").Attributes.Append(_doc.CreateAttribute("last_index"));

            _doc.SelectSingleNode("/mzk").Attributes["last_index"].Value = i.ToString();
        }

        public int last_index_played()
        {
            return _doc.SelectSingleNode("/mzk").Attributes["last_index"] == null ? -1
                : int.Parse(_doc.SelectSingleNode("/mzk").Attributes["last_index"].Value);
        }

        public List<string> get_songs(string path_folder)
        {
            if (node_pc() == null) return new List<string>();

            List<song> paths = new List<song>();
            add_songs(paths, "", path_folder);

            return new List<string>(paths.Select(x => x.path));
        }

        public List<song> get_songs()
        {
            if (node_pc() == null) return new List<song>();

            List<song> paths = new List<song>();
            foreach (XmlNode folder in _doc.SelectNodes("/mzk/folders/folder[@pc_id='" + node_pc().Attributes["id"].Value
                + "'][@active='true'][@type='cestone']"))
                add_songs(paths, folder.Attributes["name"].Value, folder.InnerText);

            return paths;
        }

        public bool add_songs(List<song> songs, string name_cestone, string path_folder)
        {
            if (!System.IO.Directory.Exists(path_folder))
                return false;

            int prev = songs.Count;
            songs.AddRange(System.IO.Directory.GetFiles(path_folder, "*.*")
                .Where(file => supported_ext(System.IO.Path.GetExtension(file.ToLower()))).Select(x => new song(x, name_cestone)));

            return songs.Count > prev;
        }

        public List<folder> get_folders(folder.folder_type type)
        {
            List<folder> folders = new List<folder>();
            if (node_pc() == null) return folders;
            foreach (XmlNode folder in _doc.SelectNodes("/mzk/folders/folder[@pc_id='" + node_pc().Attributes["id"].Value
                + "'][@type='" + type.ToString() + "']"))
                folders.Add(new folder(int.Parse(folder.Attributes["id"].Value), folder.InnerText, folder.Attributes["name"].Value
                    , folder.Attributes["des"] != null ? folder.Attributes["des"].Value : ""
                    , folder.Attributes["active"] != null ? bool.Parse(folder.Attributes["active"].Value) : true, type));

            return folders;
        }

        public int new_folder_id()
        {
            return _doc.SelectSingleNode("/mzk/folders/folder") != null
                ? _doc.SelectNodes("/mzk/folders/folder").Cast<XmlNode>().Max(x => int.Parse(x.Attributes["id"].Value)) + 1 : 1;
        }

        public void save_folder(folder fld)
        {
            try
            {
                XmlNode node = _doc.SelectSingleNode("/mzk/folders/folder[@id='" + fld.id.ToString() + "']");
                if (node == null)
                {
                    if (node_pc() == null) throw new Exception("non è configurato correttamente il pc corrente all'interno del config xml");

                    node = _doc.SelectSingleNode("/mzk/folders").AppendChild(_doc.CreateElement("folder"));
                    node.Attributes.Append(_doc.CreateAttribute("id")).Value = fld.id.ToString();
                    node.Attributes.Append(_doc.CreateAttribute("pc_id")).Value = node_pc().Attributes["id"].Value;
                    node.Attributes.Append(_doc.CreateAttribute("name"));
                    node.Attributes.Append(_doc.CreateAttribute("des"));
                    node.Attributes.Append(_doc.CreateAttribute("active"));
                    node.Attributes.Append(_doc.CreateAttribute("type")).Value = fld.type.ToString();
                }

                node.Attributes["name"].Value = fld.name;
                node.Attributes["des"].Value = fld.des;
                node.Attributes["active"].Value = fld.active ? "true" : "false";
                node.InnerText = fld.path;

                _modified = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public void remove_folder(int id)
        {
            try
            {
                XmlNode node = _doc.SelectSingleNode("/mzk/folders/folder[@id='" + id + "']");
                if (node != null)
                    node.ParentNode.RemoveChild(node);

                _modified = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        protected bool supported_ext(string ext) { return _doc.SelectSingleNode("/mzk/supported/ext[.='" + ext.ToLower() + "']") != null; }

        public XmlNodeList displays() { return _doc.SelectNodes("/mzk/screen/display"); }
        public display create_display(XmlNode node, int width, int y)
        {
            return new display(node.Attributes["name"].Value, width, y, int.Parse(node.Attributes["led-size"].Value)
              , double.Parse(node.Attributes["coeff"].Value), node.Attributes["back-color"].Value
              , node.Attributes["on-color"].Value, node.Attributes["off-color"].Value
              , node.SelectNodes("row").Cast<XmlNode>().Select(row => new display_row(row.Attributes["name"] != null ? row.Attributes["name"].Value : ""
                , row.Attributes["align"] != null ? row.Attributes["align"].Value : ""
                , row.SelectNodes("text|btn").Cast<XmlNode>().Select(x => new display_row_element(x.Name, x.Attributes["name"] != null ? x.Attributes["name"].Value : "", x.InnerText)))));
        }

        public XmlNodeList btn_keys() { return _doc.SelectNodes("/mzk/screen//display//row/btn[@keys]"); }
    }
}
