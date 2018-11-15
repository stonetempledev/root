using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace muzikaaa
{
  public class key_info
  {
    private enum KeyStates { None = 0, Down = 1, Toggled = 2 }

    private key_info() { }
    [DllImport("user32")]

    private static extern short GetKeyState(int vKey);

    private static KeyStates key_state(int key) {
      KeyStates state = KeyStates.None;

      short retVal = GetKeyState((int)key);

      //If the high-order bit is 1, the key is down otherwise, it is up.
      if ((retVal & 0x8000) == 0x8000) state |= KeyStates.Down;

      //If the low-order bit is 1, the key is toggled.
      if ((retVal & 1) == 1) state |= KeyStates.Toggled;

      return state;
    }

    public static bool is_key_down(int key) { return KeyStates.Down == (key_state(key) & KeyStates.Down); }

    public static bool is_key_toggled(int key) { return KeyStates.Toggled == (key_state(key) & KeyStates.Toggled); }
  }
}
