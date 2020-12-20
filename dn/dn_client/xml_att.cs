using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dlib.xml;
using System.IO;

namespace dn_client
{
  public class xml_att : xml_doc
  {
    public xml_att(string path) : base(path) { }

    public static xml_att open()
    {
      string folder = Program._c.config.get_var("client.client-tmp-path").value
        , iname = Program._c.config.get_var("client.index-att").value, i = Path.Combine(folder, iname);
      if(!File.Exists(i)) File.WriteAllText(i, "<root/>");
      return new xml_att(i);
    }

    public bool exists_file(int id_file)
    {
      return this.node($"/root/file[@id='{id_file}']").node != null;
    }

    public bool exists_file(int id_file, out xml_node n)
    {
      n = this.node($"/root/file[@id='{id_file}']"); return n.node != null;
    }

    public xml_node add_file(fi i, DateTime lwt)
    {
      return root_node.add_node("file", new Dictionary<string, string>() { { "id", i.id_file.ToString() }
        , { "name", i.file_name }, { "http_path", i.http_path }, { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } });
    }

  }
}
