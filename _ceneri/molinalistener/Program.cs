using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace molinalistener {
  static class Program {
    static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main (string[] args) {
      //Application.EnableVisualStyles();
      //Application.SetCompatibleTextRenderingDefault(false);
      try {
        if (args.Count() == 0) throw new Exception("non hai specificato i prefissi!");

        log.Info("starting app...");

        HttpListener listener = new HttpListener();
        foreach (string pr in args) {
          log.Info("add listener: " + pr);
          listener.Prefixes.Add(pr);
        }
        listener.Start();
        log.Info("listening...");
        while (true) {
          try {
            HttpListenerContext context = listener.GetContext();
            log.InfoFormat("web-server request: {0}", context.Request.Url.ToString());
            context.Response.StatusCode = 200;
            context.Response.SendChunked = true;

            HttpListenerResponse response = context.Response;
            string responseString = @"<html><head><style>
              body { font-family:segoe ui; overflow:auto; }
              </style></head><body><h1>benvenuto in MolinafY!</h1></body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString); 
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

          } catch (Exception ex) { log.Error(ex); }
        }

      } catch (Exception ex) { log.Error(ex); }

    }
  }
}
