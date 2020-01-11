﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace mlib.tools {
  public class file {
    protected string _path;
    public file (string path) { _path = path; }

    public string path { get { return _path; } }
    public DateTime lw { get { return File.GetLastWriteTime(_path); } }
    public string ext { get { return Path.GetExtension(_path); } }
    public string file_name { get { return Path.GetFileName(_path); } }
    public string dir_name { get { return Path.GetDirectoryName(_path); } }
    public long size { get { return new System.IO.FileInfo(_path).Length; } }

    public static List<file> dir (string path, string pattern, bool order_by_lw = false, bool order_by_name = false) {
      List<file> res = new List<file>();
      foreach (string f in Directory.EnumerateFiles(path, pattern))
        res.Add(new file(f));
      if (order_by_lw) return res.OrderByDescending(f => f.lw.ToString("yyyy/MM/dd HH:mm:ss")).ToList();
      else if (order_by_name) return res.OrderBy(f => f.file_name).ToList();
      return res;
    }
  }

  public class unc_access : IDisposable {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct USE_INFO_2 {
      internal String ui2_local;
      internal String ui2_remote;
      internal String ui2_password;
      internal UInt32 ui2_status;
      internal UInt32 ui2_asg_type;
      internal UInt32 ui2_refcount;
      internal UInt32 ui2_usecount;
      internal String ui2_username;
      internal String ui2_domainname;
    }

    [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern UInt32 NetUseAdd(
        String UncServerName,
        UInt32 Level,
        ref USE_INFO_2 Buf,
        out UInt32 ParmError);

    [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern UInt32 NetUseDel(
        String UncServerName,
        String UseName,
        UInt32 ForceCond);

    private bool _disposed = false;

    private string _sUncPath;
    private string _sUser;
    private string _sPassword;
    private string _sDomain;
    private int _iLastError;

    /// <summary>
    /// A disposeable class that allows access to a UNC resource with credentials.
    /// </summary>
    public unc_access() {
    }

    /// <summary>
    /// The last system error code returned from NetUseAdd or NetUseDel.  Success = 0
    /// </summary>
    public int LastError {
      get { return _iLastError; }
    }

    public string DesLastError {
      get { return _iLastError > 0 ? new Win32Exception(_iLastError).Message : ""; }
    }


    public void Dispose() {
      if (!_disposed) {
        NetUseDelete();
      }
      _disposed = true;
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Connects to a UNC path using the credentials supplied.
    /// </summary>
    /// <param name="uncPath">Fully qualified domain name UNC path</param>
    /// <param name="user">A user with sufficient rights to access the path.</param>
    /// <param name="domain">Domain of User.</param>
    /// <param name="password">Password of User</param>
    /// <returns>True if mapping succeeds.  Use LastError to get the system error code.</returns>
    public bool NetUseWithCredentials(string uncPath, string user, string domain, string password) {
      _sUncPath = uncPath;
      _sUser = user;
      _sPassword = password;
      _sDomain = domain;
      return NetUseWithCredentials();
    }

    private bool NetUseWithCredentials() {
      try {
        var useinfo = new USE_INFO_2 {
          ui2_remote = _sUncPath,
          ui2_username = _sUser,
          ui2_domainname = _sDomain,
          ui2_password = _sPassword,
          ui2_asg_type = 0,
          ui2_usecount = 1
        };

        uint paramErrorIndex;
        uint returncode = NetUseAdd(null, 2, ref useinfo, out paramErrorIndex);
        _iLastError = (int)returncode;
        return returncode == 0;
      } catch {
        _iLastError = Marshal.GetLastWin32Error();
        return false;
      }
    }

    /// <summary>
    /// Ends the connection to the remote resource
    /// </summary>
    /// <returns>True if it succeeds.  Use LastError to get the system error code</returns>
    public bool NetUseDelete() {
      try {
        uint returncode = NetUseDel(null, _sUncPath, 2);
        _iLastError = (int)returncode;
        return (returncode == 0);
      } catch {
        _iLastError = Marshal.GetLastWin32Error();
        return false;
      }
    }

    ~unc_access() {
      Dispose();
    }
  }
}
