using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace molinafy {
  public class sptfy {
    private static Random _random = new Random();

    static public dynamic get_playlists (sptfy_token token, string user) {
      WebRequest request = WebRequest.Create(string.Format("https://api.spotify.com/v1/users/{0}/playlists", user));
      request.Method = "GET";
      request.Headers.Add("Authorization", "Bearer " + token.access_token);
      request.ContentType = "application/json; charset=utf-8";

      string res = null;
      WebResponse response = request.GetResponse();
      try {
        if (((System.Net.HttpWebResponse)(response)).StatusCode == HttpStatusCode.OK) {
          using (Stream dataStream = response.GetResponseStream()) {
            StreamReader reader = new StreamReader(dataStream);
            res = reader.ReadToEnd();
          }
          return JObject.Parse(res);
        } else throw new Exception("ERROR");
      } catch (Exception ex) { throw ex; } finally { response.Close(); }
    }

    public static string gen_state () {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, 16)
        .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

  }
}
